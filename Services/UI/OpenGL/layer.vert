#version 430 core

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec4 vColor;
layout(location = 2) in float vDashCoord;

uniform mat4 uProjection;

out vec4 fColor;
out float dashCoord;

void main()
{
    gl_Position = uProjection * vec4(vPosition, 1.0); // <-- Apply projection
    fColor = vColor;
    dashCoord = vDashCoord;
}