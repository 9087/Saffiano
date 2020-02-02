using System;
using System.Collections;

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

            GameObject dragon = new GameObject("Dragon");
            dragon.AddComponent<Transform>();
            dragon.AddComponent<MeshFilter>();
            dragon.AddComponent<MeshLoader>().path = "../../../../Resources/dragon_recon/dragon_vrip_res4.ply";
            dragon.AddComponent<MeshRenderer>();

            Application.Run();
            Application.Uninitialize();
        }
    }
}
