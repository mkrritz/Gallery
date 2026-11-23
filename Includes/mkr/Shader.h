#ifndef SHADER_HPP
#define SHADER_HPP

#include <Focus.h>
#include <stdint.h>

enum struct Shaders {
    DrawTexture,
    Tangent,
    Step,
    Bird,
    Stage,
    Sine,
    Sign,
    Pow,
    Mod,
    Mix,
    Transition,
    Max,
    Dome,
    InverseSqrt,
    Fract,
    Floor,
    Exp,
    Cosine,
    Clamp,
    Ceil,
    ArcTangent,
    ArcSine,
    ArcCosine,
    Absolute,
    Count // Not really a shader.
};

// The gallery renders every program except this one as a tile.
inline constexpr uint32_t ScreenShaderSlot = static_cast<uint32_t>(Shaders::DrawTexture);

extern const ShaderConfig FocusShaderPrograms[];
extern const size_t FocusShaderProgramCount;

#endif
