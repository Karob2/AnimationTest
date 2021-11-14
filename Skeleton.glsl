#version 330 core

//$$.C //engine preprocessor will replace this with `#def C_VERTEX true` or `#def C_FRAGMENT true`

#ifdef C_VERTEX
layout (location = 0) in vec3 bPos;
layout (location = 1) in int boneIndex;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4[120] uBones;

out vec4 VertColor;

void main()
{

	vec4 blendPos = vec4((uBones[boneIndex] * vec4(bPos, 1)).xyz, 1);
	gl_Position = uProjection * uView * uModel * blendPos;
	VertColor = vec4(1.0, float(boneIndex) / 115.0, 0.25, 1.0);
	if(int(boneIndex) == 0) VertColor = vec4(1.0, 1.0, 1.0, 1.0);
}

#endif
#ifdef C_FRAGMENT
in vec4 VertColor;
out vec4 FragColor;

void main()
{
	FragColor = VertColor; 
}

#endif