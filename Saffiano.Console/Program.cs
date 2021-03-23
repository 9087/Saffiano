using Saffiano.UI;
using System;

namespace Saffiano.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Initialize();

            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>();

            GameObject console = new GameObject("Console");
            console.AddComponent<RectTransform>();
            var canvas = console.AddComponent<Canvas>();

            TextView textView;

            new Widget() { } [
                textView = new TextView() {
                    text = "Hello Saffiano",
                }
            ]
            .transform.parent = canvas.transform;

            Application.Run();
            Application.Uninitialize();
        }
    }
}
