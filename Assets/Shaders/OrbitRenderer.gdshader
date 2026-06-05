// Terrible shader for rendering orbits by Sushut
// REPLACE THIS WITH SOMETHING BETTER BECAUSE IT RUNS LIKE ASS
// Potential solution? idk lol https://github.com/Ryan-Mirch/Line-and-Sphere-Drawing/blob/main/Draw3D.gd

shader_type spatial;
render_mode unshaded;

uniform float nodeSize;
uniform vec3 nodePosition;

uniform bool DEBUG_renderAlpha = true;
uniform sampler2D gradient;
uniform float thickness;

// Max number of points (1024)
uniform int arrayLength = 1024;
uniform vec3 points[1024];

vec2 worldCoordsToUV(vec3 worldCoords, mat4 projectionMatrix, mat4 viewMatrix) {
	vec4 clip_space_pos = projectionMatrix * viewMatrix * vec4(worldCoords, 1.0);
    vec3 ndc = clip_space_pos.xyz / clip_space_pos.w;
    vec2 screen_uv = ndc.xy * 0.5 + 0.5;

	return screen_uv;
}

// Robbed straight from https://iquilezles.org/articles/distfunctions2d/
float dist(vec2 point0, vec2 point1, vec2 uv)
{
    vec2 pa = uv-point0, ba = point1-point0;
    float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    return length( pa - ba*h );
}

float drawLine(vec2 point0, vec2 point1, vec2 uv) {
	float dist = dist(point0, point1, uv);
    float res = smoothstep(thickness, thickness * 0.9, dist);

	return res;
}

// x is actual value and y is position in gradient
vec2 renderOrbit(vec2 uv, mat4 projectionMatrix, mat4 viewMatrix) {
	float res = 0.0;
	float clr = 0.0;

	for (int i = 0; i < arrayLength - 1; i++) {
		vec3 pos0 = (points[i] * (nodeSize) + nodePosition);
		vec3 pos1 = (points[i+1] * (nodeSize) + nodePosition);

		vec2 point0 = worldCoordsToUV(pos0, projectionMatrix, viewMatrix);
		vec2 point1 = worldCoordsToUV(pos1, projectionMatrix, viewMatrix);

		float pointZ0 = (viewMatrix * vec4(pos0, 1.0)).z;
		float pointZ1 = (viewMatrix * vec4(pos1, 1.0)).z;

		// 0,0,0 will be excluded which is a bit shiit but realistically it shouldn't ever show up
		if (pos1 != vec3(0,0,0) && pointZ0 < 0.0 && pointZ1 < 0.0) {
			float line = drawLine(point0, point1, uv);
			res += line;
			clr += line * (float(i) / float(arrayLength));
		}
	}

	return vec2(res, clr);
}

void fragment() {
	// Called for every pixel the material is visible on.

	vec2 res = normalize(renderOrbit(SCREEN_UV, PROJECTION_MATRIX, VIEW_MATRIX));

	ALBEDO = texture(gradient, vec2(res.y)).rgb;
	if (DEBUG_renderAlpha)
	{
		ALPHA = clamp(res.x, 0.0, 1.0);
	}

	// Uncomment to test lines
	//ALPHA = clamp(drawLine(vec2(0.25,0.25), vec2(0.75,0.75), SCREEN_UV) + drawLine(vec2(0.25,0.75), vec2(0.75,0.25), SCREEN_UV), 0.0, 1.0);
}