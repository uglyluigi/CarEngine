#version 330
uniform mat4 ModelMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;

layout(location = 0) in vec3 aPosition;

void main() {
	gl_Position = ProjectionMatrix * ViewMatrix * ModelMatrix * vec4(aPosition, 1.0);
}