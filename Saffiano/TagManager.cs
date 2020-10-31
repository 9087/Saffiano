using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saffiano
{
    public struct LayerMask
    {
        public int value { get; set; }

        public static int GetMask(params string[] layerNames)
        {
            if (layerNames.Length == 1)
            {
                switch (layerNames[0])
                {
                    case "Nothing":
                        return 0;
                    case "Everything":
                        return -1;
                }
            }
            int value = 0;
            foreach (var layerName in layerNames)
            {
                value |= (1 << NameToLayer(layerName));
            }
            return value;
        }

        public static string LayerToName(int layer)
        {
            if(TagManager.layers.TryGetValue(layer, out string value))
            {
                return value;
            }
            throw new Exception(string.Format("Undefined layer index {0}", layer));
        }

        public static int NameToLayer(string layerName)
        {
            foreach (var layer in TagManager.layers.Keys)
            {
                if (TagManager.layers[layer] == layerName)
                {
                    return layer;
                }
            }
            throw new Exception(string.Format("Undefined layer name {0}", layerName));
        }

        public static implicit operator int(LayerMask mask)
        {
            return mask.value;
        }

        public static implicit operator LayerMask(int intVal)
        {
            return new LayerMask() { value = intVal };
        }
    }

    public static class TagManager
    {
        internal static Dictionary<int, string> layers = new Dictionary<int, string>();

        static TagManager()
        {
            AddLayar(0, "Default");
            AddLayar(1, "TransparentFX");
            AddLayar(2, "Ignore Raycast");
            AddLayar(4, "Water");
            AddLayar(5, "UI");
            AddLayar(8, "PostProcessing");
        }

        public static void AddLayar(int layer, string name)
        {
            Debug.Assert(0 <= layer && layer < 32);
            layers[layer] = name;
        }
    }
}
