#version 330
uniform mat4 ModelMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
uniform bool ApplyViewTransform;

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aUV;

out vec3 vNormal;
out vec2 vUV;

void main() 
{
	if (ApplyViewTransform)
	{
		gl_Position = ProjectionMatrix * ViewMatrix * ModelMatrix * vec4(aPosition, 1.0);
	}
	else
	{
		gl_Position = ProjectionMatrix * ModelMatrix * vec4(aPosition, 1.0);
	}

	vNormal = aNormal;
	vUV = aUV;
}
