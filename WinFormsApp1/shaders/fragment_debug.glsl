#version 330

layout (location = 0) out vec4 Color;
in vec2 vUV;
in vec3 vSpriteColor;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;
uniform sampler2D texture_normals1;
uniform sampler2D texture_height1;

float near = 1.0;
float far = 100.0;

float LinearizeDepth(float depth)
{
	float z = depth * 2.0 - 1.0;
	return (2.0 * near * far) / (far + near - z * (far - near));
}

void main() 
{
	Color = vec4(1.0f, 1.0f, 0.0f, 1.0f);
}