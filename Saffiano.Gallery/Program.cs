using Saffiano.Gallery.Assets.Objects;
using System;

namespace Saffiano.Gallery
{
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
            light.AddComponent<Transform>().localRotation = Quaternion.Euler(50, -30, 0);
            light.AddComponent<Light>();

            // Shadow mapping Phong material
            var shadowMappingPhong = new ShadowMappingPhong();
            material = shadowMappingPhong;

            // Shadow mapping
            var lightCamera = ShadowMapping.Instance.camera;
            shadowMappingPhong.shadowMapTexture = ShadowMapping.Instance.targetTexture;
            shadowMappingPhong.lightMVP = lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix;
            shadowMappingPhong.lightPosition = lightCamera.transform.position;

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
