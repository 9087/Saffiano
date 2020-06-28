using Saffiano;
using System.Collections;
using System.Collections.Generic;

namespace Saffiano.Sample
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
        Text renderStatusText = null;
        int frameCount = 0;
        float lastFpsUpdateTimestamp = Time.time;

        void Awake()
        {
            lastMousePosition = Input.mousePosition;
            targetCamera = Camera.main;

            var canvas = new GameObject("Canvas");
            canvas.AddComponent<RectTransform>();
            canvas.transform.parent = this.transform;
            canvas.AddComponent<Canvas>();

            var renderStatus = new GameObject("RenderStatus");
            var rectTransform = renderStatus.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(128, 128);
            renderStatus.transform.parent = canvas.transform;
            renderStatus.AddComponent<CanvasRenderer>();
            renderStatusText = renderStatus.AddComponent<Text>();
            renderStatusText.font = Font.CreateDynamicFontFromOSFont("../../../../Resources/JetBrainsMono-Regular.ttf", 18);
        }

        void Update()
        {
            frameCount++;
            float duration = Time.time - lastFpsUpdateTimestamp;
            float threadhold = 1;
            if (duration > threadhold)
            {
                renderStatusText.text = string.Format("FPS: {0:F2}", (float)frameCount / duration);
                frameCount = 0;
                lastFpsUpdateTimestamp = Time.time;
            }

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

            // UI
            {
                GameObject canvas = new GameObject("Canvas");
                canvas.AddComponent<RectTransform>();
                canvas.AddComponent<Canvas>();

                GameObject lena = new GameObject("Lena");
                var rectTransform = lena.AddComponent<RectTransform>();
                rectTransform.pivot = new Vector2(0, 0.5f);
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);
                rectTransform.offsetMin = new Vector2(0, -129);
                rectTransform.offsetMax = new Vector2(256, 128);
                lena.transform.parent = canvas.transform;
                lena.AddComponent<CanvasRenderer>().enabled = false;
                lena.AddComponent<Image>().sprite = Sprite.Create(Resources.Load("../../../../Resources/lena.png") as Texture);
                var text = lena.AddComponent<Text>();
                text.text = "The quick brown fox jumps over a lazy dog. 1234567890 ~!@#$%^&*()";
                text.font = Font.CreateDynamicFontFromOSFont("../../../../Resources/JetBrainsMono-Regular.ttf", 18);
            }

            // Bunny
            {
                GameObject bunny = new GameObject("Bunny");
                bunny.AddComponent<Transform>();
                List<LOD> lods = new List<LOD>();

                {
                    GameObject lod0 = new GameObject("Bunny.LOD0");
                    lod0.AddComponent<Transform>();
                    lod0.AddComponent<MeshFilter>();
                    lod0.AddComponent<MeshRenderer>().enabled = false;
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
            }

            // Dragon
            {
                GameObject dragon = new GameObject("Dragon");
                dragon.AddComponent<Transform>();
                dragon.AddComponent<Transform>();
                dragon.AddComponent<MeshFilter>();
                dragon.AddComponent<MeshRenderer>();
                dragon.AddComponent<MeshLoader>().path = "../../../../Resources/dragon_recon/dragon_vrip.ply";
            }

            Application.Run();
            Application.Uninitialize();
        }
    }
}
