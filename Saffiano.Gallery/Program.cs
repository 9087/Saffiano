﻿using Saffiano.Gallery.Assets.Objects;
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
            camera.transform.localPosition = new Vector3(-1.3989011f, 1.5476182f, -3.427567f);
            camera.transform.localRotation = new Quaternion(0.24993895f, -0.047199044f, 0.012198978f, 0.9670336f);

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

            GameObject canvas = new GameObject();
            canvas.AddComponent<RectTransform>();
            canvas.AddComponent<Canvas>();

            GameObject button = new GameObject();
            button.AddComponent<RectTransform>();
            var buttonRectTransform = button.transform as RectTransform;
            buttonRectTransform.pivot = new Vector2(0, 0);
            buttonRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRectTransform.offsetMin = new Vector2(-256, -256);
            buttonRectTransform.offsetMax = new Vector2(+256, +256);
            buttonRectTransform.parent = canvas.transform;

            //button.AddComponent<Saffiano.UI.Button>();
            //button.AddComponent<Saffiano.UI.Image>().sprite = Sprite.Create(Font.atlas);
            //button.AddComponent<CanvasRenderer>();

            // Event system
            {
                var eventSystem = new GameObject();
                eventSystem.AddComponent<Transform>();
                eventSystem.AddComponent<EventSystem>();
            }

            //// Set one sphere model as the gizmo target
            //Gizmo.Instance.target = GameObject.Find("Sphere");

            Application.Run();
            Application.Uninitialize();
        }
    }
}
