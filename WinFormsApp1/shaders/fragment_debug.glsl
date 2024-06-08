#version 330

in vec3 vNormal;
in vec2 vUV;

uniform sampler2D Tex;

out vec4 Color;

void main() {
	// Color faces based on their directions
	// Normalized so it's between [0,1];
	// multiplied by 0.5 to guarantee no overflow when adding color floor;
	// add color floor so no fragment colors are [0, 0, 0, 1]
	Color = texture(Tex, vUV);
}