shader_type spatial;

render_mode unshaded;

uniform sampler2D grid_tex;
uniform float scale_mult;
uniform vec4 colour : source_color;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec4 res = texture(grid_tex, UV * scale_mult);
	
	ALBEDO = res.rgb * colour.rgb;
	ALPHA = res.a * colour.a;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
