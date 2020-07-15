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

        private string filePath = null;

        private static Dictionary<string, WeakReference<Asset>> fileCache = new Dictionary<string, WeakReference<Asset>>();

        public Asset()
        {
            id = System.Guid.NewGuid();
        }

        protected Asset(string filePath) : this()
        {
            FileFormatMethodInfoCollection collection = RegisterFileFormats(this.GetType());
            this.filePath = filePath;
            var extension = Path.GetExtension(filePath).ToUpper();
            collection[extension].Invoke(this, new object[] { filePath });
            lock (fileCache)
            {
                fileCache.Add(filePath, new WeakReference<Asset>(this));
            }
        }

        internal static bool TryGetCachedAssetByFilePath(string filePath, out Asset asset)
        {
            lock(fileCache)
            {
                if (!fileCache.ContainsKey(filePath))
                {
                    asset = null;
                    return false;
                }
                return fileCache[filePath].TryGetTarget(out asset);
            }
        }

        ~Asset()
        {
            if (filePath != null)
            {
                lock (fileCache)
                {
                    fileCache.Remove(filePath);
                }
            }
        }
    }
}
