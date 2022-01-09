using Saffiano.Rendering;
using System.Collections.Generic;

namespace Saffiano
{
    public sealed class Camera : Behaviour
    {
        internal static List<Camera> allCameras = new List<Camera>();

        internal Dictionary<string, GPUProgram> replacementShaders { get; private set; } = new Dictionary<string, GPUProgram>();

        public static Camera main
        {
            get;
            private set;
        }

        public Color backgroundColor { get; set; } = new Color(0.192157f, 0.301961f, 0.474510f, 1.0f);

        public float farClipPlane { set; get; } = 1000;

        public float nearClipPlane { get; set; } = 0.01f;

        public float depth { get; set; } = -1;

        public float fieldOfView { get; set; } = 60;

        public float orthographicSize { get; set; } = 5.0f;

        public bool orthographic { get; set; } = false;

        public int cullingMask { get; set; } = LayerMask.GetMask("Everything");

        internal bool IsCulled(GameObject gameObject)
        {
            return 0 == (cullingMask & (1 << gameObject.layer));
        }

        public Matrix4x4 projectionMatrix
        {
            get
            {
                var viewportSize = this.GetViewportSize();
                if (!this.orthographic)
                {
                    return Matrix4x4.Perspective(this.fieldOfView, (float)viewportSize.x / (float)viewportSize.y, this.nearClipPlane, this.farClipPlane);
                }
                else
                {
                    float verticalHalfSize = this.orthographicSize;
                    float horizontalHalfSize = verticalHalfSize * (float)viewportSize.x / (float)viewportSize.y;
                    return Matrix4x4.Ortho(-horizontalHalfSize, horizontalHalfSize, -verticalHalfSize, verticalHalfSize, this.nearClipPlane, this.farClipPlane);
                }
            }
        }

        public Matrix4x4 worldToCameraMatrix
        {
            get
            {
                Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
                return m * this.transform.worldToLocalMatrix;
            }
                
        }

        public RenderTexture targetTexture { get; set; } = null;

        public Camera() : base()
        {
        }

        void Awake()
        {
            if (main is null)
            {
                main = this;
                allCameras.Add(this);
            }
            else
            {
                allCameras.Insert(0, this);
            }
        }

        public void SetReplacementShader(GPUProgram shader, string replaceTag)
        {
            replacementShaders[replaceTag] = shader;
        }

        public void ResetReplacementShader()
        {
            replacementShaders.Clear();
        }

        internal Vector2 GetViewportSize()
        {
            if (this.targetTexture == null)
            {
                return Window.GetSize();
            }
            else
            {
                return new Vector2(this.targetTexture.width, this.targetTexture.height);
            }
        }
    }
}
