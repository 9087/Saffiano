using Saffiano;
using System.Collections;
using System.Collections.Generic;

namespace Saffiano.Sample
{
    class Program
    {
        static void Main(string[] arguments)
        {
            Application.Initialize();

            // Camera
            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>().fieldOfView = 90.0f;
            camera.transform.localPosition = new Vector3(0, 0, -0.5f);

            // Editor
            GameObject editor = Object.Instantiate(Resources.Load<Editor>()) as GameObject;

            // Lena
            //GameObject lena = Object.Instantiate(Resources.Load<Lena>()) as GameObject;

            // Bunny
            GameObject bunny = Object.Instantiate(Resources.Load<Bunny>()) as GameObject;

            Application.Run();
            Application.Uninitialize();
        }
    }
}
