
#include "pch.h"

#include "mkr/Assert.h"
#include "mkr/Input.h"
#include "mkr/Shader.h"
#include "mkr/Globals.h"
#include "mkr/Camera.h"
#include "mkr/Memory.h"
#include "mkr/UI.h"
#include "mkr/Window.h"

static std::mt19937 gen(time(nullptr));
static std::uniform_real_distribution<float> dist(-1.0f, 1.0f);
static std::uniform_real_distribution<float> dist01(0.0f, 1.0f);

GLFWwindow* WindowInit();

//Memory
void HeapsInit(Heap*[]);
void PrintMemory(const Heap* const []);
//Memory

// Camera
void CameraInit(Heap* const);
// Camera

// Shader
void ShadersInit(Heap* const,
                 Shader*[], bool);
void ReloadShader(Heap* const,
                  uint32_t);
// Shader

// Input
Input* NextRecordedInputState();
// Input

// UI
void UIInit(Heap* const);
void RenderQuadToScreen(const Shader* const shaders[],
                        const UI* const ui,
                        const GLuint sampler,
                        const GLuint texture);
// UI

static void RenderGallery(const Shader* const shaders[],
                          const Camera* const camera,
                          const UI* const ui,
                          float TotalTime) {
    const Shader* shader;
    
    glBindSampler(0, camera->sampler_clamp_linear);
    
    for (uint32_t index = 1; index < (uint32_t)Shaders::Count; index++) {
        shader = shaders[index];
        glUseProgram(shader->ID);
        glBindBufferBase(GL_UNIFORM_BUFFER, 2, ui->ubo_handle[index]);
        glDrawArrays(GL_TRIANGLE_STRIP, 0, 4);
    }
    
    glBindSampler(0, 0);
}

// TODO:
//  Click to select one quad and start rendering only that one fullscreen.
//

int main() {
    wchar_t exe_path[MAX_PATH];
    GetModuleFileNameW(NULL, exe_path, MAX_PATH);
    std::filesystem::path exe_dir = std::filesystem::path(exe_path).parent_path();
    if (std::filesystem::exists(exe_dir / "Shaders")) {
        std::filesystem::current_path(exe_dir);
    } else if (std::filesystem::exists(exe_dir.parent_path() / "Shaders")) {
        std::filesystem::current_path(exe_dir.parent_path());
    }

    GLFWwindow* window;
    window = WindowInit();
    
    Input* input = NextRecordedInputState();
    
    Heap* heaps[(uint32_t)Heaps::Count] = {nullptr};
    HeapsInit(heaps);
    
    Shader* shaders[(uint32_t)Shaders::Count] = {nullptr};
    ShadersInit(heaps[(uint32_t)Heaps::Shader], shaders, true);
    
    CameraInit(heaps[(uint32_t)Heaps::Camera]);
    Camera* const camera = (Camera*)heaps[(uint32_t)Heaps::Camera]->memory;
    
    UIInit(heaps[(uint32_t)Heaps::UI]);
    UI* const ui = (UI*)heaps[(uint32_t)Heaps::UI]->memory;
    
    Quad& quad = ui->quads[(uint32_t)Quads::SCREEN];
    
    glBindVertexArray(ui->vao); // There is only quad rendering happening.
                                // VAO is useless anyway. Attributeless vertices.
    
    glDisable(GL_DEPTH_TEST);
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    glBlendEquation(GL_FUNC_ADD);
    glPointSize(5);
    glLineWidth(2);
    glfwSwapInterval(1);
    
    struct UniformBufferObjectScene {
        glm::vec4 Time;
    };
    GLuint scene_handle;
    glCreateBuffers(1, &scene_handle);
    glNamedBufferStorage(scene_handle,
                         sizeof(UniformBufferObjectScene),
                         nullptr,
                         GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
    UniformBufferObjectScene* scene = (UniformBufferObjectScene*)glMapNamedBufferRange(
                                      scene_handle,
                                      0,
                                      sizeof(UniformBufferObjectScene),
                                      GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
    
    glBindBufferBase(GL_UNIFORM_BUFFER, 3, scene_handle); // This is fine. Everything the same..
                                                          // Fix if things get out of hand.
                                                          // Most likely here forever.
                                                  // Change binding point to 0 instead then.
    
    const float dt = 1.0f/60.0f;
    float CurrectTime, LastTime, DeltaTime;
    float AccumulatorTime, TotalTime, EveryTwoSeconds;
    LastTime = glfwGetTime();
    CurrectTime = DeltaTime = TotalTime = EveryTwoSeconds = 0;
    AccumulatorTime = dt;
    uint32_t UpdateCount= 0;
    uint32_t FixedUpdateCount= 0;
    
    while (!(input->special & InputM::ESC)) {
        CurrectTime = glfwGetTime();
        DeltaTime = CurrectTime - LastTime;
        LastTime = CurrectTime;
        AccumulatorTime += DeltaTime;
        TotalTime += DeltaTime;
        EveryTwoSeconds += DeltaTime;
        
        scene->Time.x = TotalTime;
        
        UpdateCount += 1;
        glfwPollEvents();
        
        while (shader_slots_to_reload) {
            uint32_t slot = __builtin_ctzll(shader_slots_to_reload);
            shader_slots_to_reload &= ~(1ULL << slot);
            ReloadShader(heaps[(uint32_t)Heaps::Shader], slot);
        }
        
        input = NextRecordedInputState();
        
        while(AccumulatorTime >= dt) {
            AccumulatorTime -= dt;
            FixedUpdateCount += 1;
            
            if (input->letters & InputM::H) {
                if (input->toggles & InputM::Toggle_H) {
                    PrintMemory(heaps);
                }
                input->toggles &= ~InputM::Toggle_H;
            } else input->toggles |= InputM::Toggle_H;
        }
        
        glBindFramebuffer(GL_DRAW_FRAMEBUFFER, quad.framebuffer);
        glClearColor(1, 0, 0, 0);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        
        RenderGallery(shaders, camera, ui, TotalTime);
        
        glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
        glClearColor(0, 0, 1, 0);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        
        RenderQuadToScreen(shaders, ui, camera->sampler_clamp_linear, quad.texture);
        
        glfwSwapBuffers(window);
    }
    
    return 1;
}

