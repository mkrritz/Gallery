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
    float x_offset = 40.0 * sin(Time);
    float more_x_offset = 10.0 * cos(Time);
    float domain = map(UV.x, 0.0, 1.0, -30.0 - x_offset + more_x_offset, 30.0 + x_offset);
    float range = map(UV.y, 0.0, 1.0, -5.0 + cos(Time), 10.0 + sin(2.0*Time));
    
    float some_scale = 1.0f;
    float val = smoothstep(-some_scale, some_scale, domain);
    
    float thickness = 0.2 + clamp(range, 0.0, 5.0);
    float intensity = 1.0 - smoothstep(0.0, thickness , range - val);
    
    FragColor = vec4(vec3(intensity), 1.0);
}