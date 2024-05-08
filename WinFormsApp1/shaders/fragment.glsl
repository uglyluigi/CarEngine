#version 150 compatibility
in vec3 vNorm;

void main() {
	// Color faces based on their directions
	// Normalized so it's between [0,1];
	// multiplied by 0.5 to guarantee no overflow when adding color floor;
	// add color floor so no fragment colors are [0, 0, 0, 1]
	gl_FragColor = vec4(normalize(vNorm) * 0.5 + 0.5, 1.0);
}