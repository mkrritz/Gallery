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

void main() {
    float up_down = 5.0 * sin(Time);
    
    float wiggle = 2.0;
    float fat = 5.0 * cos(Time*wiggle) * sin(Time);
    
    float domain = map(UV.x, 0.0, 1.0, -50.0 + fat, 50.0);
    float range  = map(UV.y, 0.0, 1.0, -10.0 + up_down, 10.0 + up_down);
    float val = (sin(Time))*sqrt(abs(domain));
    
    float diff = abs(range - val);
    
    float thickness = 0.2 + clamp(range, 0.0, 6.0);
    float intensity = 1.0 - smoothstep(0.30, thickness , diff);
    
    FragColor = vec4(vec3(intensity), 1.0);
}