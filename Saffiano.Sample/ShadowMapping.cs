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

        void VertexShader(
            [Attribute(AttributeType.Position)] Vector3 a_position,
            [Attribute(AttributeType.Normal)] Vector3 a_normal,
            out Vector4 gl_Position,
            out Vector4 v_position,
            out Vector3 v_normal,
            out Color v_diffuseColor
        )
        {
            Vector3 normal = (mv * new Vector4(a_normal, 0)).xyz.normalized;
            gl_Position = mvp * new Vector4(a_position, 1.0f);
            Vector4 color = (Vector4)directionLightColor;
            v_diffuseColor = (Color)new Vector4(color.xyz * Mathf.Max(Vector3.Dot(normal, directionLight), 0), 1);
            v_position = new Vector4(a_position, 1.0f);
            v_normal = a_normal;
        }

        void FragmentShader(
            Vector4 v_position,
            Vector3 v_normal,
            Color v_diffuseColor,
            out Color f_color
        )
        {
            var targetPosition = mv * lightMVP * v_position;
            targetPosition = targetPosition / targetPosition.w;
            var depthColor = shadowMapTexture.Sample((targetPosition.xy + new Vector2(1, 1)) * 0.5f);
            var depth = depthColor.r * 256.0f * 256.0f + depthColor.g * 256.0f + depthColor.b + depthColor.a / 32.0f;
            var distance = (v_position.xyz - lightPosition).magnitude;
            if (distance < depth + epsilon)
            {
                Vector3 worldNormal = (mv * new Vector4(v_normal, 0)).xyz.normalized;
                var r = Vector3.Reflect(-directionLight.normalized, worldNormal) * Mathf.Max(Vector3.Dot(worldNormal, directionLight), 0);
                var viewDirection = (-(mv * v_position).xyz + cameraPosition).normalized;
                var specularColor = (Vector4)directionLightColor * Mathf.Pow(Mathf.Max(Vector3.Dot(viewDirection, r), 0), shininess);
                f_color = (Color)(new Vector4(specularColor.xyz, 1) * 0.5f + (Vector4)v_diffuseColor * 0.5f + (Vector4)(ambientColor));
            }
            else
            {
                f_color = new Color(0, 0, 0, 1);
            }
        }
    }

    class ShadowMapping : ScriptablePrefab
    {
        public override void Construct(GameObject gameObject)
        {
            gameObject.AddComponent<Transform>();
            gameObject.AddComponent<Camera>().fieldOfView = 90.0f;
            gameObject.transform.localPosition = new Vector3(0, 2.0f, -2);
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
