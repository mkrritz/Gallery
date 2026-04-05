#version 430 core

in vec2 UV;

out vec4 FragColor;

layout(binding = 1) uniform sampler2D Sampler;
#include "UniformBufferObjects/Scene.glsl"
layout(std140, binding = 3) uniform SceneUniformBufferObject {
    SceneUBO Scene;
};
#define Time Scene.Time.x

void main() {
    vec4 Color = texture(Sampler, UV);
    //Color = texture(Sampler, UV * abs(sin(Time)) + 0.3);
    //Color.x += fract(Time);
    //Color.x = 1 - Color.x;
    FragColor = Color;
    //FragColor.a = 0.5;
    //FragColor = vec4(UV, 0, 1);
}