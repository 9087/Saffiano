

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

            var canvas = new GameObject();
            canvas.AddComponent<RectTransform>();
            canvas.AddComponent<Canvas>();

            var console = new Console()
            {
                anchorMin = new Vector2(0, 0),
                anchorMax = new Vector2(1, 1),
                offsetMin = Vector2.zero,
                offsetMax = Vector2.zero,
            };
            console.transform.parent = canvas.transform;

            Application.Run();
            Application.Uninitialize();
        }
    }
}
