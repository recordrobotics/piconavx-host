#version 330 core

#ifdef GL_ES
#define LOWP lowp
precision mediump float;
#else
#define LOWP
#endif

uniform sampler2D TextureSampler;

in vec4 v_color;
in vec2 v_texCoords;

out vec4 FragColor;

void main()
{
	FragColor = v_color * texture(TextureSampler, v_texCoords);
}