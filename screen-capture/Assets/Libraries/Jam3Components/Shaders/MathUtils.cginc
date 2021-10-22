#ifndef MATH_UTILS_FUNC
#define MATH_UTILS_FUNC

float mapLinear(float value, float in_min, float in_max, float out_min, float out_max) {
	return (value - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

float circle(float2 uv, float2 pos, float rad) {
    float d = length(pos - uv) - rad;
    return step(d, 0.0);
}

#endif
