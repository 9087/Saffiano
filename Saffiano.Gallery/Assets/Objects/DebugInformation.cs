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
            rectTransform.offsetMax = new Vector2(256, 26);
            information.transform.parent = this.transform;

            information.AddComponent<CanvasRenderer>();
            var text = information.AddComponent<Text>();
            text.font = Font.CreateDynamicFontFromOSFont("fonts/JetBrainsMono-Regular.ttf", 22);
            text.alignment = TextAnchor.MiddleLeft;
            information.AddComponent<Shadow>();

            this.AddComponent<DebugInformationComponent>().text = text;
        }
    }
}
