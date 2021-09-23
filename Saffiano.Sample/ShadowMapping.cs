﻿using Saffiano.UI;
using static Saffiano.Resources.Default.Material;

namespace Saffiano.Sample
{
    public class ShadowMappingMaterial : ScriptableMaterial
    {
        [Uniform]
        public Vector3 lightPosition { get; set; }

        void VertexShader(
            [Attribute(AttributeType.Position)] Vector3 a_position,
            [Attribute(AttributeType.Normal)] Vector3 a_normal,
            [Attribute(AttributeType.TexCoord)] Vector2 a_texcoord,
            [Attribute(AttributeType.Color)] Color a_color,
            out Vector4 gl_Position,
            out Vector4 v_position,
            out Vector2 v_texcoord,
            out Color v_color
        )
        {
            gl_Position = mvp * new Vector4(a_position, 1.0f);
            v_texcoord = a_texcoord;
            v_color = a_color;
            v_position = mv * new Vector4(a_position, 1.0f);
        }

        void FragmentShader(
            Vector4 v_position,
            Vector2 v_texcoord,
            Color v_color,
            out Color f_color
        )
        {
            float distance = (v_position.xyz - lightPosition).magnitude;
            float floor = (int)(distance);
            float fract = (int)((distance - floor) * 1024.0f);
            f_color = (Color)(new Vector4(
                (float)((int)(floor / 256.0f)) / 256.0f,
                (float)((int)(floor % 256.0f)) / 256.0f,
                (float)((int)(fract / 32.0f )) / 32.0f,
                (float)((int)(fract % 32.0f )) / 32.0f
            ));
        }
    }

    public class ShadowMappingPhong : Phong
    {
        [Uniform]
        public Texture shadowMapTexture { get; set; }

        [Uniform]
        public Matrix4x4 lightMVP { get; set; }

        [Uniform]
        public Vector3 lightPosition { get; set; }

        [Uniform]
        public float epsilon { get; set; } = 0.05f;

        public override void FragmentShader(
            Vector4 v_position,
            Vector3 v_normal,
            Color v_diffuseColor,
            out Color f_color
        )
        {
            base.FragmentShader(v_position, v_normal, v_diffuseColor, out f_color);

            // shadow mapping processing
            var targetPosition = mv * lightMVP * v_position;
            targetPosition = targetPosition / targetPosition.w;
            var distance = (v_position.xyz - lightPosition).magnitude;
            var texelSize = 1.0f / shadowMapTexture.size;
            float shadow = 0;
            int half = 8;
            int step = 2;
            float count = (half / step * 2 + 1) * (half / step * 2 + 1);
            for (int x = -half; x <= half; x += step) for (int y = -half; y <= half; y += step)
            {
                var color = shadowMapTexture.Sample((targetPosition.xy + new Vector2(x, y) * texelSize + new Vector2(1, 1)) * 0.5f);
                var depth = color.r * 256.0f * 256.0f + color.g * 256.0f + color.b + color.a / 32.0f;
                if (depth + epsilon > distance) { shadow += 1.0f / count; }
            }
            f_color = (Color)((Vector4)f_color * shadow + new Vector4(0, 0, 0, 1));
        }
    }

    class ShadowMapping : ScriptablePrefab
    {
        public override void Construct(GameObject gameObject)
        {
            gameObject.AddComponent<Transform>();
            gameObject.AddComponent<Camera>().fieldOfView = 90.0f;
            gameObject.transform.localPosition = new Vector3(0, 1.2f, -1.2f);
            gameObject.transform.localRotation = Quaternion.Euler(45, 0, 0);

            RenderTexture rt = new RenderTexture(512, 512);
            gameObject.GetComponent<Camera>().TargetTexture = rt;
            gameObject.GetComponent<Camera>().cullingMask = LayerMask.GetMask("Everything") & (~LayerMask.GetMask("UI"));
            gameObject.GetComponent<Camera>().SetReplacementShader(new ShadowMappingMaterial().shader, "");

            GameObject canvas = new GameObject("Camera");
            canvas.AddComponent<RectTransform>();
            canvas.AddComponent<Canvas>();
            canvas.transform.parent = gameObject.transform;

            GameObject target = new GameObject("Target");
            var rectTransform = target.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.offsetMin = new Vector2(0, -129);
            rectTransform.offsetMax = new Vector2(256, 128);
            target.transform.parent = canvas.transform;
            target.AddComponent<CanvasRenderer>();
            target.AddComponent<Image>().sprite = Sprite.Create(rt as Texture);
        }
    }
}
