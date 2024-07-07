#version 330 core

in vec2 fTexCoords;

uniform vec4 gridColor;

out vec4 FragColor;

void main()
{
    // Pristine Grid Shader from https://medium.com/@bgolus/the-best-darn-grid-shader-yet-727f9278b9d8
    vec2 uv = fTexCoords * 10.0;
    float lineWidth = 0.01;
    vec4 uvDDXY = vec4(dFdx(uv), dFdy(uv)); //
    vec2 uvDeriv = vec2(length(uvDDXY.xz), length(uvDDXY.yw)); //
    bool invertLine = lineWidth > 0.5;
    vec2 targetWidth = vec2(invertLine ? 1.0 - lineWidth : lineWidth);
    vec2 drawWidth = clamp(targetWidth, uvDeriv, vec2(0.5));
    vec2 lineAA = uvDeriv * 1.5;
    vec2 gridUV = abs(fract(uv) * 2.0 - 1.0);
    gridUV = invertLine ? gridUV : 1.0 - gridUV;
    vec2 grid2 = smoothstep(drawWidth + lineAA, drawWidth - lineAA, gridUV);
    grid2 *= clamp(targetWidth / drawWidth,0.0,1.0);
    grid2 = mix(grid2, targetWidth, clamp(uvDeriv * 2.0 - 1.0,0.0,1.0));
    grid2 = invertLine ? 1.0 - grid2 : grid2;
    float grid = mix(grid2.x, 1.0, grid2.y);
    if (grid == 0)
        discard;
    FragColor = vec4(gridColor.rgb, gridColor.a * grid);
}