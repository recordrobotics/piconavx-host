#version 330 core

layout(location = 0) in vec2 a_position;
layout(location = 1) in vec4 a_color;
layout(location = 2) in vec2 a_texCoords0;
layout(location = 3) in vec2 a_texCoords1;

uniform mat4 tMatrix;
uniform mat4 uMatrix;

out vec4 v_color;
out vec2 v_texCoords;
out vec2 v_texCoordsAlt;

void main()
{
	v_color = a_color;
	v_texCoords = a_texCoords0;
	v_texCoordsAlt = a_texCoords1;
	gl_Position = uMatrix * tMatrix * vec4(a_position, 0.0, 1.0);
}