using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Saffiano
{
    public enum Space
    {
        World = 0,
        Self = 1
    }

    public class Transform : Component, IEnumerable
    {
        internal static Transform root;
        private Transform internalParent = null;
        internal List<Transform> children = new List<Transform>();

        public Vector3 localPosition { get; set; } = Vector3.zero;

        public Vector3 position
        {
            get => this.internalParent is null ? this.localPosition : (this.localPosition + this.internalParent.position);
            set => this.localPosition = this.internalParent is null ? value : (value - this.internalParent.position);
        }

        public Quaternion localRotation { get; set; } = Quaternion.Euler(0, 0, 0);

        public Quaternion rotation
        {
            get => this.internalParent is null ? this.localRotation : (this.localRotation * this.internalParent.rotation);
            set => this.localRotation = this.internalParent is null ? value : (value * Quaternion.Inverse(this.internalParent.rotation));
        }

        public Vector3 scale => new Vector3(1, 1, 1);

        public Vector3 right => this.rotation * Vector3.right;

        public Vector3 up => this.rotation * Vector3.up;

        public Vector3 forward => this.rotation * Vector3.forward;

        public Transform parent
        {
            get
            {
                return this.internalParent == Transform.root ? null : this.internalParent;
            }

            set
            {
                if (this.internalParent != null)
                {
                    this.internalParent.children.Remove(this);
                }
                this.internalParent = value;
                if (this.internalParent != null)
                {
                    this.internalParent.children.Add(this);
                }
            }
        }

        public Matrix4x4 worldToLocalMatrix => Matrix4x4.TRS(this.position, this.rotation, this.scale);

        internal static Transform CreateRoot()
        {
            return new Transform();
        }

        public Transform()
        {
        }

        private static void Initialize()
        {
            Transform.root = CreateRoot();
        }

        private static void Uninitialize()
        {
            Transform.root = null;
        }

        internal override void OnComponentAdded(GameObject gameObject)
        {
            base.OnComponentAdded(gameObject);
            this.parent = Transform.root;
        }

        internal override void OnComponentRemoved()
        {
            this.parent = null;
        }

        public Transform Find(string name)
        {
            foreach (Transform transform in this.children)
            {
                if (transform.gameObject.name == name)
                {
                    return transform;
                }
                Transform result = transform.Find(name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Rotate(float xAngle, float yAngle, float zAngle, [DefaultValue("Space.Self")] Space relativeTo)
        {
            throw new NotImplementedException();
        }

        private static bool Update()
        {
            foreach (Transform transform in Transform.root.children)
            {
                transform.gameObject.RequestUpdate();
            }
            return true;
        }
    }
}
