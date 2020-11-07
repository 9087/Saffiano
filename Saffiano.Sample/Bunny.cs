﻿using System.Collections.Generic;

namespace Saffiano.Sample
{
    public class Rotating : Behaviour
    {
        void Update()
        {
            var angle = this.transform.localRotation.eulerAngles;
            this.transform.localRotation = Quaternion.Euler(angle.x, angle.y + 60 * Time.deltaTime, angle.z);
        }
    }

    public class Bunny : ScriptablePrefab
    {
        public override void Construct(GameObject gameObject)
        {
            gameObject.name = "Bunny";
            gameObject.AddComponent<Transform>();
            List<LOD> lods = new List<LOD>();

            GameObject light = new GameObject("Light");
            light.AddComponent<Transform>().localRotation = Quaternion.Euler(50, -30, 0);
            light.AddComponent<Light>();
            light.AddComponent<Rotating>();

            // LOD0
            GameObject lod0 = new GameObject("Bunny.LOD0");
            lod0.AddComponent<Transform>();
            lod0.AddComponent<MeshFilter>();
            lod0.AddComponent<MeshRenderer>();
            lod0.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper.ply";
            lod0.transform.parent = gameObject.transform;
            lods.Add(new LOD(0.60f, new Renderer[] { lod0.GetComponent<Renderer>() }));

            // LOD1
            GameObject lod1 = new GameObject("Bunny.LOD1");
            lod1.AddComponent<Transform>();
            lod1.AddComponent<MeshFilter>();
            lod1.AddComponent<MeshRenderer>();
            lod1.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper_res2.ply";
            lod1.transform.parent = gameObject.transform;
            lods.Add(new LOD(0.30f, new Renderer[] { lod1.GetComponent<Renderer>() }));

            // LOD2
            GameObject lod2 = new GameObject("Bunny.LOD2");
            lod2.AddComponent<Transform>();
            lod2.AddComponent<MeshFilter>();
            lod2.AddComponent<MeshRenderer>();
            lod2.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper_res3.ply";
            lod2.transform.parent = gameObject.transform;
            lods.Add(new LOD(0.10f, new Renderer[] { lod2.GetComponent<Renderer>() }));

            gameObject.AddComponent<LODGroup>().SetLODs(lods.ToArray());

            // Plane
            GameObject plane = new GameObject("Plane");
            plane.AddComponent<Transform>();
            plane.AddComponent<MeshFilter>().mesh = new Resources.Default.Mesh.Plane();
            plane.AddComponent<MeshRenderer>();
        }
    }
}
