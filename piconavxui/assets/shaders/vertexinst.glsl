#version 440 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec3 vTangent;
layout (location = 3) in vec2 vTexCoords;
layout (location = 4) in vec3 vBitangent;

layout(std140, binding = 0) uniform MatrixBlock
{
    mat4 uModel[1024];
} block_matrix;

uniform mat4 uView;
uniform mat4 uProjection;

out vec3 fPos;
out vec3 fNormal;
out vec3 fTangent;
out vec3 fBitangent;
out vec2 fTexCoords;

void main()
{
    mat4 modelMat = block_matrix.uModel[gl_InstanceID];
    gl_Position = uProjection * uView * modelMat * vec4(vPos, 1.0);
    fPos = vec3(modelMat * vec4(vPos, 1.0));
    fNormal = mat3(transpose(inverse(modelMat))) * vNormal;
    fTangent = vTangent;
    fBitangent = vBitangent;
    fTexCoords = vTexCoords;
}