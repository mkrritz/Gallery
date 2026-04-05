#version 430 core

#define TWOPI 6.2831850
#define PI    3.1415925
#define HPI   1.5707963

// Input UV coordinate (0 to 1 range)
in vec2 UV;

// Final output color
out vec4 FragColor;

// Uniform time input for animation
#include "UniformBufferObjects/Scene.glsl"
layout(std140, binding = 3) uniform SceneUniformBufferObject {
    SceneUBO Scene;
};
#define Time Scene.Time.x

const float ASPECT = 4.0 / 3.0; // Do this properly
const float FOV = 90.0; // What?
const vec3 minorColor = vec3(0.3, 0.4, 0.5); // Soft blue-gray
const vec3 majorColor = vec3(0.6, 0.2, 0.2); // Strong reddish grid
const vec3 originColor = vec3(1.0, 0.0, 0.0); // Bright red origin marker
const vec3 targetColor = vec3(1.0, 1.0, 0.0); // Bright yellow target marker // DO with FORWARD

// Returns a line value (0 to 1) indicating how close a world position is to a grid line
float gridLine(vec2 worldPos, float spacing, float thickness) {
    // Calculate position relative to grid center
    vec2 grid = abs(mod(worldPos, spacing) - spacing * 0.5);

    // Find distance to nearest grid axis
    float dist = min(grid.x, grid.y);

    // Smooth line appearance based on thickness
    return 1.0 - smoothstep(0.0, thickness, dist);
}

vec3 getRayDirection(vec2 uv, float aspect, float fov, vec3 camPos, vec3 camTarget) {
    // Camera basis
    vec3 forward = normalize(camTarget - camPos);
    vec3 right = normalize(cross(forward, vec3(0.0, 1.0, 0.0)));
    vec3 up = cross(right, forward);
    
    // Convert screen UV to NDC space [-1, 1]
    vec2 ndc = uv * 2.0 - 1.0;
    ndc.x *= aspect;
    
    // Project to camera space
    float scale = tan(fov * 0.5);
    vec3 rayDir = normalize(forward + ndc.x * scale * right + ndc.y * scale * up);
    return rayDir;
}

vec2 getPlaneIntersection(vec3 rayOrigin, vec3 rayDir) {
    float t = 30.0 + 20.0 * cos(Time);//-rayOrigin.z / rayDir.z;
    vec3 hit = rayOrigin + rayDir * t;
    return hit.xy;
}

void main() {
    // FROM BUFFER?
    vec3 CameraPosition = vec3(5.0 + 2.0 * sin(Time*4.0),
                               -5.0 + 2.0 * cos(Time*1.0),
                               5.0 + 4.0 * cos(Time*1.0));
    vec3 CameraTarget = CameraPosition + vec3(sin(Time*0.7), 2.0 + 1.9 * cos(Time * 0.5), -4.0 * cos(Time * 0.5));
    //vec3 CameraTarget = CameraPosition + normalize(vec3(0.0, 20.0 + 19.9 * cos(Time * 0.5), -100.0));
    //vec3 CameraTarget = CameraPosition + vec3(cos(Time), sin(Time), -1.0);
    //vec3 CameraTarget = CameraPosition;
    //CameraTarget.x += 0.05 * cos(Time * 1.4);
    //CameraTarget.y += 20.0 + 30.0 * cos(Time * 1.4);
    //CameraTarget.z -= 10.0;
    
    vec3 rayDir = getRayDirection(UV, ASPECT, FOV, CameraPosition, CameraTarget);
    vec2 worldPos = getPlaneIntersection(CameraPosition, rayDir);

    float minorGrid = gridLine(worldPos, 1.0, 0.05);
    float majorGrid = gridLine(worldPos, 10.0, 0.5);
    float originIntensity = 1.0 - smoothstep(0.5, 1.0, length(worldPos));
    float targetIntensity = 1.0 - smoothstep(0.5, 1.0, length(worldPos - CameraTarget.xy));
    
    vec3 color = vec3(0.0);
    color += minorColor * minorGrid * 0.6;
    color += majorColor * majorGrid * 1.0;
    color += originColor * originIntensity;
    color += targetColor * targetIntensity;

    float fade = smoothstep(0.0, 250.0 , length(worldPos - CameraPosition.xy));
    float visibility = 1.0 - fade;
    color *= visibility;

    FragColor = vec4(color, 1.0);
}

// TODO 2:
// Get Camera position and Camera forward and other camera stuff from a buffer
// so that we can enter the fragment world.

// TODO 2:
// Do quaternions?