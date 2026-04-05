#version 430 core

#define FOV    0
#define ASPECT 1
#define NEAR   2
#define FAR    3

// TODO:
// 3D Gallery 

layout(local_size_x = 1) in;

layout(std430, binding = 3) writeonly buffer CameraVP {
    mat4 VP;
};

layout(std430, binding = 4) writeonly buffer CameraVPInverse {
    mat4 InvVP;
};

uniform vec4 CameraAttributes;
uniform vec4 CameraPosition;
uniform vec4 CameraForward;

mat4 LookAt(vec3 Eye, vec3 F, out mat4 invView) {
    vec3 R = normalize(cross(F, vec3(0, 1, 0)));
    vec3 U = normalize(cross(R, F));

    mat4 View = mat4(
         R.x,  U.x, -F.x, 0.0,
         R.y,  U.y, -F.y, 0.0,
         R.z,  U.z, -F.z, 0.0,
        -dot(R, Eye), -dot(U, Eye), dot(F, Eye), 1.0
    );

    mat4 Rot = mat4(
        R.x, R.y, R.z, 0.0,
        U.x, U.y, U.z, 0.0,
       -F.x, -F.y, -F.z, 0.0,
        0.0, 0.0,  0.0, 1.0
    );

    mat4 Trans = mat4(
        1.0, 0.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 1.0, 0.0,
        Eye.x, Eye.y, Eye.z, 1.0
    );

    invView = Trans * Rot;
    return View;
}

mat4 Perspective(out mat4 invProj) {
    float fov = radians(CameraAttributes[FOV]);
    float tanHalfFov = tan(fov * 0.5);
    float aspect = CameraAttributes[ASPECT];
    float near = CameraAttributes[NEAR];
    float far  = CameraAttributes[FAR];
    float range = far - near;

    mat4 proj = mat4(
        1.0 / (tanHalfFov * aspect), 0.0,  0.0,  0.0,
        0.0, 1.0 / tanHalfFov,  0.0,  0.0,
        0.0, 0.0, -(far + near) / range, -1.0,
        0.0, 0.0, -(2.0 * far * near) / range,  0.0
    );

    invProj = mat4(
        tanHalfFov * aspect, 0.0, 0.0, 0.0,
        0.0, tanHalfFov, 0.0, 0.0,
        0.0, 0.0, 0.0, -1.0,
        0.0, 0.0, -1.0 * range / (2.0 * far * near), (far + near) / (2.0 * far * near)
    );

    return proj;
}

void main() {
    vec3 P = CameraPosition.xyz;
    vec3 F = normalize(CameraForward.xyz);

    mat4 invView;
    mat4 View = LookAt(P, F, invView);

    mat4 invProj;
    mat4 Proj = Perspective(invProj);

    VP = Proj * View;
    InvVP = invView * invProj;
}

/*

#if 0 CPU SIDE
void UpdateCamera(const Shader* const shaders[],
                  const Camera* const camera) {
    const Shader* shader;
    
    glm::vec4 game_camera = glm::vec4(camera->fov, camera->aspect, camera->near_plane, camera->far_plane);
    glm::vec4 position = glm::vec4(10.0f, 15.0f, 20.0f, 1.0);
    glm::vec4 forward = glm::vec4(0.0f, 0.0f, 1.0f, 0.0);
    shader = shaders[(uint32_t)Shaders::ViewProjection];
    glUseProgram(shader->ID);
    for (uint32_t U = 0; U < shader->uniforms_count; U++) {
        GLuint location = shader->uniform_location[U];
        switch(shader->uniform_name[U]) {
            case Uniforms::CameraAttributes: glUniform4fv(location, 1, &game_camera[0]); break;
            case Uniforms::CameraPosition: glUniform4fv(location, 1, &position[0]); break;
            case Uniforms::CameraForward: glUniform4fv(location, 1, &forward[0]); break;
            default: SHADER_WARNING("Uniform mismatch"); break;
        }
    }
    glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 3, camera->view_projection_buffer);
    glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 4, camera->inverse_view_projection_buffer);
    glDispatchCompute(1, 1, 1);
    glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 3, 0);
    glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 4, 0);
    
    glMemoryBarrier(GL_SHADER_STORAGE_BARRIER_BIT);
}
#endif
*/