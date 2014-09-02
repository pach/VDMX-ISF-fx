/*{
	"CREDIT": "by pach, based on FalseColor.fs by zoidberg",
	"CATEGORIES": [
		"Color Effect"
	],
	"INPUTS": [
		{
			"NAME": "inputImage",
			"TYPE": "image"
		},
		{
			"NAME": "brightColor",
			"TYPE": "color",
			"DEFAULT": [
				1.0,
				0.0,
				0.0,
				1.0
			]
		},
		{
			"NAME": "mediumColor",
			"TYPE": "color",
			"DEFAULT": [
				0.0,
				1.0,
				0.0,
				1.0
			]
		},
		{
			"NAME": "darkColor",
			"TYPE": "color",
			"DEFAULT": [
				0.0,
				0.0,
				1.0,
				1.0
			]
		}
	]
}*/

const vec4		lumcoeff = vec4(0.299, 0.587, 0.114, 0.0);

void main() {
	vec4		srcPixel = IMG_THIS_PIXEL(inputImage);
	float		luminance = dot(srcPixel,lumcoeff);
	vec4 color1 = mix(darkColor, mediumColor, clamp(luminance, 0., 0.5)*2.);
	vec4 color2 = mix(mediumColor, brightColor, clamp(luminance, 0.5, 1.)*2.-1.);

	gl_FragColor = mix(color1, color2, luminance);
}
