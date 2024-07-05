using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Silk.NET.Assimp;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using AssimpMaterial = Silk.NET.Assimp.Material;

namespace piconavx.ui.graphics
{
    public class Model : Controller, IDisposable
    {
        public Model(string path, bool gamma = false)
        {
            Transform = new Transform();
            var assimp = Silk.NET.Assimp.Assimp.GetApi();
            _assimp = assimp;
            LoadModel(path);
        }

        private Assimp _assimp;
        private List<Texture> _texturesLoaded = new List<Texture>();
        public string Directory { get; protected set; } = string.Empty;
        public List<Mesh> Meshes { get; protected set; } = new List<Mesh>();
        public Transform Transform { get; set; }
        public Material? Material { get; set; }

        private unsafe void LoadModel(string path)
        {
            var scene = _assimp.ImportFile(path, (uint)PostProcessSteps.Triangulate);

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
            AssimpMaterial* material = scene->MMaterials[mesh->MMaterialIndex];
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
            var result = new Mesh(BuildVertices(vertices), BuildIndices(indices), textures);
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
                for (int j = 0; j < _texturesLoaded.Count; j++)
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

        public override void Subscribe()
        {
            Scene.Render += new PrioritizedAction<RenderPriority, double, RenderProperties>(RenderPriority.DrawObjects, Render);
        }

        public override void Unsubscribe()
        {
            Scene.Render -= Render;
        }

        public void Render(double deltaTime, RenderProperties properties)
        {
            properties.Transform = Transform;
            foreach (var mesh in Meshes)
            {
                mesh.Bind();
                if (Material == null)
                    Material.DefaultMaterial.Use(properties);
                else
                    Material.Use(properties);
                unsafe
                {
                    Window.GL.DrawElements(Silk.NET.OpenGL.PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt, null);
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
