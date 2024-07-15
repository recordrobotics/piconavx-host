#version 330 core

uniform sampler2D TextureSampler;
uniform uint uHitID;
uniform float uAlphaClip;

in vec4 v_color;
in vec2 v_texCoords;

out uint raycastId;

void main()
{
	if (texture(TextureSampler, v_texCoords).a < uAlphaClip) {
		discard;
	}
	else {
		raycastId = uHitID;
	}
}