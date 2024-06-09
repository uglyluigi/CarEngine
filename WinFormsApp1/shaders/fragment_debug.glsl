#version 330

layout(location = 0) out vec4 Color;
in vec3 vNormal;
in vec2 vUV;

uniform sampler2D Tex1;
uniform sampler2D Tex2;
uniform sampler2D Tex3;



void main() {
	Color = texture(Tex1, vUV);
}