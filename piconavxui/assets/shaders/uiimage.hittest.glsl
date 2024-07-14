#version 330 core

uniform sampler2D TextureSampler;
uniform uint uHitID;

in vec4 v_color;
in vec2 v_texCoords;

out uint raycastId;

void main()
{
	if (texture(TextureSampler, v_texCoords).a < 0.8) {
		discard;
	}
	else {
		raycastId = uHitID;
	}
}