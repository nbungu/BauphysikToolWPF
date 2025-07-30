#version 430 core

in vec4 fColor;       // from layer vertex
in vec2 vTexCoord;    // from layer vertex
in vec2 vDashParams;
in vec2 vFragPos;
in float vLineDistance;

uniform sampler2D texture0;
uniform sampler2D sdfFont;
uniform int isSdfText;
uniform int useHatchPattern;
uniform float hatchScale;


out vec4 outputColor;

void main()
{      
    vec4 baseColor = fColor;

    // Check if we're rendering a dashed line
    if (vDashParams.x > 0.0)
    {
        float dashLength = vDashParams.x;
        float gapLength = vDashParams.y;
        float total = dashLength + gapLength;
        float dashPos = mod(vLineDistance, total);
        if (dashPos > dashLength)
        {
            discard; // inside the gap
        }
    }
    
    if (useHatchPattern == 1) {
        vec2 tiledCoord = vTexCoord * hatchScale;
        vec4 hatch = texture(texture0, tiledCoord);
        // overlay blend
        outputColor = mix(baseColor, hatch, hatch.a);
    } else if (isSdfText == 1) {
        float sdfDist = texture(sdfFont, vTexCoord).a; // sample alpha channel (.a)
        float smoothing = fwidth(sdfDist) * 0.5; // dynamic smoothing based on screen-space derivatives
        float alpha = smoothstep(0.28, 0.72, sdfDist); // Tweak the threshold: Closer to 0.5 = sharper edges
        //float alpha = smoothstep(0.33, 0.67, sdfDist); // Tweak the threshold: Closer to 0.5 = sharper edges
        //float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, sdfDist);
        // Apply gamma correction for final output
        alpha = pow(alpha, 1.0 / 1.5); // linear to sRGB gamma
        outputColor = vec4(baseColor.rgb, baseColor.a * alpha);
    } else {
        outputColor = baseColor;
    }
}