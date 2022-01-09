﻿using Saffiano.Gallery.Assets.Classes;
using Saffiano.UI;
using static Saffiano.Resources.Default.Material;

namespace Saffiano.Gallery.Assets.Objects
{
    class ShadowMappingMaterial : ScriptableMaterial
    {
        public override CullMode cullMode { get; set; } = CullMode.Front;

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
                (float)((int)(fract / 32.0f)) / 32.0f,
                (float)((int)(fract % 32.0f)) / 32.0f
            ));
        }
    }

    public class ShadowMappingPhong : Phong
    {
        private Camera lightCamera => ShadowMapping.Instance.camera;

        [Uniform]
        public Texture shadowMapTexture => ShadowMapping.Instance.targetTexture;

        [Uniform]
        public Matrix4x4 lightMVP => lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix;

        [Uniform]
        public Vector3 lightPosition => lightCamera.transform.position;

        [Uniform]
        public float epsilon { get; set; } = 0.02f;

        public override void FragmentShader(
            Vector4 v_position,
            Vector3 v_normal,
            Color v_diffuseColor,
            out Color f_color
        )
        {
            int half = 8;
            int step = 2;

            base.FragmentShader(v_position, v_normal, v_diffuseColor, out f_color);

            // shadow mapping processing
            var targetPosition = lightMVP * mv * v_position;
            targetPosition = targetPosition / targetPosition.w;

            if (targetPosition.x > +1) return;
            if (targetPosition.x < -1) return;
            if (targetPosition.y > +1) return;
            if (targetPosition.y < -1) return;

            var distance = ((mv * v_position).xyz - lightPosition).magnitude;
            var texelSize = 1.0f / shadowMapTexture.size;
            float shadow = 0;
            float count = (half / step * 2 + 1) * (half / step * 2 + 1);
            for (int x = -half; x <= half; x += step)
            {
                for (int y = -half; y <= half; y += step)
                {
                    var color = shadowMapTexture.Sample((targetPosition.xy + new Vector2(x, y) * texelSize + new Vector2(1, 1)) * 0.5f);
                    var depth = color.r * 256.0f * 256.0f + color.g * 256.0f + color.b + color.a / 32.0f;
                    if (depth - epsilon <= distance)
                    {
                        shadow += 0.3f / count;
                    }
                }
            }
            f_color = (Color)((Vector4)f_color * (1 - shadow) + shadow * new Vector4(0, 0, 0, 1));
        }
    }
    
    class ShadowMappingComponent : Behaviour
    {
        void Update()
        {
            var shadowMapping = this.gameObject as ShadowMapping;
            var camera = shadowMapping.camera;
            var light = shadowMapping.light;
            if (light == null)
            {
                camera.transform.parent = null;
                return;
            }
            if (shadowMapping.light.type == LightType.Directional)
            {
                Camera mainCamera = Camera.main;
                camera.orthographic = true;
                camera.transform.parent = null;

                var targetDistance = 1.0f;
                camera.transform.localPosition = mainCamera.transform.position + mainCamera.transform.forward * targetDistance - light.transform.forward * targetDistance;
                camera.orthographicSize = targetDistance * 2;
                camera.transform.localRotation = light.transform.rotation;
                camera.transform.localScale = Vector3.one;
            }
            else
            {
                camera.orthographic = false;
                camera.transform.parent = light.transform;
                camera.transform.localPosition = Vector3.zero;
                camera.transform.localRotation = Quaternion.identity;
                camera.transform.localScale = Vector3.one;
            }
        }
    }

    public class ShadowMapping : SingletonGameObject<ShadowMapping>
    {
        public Light light { get; set; }

        public Camera camera { get; protected set; }

        public RenderTexture targetTexture => camera.targetTexture;

        public ShadowMapping()
        {
            this.AddComponent<Transform>();
            this.camera = this.AddComponent<Camera>();
            this.AddComponent<ShadowMappingComponent>();
            this.camera.fieldOfView = 90.0f;

            RenderTexture rt = new RenderTexture(2048, 2048);
            rt.wrapMode = TextureWrapMode.Clamp;
            this.camera.targetTexture = rt;
            this.camera.cullingMask = LayerMask.GetMask("Everything") & (~LayerMask.GetMask("UI"));
            this.camera.SetReplacementShader(new ShadowMappingMaterial().shader, "");

#if true // DEBUG
            GameObject canvas = new GameObject("Canvas");
            canvas.AddComponent<RectTransform>();
            canvas.AddComponent<Canvas>();
            canvas.transform.parent = this.transform;

            GameObject target = new GameObject("Target");
            var rectTransform = target.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.offsetMin = new Vector2(0, -256);
            rectTransform.offsetMax = new Vector2(512, 256);
            target.transform.parent = canvas.transform;
            target.AddComponent<CanvasRenderer>();
            target.AddComponent<Image>().sprite = Sprite.Create(rt as Texture);
#endif
        }
    }
}
