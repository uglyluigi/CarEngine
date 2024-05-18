#version 330

in vec3 vColor;

void main() {
	// Color faces based on their directions
	// Normalized so it's between [0,1];
	// multiplied by 0.5 to guarantee no overflow when adding color floor;
	// add color floor so no fragment colors are [0, 0, 0, 1]
	gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}