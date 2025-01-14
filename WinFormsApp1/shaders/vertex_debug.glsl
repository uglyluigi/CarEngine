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

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aUV;

out vec2 vUV;
out vec3 vSpriteColor;

void main() 
{
	gl_Position = ProjectionMatrix * ViewMatrix * ModelMatrix * vec4(aPosition, 0.0, 1.0);
	vUV = aUV;
}
