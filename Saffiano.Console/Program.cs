using Saffiano.UI;
using Saffiano.Widgets;
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
            camera.AddComponent<Camera>().backgroundColor = new Color(0, 0, 0);

            var console = new GameObject();
            console.AddComponent<RectTransform>();
            var canvas = console.AddComponent<Canvas>();

            var cmd = new CommandLine()
            {
                anchorMin = new Vector2(0, 0),
                anchorMax = new Vector2(1, 1),
                offsetMin = Vector2.zero,
                offsetMax = Vector2.zero,
            };
            cmd.transform.parent = canvas.transform;

            Application.Run();
            Application.Uninitialize();
        }
    }
}
