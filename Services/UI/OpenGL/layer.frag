#version 430 core

in vec4 fColor;
in float dashCoord;
out vec4 FragColor;

void main()
{
    float dashLength = 10.0;
    float gapLength = 10.0;
    float cycle = dashLength + gapLength;

    if (mod(dashCoord, cycle) > dashLength)
        discard;
    
    FragColor = fColor;
}