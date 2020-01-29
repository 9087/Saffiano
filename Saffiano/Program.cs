using System;
using System.Collections;

namespace Saffiano
{
    class AsyncResourceLoader : Behaviour
    {
        IEnumerator reportingProgress = null;
        public String path = null;
        protected ResourceRequest resourceRequest;

        void Start()
        {
            this.StartCoroutine(this.Load());
        }

        public virtual IEnumerator Load()
        {
            yield return new WaitForSeconds(0.5f);
            this.resourceRequest = Resources.LoadAsync(this.path);

            this.reportingProgress = this.ReportingProgress(this.resourceRequest);
            this.StartCoroutine(reportingProgress);
            yield return this.resourceRequest;
            this.StartCoroutine(this.StopReportingProgress());
        }

        IEnumerator ReportingProgress(ResourceRequest resourceRequest)
        {
            while (true)
            {
                Debug.LogFormat("Loading {0} progress: {1}", this.path, resourceRequest.progress);
                yield return new WaitForSeconds(1);
            }
        }

        IEnumerator StopReportingProgress()
        {
            yield return new WaitForSeconds(1.0f);
            this.StopCoroutine(this.reportingProgress);
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

    class Controller : Behaviour
    {
        public float positionSpeed = 0.02f;

        void Update()
        {
            Vector3 deltaPosition = Vector3.zero;
            if (Input.GetKey(KeyCode.Q))
            {
                deltaPosition = -Camera.main.transform.up * this.positionSpeed;
            }
            if (Input.GetKey(KeyCode.E))
            {
                deltaPosition = Camera.main.transform.up * this.positionSpeed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                deltaPosition = -Camera.main.transform.right * this.positionSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                deltaPosition = Camera.main.transform.right * this.positionSpeed;
            }
            if (Input.GetKey(KeyCode.W))
            {
                deltaPosition = Camera.main.transform.forward * this.positionSpeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                deltaPosition = -Camera.main.transform.forward * this.positionSpeed;
            }
            Camera.main.transform.position += deltaPosition;
        }
    }

    class Program
    {
        static void Main(String[] arguments)
        {
            Application.Initialize();

            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>().fieldOfView = 60.0f;
            camera.AddComponent<Controller>();
            camera.transform.localPosition = new Vector3(0, 0, -0.5f);
            camera.transform.localRotation = Quaternion.Euler(0, 0, 0);

            GameObject dragon = new GameObject();
            dragon.AddComponent<Transform>();
            dragon.AddComponent<MeshFilter>();
            dragon.AddComponent<MeshLoader>().path = "../../../../Resources/dragon_recon/dragon_vrip_res4.ply";
            dragon.AddComponent<MeshRenderer>();

            GameObject dragon1 = new GameObject();
            dragon1.AddComponent<Transform>();
            dragon1.AddComponent<MeshFilter>();
            dragon1.AddComponent<MeshLoader>().path = "../../../../Resources/dragon_recon/dragon_vrip_res4.ply";
            dragon1.AddComponent<MeshRenderer>();

            Application.Run();
            Application.Uninitialize();
        }
    }
}
