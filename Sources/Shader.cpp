#include "pch.h"

#include "mkr/Shader.h"

extern const ShaderConfig FocusShaderPrograms[] = {
    { "Quad.vs", nullptr, "DrawTexture.fs", nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Tangent.fs",     nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Step.fs",        nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Sqrt.fs",        nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Smoothstep.fs",  nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Sine.fs",        nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Sign.fs",        nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Pow.fs",         nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Mod.fs",         nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Mix.fs",         nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Min.fs",         nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Max.fs",         nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Log.fs",         nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "InverseSqrt.fs", nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Fract.fs",       nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Floor.fs",       nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Exp.fs",         nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Cosine.fs",      nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Clamp.fs",       nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Ceil.fs",        nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "ArcTangent.fs",  nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "ArcSine.fs",     nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "ArcCosine.fs",   nullptr, nullptr, nullptr },
    { "Quad.vs", nullptr, "Absolute.fs",    nullptr, nullptr, nullptr },
};

extern const size_t FocusShaderProgramCount =
    sizeof(FocusShaderPrograms) / sizeof(FocusShaderPrograms[0]);

static_assert(sizeof(FocusShaderPrograms) / sizeof(FocusShaderPrograms[0]) ==
              (size_t)Shaders::Count,
              "The enum and the Focus table must list the same programs.");
