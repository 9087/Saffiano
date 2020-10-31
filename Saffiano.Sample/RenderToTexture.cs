using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Sample
{
    class RenderToTexture : ScriptablePrefab
    {
        public override void Construct(GameObject gameObject)
        {
            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>().fieldOfView = 90.0f;
            camera.transform.localPosition = new Vector3(0, 0.5f, -0.25f);

            RenderTexture rt = new RenderTexture(255, 255);
            camera.GetComponent<Camera>().TargetTexture = rt;

            gameObject.AddComponent<RectTransform>();
            gameObject.AddComponent<Canvas>();
            GameObject target = new GameObject("Target");
            var rectTransform = target.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.offsetMin = new Vector2(0, -129);
            rectTransform.offsetMax = new Vector2(256, 128);
            target.transform.parent = gameObject.transform;
            target.AddComponent<CanvasRenderer>();
            target.AddComponent<Image>().sprite = Sprite.Create(rt as Texture);
        }
    }
}
