using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public sealed class MeshRenderer : Renderer
    {
        private MeshFilter meshFilter
        {
            get;
            set;
        }

        internal override void OnComponentAdded(GameObject gameObject)
        {
            base.OnComponentAdded(gameObject);
            meshFilter = gameObject.GetComponent<MeshFilter>();
        }

        internal override void OnComponentRemoved()
        {
            meshFilter = null;
            base.OnComponentRemoved();
        }

        internal override void Render()
        {
            if (meshFilter.mesh == null)
            {
                return;
            }
            Rendering.DrawMesh(meshFilter.mesh);
        }
    }
}
