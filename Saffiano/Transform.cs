using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Saffiano
{
    public enum Space
    {
        World = 0,
        Self = 1
    }

    internal class InternalTransform : Transform
    {
    }

    public class Transform : Component, IEnumerable<Transform>
    {
        internal static InternalTransform scene { get; private set; }

        internal static InternalTransform background { get; private set; }

        private Transform internalParent = null;
        internal List<Transform> children = new List<Transform>();

        private Vector3 _localPosition = Vector3.zero;

        public virtual Vector3 localPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
            }
        }

        public Vector3 position
        {
            get
            {
                var p = localToWorldMatrix * new Vector4(0, 0, 0, 1);
                return new Vector3(p.x, p.y, p.z);
            }
            set
            {
                if (parent == null)
                {
                    localPosition = new Vector3(value.x, value.y, value.z);
                }
                else
                {
                    var p = parent.worldToLocalMatrix * new Vector4(value.x, value.y, value.z, 1);
                    localPosition = new Vector3(p.x, p.y, p.z);
                }
            }
        }

        private Quaternion _localRotation = Quaternion.Euler(0, 0, 0);

        public Quaternion localRotation
        {
            get => _localRotation;
            set
            {
                _localRotation = value;
            }
        }
        
        public Quaternion rotation
        {
            get => this.localToWorldMatrix.rotation;
            set
            {
                if (parent == null)
                {
                    localRotation = new Quaternion(value.x, value.y, value.z, value.w);
                }
                else
                {
                    var matrix = Matrix4x4.TRS(position, rotation, scale).inverse;
                    localRotation = (parent.localToWorldMatrix * matrix).rotation;
                }
            }
        }

        private Vector3 _localScale = new Vector3(1, 1, 1);

        public Vector3 localScale
        {
            get => _localScale;
            set
            {
                _localScale = value;
            }
        }

        public Vector3 scale
        {
            get => this.localToWorldMatrix.lossyScale;
            set
            {
                if (parent == null)
                {
                    localScale = new Vector3(value.x, value.y, value.z);
                }
                else
                {
                    var matrix = Matrix4x4.TRS(position, rotation, scale).inverse;
                    localScale = (parent.localToWorldMatrix * matrix).lossyScale;
                }
            }
        }

        public Vector3 right => this.rotation * Vector3.right;

        public Vector3 up => this.rotation * Vector3.up;

        public Vector3 forward => this.rotation * Vector3.forward;

        public Transform parent
        {
            get
            {
                return this.internalParent is InternalTransform ? null : this.internalParent;
            }

            set
            {
                if (value != null && value.IsChildOf(this))
                {
                    throw new Exception();
                }
                if (this.internalParent == Transform.background)
                {
                    return;
                }
                if (this.parent == value)
                {
                    return;
                }
                var old = this.parent;
                this.SetInternalParent(value == null ? Transform.scene : value);
                if (old != this.parent)
                {
                    OnParentChanged(old, this.parent);
                }
            }
        }

        internal void SetInternalParent(Transform transform)
        {
            var old = this.internalParent;
            if (this.internalParent != null)
            {
                this.internalParent.children.Remove(this);
                this.internalParent.OnChildRemoved(this);
            }
            this.internalParent = transform;
            if (this.internalParent != null)
            {
                this.internalParent.children.Add(this);
                this.internalParent.OnChildAdded(this);
            }
            if (old != this.internalParent)
            {
                OnInternalParentChanged(old, this.internalParent);
            }
        }

        internal Transform GetInternalParent()
        {
            return this.internalParent;
        }

        internal void EnsureChild(Transform child)
        {
            if (this.children.Contains(child))
            {
                return;
            }
            this.children.Add(child);
        }

        protected virtual void OnChildAdded(Transform child)
        {
        }

        protected virtual void OnChildRemoved(Transform child)
        {
        }

        protected virtual void OnParentChanged(Transform old, Transform current)
        {
            SendMessage("OnTransformParentChanged");
        }

        protected virtual void OnInternalParentChanged(Transform old, Transform current)
        {
            this.gameObject.OnInternalParentChanged(old, current);
        }

        public virtual Matrix4x4 worldToLocalMatrix
        {
            get => localToWorldMatrix.inverse;
        }

        public virtual Matrix4x4 localToWorldMatrix
        {
            get
            {
                Matrix4x4 local = Matrix4x4.TRS(this.localPosition, this.localRotation, this.localScale);
                if (parent == null)
                {
                    return local;
                }
                else
                {
                    return parent.localToWorldMatrix * local;
                }
            }
        }

        public Transform()
        {
        }

        private static void Initialize()
        {
            Transform.scene = new GameObject("SCENE").AddComponent<InternalTransform>();
            Transform.scene.gameObject.UpdateActiveInHierarchyState();
            Transform.background = new GameObject("BACKGROUND") { target = null }.AddComponent<InternalTransform>();
            Transform.background.gameObject.SetActive(false);
        }

        private static void Uninitialize()
        {
            Transform.scene = null;
            Transform.background = null;
        }

        internal override void OnComponentAdded(GameObject gameObject)
        {
            base.OnComponentAdded(gameObject);
            this.SetInternalParent(gameObject.target);
        }

        internal override void OnComponentRemoved()
        {
            this.SetInternalParent(Transform.background);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            var children = this.children.ToList();
            foreach (var child in children)
            {
                yield return child;
            }
        }

        IEnumerator<Transform> IEnumerable<Transform>.GetEnumerator()
        {
            var children = this.children.ToList();
            foreach (var child in children)
            {
                yield return child;
            }
        }

        public void Rotate(float xAngle, float yAngle, float zAngle, [DefaultValue("Space.Self")] Space relativeTo)
        {
            throw new NotImplementedException();
        }

        private static bool Update()
        {
            Transform.scene.gameObject.RequestUpdate();
            Transform.scene.gameObject.RequestLateUpdate();
            return true;
        }

        public bool IsChildOf(Transform parent)
        {
            Transform current = this.transform;
            while (current != null)
            {
                if (current == parent)
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }

        public void SetSiblingIndex(int index)
        {
            parent.children.Remove(this);
            parent.children.Insert(index, this);
        }

        public int GetSiblingIndex()
        {
            return parent.children.IndexOf(this);
        }

        public int childCount => children.Count;
    }
}
