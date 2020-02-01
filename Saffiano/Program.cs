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

    class Controller : Behaviour
    {
        public float positionSpeed = 0.02f;
        public GameObject rotateY;
        public GameObject rotateX;
        public GameObject rotateZ;

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
            if (deltaPosition.magnitude != 0)
            {
                Debug.LogFormat("Camera.main.transform.position {0}", Camera.main.transform.localPosition);
                Camera.main.transform.localPosition += deltaPosition;
            }
            Vector3 deltaRotation = Vector3.zero;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                deltaRotation.x += 1.0f;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                deltaRotation.x -= 1.0f;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                deltaRotation.y -= 1.0f;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                deltaRotation.y += 1.0f;
            }
            if (Input.GetKey(KeyCode.N))
            {
                deltaRotation.z -= 1.0f;
            }
            if (Input.GetKey(KeyCode.M))
            {
                deltaRotation.z += 1.0f;
            }
            if (deltaRotation.magnitude != 0)
            {
                var eulerAngles = Camera.main.transform.localRotation.eulerAngles;
                Debug.LogFormat("eulerAngles = {0} deltaRotation = {1}", eulerAngles, deltaRotation);
                eulerAngles += deltaRotation;
                Camera.main.transform.localRotation = Quaternion.Euler(eulerAngles);
                Debug.LogFormat("Camera.main.transform.localRotation = {0}", Camera.main.transform.localRotation.eulerAngles);
            }

            if (rotateX != null)
            {
                rotateX.transform.localRotation = Quaternion.Euler(rotateX.transform.localRotation.eulerAngles + new Vector3(1, 0, 0));
            }
            if (rotateY != null)
            {
                rotateY.transform.localRotation = Quaternion.Euler(rotateY.transform.localRotation.eulerAngles + new Vector3(0, 1, 0));
            }
            if (rotateZ != null)
            {
                rotateZ.transform.localRotation = Quaternion.Euler(rotateZ.transform.localRotation.eulerAngles + new Vector3(0, 0, 1));
            }
            
        }
    }

    class Program
    {
        static void Test(string[] arguments)
        {
            int _case = 1;
            switch(_case)
            {
                case 0:
                    Debug.Log(Matrix4x4.TRS(new Vector3(1, 2, 3), Quaternion.Euler(128, 88, 30), new Vector3(1, 1, 1)));

                    var x = 128;
                    var y = 88;
                    var z = 30;
                    Debug.Log(Quaternion.Euler(x, 0, z));
                    Debug.Log(Quaternion.Euler(x, y, 0));
                    Debug.Log(Quaternion.Euler(0, y, z));

                    Quaternion qy = Quaternion.AngleAxis(y, new Vector3(0, 1, 0));
                    Quaternion qz = Quaternion.AngleAxis(z, new Vector3(0, 0, 1));

                    Debug.Log(qy);
                    Debug.Log(qz);
                    Debug.Log(qz * qy);
                    Debug.Log("=====================================");
                    break;
                case 1:
                    var a = Quaternion.Euler(128, 88, 0);
                    Debug.Log(a);
                    Debug.Log(a.eulerAngles);
                    var b = Quaternion.Euler(a.eulerAngles);
                    Debug.Log(b);
                    Debug.Log(b.eulerAngles);
                    var c = Quaternion.Euler(b.eulerAngles);
                    Debug.Log(c);
                    Debug.Log(c.eulerAngles);
                    Debug.Log(Quaternion.Euler(52, -92, 180) * Vector3.forward);
                    Debug.Log(Quaternion.Euler(52, -92, 180));
                    Debug.Log(Quaternion.Euler(52, -92, -180) * Vector3.forward);
                    Debug.Log(Quaternion.Euler(52, -92, -180));
                    Debug.Log(Quaternion.Euler(52, -92, 0));
                    break;
                case 2:
                    Debug.Log(Quaternion.Euler(0, -92, 160));
                    Debug.Log(Quaternion.Euler(0, -92, 180));
                    Debug.Log(Quaternion.Euler(52, 0, 160));
                    Debug.Log(Quaternion.Euler(52, 0, 180));
                    break;
            }
        }

        static void Main(string[] arguments)
        {
            Runtime(arguments);
        }

        static void Runtime(string[] arguments)
        {
            Application.Initialize();

            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>().fieldOfView = 60.0f;
            var controller = camera.AddComponent<Controller>();
            camera.transform.localPosition = new Vector3(0, 0, 0);
            camera.transform.localRotation = Quaternion.Euler(0, 0, 0);

            GameObject parent = new GameObject("Parent");
            parent.AddComponent<Transform>();
            parent.transform.localPosition = new Vector3(0, 0, 0.5f);
            //controller.rotateX = parent;

            GameObject dragon0 = new GameObject("Dragon0");
            dragon0.AddComponent<Transform>().parent = parent.transform;
            dragon0.AddComponent<MeshFilter>();
            dragon0.AddComponent<MeshLoader>().path = "../../../../Resources/dragon_recon/dragon_vrip_res4.ply";
            dragon0.AddComponent<MeshRenderer>();
            dragon0.transform.localPosition = new Vector3(0, 0, 0.5f);
            dragon0.transform.localRotation = Quaternion.Euler(0, 0, 0);
            controller.rotateY = dragon0;

            GameObject dragon1 = new GameObject("Dragon1");
            dragon1.AddComponent<Transform>().parent = parent.transform;
            dragon1.AddComponent<MeshFilter>();
            dragon1.AddComponent<MeshLoader>().path = "../../../../Resources/dragon_recon/dragon_vrip_res4.ply";
            dragon1.AddComponent<MeshRenderer>();
            dragon1.transform.localPosition = new Vector3(0.1f, 0.1f, 0);
            dragon1.transform.localRotation = Quaternion.Euler(0, 90, 0);
            controller.rotateZ = dragon1;

            GameObject dragon2 = new GameObject("Dragon2");
            dragon2.AddComponent<Transform>().parent = parent.transform;
            dragon2.AddComponent<MeshFilter>();
            dragon2.AddComponent<MeshLoader>().path = "../../../../Resources/dragon_recon/dragon_vrip_res3.ply";
            dragon2.AddComponent<MeshRenderer>();
            dragon2.transform.localPosition = new Vector3(-0.1f, -0.1f, 0);
            dragon2.transform.localRotation = Quaternion.Euler(90, 0, 0);

            Application.Run();
            Application.Uninitialize();
        }
    }
}
