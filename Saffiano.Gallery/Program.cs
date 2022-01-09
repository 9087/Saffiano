using Saffiano.Gallery.Assets.Objects;
using System;

namespace Saffiano.Gallery
{
    class Rotating : Behaviour
    {
        public Vector3 speed { get; set; }

        void Update()
        {
            this.transform.localRotation = Quaternion.Euler(this.transform.rotation.eulerAngles + speed * Time.deltaTime);
        }
    }

    class Program
    {
        public static Material material = null;

        static void Main(string[] args)
        {
            Resources.SetRootDirectory("../../../../Resources");
            Application.Initialize();

            // Debug information display
            { var _ = DebugInformation.Instance; }

            // Camera
            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>().fieldOfView = 90.0f;
            camera.transform.localPosition = new Vector3(0, 0.1f, -0.5f);

            // Light
            GameObject light = new GameObject("Light");
            light.AddComponent<Transform>();
            light.transform.localPosition = new Vector3(0, 1.2f, -1.2f);
            light.transform.localRotation = Quaternion.Euler(45, 0, 0);
            light.AddComponent<Light>().type = LightType.Directional;
            light.AddComponent<Rotating>().speed = new Vector3(0, 6, 0);

            // Shadow mapping Phong material
            var shadowMappingPhong = new ShadowMappingPhong();
            material = shadowMappingPhong;

            // Shadow mapping
            ShadowMapping.Instance.light = light.GetComponent<Light>();

            // Scene roaming control
            SceneRoaming.Instance.targetCamera = camera.GetComponent<Camera>();

            // Terrain
            { Terrain.Instance.material = material; }

            // Bunny
            { var _ = new Bunny() { material = material }; }

            // Several spheres
            var mesh = new Resources.Default.Mesh.Sphere();
            for (var x = 0; x < 2; x++)
            {
                for (var y = 0; y < 2; y++)
                {
                    for (var z = 0; z < 2; z++)
                    {
                        GameObject sphere = new GameObject("Sphere");
                        sphere.AddComponent<Transform>().localPosition = new Vector3(0.3f * (x + 1), 0.3f * (y + 1), -0.3f * z);
                        sphere.GetComponent<Transform>().localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        sphere.AddComponent<MeshFilter>().mesh = mesh;
                        sphere.AddComponent<MeshRenderer>().material = material;
                    }
                }
            }

            Application.Run();
            Application.Uninitialize();
        }
    }
}
