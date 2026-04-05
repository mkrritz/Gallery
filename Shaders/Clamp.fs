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

// Aspect ratio of the screen (width / height)
const float ASPECT = 4.0 / 3.0;

// Rotate a 2D point around the origin by a given angle (in radians)
vec2 rotate(vec2 p, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return mat2(c, -s, s, c) * p;
}

// Converts screen-space UV coordinates to a pseudo-perspective world position
vec2 applyCamera(vec2 coord, float pitch, float zoomFOV) {
    // Convert UV to range [-1, 1]
    coord = coord * 2.0 - 1.0;

    // Adjust X by aspect ratio to maintain proportions
    coord.x *= ASPECT;

    // Apply vertical pitch distortion
    coord.y *= pitch; // Range?
    
    // Simulate a simple perspective depth effect
    float depth = -1.0*cos(Time) +1.0*sin(Time)*coord.y;
    //float depth = -1.0 + 1.0*coord.y;
    float scale = 1.0 / (1.0 + depth * depth); // Depth-based scale
    coord.x *= scale;

    // Apply zoom level
    return coord * zoomFOV;
}

// Returns a line value (0 to 1) indicating how close a world position is to a grid line
float gridLine(vec2 worldPos, float spacing, float thickness) {
    // Calculate position relative to grid center
    vec2 grid = abs(mod(worldPos, spacing) - spacing * 0.5);

    // Find distance to nearest grid axis
    float dist = min(grid.x, grid.y);

    // Smooth line appearance based on thickness
    return 1.0 - smoothstep(0.0, thickness, dist);
}

void main() {
    // Calculate pitch distortion based on time (animated)
    float pitch = 1.6 + 1.4 * sin(Time * 1.0);
    
    // Calculate zoom factor (animated field of view)
    float zoomFOV = 70.0 + 60.0 * sin(Time * 0.5);
    
    // Convert UV coordinate into world space, applying camera effects
    vec2 worldPos = applyCamera(UV, HPI, zoomFOV);
    
    // Calculate rotation angle based on time
    // float angle = Time * 4.15;
    // worldPos = rotate(worldPos, angle);
    
    // Grid spacing and thickness settings
    float minorSpacing = 1.0;
    float majorSpacing = 10.0;
    
    // Line thickness scales with zoom
    float minorThickness = 0.05 * (1.0 + zoomFOV * 0.1);
    float majorThickness = 1.0 * (1.0 + zoomFOV * 0.01);
    
    // Compute minor and major grid values (0 to 1 line intensities)
    float minorGrid = gridLine(worldPos, minorSpacing, minorThickness);
    float majorGrid = gridLine(worldPos, majorSpacing, majorThickness);
    
    // Compute how close we are to the origin for the origin marker
    float originDist = length(worldPos);
    float originIntensity = 1.0 - smoothstep(0.0, 0.5, originDist);
    
    // Set base colors for different grid elements
    vec3 minorColor = vec3(0.3, 0.4, 0.5); // Soft blue-gray
    vec3 majorColor = vec3(0.6, 0.2, 0.2); // Strong reddish grid
    vec3 originColor = vec3(1.0, 0.0, 0.0); // Bright red origin marker
    
    // Start with black background
    vec3 color = vec3(0.0);
    
    // Accumulate grid colors with respective intensities
    color += minorColor * minorGrid * 0.4;      // Dimmer minor grid lines
    color += majorColor * majorGrid * 1.0;      // Stronger major grid lines
    color += originColor * originIntensity;     // Origin highlight
    
    // Fade out the grid lines based on distance from center
    float distFade = smoothstep(0.0, 500.0, length(worldPos));
    float visibility = 1.0 - distFade;
    color *= visibility;
    
    // Final color output with full opacity
    FragColor = vec4(color, 1.0);
}
