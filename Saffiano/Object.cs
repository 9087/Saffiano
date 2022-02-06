using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Saffiano
{
    internal interface IPrimitive
    {
    }

    public class Object
    {
        public string name { get; set; } = "Unnamed";

        bool alive = false;

        public Object()
        {
            this.alive = true;
        }

        public override string ToString()
        {
            return string.Format("({0}: {1})", this.GetType().Name, this.name);
        }

        public static void Destroy(Object obj)
        {
            obj.RequestDestroy();
            obj.alive = false;
        }

        public override bool Equals(object obj)
        {
            if (!this.alive && obj == null)
            {
                return true;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(name);
        }

        protected virtual void RequestDestroy()
        {
        }

        #region Instantiate implement

        private static readonly MethodInfo MemberwiseCloneMethodInfo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || typeof(IPrimitive).IsAssignableFrom(type);
        }

        private static GameObject GetGameObject(object obj)
        {
            if (obj is Component)
            {
                return (obj as Component).gameObject;
            }
            else if (obj is GameObject)
            {
                return obj as GameObject;
            }
            else
            {
                return null;
            }
        }

        public static object Instantiate(object original, Func<object, bool> filter, IDictionary<object, object> cache)
        {
            if (original == null)
            {
                return null;
            }
            var type = original.GetType();
            if (IsPrimitive(type))
            {
                return original;
            }
            if (filter != null && filter(original) == false)
            {
                return original;
            }
            if (cache.ContainsKey(original))
            {
                return cache[original];
            }
            if (typeof(Delegate).IsAssignableFrom(type))
            {
                throw new NotImplementedException();
            }
            var target = MemberwiseCloneMethodInfo.Invoke(original, null);
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (!IsPrimitive(elementType))
                {
                    Array array = target as Array;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array.SetValue(Instantiate(array.GetValue(i), filter, cache), i);
                    }
                }
            }
            cache.Add(original, target);
            RecursiveInstantiateMembers(original, target, filter, cache);
            return target;
        }

        static BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        private static void RecursiveInstantiateMembers(object original, object target, Func<object, bool> filter, IDictionary<object, object> cache)
        {
            var type = original.GetType();
            while (type != null)
            {
                foreach (FieldInfo fieldInfo in type.GetFields(FieldBindingFlags))
                {
                    if (IsPrimitive(fieldInfo.FieldType))
                    {
                        continue;
                    }
                    var originalValue = fieldInfo.GetValue(original);
                    var targetValue = Instantiate(originalValue, filter, cache);
                    fieldInfo.SetValue(target, targetValue);
                }
                type = type.BaseType;
            }
        }

        public static Object Instantiate(Object original)
        {
            GameObject root = GetGameObject(original);
            if (!(root is GameObject))
            {
                throw new NotImplementedException();
            }
            Func<object, bool> filter = (object obj) =>
            {
                if (!(obj is Object))
                {
                    return true;
                }
                var gameObject = GetGameObject(obj);
                if (gameObject != null && gameObject.transform.IsChildOf(root.transform))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            };
            var cache = new Dictionary<object, object>();
            object target = Instantiate(root, filter, cache);
            Transform transform = GetGameObject(target).transform;
            var internalParent = transform.GetInternalParent();
            if (internalParent == Transform.background)
            {
                transform.SetInternalParent(Transform.scene);
            }
            else
            {
                internalParent.EnsureChild(transform);
            }
            return cache[original] as Object;
        }

        #endregion
    }
}
