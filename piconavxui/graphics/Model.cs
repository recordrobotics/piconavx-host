using Silk.NET.OpenGL;
using System.Numerics;
using Silk.NET.Assimp;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using AssimpMaterial = Silk.NET.Assimp.Material;

namespace piconavx.ui.graphics
{
    public class Model : Controller, IDisposable
    {
        public Model(string path, bool gamma = false)
        {
            materialNames = new Dictionary<string, int>();
            Transforms = [new Transform()];
            var assimp = Silk.NET.Assimp.Assimp.GetApi();
            _assimp = assimp;
            LoadModel(path);
            Materials = new Material?[MaterialNames.Values.Max() + 1];
        }

        private Model(Dictionary<string, int> materialNames, Assimp assimp, List<Texture>? texturesLoaded, List<Mesh> meshes)
        {
            this.materialNames = materialNames;
            Transforms = [new Transform()];
            _assimp = assimp;
            Materials = new Material?[MaterialNames.Values.Max() + 1];
            _texturesLoaded = texturesLoaded;
            Meshes = meshes;
        }

        public Model Clone()
        {
            return new Model(materialNames, _assimp, _texturesLoaded, Meshes)
            {
                renderPriority = this.renderPriority
            };
        }

        private Assimp _assimp;
        private List<Texture>? _texturesLoaded = new List<Texture>();
        public string Directory { get; protected set; } = string.Empty;
        public List<Mesh> Meshes { get; protected set; } = new List<Mesh>();
        public Transform?[] Transforms { get; set; }
        public Transform Transform
        {
            get => GetTransform(0)!; set
            {
                Array.Fill(Transforms, value);
            }
        }

        public Transform? GetTransform(int index)
        {
            if (Transforms.Length > index)
                return Transforms[index];
            else
                return null;
        }

        public Material?[] Materials { get; set; }
        public Material? Material { get => GetMaterial(0); set
            {
                Materials = new Material?[MaterialNames.Values.Max() + 1];
                Array.Fill(Materials, value);
            }
        }

        private Dictionary<string, int> materialNames;
        public IReadOnlyDictionary<string, int> MaterialNames { get => materialNames.AsReadOnly(); }

        public Material? GetMaterial(int index)
        {
            if (Materials.Length > index)
                return Materials[index];
            else
                return null;
        }

        public void SetMaterial(string name, Material? material)
        {
            Materials[MaterialNames[name]] = material;
        }

        private RenderPriority renderPriority = RenderPriority.DrawOpaque;
        public RenderPriority RenderPriority
        {
            get => renderPriority; set
            {
                renderPriority = value;
                if (Scene.InEvent) // If set from inside an event, defer until finished
                    Scene.InvokeLater(Resubscribe, DeferralMode.NextEvent);
                else if(isSubscribed)
                    Resubscribe();
            }
        }

        private unsafe void LoadModel(string path)
        {
            byte[] bytes = EmbeddedResource.ReadAllBytes(path)!;
            var scene = _assimp.ImportFileFromMemory(new ReadOnlySpan<byte>(bytes), (uint)bytes.Length, (uint)PostProcessSteps.Triangulate, (byte*)null);

            if (scene == null || scene->MFlags == Silk.NET.Assimp.Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
            {
                var error = _assimp.GetErrorStringS();
                throw new Exception(error);
            }
            Directory = path;

            ProcessNode(scene->MRootNode, scene);
        }

        private unsafe void ProcessNode(Node* node, Silk.NET.Assimp.Scene* scene)
        {
            for (var i = 0; i < node->MNumMeshes; i++)
            {
                var mesh = scene->MMeshes[node->MMeshes[i]];
                Meshes.Add(ProcessMesh(mesh, scene));

            }

            for (var i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(node->MChildren[i], scene);
            }
        }

        private unsafe Mesh ProcessMesh(AssimpMesh* mesh, Silk.NET.Assimp.Scene* scene)
        {
            // data to fill
            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();
            List<Texture> textures = new List<Texture>();

            // walk through each of the mesh's vertices
            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                Vertex vertex = new Vertex();
                vertex.BoneIds = new int[Vertex.MAX_BONE_INFLUENCE];
                vertex.Weights = new float[Vertex.MAX_BONE_INFLUENCE];

                vertex.Position = mesh->MVertices[i];

                // normals
                if (mesh->MNormals != null)
                    vertex.Normal = mesh->MNormals[i];
                // tangent
                if (mesh->MTangents != null)
                    vertex.Tangent = mesh->MTangents[i];
                // bitangent
                if (mesh->MBitangents != null)
                    vertex.Bitangent = mesh->MBitangents[i];

                // texture coordinates
                if (mesh->MTextureCoords[0] != null) // does the mesh contain texture coordinates?
                {
                    // a vertex can contain up to 8 different texture coordinates. We thus make the assumption that we won't 
                    // use models where a vertex can have multiple texture coordinates so we always take the first set (0).
                    Vector3 texcoord3 = mesh->MTextureCoords[0][i];
                    vertex.TexCoords = new Vector2(texcoord3.X, texcoord3.Y);
                }

                vertices.Add(vertex);
            }

            // now wak through each of the mesh's faces (a face is a mesh its triangle) and retrieve the corresponding vertex indices.
            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                Face face = mesh->MFaces[i];
                // retrieve all indices of the face and store them in the indices vector
                for (uint j = 0; j < face.MNumIndices; j++)
                    indices.Add(face.MIndices[j]);
            }

