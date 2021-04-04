using Saffiano.Rendering;
using System.Collections.Generic;

namespace Saffiano
{
    public sealed class Camera : Behaviour
    {
        internal static List<Camera> allCameras = new List<Camera>();

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

        public float orthographicSize
        {
            get;
            set;
        }

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
                var viewport = RenderPipeline.viewport;
                if (!this.orthographic)
                {
                    return Matrix4x4.Perspective(this.fieldOfView, (float)viewport.width / (float)viewport.height, this.nearClipPlane, this.farClipPlane);
                }
                else
                {
                    float verticalHalfSize = this.orthographicSize;
                    float horizontalHalfSize = verticalHalfSize * (float)viewport.width / (float)viewport.height;
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

        public RenderTexture TargetTexture { get; set; } = null;

        public Camera() : base()
        {
        }

        void Awake()
        {
            allCameras.Add(this);
            if (main is null)
            {
                main = this;
            }
        }
    }
}
