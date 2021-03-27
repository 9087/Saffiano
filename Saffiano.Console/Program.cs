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
            camera.AddComponent<Camera>();

            GameObject console = new GameObject("Console");
            console.AddComponent<RectTransform>();
            var canvas = console.AddComponent<Canvas>();

            new Widget()
            { }[
                new ListView()
                { itemsMargin = 5, }[
                    new TextView()
                    { text = "Hello Saffiano", },
                    new TextView()
                    { text = "List view item", },
                    new TextView()
                    { text = "Text", }
                ]
            ]
            .transform.parent = canvas.transform;

            Application.Run();
            Application.Uninitialize();
        }
    }
}