            // process materials
            int materialIndex = (int)mesh->MMaterialIndex;
            AssimpMaterial* material = scene->MMaterials[materialIndex];
            AssimpString name;
            string materialName = string.Empty;
            if(_assimp.GetMaterialString(material, Assimp.MaterialNameBase, 0, 0, &name) == Return.Success)
            {
                materialName = name.AsString;
            }

            materialNames.Add(materialName, materialIndex);

            // we assume a convention for sampler names in the shaders. Each diffuse texture should be named
            // as 'texture_diffuseN' where N is a sequential number ranging from 1 to MAX_SAMPLER_NUMBER. 
            // Same applies to other texture as the following list summarizes:
            // diffuse: texture_diffuseN
            // specular: texture_specularN
            // normal: texture_normalN

            // 1. diffuse maps
            var diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
            if (diffuseMaps.Any())
                textures.AddRange(diffuseMaps);
            // 2. specular maps
            var specularMaps = LoadMaterialTextures(material, TextureType.Specular, "texture_specular");
            if (specularMaps.Any())
                textures.AddRange(specularMaps);
            // 3. normal maps
            var normalMaps = LoadMaterialTextures(material, TextureType.Normals, "texture_normal");
            if (normalMaps.Any())
                textures.AddRange(normalMaps);
            // 4. height maps
            var heightMaps = LoadMaterialTextures(material, TextureType.Height, "texture_height");
            if (heightMaps.Any())
                textures.AddRange(heightMaps);

            // return a mesh object created from the extracted mesh data
            var result = new Mesh(BuildVertices(vertices), BuildIndices(indices), textures, materialIndex);
            return result;
        }

        private unsafe List<Texture> LoadMaterialTextures(AssimpMaterial* mat, TextureType type, string typeName)
        {
            var textureCount = _assimp.GetMaterialTextureCount(mat, type);
            List<Texture> textures = new List<Texture>();
            for (uint i = 0; i < textureCount; i++)
            {
                AssimpString path;
                _assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null);
                bool skip = false;
                for (int j = 0; j < _texturesLoaded!.Count; j++)
                {
                    if (_texturesLoaded[j].Path == path)
                    {
                        textures.Add(_texturesLoaded[j]);
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                {
                    var texture = new Texture(Directory, type);
                    texture.Path = path;
                    textures.Add(texture);
                    _texturesLoaded.Add(texture);
                }
            }
            return textures;
        }

        private float[] BuildVertices(List<Vertex> vertexCollection)
        {
            var vertices = new List<float>();

            foreach (var vertex in vertexCollection)
            {
                vertices.Add(vertex.Position.X);
                vertices.Add(vertex.Position.Y);
                vertices.Add(vertex.Position.Z);
                vertices.Add(vertex.Normal.X);
                vertices.Add(vertex.Normal.Y);
                vertices.Add(vertex.Normal.Z);
                vertices.Add(vertex.Tangent.X);
                vertices.Add(vertex.Tangent.Y);
                vertices.Add(vertex.Tangent.Z);
                vertices.Add(vertex.TexCoords.X);
                vertices.Add(vertex.TexCoords.Y);
                vertices.Add(vertex.Bitangent.X);
                vertices.Add(vertex.Bitangent.Y);
                vertices.Add(vertex.Bitangent.Z);
            }

            return vertices.ToArray();
        }

        private uint[] BuildIndices(List<uint> indices)
        {
            return indices.ToArray();
        }

        bool isSubscribed = false;
        public override void Subscribe()
        {
            isSubscribed = true;
            Scene.Render += new PrioritizedAction<RenderPriority, double, RenderProperties>(renderPriority, Render);
        }

        public override void Unsubscribe()
        {
            isSubscribed = false;
            Scene.Render -= Render;
        }

        private void Resubscribe()
        {
            Unsubscribe();
            Subscribe();
        }

        public void Render(double deltaTime, RenderProperties properties)
        {
            properties.Transforms = Transforms;

            Material? lastMaterial = null;

            foreach (var mesh in Meshes)
            {
                Material material = GetMaterial(mesh.Material) ?? Material.DefaultMaterial;
                if (material != lastMaterial)
                {
                    lastMaterial = material;
                    material.Use(properties);
                }

                mesh.Bind();
                unsafe
                {
                    if (material.UseInstanced)
                    {
                        if (material.ExtendedDrawCall && Transforms.Length > Material.MAX_INSTANCE_COUNT)
                        {
                            int rendered = 0;
                            while (rendered < Transforms.Length)
                            {
                                int left = Transforms.Length - rendered;
                                int instances = left > Material.MAX_INSTANCE_COUNT ? Material.MAX_INSTANCE_COUNT : left;
                                properties.Transforms = Transforms[rendered..(rendered + instances)];
                                material.UpdateInstanceBuffer(properties);
                                Window.GL.DrawElementsInstanced(Silk.NET.OpenGL.PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt, null, (uint)instances);
                                rendered += instances;
                            }
                        }
                        else
                        {
                            material.UpdateInstanceBuffer(properties);
                            Window.GL.DrawElementsInstanced(Silk.NET.OpenGL.PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt, null, (uint)Math.Min(Material.MAX_INSTANCE_COUNT, Transforms.Length));
                        }
                    }
                    else
                    {
                        material.UpdateModelBuffer(properties);
                        Window.GL.DrawElements(Silk.NET.OpenGL.PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt, null);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var mesh in Meshes)
            {
                mesh.Dispose();
            }

            _texturesLoaded = null;
        }
    }
}
