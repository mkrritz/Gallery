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

const float ASPECT = 4.0 / 3.0; // Do this properly
const float FOV = 90.0;
const vec3 minorColor = vec3(0.3, 0.4, 0.5);
const vec3 majorColor = vec3(0.6, 0.2, 0.2);
const vec3 originColor = vec3(1.0, 0.0, 0.0);

vec2 rotate(vec2 p, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return mat2(c, -s, s, c) * p;
}

float gridLine(vec2 worldPos, float spacing, float thickness) {
    vec2 grid = abs(mod(worldPos, spacing) - spacing * 0.5);
    float dist = min(grid.x, grid.y);
    return 1.0 - smoothstep(0.0, thickness, dist);
}


vec3 getRayDirection(vec2 uv, float aspect, float fov, vec3 camPos, vec3 camTarget) {
    vec3 forward = normalize(camTarget - camPos);
    vec3 right = normalize(cross(forward, vec3(0.0, 1.0, 0.0)));
    vec3 up = cross(right, forward);

    vec2 ndc = uv * 2.0 - 1.0;
    ndc.x *= aspect;

    float scale = tan(fov * 0.5);
    vec3 rayDir = normalize(forward + ndc.x * scale * right + ndc.y * scale * up);
    return rayDir;
}

vec2 getPlaneIntersection(vec3 rayOrigin, vec3 rayDir) {
    float t = -rayOrigin.z / rayDir.z;
    vec3 hit = rayOrigin + rayDir * t;
    return hit.xy;
}

void main() {
    // FROM BUFFER?
    vec3 CameraPosition = vec3(0.0 + 25.0 * sin(Time*0.8),
                               0.0 + 15.0 * sin(Time*1.2),
                               15.0 + 5.0 * cos(Time*1.0));
    //vec3 CameraTarget = vec3(0.0, 0.0, 0.0);
    //vec3 CameraTarget = CameraPosition + vec3(0.0,0.0, 0.0001);
    vec3 CameraTarget = CameraPosition;
    CameraTarget.z = -1.0;
    
    vec3 rayDir = 0.1 * cos(Time * 2.0) + getRayDirection(UV, ASPECT, FOV, CameraPosition, CameraTarget);
    vec2 worldPos = getPlaneIntersection(CameraPosition, rayDir);

    float minorGrid = gridLine(worldPos, 1.0, 0.05);
    float majorGrid = gridLine(worldPos, 10.0, 0.5);
    float originIntensity = 1.0 - smoothstep(0.5, 1.0, length(worldPos));

    vec3 color = vec3(0.0);
    color += minorColor * minorGrid * 0.6;
    color += majorColor * majorGrid * 1.0;
    color += originColor * originIntensity;

    float fade = smoothstep(0.0, 250.0 , length(worldPos - CameraPosition.xy));
    float visibility = 1.0 - fade;
    // visibility = 1.0 - fade + (1.0 - UV.y);
    color *= visibility;

    FragColor = vec4(color, 1.0);
}
