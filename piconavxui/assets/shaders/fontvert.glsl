#version 330 core

layout(location = 0) in vec3 a_position;
layout(location = 1) in vec4 a_color;
layout(location = 2) in vec2 a_texCoords0;

uniform mat4 MatrixTransform;

out vec4 v_color;
out vec2 v_texCoords;

void main()
{
	v_color = a_color;
	v_texCoords = a_texCoords0;
	gl_Position = MatrixTransform * vec4(a_position, 1.0);
}