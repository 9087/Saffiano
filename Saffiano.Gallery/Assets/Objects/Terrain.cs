﻿using Saffiano.Gallery.Assets.Classes;
using System.Collections.Generic;

namespace Saffiano.Gallery.Assets.Objects
{
    public class Terrain : SingletonGameObject<Terrain>
    {
        private Material _material = new Resources.Default.Material.Phong();

        public Material material
        {
            get => _material;
            set
            {
                _material = value;
                foreach (var renderer in renderers)
                {
                    renderer.material = _material;
                }
            }
        }

        private List<MeshRenderer> renderers = new List<MeshRenderer>();

        public Terrain()
        {
            // Plane
            GameObject plane = new GameObject("Plane");
            plane.AddComponent<Transform>();
            plane.AddComponent<MeshFilter>().mesh = new Resources.Default.Mesh.Plane(new Vector2(16, 16));
            renderers.Add(plane.AddComponent<MeshRenderer>());
            plane.transform.parent = this.transform;

            // Grass
            var grass = new Grass();
            grass.transform.parent = this.transform;
            grass.transform.localPosition = new Vector3(-1, 0, -1);

            foreach (var renderer in renderers)
            {
                renderer.material = _material;
            }
        }
    }
}
