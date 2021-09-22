using Saffiano.UI;
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
            var targetPosition = mv * lightMVP * v_position;
            targetPosition = targetPosition / targetPosition.w;
            var distance = (v_position.xyz - lightPosition).magnitude;
            var texelSize = 1.0f / shadowMapTexture.size;

            float shadow = 0;
            var a00 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(-1, -1) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d00 = a00.r * 256.0f * 256.0f + a00.g * 256.0f + a00.b + a00.a / 32.0f;
            if (d00 + epsilon > distance) { shadow += 1.0f / 9.0f; }
            var a10 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(+0, -1) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d10 = a10.r * 256.0f * 256.0f + a10.g * 256.0f + a10.b + a10.a / 32.0f;
            if (d10 + epsilon > distance) { shadow += 1.0f / 9.0f; }
            var a20 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(+1, -1) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d20 = a20.r * 256.0f * 256.0f + a20.g * 256.0f + a20.b + a20.a / 32.0f;
            if (d20 + epsilon > distance) { shadow += 1.0f / 9.0f; }
            var a01 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(-1, +0) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d01 = a01.r * 256.0f * 256.0f + a01.g * 256.0f + a01.b + a01.a / 32.0f;
            if (d01 + epsilon > distance) { shadow += 1.0f / 9.0f; }
            var a11 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(+0, +0) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d11 = a11.r * 256.0f * 256.0f + a11.g * 256.0f + a11.b + a11.a / 32.0f;
            if (d11 + epsilon > distance) { shadow += 1.0f / 9.0f; }
            var a21 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(+1, +0) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d21 = a21.r * 256.0f * 256.0f + a21.g * 256.0f + a21.b + a21.a / 32.0f;
            if (d21 + epsilon > distance) { shadow += 1.0f / 9.0f; }
            var a02 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(-1, +1) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d02 = a02.r * 256.0f * 256.0f + a02.g * 256.0f + a02.b + a02.a / 32.0f;
            if (d02 + epsilon > distance) { shadow += 1.0f / 9.0f; }
            var a12 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(+0, +1) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d12 = a12.r * 256.0f * 256.0f + a12.g * 256.0f + a12.b + a12.a / 32.0f;
            if (d12 + epsilon > distance) { shadow += 1.0f / 9.0f; }
            var a22 = shadowMapTexture.Sample((targetPosition.xy + new Vector2(+1, +1) * texelSize + new Vector2(1, 1)) * 0.5f);
            var d22 = a22.r * 256.0f * 256.0f + a22.g * 256.0f + a22.b + a22.a / 32.0f;
            if (d22 + epsilon > distance) { shadow += 1.0f / 9.0f; }

            base.FragmentShader(v_position, v_normal, v_diffuseColor, out f_color);
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
