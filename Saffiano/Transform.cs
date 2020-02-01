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
            get
            {
                var p = matrix * new Vector4(0, 0, 0, 1);
                return new Vector3(p.x, p.y, p.z);
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

        internal static Transform CreateRoot()
        {
            return new GameObject().AddComponent<Transform>();
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
            foreach (Transform transform in Transform.root.children)
            {
                transform.gameObject.RequestUpdate();
            }
            return true;
        }
    }
}
