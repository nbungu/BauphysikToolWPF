#version 430 core

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec4 vColor;

uniform mat4 uProjection; // <-- Add this

out vec4 fColor;

void main()
{
    gl_Position = uProjection * vec4(vPosition, 1.0); // <-- Apply projection
    fColor = vColor;
}