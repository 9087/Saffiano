using Saffiano.Widgets;

namespace Saffiano.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            PlayerSettings.backgroundColor = (Color)(new Vector4(Vector3.one * 30.0f / 255.9f, 1.0f));
            Resources.SetRootDirectory("../../../../Resources");
            Application.Initialize();

            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>().backgroundColor = PlayerSettings.backgroundColor;

            var canvas = new Widget()
            [
                new ImageView()
                {
                    anchorMin = Vector2.zero,
                    anchorMax = new Vector2(1, 1),
                    offsetMin = Vector2.zero,
                    offsetMax = Vector2.zero,
                    sprite = Sprite.Create(Texture.blackTexture),
                },
                new Console()
                {
                    anchorMin = Vector2.zero,
                    anchorMax = new Vector2(1, 1),
                    offsetMin = Vector2.zero,
                    offsetMax = Vector2.zero,
                    color = (Color)(new Vector4(Vector3.one * 212.0f / 255.9f, 1.0f)),
                }
            ]
            .AddComponent<Canvas>();

            Application.Run();
            Application.Uninitialize();
        }
    }
}
