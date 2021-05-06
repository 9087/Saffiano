using Saffiano.UI;

namespace Saffiano.Sample
{
    public class Lena : ScriptablePrefab
    {
        public override void Construct(GameObject gameObject)
        {
            gameObject.AddComponent<RectTransform>();
            gameObject.AddComponent<Canvas>();
            GameObject lena = new GameObject("Lena");
            var rectTransform = lena.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.offsetMin = new Vector2(0, -129);
            rectTransform.offsetMax = new Vector2(256, 128);
            lena.transform.parent = gameObject.transform;
            lena.AddComponent<CanvasRenderer>();
            lena.AddComponent<Image>().sprite = Sprite.Create(Resources.Load("textures/lena.png") as Texture);
            var text = lena.AddComponent<Text>();
            text.text = "Lena picture";
            text.font = Font.CreateDynamicFontFromOSFont("fonts/JetBrainsMono-Regular.ttf", 18);
        }
    }
}
