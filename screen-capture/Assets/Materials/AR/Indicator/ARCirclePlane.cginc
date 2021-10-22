#ifndef AR_CIRCLE_PLANE_FUNC
#define AR_CIRCLE_PLANE_FUNC

float _Shape;
float _ShapeWave;

float ARCirclePlaneShapeRadius(float2 uv) {
    float wave = sin(uv.y * _ShapeWave) * 0.5 + 0.5;
    return 0.5 + lerp(-_Shape, _Shape, wave);
}

#endif
