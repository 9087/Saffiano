using System;
using System.Collections;

namespace Saffiano
{
    class Sample : Behaviour
    {
        void Start()
        {
            this.StartCoroutine(this.Count());
        }

        IEnumerator Count()
        {
            while (true)
            {
                Debug.LogFormat("Time.time = {0}", Time.time);
                yield return new WaitForSeconds(1);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("Key A pressed down");
            }
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

            GameObject gameObject = new GameObject();
            gameObject.AddComponent<Transform>();
            gameObject.AddComponent<Sample>();
            gameObject.AddComponent<MeshFilter>().mesh = new Mesh("../../../../Resources/dragon_recon/dragon_vrip_res3.ply");
            gameObject.AddComponent<MeshRenderer>();

            Application.Run();
            Application.Uninitialize();
        }
    }
}
