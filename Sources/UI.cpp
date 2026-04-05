
#include "pch.h"

#include "mkr/Assert.h"
#include "mkr/Memory.h"
#include "mkr/UI.h"
#include "mkr/Shader.h"
#include "mkr/Camera.h"
#include "mkr/Window.h"

void RenderQuadToScreen(const Shader* const shaders[],
                        const UI* const ui,
                        const GLuint sampler,
                        const GLuint texture) {
    const Shader* shader = shaders[static_cast<uint32_t>(Shaders::DrawTexture)];
    
    glBindSampler(1, sampler);
    
    glUseProgram(shader->ID);
    glBindBufferBase(GL_UNIFORM_BUFFER, 2, ui->ubo_handle[static_cast<uint32_t>(Shaders::DrawTexture)]);
    glBindTextureUnit(1, texture);
    glDrawArrays(GL_TRIANGLE_STRIP, 0, 4);
    glBindTextureUnit(1, 0);
    glBindBufferBase(GL_UNIFORM_BUFFER, 2, 0);
}

void UIInit(Heap* const heap) {
    UI* ui = (UI*)HeapAllocate(heap, sizeof(UI));
    glCreateVertexArrays(1, &ui->vao);
    
    {
        Quad& quad = ui->quads[static_cast<uint32_t>(Quads::SCREEN)];
        
        glCreateTextures(GL_TEXTURE_2D, 1, &quad.texture);
        glTextureStorage2D(quad.texture, 1, GL_RGBA8, WINDOW_WIDTH, WINDOW_HEIGHT);
        glCreateTextures(GL_TEXTURE_2D, 1, &quad.depthbuffer);
        glTextureStorage2D(quad.depthbuffer, 1, GL_DEPTH_COMPONENT24, WINDOW_WIDTH, WINDOW_HEIGHT);
        glCreateFramebuffers(1, &quad.framebuffer);
        glNamedFramebufferTexture(quad.framebuffer, GL_COLOR_ATTACHMENT0, quad.texture, 0);
        glNamedFramebufferTexture(quad.framebuffer, GL_DEPTH_ATTACHMENT, quad.depthbuffer, 0);
        
        if(glCheckNamedFramebufferStatus(quad.framebuffer, GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE) {
            ASSERT(true, "Framebuffer Error");
        }
    }
    
    UniformBufferObjectUI buffer[static_cast<uint32_t>(Shaders::Count)];
    buffer[static_cast<uint32_t>(Shaders::DrawTexture)].rect = glm::vec4(-1.0f, -1.0f, 2.0f, 2.0f);
    glm::vec2 size = glm::vec2(0.35f, 0.35f);
    glm::vec2 padding = glm::vec2(0.015f, 0.015f);
    for (uint32_t index = 1; index < static_cast<uint32_t>(Shaders::Count); index++) {
        uint32_t col = (index - 1) % 5;
        uint32_t row = (index - 1) / 5;
        glm::vec2 position = glm::vec2(-0.9f, -0.9f) + (size + padding) * glm::vec2(col, row);
        buffer[index].rect = glm::vec4(position.x, position.y, size.x, size.y);
    }
    
    ui->ubo_handle = (GLuint*)HeapAllocate(heap, sizeof(GLuint) * static_cast<uint32_t>(Shaders::Count));
    glCreateBuffers(static_cast<uint32_t>(Shaders::Count), ui->ubo_handle);
    for (uint32_t index = 0; index < static_cast<uint32_t>(Shaders::Count); index++) {
        glNamedBufferStorage(ui->ubo_handle[index],
                             sizeof(UniformBufferObjectUI),
                             &buffer[index],
                             0);
    }
    
}

