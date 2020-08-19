#version 330 core

in vec2 v_texcoord;

uniform sampler2D tex;

out vec4 color;

void main()
{
    color = texture(tex, v_texcoord);
}