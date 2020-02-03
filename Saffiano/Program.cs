using System;
using System.Collections;
using System.Collections.Generic;

namespace Saffiano
{
    class AsyncResourceLoader : Behaviour
    {
        public string path = null;
        protected ResourceRequest resourceRequest;

        void Start()
        {
            this.StartCoroutine(this.Load());
        }

        public virtual IEnumerator Load()
        {
            yield return new WaitForSeconds(0.5f);
            this.resourceRequest = Resources.LoadAsync(this.path);
            yield return this.resourceRequest;
        }
    }

    class MeshLoader : AsyncResourceLoader
    {
        public override IEnumerator Load()
        {
            yield return base.Load();
            this.GetComponent<MeshFilter>().mesh = this.resourceRequest.asset as Mesh;
            this.resourceRequest = null;
        }
    }

    class Editor : Behaviour
    {
        bool viewState = false;
        float translateSpeed = 1.0f;
        float rotateSpeed = 2.0f;
        Vector3 lastMousePosition;
        Camera targetCamera;

        void Awake()
        {
            lastMousePosition = Input.mousePosition;
            targetCamera = Camera.main;
        }

        void Update()
        {
            Vector3 deltaMousePosition = Input.mousePosition - lastMousePosition;
            viewState = Input.GetMouseButton(1);
            if (viewState)
            {
                Vector3 direction =
                    ((Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0)) * Camera.main.transform.right +
                    ((Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0)) * Camera.main.transform.up +
                    ((Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0)) * Camera.main.transform.forward;
                if (direction.magnitude != 0)
                {
                    targetCamera.transform.localPosition += direction.normalized * this.translateSpeed * Time.deltaTime;
                }
                Vector3 deltaRotation = new Vector3(-deltaMousePosition.y, deltaMousePosition.x) * rotateSpeed * Time.deltaTime;
                if (deltaRotation.magnitude != 0)
                {
                    targetCamera.transform.localRotation = Quaternion.Euler(targetCamera.transform.localRotation.eulerAngles + deltaRotation);
                }
            }
            lastMousePosition = Input.mousePosition;
        }
    }

    class Program
    {
        static void Main(string[] arguments)
        {
            Application.Initialize();

            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>().fieldOfView = 60.0f;
            camera.transform.localPosition = new Vector3(0, 0, -0.5f);

            GameObject editor = new GameObject("Editor");
            editor.AddComponent<Transform>();
            editor.AddComponent<Editor>();

            GameObject bunny = new GameObject("Bunny");
            bunny.AddComponent<Transform>();
            List<LOD> lods = new List<LOD>();

            {
                GameObject lod0 = new GameObject("Bunny.LOD0");
                lod0.AddComponent<Transform>();
                lod0.AddComponent<MeshFilter>();
                lod0.AddComponent<MeshRenderer>();
                lod0.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper.ply";
                lod0.transform.parent = bunny.transform;
                lods.Add(new LOD(0.60f, new Renderer[] { lod0.GetComponent<Renderer>() }));

                GameObject lod1 = new GameObject("Bunny.LOD1");
                lod1.AddComponent<Transform>();
                lod1.AddComponent<MeshFilter>();
                lod1.AddComponent<MeshRenderer>();
                lod1.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper_res2.ply";
                lod1.transform.parent = bunny.transform;
                lods.Add(new LOD(0.30f, new Renderer[] { lod1.GetComponent<Renderer>() }));

                GameObject lod2 = new GameObject("Bunny.LOD2");
                lod2.AddComponent<Transform>();
                lod2.AddComponent<MeshFilter>();
                lod2.AddComponent<MeshRenderer>();
                lod2.AddComponent<MeshLoader>().path = "../../../../Resources/bunny/reconstruction/bun_zipper_res3.ply";
                lod2.transform.parent = bunny.transform;
                lods.Add(new LOD(0.10f, new Renderer[] { lod2.GetComponent<Renderer>() }));
            }

            bunny.AddComponent<LODGroup>().SetLODs(lods.ToArray());

            Application.Run();
            Application.Uninitialize();
        }
    }
}
