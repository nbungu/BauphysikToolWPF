#version 430 core

in vec4 fColor;       // from vertex
in vec2 vTexCoord;    // from vertex shader
out vec4 FragColor;

uniform sampler2D uHatch;      // hatch pattern texture
uniform bool useHatchPattern;  // control whether pattern is enabled
uniform float hatchScale;      // texture tiling factor (e.g., 10.0)

void main()
{   
    vec4 baseColor = fColor;

    if (useHatchPattern)
    {
        vec2 tiledCoord = vTexCoord * hatchScale;
        vec4 hatch = texture(uHatch, tiledCoord);

        // Option 1: multiply blend (dark-on-light)
        baseColor *= hatch;

        // Option 2 (uncomment to try): overlay blend
        // baseColor = mix(baseColor, hatch, hatch.a);
    }
    
    FragColor = fColor;
}