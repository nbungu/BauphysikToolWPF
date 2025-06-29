#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;

out vec3 ourColor; // specify a color output to the fragment shader

uniform mat4 aTransform; // Add a uniform for the transformation matrix.

void main()
{
    ourColor = aColor;
    gl_Position = vec4(aPosition, 1.0) * aTransform;
}