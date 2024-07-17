#version 330 core

uniform sampler2D TextureSampler;
uniform sampler2D MaskTextureSampler;

in vec4 v_color;
in vec2 v_texCoords;
in vec2 v_texCoordsAlt;

out vec4 FragColor;

void main()
{
	FragColor = v_color * texture(TextureSampler, v_texCoords) * texture(MaskTextureSampler, v_texCoordsAlt);
}