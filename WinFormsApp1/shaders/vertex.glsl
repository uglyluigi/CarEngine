#version 150 compatibility
uniform mat4 uMVP;
uniform mat4 WorldScaleMat;

in vec3 aPosition;
in vec3 aNorm;
in vec2 aUV;

out vec3 vNorm;
out vec3 vUV;

void main() {
	gl_Position = uMVP * vec4(aPosition, 1.0) * WorldScaleMat;
	vNorm = aNorm;
}