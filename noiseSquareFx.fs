
/*{
	"CREDIT": "by pach",
	"CATEGORIES": [
		"Stylize"
	],
	"INPUTS": [
        {
            "NAME": "inputImage",
            "TYPE": "image"
        },
		{
			"NAME": "cell_x",
			"TYPE": "float",
			"MIN": 1,
			"MAX": 100,
			"DEFAULT": 40
		},
        {
            "NAME": "cell_y",
            "TYPE": "float",
            "MIN": 1,
            "MAX": 100,
            "DEFAULT": 20
        },
        {
            "NAME": "speed",
            "TYPE": "float",
            "MIN": 0,
            "MAX": 1,
            "DEFAULT": 0.5
        },
        {
            "NAME": "useFractalNoise",
            "TYPE": "bool",
            "MIN": 0,
            "MAX": 1,
            "DEFAULT": 0.5
        },
        {
            "NAME": "brightColor",
            "TYPE": "color",
            "DEFAULT": [
                1.0,
                0.9,
                0.8,
                1.0
            ]
        },
        {
            "NAME": "darkColor",
            "TYPE": "color",
            "DEFAULT": [
                0.3,
                0.0,
                0.0,
                1.0
            ]
        },
        {
            "NAME": "backgroundColor",
            "TYPE": "color",
            "DEFAULT": [
                0.2,
                0.1,
                0.1,
                1.0
            ]
        },
        {
            "NAME": "seuil",
            "TYPE": "float",
            "MIN": 0,
            "MAX": 1,
            "DEFAULT": 0.5
        }
	]
}*/

const vec4		lumcoeff = vec4(0.299, 0.587, 0.114, 0.0);


float distance (vec2 center, vec2 pt)
{
	float tmp = pow(center.x-pt.x,2.0)+pow(center.y-pt.y,2.0);
	return pow(tmp,0.5);
}
/* discontinuous pseudorandom uniformly distributed in [-0.5, +0.5]^3 */
vec3 random3(vec3 c) {
	float j = 4096.0*sin(dot(c,vec3(17.0, 59.4, 15.0)));
	vec3 r;
	r.z = fract(512.0*j);
	j *= .125;
	r.x = fract(512.0*j);
	j *= .125;
	r.y = fract(512.0*j);
	return r-0.5;
}

/* skew constants for 3d simplex functions */
const float F3 =  0.3333333;
const float G3 =  0.1666667;

/* 3d simplex noise */
float simplex3d(vec3 p) {
    /* 1. find current tetrahedron T and it's four vertices */
    /* s, s+i1, s+i2, s+1.0 - absolute skewed (integer) coordinates of T vertices */
    /* x, x1, x2, x3 - unskewed coordinates of p relative to each of T vertices*/
    
    /* calculate s and x */
    vec3 s = floor(p + dot(p, vec3(F3)));
    vec3 x = p - s + dot(s, vec3(G3));
    
    /* calculate i1 and i2 */
    vec3 e = step(vec3(0.0), x - x.yzx);
    vec3 i1 = e*(1.0 - e.zxy);
    vec3 i2 = 1.0 - e.zxy*(1.0 - e);
    
    /* x1, x2, x3 */
    vec3 x1 = x - i1 + G3;
    vec3 x2 = x - i2 + 2.0*G3;
    vec3 x3 = x - 1.0 + 3.0*G3;
    
    /* 2. find four surflets and store them in d */
    vec4 w, d;
    
    /* calculate surflet weights */
    w.x = dot(x, x);
    w.y = dot(x1, x1);
    w.z = dot(x2, x2);
    w.w = dot(x3, x3);
    
    /* w fades from 0.6 at the center of the surflet to 0.0 at the margin */
    w = max(0.6 - w, 0.0);
    
    /* calculate surflet components */
    d.x = dot(random3(s), x);
    d.y = dot(random3(s + i1), x1);
    d.z = dot(random3(s + i2), x2);
    d.w = dot(random3(s + 1.0), x3);
    
    /* multiply d by w^4 */
    w *= w;
    w *= w;
    d *= w;
    
    /* 3. return the sum of the four surflets */
    return dot(d, vec4(52.0));
}

float simplex3d_fractal(vec3 m) {
    return   0.5333333*simplex3d(m)
    +0.2666667*simplex3d(2.0*m)
    +0.1333333*simplex3d(4.0*m)
    +0.0666667*simplex3d(8.0*m);
}

void main()
{
// CALCULATE EDGES OF CURRENT CELL
    
    // Position of current pixel
    vec2 xy; 
    xy.x = vv_FragNormCoord[0];
    xy.y = vv_FragNormCoord[1];
    
    float CellWidth = 1./cell_x ;
    float CellHeight = 1./cell_y;
    
    // Left and right of tile
    float x1 = floor(xy.x / CellWidth)*CellWidth;
    float x2 = clamp((ceil(xy.x / CellWidth)*CellWidth), 0.0, 1.0);
    // Top and bottom of tile
    float y1 = floor(xy.y / CellHeight)*CellHeight;
    float y2 = clamp((ceil(xy.y / CellHeight)*CellHeight), 0.0, 1.0);

    vec3 p3 = vec3(x1+(CellWidth/2.0), y2+(CellHeight/2.0), TIME*speed);
    float noiseValue;

    if (!useFractalNoise) {
		noiseValue = simplex3d(p3*32.0);
	} else {
		noiseValue = simplex3d_fractal(p3*8.0+8.0);
	}
	
	noiseValue = 0.5 + 0.5*noiseValue;
	noiseValue *= smoothstep(0.0, 0.005, abs(xy.x));
//    vec4 noiseColor = mix(darkColor, brightColor, noiseValue);
    
    vec4 imgColor = IMG_THIS_PIXEL(inputImage);
    float luminance = dot(imgColor,lumcoeff);

    vec4 outColor;
    if (luminance<seuil) {
        outColor = backgroundColor;
    }else{
        outColor = mix(darkColor, brightColor, noiseValue);
    }
    
    gl_FragColor = outColor;
}
