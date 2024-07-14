using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace piconavx.ui.graphics
{
    public class Shader : IDisposable
    {
        private uint _handle;
        
        public string VertexPath { get; }
        public string FragmentPath { get; }

        public Shader(string vertexPath, string fragmentPath)
        {
            VertexPath = vertexPath;
            FragmentPath = fragmentPath;
            //Load the individual shaders.
            uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
            uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
            //Create the shader program.
            _handle = Window.GL.CreateProgram();
            //Attach the individual shaders.
            Window.GL.AttachShader(_handle, vertex);
            Window.GL.AttachShader(_handle, fragment);
            Window.GL.LinkProgram(_handle);
            //Check for linking errors.
            Window.GL.GetProgram(_handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {Window.GL.GetProgramInfoLog(_handle)}");
            }
            //Detach and delete the shaders
            Window.GL.DetachShader(_handle, vertex);
            Window.GL.DetachShader(_handle, fragment);
            Window.GL.DeleteShader(vertex);
            Window.GL.DeleteShader(fragment);
        }

        public void Use()
        {
            //Using the program
            Window.GL.UseProgram(_handle);
        }

        //Uniforms are properties that applies to the entire geometry
        public void SetUniform(string name, int value)
        {
            //Setting a uniform on a shader using a name.
            int location = Window.GL.GetUniformLocation(_handle, name);
            if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Window.GL.Uniform1(location, value);
        }

        public void SetUniform(string name, uint value)
        {
            //Setting a uniform on a shader using a name.
            int location = Window.GL.GetUniformLocation(_handle, name);
            if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Window.GL.Uniform1(location, value);
        }

        public unsafe void SetUniform(string name, Matrix4x4 value)
        {
            //A new overload has been created for setting a uniform so we can use the transform in our shader.
            int location = Window.GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Window.GL.UniformMatrix4(location, 1, false, (float*)&value);
        }

        public unsafe void SetUniform(string name, Matrix4x4[] value)
        {
            //A new overload has been created for setting a uniform so we can use the transform in our shader.
            int location = Window.GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            fixed (Matrix4x4* d = value)
            {
                Window.GL.UniformMatrix4(location, (uint)value.Length, false, (float*)d);
            }
        }

        public void SetUniform(string name, float value)
        {
            int location = Window.GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Window.GL.Uniform1(location, value);
        }

        public void SetUniform(string name, Vector3 value)
        {
            int location = Window.GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Window.GL.Uniform3(location, value.X, value.Y, value.Z);
        }

        public void SetUniform(string name, Vector4 value)
        {
            int location = Window.GL.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Window.GL.Uniform4(location, value.X, value.Y, value.Z, value.W);
        }

        public uint GetUniformBlockIndex(string name)
        {
            uint index = Window.GL.GetUniformBlockIndex(_handle, name);
            if(index == 0xFFFFFFFF)
            {
                throw new Exception($"{name} uniform block not found on shader.");
            }
            return index;
        }

        public void SetUniformBlock(string name, uint binding)
        {
            Window.GL.UniformBlockBinding(_handle, GetUniformBlockIndex(name), binding);
        }

        public void Dispose()
        {
            //Remember to delete the program when we are done.
            Window.GL.DeleteProgram(_handle);
        }

        private uint LoadShader(ShaderType type, string path)
        {
            string src = EmbeddedResource.ReadAllText(path)!;
            uint handle = Window.GL.CreateShader(type);
            Window.GL.ShaderSource(handle, src);
            Window.GL.CompileShader(handle);
            string infoLog = Window.GL.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
            }

            return handle;
        }
    }
}
