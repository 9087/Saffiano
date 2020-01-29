using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Saffiano
{
    internal class FileFormatAttribute : Attribute
    {
    }

    
    public class Asset : Object
    {
        internal class FileFormatMethodInfoCollection : Dictionary<string, MethodInfo>
        {
        }

        private static Dictionary<Type, FileFormatMethodInfoCollection> fileFormatMethods = new Dictionary<Type, FileFormatMethodInfoCollection>();

        internal static FileFormatMethodInfoCollection RegisterFileFormats(Type type)
        {
            if (fileFormatMethods.ContainsKey(type))
            {
                return fileFormatMethods[type];
            }
            Debug.LogFormat("Register file formats by asset type: {0}", type);
            var methodInfos = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            FileFormatMethodInfoCollection methods = new FileFormatMethodInfoCollection();
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.GetCustomAttribute<FileFormatAttribute>() == null)
                {
                    continue;
                }
                Debug.LogFormat("Format handler found: {0}", methodInfo.Name);
                methods.Add(string.Format(".{0}", methodInfo.Name).ToUpper(), methodInfo);
            }
            fileFormatMethods.Add(type, methods);
            return methods;
        }

        public Guid id
        {
            get;
            private set;
        }

        public Asset()
        {
            id = System.Guid.NewGuid();
        }

        protected Asset(string filePath) : this()
        {
            FileFormatMethodInfoCollection collection = RegisterFileFormats(this.GetType());
            var extension = Path.GetExtension(filePath).ToUpper();
            collection[extension].Invoke(this, new object[] { filePath });
        }
    }
}
