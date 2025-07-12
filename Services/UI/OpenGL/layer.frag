#version 430 core

uniform sampler2D texture0;
uniform int useHatchPattern;
uniform float hatchScale;

in vec4 fColor;       // from layer vertex
in vec2 vTexCoord;    // from layer vertex

out vec4 outputColor;

void main()
{   
    vec4 baseColor = fColor;
    
    if (useHatchPattern == 1) {
        vec2 tiledCoord = vTexCoord * hatchScale;
        vec4 hatch = texture(texture0, tiledCoord);
        // overlay blend
        outputColor = mix(baseColor, hatch, hatch.a);
    } else {
        outputColor = baseColor;
    }
}