#version 330

layout(location = 0) out vec4 Color;
in vec3 vNormal;
in vec2 vUV;

uniform sampler2D Texture0;

void main() {
	Color = texture(Texture0, vUV);
}