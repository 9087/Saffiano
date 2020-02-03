using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public sealed class MeshRenderer : Renderer
    {
        private MeshFilter meshFilter
        {
            get
            {
                return gameObject.GetComponent<MeshFilter>();
            }
        }

        protected override void OnRender()
        {
            if (meshFilter == null || meshFilter.mesh == null)
            {
                return;
            }
            Rendering.DrawMesh(meshFilter.mesh);
        }
    }
}
