#version 430 core

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec4 vColor;
layout(location = 2) in vec2 vTexCoordIn;
layout(location = 3) in vec2 vDashParamsIn;     // NEW: (dashLength, gapLength) for lines

uniform mat4 uProjection;

out vec4 fColor;
out vec2 vTexCoord;
out vec2 vDashParams;     // Pass to frag
out vec2 vFragPos;        // Needed for dashed lines

void main()
{
    gl_Position = uProjection * vec4(vPosition, 1.0);
    fColor = vColor;
    vTexCoord = vTexCoordIn;
    vDashParams = vDashParamsIn;
    vFragPos = vPosition.xy;
}