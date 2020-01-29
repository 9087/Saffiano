using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Saffiano
{
    public sealed class Resources
    {
        private static Dictionary<string, ResourceRequest> resourceRequsts = new Dictionary<string, ResourceRequest>();
        private static Dictionary<string, Type> extensionNames = new Dictionary<string, Type>();
        private static List<Type> supportedAssetTypes = new List<Type> { typeof(Mesh), typeof(Texture), };

        static Resources()
        {
            foreach (var type in supportedAssetTypes)
            {
                var collection = Asset.RegisterFileFormats(type);
                foreach (var extensionName in collection.Keys)
                {
                    extensionNames.Add(extensionName.ToUpper(), type);
                }
            }
        }

        public static ResourceRequest LoadAsync(string path)
        {
            ResourceRequest resourceRequst = new ResourceRequest(path);
            Resources.resourceRequsts.Add(path, resourceRequst);
            return resourceRequst;
        }

        internal static Asset LoadInternal(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogErrorFormat("Invalid file path: {0}", path);
                return null;
            }
            string extensionName = Path.GetExtension(path).ToUpper();
            if (!extensionNames.ContainsKey(extensionName))
            {
                Debug.LogErrorFormat("Unsupported file format: {0}", extensionName);
                return null;
            }
            Type assetType = extensionNames[extensionName];
            return System.Activator.CreateInstance(assetType, path) as Asset;
        }
    }
}
