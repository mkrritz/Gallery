#version 430 core

#define TWOPI 6.2831850
#define PI    3.1415925
#define HPI   1.5707963

in vec2 UV;

#include "UniformBufferObjects/Scene.glsl"
layout(std140, binding = 3) uniform SceneUniformBufferObject {
    SceneUBO Scene;
};
#define Time Scene.Time.x

out vec4 FragColor;

float map(float value, float a, float b, float c, float d) {
    return c + (d - c) * ((value - a) / (b - a));
}

// Tail

void main() {
    float t1 = cos(Time);
    float t2 = sin(Time*2.0);
    float s1 = sin(Time);
    float domain = map(UV.x, 0.0, 1.0, -HPI, HPI);
    float range = map(UV.y, 0.0, 1.0, -s1 + t1, s1 + t1 + 3.0*t2);
    
    float val = tan(domain * 3.0);
    
    float diff = abs(range - val);
    float thickness = 1.0;
    float intensity = 1.0 - smoothstep(0.4, thickness , diff);
    
    FragColor = vec4(vec3(intensity), 1.0);
}