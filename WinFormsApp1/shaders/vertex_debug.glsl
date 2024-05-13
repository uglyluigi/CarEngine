#version 330
uniform mat4 uMVP;
uniform mat4 WorldScaleMat;

layout(location = 0) in vec3 aPosition;



void main() {
	gl_Position = uMVP * vec4(aPosition, 1.0) * WorldScaleMat;
}