﻿namespace Saffiano
{
    public sealed class MeshRenderer : Renderer
    {
        private MeshFilter meshFilter => gameObject.GetComponent<MeshFilter>();

        public Material material { get; set; } = new Resources.Default.Material.Lambert();

        protected override void OnRender()
        {
            if (meshFilter == null || meshFilter.mesh == null || material == null)
            {
                return;
            }
            var command = new Command()
            {
                projection = Rendering.projection,
                transform = transform.localToWorldMatrix,
                mesh = meshFilter.mesh,
                material = material,
            };
            Rendering.Draw(command);
        }
    }
}
