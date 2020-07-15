﻿using System;
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

    internal class InternalTransform : Transform
    {
    }

    public class Transform : Component, IEnumerable
    {
        internal static InternalTransform scene { get; private set; }

        internal static InternalTransform background { get; private set; }

        private Transform internalParent = null;
        internal List<Transform> children = new List<Transform>();

        public Vector3 localPosition { get; set; } = Vector3.zero;

        public Vector3 position
        {
            get
            {
                var p = matrix * new Vector4(0, 0, 0, 1);
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

        public Quaternion localRotation { get; set; } = Quaternion.Euler(0, 0, 0);
        
        public Quaternion rotation
        {
            get
            {
                // Reference: Maths - Conversion Matrix to Quaternion
                // http://euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
                // http://euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/christian.htm
                var matrix = this.matrix;
                var determinant = matrix.determinant;
                var absQ2 = MathF.Pow(Mathf.Abs(determinant), 1.0f / 3.0f) * Mathf.Sign(determinant);
                var w = Mathf.Sqrt(Mathf.Max(0, absQ2 + matrix.m00 + matrix.m11 + matrix.m22)) / 2;
                var x = Mathf.Sqrt(Mathf.Max(0, 1 + matrix.m00 - matrix.m11 - matrix.m22)) / 2 * Mathf.Sign(matrix.m21 - matrix.m12);
                var y = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.m00 + matrix.m11 - matrix.m22)) / 2 * Mathf.Sign(matrix.m02 - matrix.m20);
                var z = Mathf.Sqrt(Mathf.Max(0, 1 - matrix.m00 - matrix.m11 + matrix.m22)) / 2 * Mathf.Sign(matrix.m10 - matrix.m01);
                return new Quaternion(x, y, z, w);
            }
        }

        public Vector3 scale => new Vector3(1, 1, 1);

        public Vector3 localScale => new Vector3(1, 1, 1);

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
        }

        protected virtual void OnInternalParentChanged(Transform old, Transform current)
        {
            this.gameObject.OnInternalParentChanged(old, current);
        }

        public Matrix4x4 worldToLocalMatrix
        {
            get
            {
                return GenerateWorldToLocalMatrix(CoordinateSystems.LeftHand);
            }
        }

        internal Matrix4x4 GenerateWorldToLocalMatrix(CoordinateSystems coordinateSystem)
        {
            Matrix4x4 local = Matrix4x4.TRS(localPosition, localRotation, localScale, coordinateSystem).inverse;
            if (parent == null)
            {
                return local;
            }
            else
            {
                return local * parent.GenerateWorldToLocalMatrix(coordinateSystem);
            }
        }

        internal Matrix4x4 matrix
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
                    return parent.matrix * local;
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

        public IEnumerator GetEnumerator()
        {
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
            foreach (Transform transform in Transform.scene.children)
            {
                transform.gameObject.RequestUpdate();
            }
            return true;
        }

        internal virtual Matrix4x4 ToRenderingMatrix(CoordinateSystems coordinateSystem)
        {
            return Matrix4x4.TRS(position, rotation, scale, coordinateSystem);
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
    }
}
