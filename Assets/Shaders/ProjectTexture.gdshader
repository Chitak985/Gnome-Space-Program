shader_type spatial;
render_mode unshaded;

uniform sampler2D tex;

void fragment() {
	// Called for every pixel the material is visible on.
	vec4 res = texture(tex, SCREEN_UV);
	
	ALBEDO = res.rgb;
	ALPHA = res.a;
}