#version 420

out vec4 outputColor;

in vec2 texCoord;

// layout (binding=x) specifies which texture slot to bind the sampler type variable to.
layout (binding=0) uniform sampler2D texture0;
layout (binding=1) uniform sampler2D texture1;

void main()
{
    outputColor = mix(texture(texture0, texCoord), texture(texture1, texCoord), 0.2);
}