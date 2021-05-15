

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
            camera.AddComponent<Camera>().backgroundColor = new Color(0, 0, 0);

            var canvas = new GameObject();
            canvas.AddComponent<RectTransform>();
            canvas.AddComponent<Canvas>();

            #region Solid background
            {
                var texture = new Texture(1, 1);
                texture.SetPixels(
                    new Color[] { PlayerSettings.backgroundColor }
                );
                var background = new ImageView()
                {
                    sprite = Sprite.Create(texture),
                };
                background.transform.parent = canvas.transform;
                background.offsetMin = Vector2.zero;
                background.offsetMax = Vector2.zero;
                background.anchorMin = Vector2.zero;
                background.anchorMax = new Vector2(1, 1);
            }
            #endregion

            #region Console
            {
                var console = new Console()
                {
                    anchorMin = new Vector2(0, 0),
                    anchorMax = new Vector2(1, 1),
                    offsetMin = Vector2.zero,
                    offsetMax = Vector2.zero,
                };
                console.transform.parent = canvas.transform;
                console.color = (Color)(new Vector4(Vector3.one * 212.0f / 255.9f, 1.0f));
            }
            #endregion

            Application.Run();
            Application.Uninitialize();
        }
    }
}
