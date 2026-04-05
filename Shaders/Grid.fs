#version 430 core

#if 1
in vec2 UV;
out vec4 FragColor;

#include "UniformBufferObjects/Scene.glsl"
layout(std140, binding = 3) uniform SceneUniformBufferObject {
    SceneUBO Scene;
};
#define Time Scene.Time.x

const float ASPECT = 4.0 / 3.0;

// --- Helper Functions ---
vec2 rotate(vec2 p, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return mat2(c, -s, s, c) * p;
}

vec2 applyCamera(vec2 coord, float pitch, float zoomFOV) {
    coord = coord * 2.0 - 1.0;
    coord.x *= ASPECT;
    coord.y *= pitch;

    float depth = 1.0 - coord.y;
    float scale = 1.0 / (1.0 + depth * 2.0);
    coord.x *= scale;

    return coord * zoomFOV;
}

float gridLine(vec2 worldPos, float spacing, float thickness) {
    vec2 grid = abs(mod(worldPos, spacing) - spacing * 0.5);
    float dist = min(grid.x, grid.y);
    return 1.0 - smoothstep(0.0, thickness, dist);
}
void main() {
    float pitch = 1.6 + 0.4 * sin(Time * 0.91);
    float zoomFOV = 70.0 + 60.0 * sin(Time * 0.5);
    float angle = Time * 0.15;

    vec2 worldPos = applyCamera(UV, pitch, zoomFOV);
    worldPos = rotate(worldPos, angle);

    // Spacing & thickness
    float minorSpacing = 1.0;
    float majorSpacing = 10.0;
    float minorThickness = 0.05 * (1.0 + zoomFOV * 0.1);
    float majorThickness = 1.0 * (1.0 + zoomFOV * 0.01);

    // Grid intensities
    float minorGrid = gridLine(worldPos, minorSpacing, minorThickness);
    float majorGrid = gridLine(worldPos, majorSpacing, majorThickness);

    // Origin marker
    float originDist = length(worldPos);
    float originIntensity = 1.0 - smoothstep(0.0, 0.5, originDist);

    // Colors
    vec3 minorColor = vec3(0.3, 0.4, 0.5);
    vec3 majorColor = vec3(0.6, 0.2, 0.2);
    vec3 originColor = vec3(1.0, 0.0, 0.0);

    vec3 color = vec3(0.0);
    color += minorColor * minorGrid * 0.4;
    color += majorColor * majorGrid * 1.0;
    color += originColor * originIntensity;

    // Fade out in distance
    float distFade = smoothstep(0.0, 500.0, length(worldPos));
    float visibility = 1.0 - distFade;
    color *= visibility;

    FragColor = vec4(color, 1.0);
}

#else

in vec2 UV;
out vec4 FragColor;

#include "UniformBufferObjects/Scene.glsl"
layout(std140, binding = 3) uniform SceneUniformBufferObject {
    SceneUBO Scene;
};
#define Time Scene.Time.x

layout(std430, binding = 3) readonly buffer CameraVP {
    mat4 VP;
};
layout(std430, binding = 4) readonly buffer CameraVPInverse {
    mat4 InvVP;
};

const float ASPECT = 4.0 / 3.0;

vec2 worldFromClip(vec2 uvClip) {
    // Convert clip coordinates [-1,1] to world space assuming Z = 0
    vec4 clip = vec4(uvClip, 0.0, 1.0);
    vec4 world = InvVP * clip;
    return world.xy / world.w;
}

float gridLine(vec2 worldPos, float spacing, float thickness) {
    vec2 grid = abs(mod(worldPos, spacing) - spacing * 0.5);
    float dist = min(grid.x, grid.y);
    return 1.0 - smoothstep(0.0, thickness, dist);
}

void main() {
    // Convert UV to NDC space [-1, 1]
    vec2 ndc = UV * 2.0 - 1.0;
    ndc.x *= ASPECT;

    // Reconstruct world position from VP matrix
    vec2 worldPos = worldFromClip(ndc);

    // Grid parameters
    float zoom = length(worldPos);
    float minorSpacing = 1.0;
    float majorSpacing = 10.0;
    float minorThickness = 0.03 * (1.0 + zoom * 0.01);
    float majorThickness = 0.2 * (1.0 + zoom * 0.01);

    // Grid lines
    float minorGrid = gridLine(worldPos, minorSpacing, minorThickness);
    float majorGrid = gridLine(worldPos, majorSpacing, majorThickness);

    // Origin highlight
    float originDist = length(worldPos);
    float originIntensity = 1.0 - smoothstep(0.0, 0.5, originDist);

    // Colors
    vec3 minorColor = vec3(0.3, 0.4, 0.5);
    vec3 majorColor = vec3(0.6, 0.2, 0.2);
    vec3 originColor = vec3(1.0, 0.0, 0.0);

    vec3 color = vec3(0.0);
    color += minorColor * minorGrid * 0.4;
    color += majorColor * majorGrid;
    color += originColor * originIntensity;

    // Distance fade
    float distFade = smoothstep(0.0, 300.0, length(worldPos));
    color *= 1.0 - distFade;

    FragColor = vec4(color, 1.0);
}

#endif