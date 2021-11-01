using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Saffiano
{
    public sealed partial class Resources
    {
        internal class LoadingIdentity : IEquatable<LoadingIdentity>
        {
            public Guid id { get; private set; }

            public string path { get; private set; }

            public LoadingIdentity(string path)
            {
                this.path = path;
                this.id = Guid.NewGuid();
            }

            public bool Equals(LoadingIdentity other)
            {
                return this.id == other.id && this.path == path;
            }
        }

        private class LoadingInfo
        {
            public Dictionary<LoadingIdentity, Action<Asset>> callbacks = new Dictionary<LoadingIdentity, Action<Asset>>();
            public Task task = null;
            public CancellationTokenSource cancellationTokenSource = null;
        }

        private static List<ResourceRequest> resourceRequests = new List<ResourceRequest>();
        private static Dictionary<string, Type> extensionNames = new Dictionary<string, Type>();
        private static List<Type> supportedAssetTypes = new List<Type> { typeof(Saffiano.Mesh), typeof(Texture), };
        private static Dictionary<string, LoadingInfo> loadingInfos = new Dictionary<string, LoadingInfo>();
        internal static string rootDirectory = string.Empty;

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

        public static Object Load<T>() where T : Prefab, new()
        {
            return Prefab.Load<T>();
        }

        public static Asset Load(string path)
        {
            lock (loadingInfos)
            {
                if (!loadingInfos.ContainsKey(path))
                {
                    loadingInfos.Add(path, new LoadingInfo());
                }
                LoadingInfo loadingInfo = loadingInfos[path];
                lock (loadingInfo)
                {
                    if (loadingInfo.task != null)
                    {
                        Debug.Assert(loadingInfo.cancellationTokenSource != null);
                        loadingInfo.cancellationTokenSource.Cancel();
                        loadingInfo.task = null;
                        loadingInfo.cancellationTokenSource = null;
                    }
                }
            }
            Asset asset = LoadInternal(path);
            lock (loadingInfos)
            {
                if (loadingInfos.TryGetValue(path, out LoadingInfo loadingInfo))
                {
                    lock (loadingInfo)
                    {
                        foreach (var callback in loadingInfo.callbacks.Values)
                        {
                            callback(asset);
                        }
                        loadingInfo.callbacks.Clear();
                    }
                    loadingInfos.Remove(path);
                }
            }
            return asset;
        }

        private static void Loading(string path)
        {
            Asset asset = LoadInternal(path);
            lock (loadingInfos)
            {
                if (loadingInfos.TryGetValue(path, out LoadingInfo loadingInfo))
                {
                    lock (loadingInfo)
                    {
                        foreach (var callback in loadingInfo.callbacks.Values)
                        {
                            callback(asset);
                        }
                        loadingInfo.callbacks.Clear();
                    }
                    loadingInfos.Remove(path);
                }
            }
        }

        public static ResourceRequest LoadAsync(string path)
        {
            var resourceRequest = new ResourceRequest();
            resourceRequests.Add(resourceRequest);
            lock (loadingInfos)
            {
                bool task = false;
                if (!loadingInfos.ContainsKey(path))
                {
                    loadingInfos.Add(path, new LoadingInfo());
                    task = true;
                }
                LoadingInfo loadingInfo = loadingInfos[path];
                lock (loadingInfo)
                {
                    loadingInfo.callbacks.Add(new LoadingIdentity(path), new Action<Asset>(resourceRequest.OnLoaded));
                    if (task)
                    {
                        loadingInfo.cancellationTokenSource = new CancellationTokenSource();
                        loadingInfo.task = new Task(new Action(() => { Loading(path); }), loadingInfo.cancellationTokenSource.Token);
                    }
                }
                if (task)
                {
                    loadingInfo.task.Start();
                }
            }
            return resourceRequest;
        }

        internal static Asset LoadInternal(string path)
        {
            path = Path.Combine(rootDirectory, path);
            if (Asset.TryGetCachedAssetByFilePath(path, out Asset asset))
            {
                return asset;
            }
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
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            object[] parameters = new object[] { path };
            asset = Activator.CreateInstance(assetType, flags, null, parameters, null) as Asset;
            return asset;
        }

        public static void SetRootDirectory(string path)
        {
            rootDirectory = path;
        }
    }
}
