shader_type spatial;
render_mode unshaded;
uniform vec4 colour : source_color;

void fragment() {
	ALBEDO = colour.rgb;
	ALPHA = colour.a;
}