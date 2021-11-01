using Saffiano;
using Saffiano.Gallery.Assets.Components;
using System.Collections.Generic;

namespace Saffiano.Gallery.Assets.Objects
{
    public class Bunny : GameObject
    {
        List<LOD> lods = new List<LOD>();

        private Material _material = new Resources.Default.Material.Phong();

        public Material material
        {
            get => _material;
            set
            {
                _material = value;
                foreach (var lod in lods)
                {
                    foreach (var renderer in lod.renderers)
                    {
                        renderer.GetComponent<MeshRenderer>().material = _material;
                    }
                }
            }
        }

        public Bunny()
        {
            this.name = "Bunny";
            this.AddComponent<Transform>();

            // LOD0
            GameObject lod0 = new GameObject("Bunny.LOD0");
            lod0.AddComponent<Transform>();
            lod0.AddComponent<MeshFilter>();
            lod0.AddComponent<MeshRenderer>().material = _material;
            lod0.AddComponent<MeshAsyncLoader>().path = "models/bunny/reconstruction/bun_zipper.ply";
            lod0.transform.parent = this.transform;
            lods.Add(new LOD(0.60f, new Renderer[] { lod0.GetComponent<Renderer>() }));

            // LOD1
            GameObject lod1 = new GameObject("Bunny.LOD1");
            lod1.AddComponent<Transform>();
            lod1.AddComponent<MeshFilter>();
            lod1.AddComponent<MeshRenderer>().material = _material;
            lod1.AddComponent<MeshAsyncLoader>().path = "models/bunny/reconstruction/bun_zipper_res2.ply";
            lod1.transform.parent = this.transform;
            lods.Add(new LOD(0.30f, new Renderer[] { lod1.GetComponent<Renderer>() }));

            // LOD2
            GameObject lod2 = new GameObject("Bunny.LOD2");
            lod2.AddComponent<Transform>();
            lod2.AddComponent<MeshFilter>();
            lod2.AddComponent<MeshRenderer>().material = _material;
            lod2.AddComponent<MeshAsyncLoader>().path = "models/bunny/reconstruction/bun_zipper_res3.ply";
            lod2.transform.parent = this.transform;
            lods.Add(new LOD(0.10f, new Renderer[] { lod2.GetComponent<Renderer>() }));

            this.AddComponent<LODGroup>().SetLODs(lods.ToArray());
        }
    }
}
