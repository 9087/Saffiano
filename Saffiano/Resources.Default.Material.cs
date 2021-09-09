using System.Collections.Generic;
using System.Linq;

namespace Saffiano
{
    public partial class Resources
    {
        public static partial class Default
        { 
            public static class Material
            {
                public class Basic : ScriptableMaterial
                {
                    public virtual void VertexShader(
                        [Attribute(AttributeType.Position)] Vector3 a_position,
                        [Attribute(AttributeType.Normal)] Vector3 a_normal,
                        [Attribute(AttributeType.TexCoord)] Vector2 a_texcoord,
                        [Attribute(AttributeType.Color)] Color a_color,
                        out Vector4 gl_Position,
                        out Vector2 v_texcoord,
                        out Color v_color
                    )
                    {
                        gl_Position = mvp * new Vector4(a_position, 1.0f);
                        v_texcoord = a_texcoord;
                        v_color = a_color;
                    }

                    public virtual void FragmentShader(
                        Vector2 v_texcoord,
                        Color v_color,
                        out Color f_color
                    )
                    {
                        f_color = (Color)((Vector4)mainTexture.Sample(v_texcoord) * (Vector4)v_color);
                    }
                }

                public class Lambert : ScriptableMaterial
                {
                    [Uniform]
                    public Vector3 directionLight
                    {
                        get
                        {
                            if (Light.directionLights.Count == 0)
                            {
                                return Vector3.zero;
                            }
                            Debug.Assert(Light.directionLights.Count == 1);
                            var directionLight = Light.directionLights[0];
                            var lightDirection = directionLight.transform.localToWorldMatrix * new Vector4(Vector3.back, 0);
                            return lightDirection.xyz;
                        }
                    }

                    [Uniform]
                    public Color directionLightColor
                    {
                        get
                        {
                            if (Light.directionLights.Count == 0)
                            {
                                return new Color(1, 1, 1, 1);
                            }
                            Debug.Assert(Light.directionLights.Count == 1);
                            return Light.directionLights[0].color;
                        }
                    }

                    [Uniform]
                    public Color ambientColor
                    {
                        get
                        {
                            return RenderSettings.ambientLight;
                        }
                    }

                    public virtual void VertexShader(
                        [Attribute(AttributeType.Position)] Vector3 a_position,
                        [Attribute(AttributeType.Normal)] Vector3 a_normal,
                        [Attribute(AttributeType.TexCoord)] Vector2 a_texcoord,
                        [Attribute(AttributeType.Color)] Color a_color,
                        out Vector4 gl_Position,
                        out Color v_color
                    )
                    {
                        Vector3 normal = (mv * new Vector4(a_normal, 0)).xyz.normalized;
                        gl_Position = mvp * new Vector4(a_position, 1.0f);
                        Vector4 color = (Vector4)directionLightColor;
                        var diffuseColor = new Vector4(color.xyz * Mathf.Max(Vector3.Dot(normal, directionLight), 0), 1);
                        v_color = (Color)(diffuseColor + (Vector4)(ambientColor));
                    }

                    public virtual void FragmentShader(
                        Vector4 v_color,
                        out Vector4 f_color
                    )
                    {
                        f_color = v_color;
                    }
                }
            
                public class Phong : Lambert
                {
                    [Uniform]
                    public float shininess { get; set; } = 32;

                    [Uniform]
                    public Vector3 cameraPosition => Camera.main.transform.position;

                    public virtual void VertexShader(
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

                    public virtual void FragmentShader(
                        Vector4 v_position,
                        Vector3 v_normal,
                        Color v_diffuseColor,
                        out Color f_color
                    )
                    {
                        Vector3 worldNormal = (mv * new Vector4(v_normal, 0)).xyz.normalized;
                        var r = Vector3.Reflect(-directionLight.normalized, worldNormal) * Mathf.Max(Vector3.Dot(worldNormal, directionLight), 0);
                        var viewDirection = (-(mv * v_position).xyz + cameraPosition).normalized;
                        var specularColor = (Vector4)directionLightColor * Mathf.Pow(Mathf.Max(Vector3.Dot(viewDirection, r), 0), shininess);
                        f_color = (Color)(new Vector4(specularColor.xyz, 1) * 0.5f + (Vector4)v_diffuseColor * 0.5f + (Vector4)(ambientColor));
                    }
                }
            }
        }
    }  // Resources.Default.Material
}