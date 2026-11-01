#version 430 core

#define TWOPI 6.2831850
#define PI    3.1415925
#define HPI   1.5707963

in vec2 UV;

out vec4 FragColor;

#include "UniformBufferObjects/Scene.glsl"
layout(std140, binding = 3) uniform SceneUniformBufferObject {
    SceneUBO Scene;
};
#define Time Scene.Time.x

const float ASPECT = 4.0 / 3.0;

vec2 rotate(vec2 p, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return mat2(c, -s, s, c) * p;
}

vec2 applyCamera(vec2 coord, float pitch, float zoomFOV) {
    coord = coord * 2.0 - 1.0;
    coord.x *= ASPECT;
    coord.y *= pitch; // Range?
    
    float depth = -1.0*cos(Time) +1.0*sin(Time)*coord.y;
    //float depth = -1.0 + 1.0*coord.y;
    float scale = 1.0 / (1.0 + depth * depth);
    coord.x *= scale;
    
    return coord * zoomFOV;
}

float gridLine(vec2 worldPos, float spacing, float thickness) {
    vec2 grid = abs(mod(worldPos, spacing) - spacing * 0.5);
    float dist = min(grid.x, grid.y);
    return 1.0 - smoothstep(0.0, thickness, dist);
}

void main() {
    float pitch = 1.6 + 1.4 * sin(Time * 1.0);
    
    float zoomFOV = 70.0 + 60.0 * sin(Time * 0.5);
    
    vec2 worldPos = applyCamera(UV, HPI, zoomFOV);
    
    float minorSpacing = 1.0;
    float majorSpacing = 10.0;
    
    float minorThickness = 0.05 * (1.0 + zoomFOV * 0.1);
    float majorThickness = 1.0 * (1.0 + zoomFOV * 0.01);
    
    float minorGrid = gridLine(worldPos, minorSpacing, minorThickness);
    float majorGrid = gridLine(worldPos, majorSpacing, majorThickness);
    
    float originDist = length(worldPos);
    float originIntensity = 1.0 - smoothstep(0.0, 0.5, originDist);
    
    vec3 minorColor = vec3(0.3, 0.4, 0.5);
    vec3 majorColor = vec3(0.6, 0.2, 0.2);
    vec3 originColor = vec3(1.0, 0.0, 0.0);
    
    vec3 color = vec3(0.0);
    
    color += minorColor * minorGrid * 0.4;
    color += majorColor * majorGrid * 1.0;
    color += originColor * originIntensity;
    
    float distFade = smoothstep(0.0, 500.0, length(worldPos));
    float visibility = 1.0 - distFade;
    color *= visibility;
    
    FragColor = vec4(color, 1.0);
}
