using System.Collections.Generic;

namespace Saffiano.Sample
{
    public class Bunny : ScriptingPrefab
    {
        public override void Construct(GameObject gameObject)
        {
            GPUProgram shader = new GPUProgram("../../../../Resources/shader/normal.vs", "../../../../Resources/shader/normal.fs");
            gameObject.name = "Bunny";
            gameObject.AddComponent<Transform>();
            List<LOD> lods = new List<LOD>();
            GameObject lod0 = new GameObject("Bunny.LOD0");
            lod0.AddComponent<Transform>();
            lod0.AddComponent<MeshFilter>();
            lod0.AddComponent<MeshRenderer>().shader = shader;
            lod0.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper.ply";
            lod0.transform.parent = gameObject.transform;
            lods.Add(new LOD(0.60f, new Renderer[] { lod0.GetComponent<Renderer>() }));
            GameObject lod1 = new GameObject("Bunny.LOD1");
            lod1.AddComponent<Transform>();
            lod1.AddComponent<MeshFilter>();
            lod1.AddComponent<MeshRenderer>().shader = shader;
            lod1.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper_res2.ply";
            lod1.transform.parent = gameObject.transform;
            lods.Add(new LOD(0.30f, new Renderer[] { lod1.GetComponent<Renderer>() }));
            GameObject lod2 = new GameObject("Bunny.LOD2");
            lod2.AddComponent<Transform>();
            lod2.AddComponent<MeshFilter>();
            lod2.AddComponent<MeshRenderer>().shader = shader;
            lod2.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper_res3.ply";
            lod2.transform.parent = gameObject.transform;
            lods.Add(new LOD(0.10f, new Renderer[] { lod2.GetComponent<Renderer>() }));
            gameObject.AddComponent<LODGroup>().SetLODs(lods.ToArray());
        }
    }
}
