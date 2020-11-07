using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    internal enum InvisibleType
    {
        Empty = 0,
        LOD = 1 << 0,
    }

    public abstract class Renderer : Component
    {
        private InvisibleType invisibleType = InvisibleType.Empty;

        public bool enabled { get; set; } = true;

        public bool isVisible
        {
            get
            {
                return enabled && invisibleType == InvisibleType.Empty && this.gameObject.activeInHierarchy;
            }
        }

        protected abstract void OnRender();

        internal void Render()
        {
            if (!isVisible)
            {
                return;
            }
            OnRender();
        }

        internal void SetLODVisible(bool visible)
        {
            if (visible)
            {
                invisibleType &= ~InvisibleType.LOD;
            }
            else
            {
                invisibleType |= InvisibleType.LOD;
            }
        }
    }
}
