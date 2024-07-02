#version 330
uniform mat4 ModelMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;

// Should the view transform be applied?
// the view transform should be applied to
// almost every vertex in the scene EXCEPT
// for the AABB that bounds the camera.
// so when we're rendering hitboxes,
// do not apply the view transform to that one.
uniform bool ApplyViewTransform;
// is the current vertex part of an AABB?
uniform bool DrawingAABB;

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
