using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Saffiano
{
    public sealed class Resources
    {
        private static List<ResourceRequest> resourceRequests = new List<ResourceRequest>();
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
            var resourceRequest = new ResourceRequest(path);
            resourceRequests.Add(resourceRequest);
            return resourceRequest;
        }

        private static bool Update()
        {
            List<ResourceRequest> list = new List<ResourceRequest>();
            foreach (var resourceRequest in resourceRequests)
            {
                if (resourceRequest.interrupted)
                {
                    list.Add(resourceRequest);
                }
                else if (resourceRequest.progress >= 1.0)
                {
                    list.Add(resourceRequest);
                    resourceRequest.Finish();
                }
            }
            foreach (var resourceRequest in list)
            {
                resourceRequests.Remove(resourceRequest);
            }
            return true;
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
