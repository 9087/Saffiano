using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public struct LOD
    {
        public float screenRelativeTransitionHeight;
        public Renderer[] renderers;

        public LOD(float screenRelativeTransitionHeight, Renderer[] renderers)
        {
            this.screenRelativeTransitionHeight = screenRelativeTransitionHeight;
            this.renderers = renderers;
        }

        internal static LOD culled
        {
            get
            {
                return new LOD(0, new Renderer[] { });
            }
        }
    }

    public class LODGroup : Component
    {
        protected LOD[] lods = null;
        protected HashSet<Renderer> renderers = null;

        public int lodCount
        {
            get
            {
                return lods == null ? 0 : lods.Length;
            }
        }

        public bool enabled { get; set; } = true;

        public LOD[] GetLODs()
        {
            return lods;
        }

        public void SetLODs(LOD[] lods)
        {
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    renderer.SetLODVisible(true);
                }
            }
            this.lods = lods;
            renderers = new HashSet<Renderer>();
            foreach (var lod in lods)
            {
                foreach (var renderer in lod.renderers)
                {
                    renderers.Add(renderer);
                }
            }
        }

        public void RecalculateBounds()
        {
            throw new NotImplementedException();
        }

        internal void Update(Vector3 position)
        {
            var ratio = 1.0f / (position - transform.position).magnitude;
            LOD current = LOD.culled;
            foreach (var lod in lods)
            {
                if (ratio >= lod.screenRelativeTransitionHeight)
                {
                    current = lod;
                    break;
                }
            }
            foreach (var renderer in renderers)
            {
                renderer.SetLODVisible(false);
            }
            foreach (var renderer in current.renderers)
            {
                renderer.SetLODVisible(true);
            }
        }
    }
}
