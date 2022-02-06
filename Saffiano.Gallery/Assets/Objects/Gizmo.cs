using System;
using Saffiano.Gallery.Assets.Classes;

namespace Saffiano.Gallery.Assets.Objects
{
    class GizmoMaterial : ScriptableMaterial
    {
        public override ZTest zTest { get; set; } = ZTest.Always;

        public override Blend blend { get; set; } = Blend.transparency;

        public virtual void VertexShader(
            [Attribute(AttributeType.Position)] Vector3 a_position,
            [Attribute(AttributeType.Normal)] Vector3 a_normal,
            out Vector4 gl_Position
        )
        {
            gl_Position = mvp * new Vector4(a_position, 1.0f);
        }

        public new virtual void GeometryShader(
            [InputMode(InputMode.Triangles)] Input<Vertex> input,
            [OutputMode(OutputMode.LineStrip, 6)] Output<Vertex> output
        )
        {
            output.AddPrimitive(new Vertex(input[0].gl_Position), new Vertex(input[1].gl_Position));
            output.AddPrimitive(new Vertex(input[1].gl_Position), new Vertex(input[2].gl_Position));
            output.AddPrimitive(new Vertex(input[0].gl_Position), new Vertex(input[2].gl_Position));
        }

        public new virtual void FragmentShader(
            out Color f_color
        )
        {
            f_color = new Color(1.0f, 0, 0, 0.1f);
        }
    }

    class GizmoComponent : Behaviour
    {
        public new Gizmo gameObject => this.gameObject as Gizmo;

        private GameObject _target = null;

        private GizmoMaterial gizmoMaterial = new GizmoMaterial();

        private MeshFilter meshFilter = null;

        public GameObject target
        {
            get => _target;
            set
            {
                _target = value;
                var targetMeshFilter = _target != null ? _target.GetComponent<MeshFilter>() : null;
                if (_target != null && targetMeshFilter != null)
                {
                    meshFilter.mesh = targetMeshFilter.mesh;
                }
                else
                {
                    meshFilter.mesh = null;
                }
            }
        }

        void Awake()
        {
            this.meshFilter = this.GetComponent<MeshFilter>();
        }

        void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }
            this.transform.position = _target.transform.position;
            this.transform.rotation = _target.transform.rotation;
            this.transform.scale = _target.transform.localScale;
        }
    }

    public class Gizmo : SingletonGameObject<Gizmo>
    {
        private GizmoComponent gizmoComponent = null;

        public GameObject target
        {
            get => this.gizmoComponent.target;
            set
            {
                this.gizmoComponent.target = value;
            }
        }

        public Gizmo()
        {
            this.layer = LayerMask.NameToLayer("UI");
            this.AddComponent<Transform>();
            this.AddComponent<MeshFilter>();
            this.AddComponent<MeshRenderer>().material = new GizmoMaterial();
            this.gizmoComponent = this.AddComponent<GizmoComponent>();
        }
    }
}
