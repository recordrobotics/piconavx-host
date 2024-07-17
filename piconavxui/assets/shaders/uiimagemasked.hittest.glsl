#version 330 core

uniform sampler2D TextureSampler;
uniform sampler2D MaskTextureSampler;
uniform uint uHitID;
uniform float uAlphaClip;

in vec4 v_color;
in vec2 v_texCoords;
in vec2 v_texCoordsAlt;

out uint raycastId;

void main()
{
	if ((texture(TextureSampler, v_texCoords).a * texture(MaskTextureSampler, v_texCoordsAlt).a) < uAlphaClip) {
		discard;
	}
	else {
		raycastId = uHitID;
	}
}