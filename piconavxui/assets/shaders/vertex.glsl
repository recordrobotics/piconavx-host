#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec3 vTangent;
layout (location = 3) in vec2 vTexCoords;
layout (location = 4) in vec3 vBitangent;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec3 fPos;
out vec3 fNormal;
out vec3 fTangent;
out vec3 fBitangent;
out vec2 fTexCoords;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    fPos = vec3(uModel * vec4(vPos, 1.0));
    fNormal = mat3(transpose(inverse(uModel))) * vNormal;
    fTangent = vTangent;
    fBitangent = vBitangent;
    fTexCoords = vTexCoords;
}