using Saffiano.Gallery.Assets.Classes;
using Saffiano.UI;

namespace Saffiano.Gallery.Assets.Objects
{
    class DebugInformationComponent : Behaviour
    {
        public Text text { get; set; }

        private int frameCount = 0;

        private float lastFpsUpdateTimestamp = Time.time;

        void Update()
        {
            frameCount++;
            float duration = Time.time - lastFpsUpdateTimestamp;
            float threadhold = 1;
            if (duration > threadhold)
            {
                text.text = string.Format("FPS: {0:F2}", (float)frameCount / duration);
                frameCount = 0;
                lastFpsUpdateTimestamp = Time.time;
            }
        }
    }

    public class DebugInformation : SingletonGameObject<DebugInformation>
    {
        public DebugInformation()
        {
            this.AddComponent<RectTransform>();
            this.AddComponent<Canvas>();

            var information = new GameObject("Text");

            var rectTransform = information.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(375, 65);
            information.transform.parent = this.transform;

            var image = new GameObject();
            image.AddComponent<RectTransform>().parent = rectTransform;
            image.AddComponent<CanvasRenderer>();
            var imageComponent = image.AddComponent<Image>();
            imageComponent.sprite = Sprite.Create(Texture.blackTexture);

            var text = new GameObject();
            text.AddComponent<RectTransform>().parent = rectTransform;
            text.AddComponent<CanvasRenderer>();
            var textComponent = text.AddComponent<Text>();
            textComponent.font = Font.CreateDynamicFontFromOSFont("fonts/JetBrainsMono-Regular.ttf", 60);
            textComponent.alignment = TextAnchor.MiddleLeft;
            text.AddComponent<Shadow>().effectColor = new Color(1, 1, 1, 0.5f);

            this.AddComponent<DebugInformationComponent>().text = textComponent;
        }
    }
}
