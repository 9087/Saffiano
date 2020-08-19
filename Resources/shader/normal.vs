#version 330 core

layout (location = 0) in vec3 a_position;
layout (location = 1) in vec3 a_normal;
layout (location = 2) in vec2 a_textcoord;

varying vec4 a_color;
uniform mat4 u_MVP;

void main()
{
    gl_Position = u_MVP * vec4(a_position, 1.0);
    vec4 normal = u_MVP * vec4(a_normal, 1.0);
    a_color = vec4(1.0 - normal.z, 1.0 - normal.z, 1.0 - normal.z, 1.0);
}