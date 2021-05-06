using Saffiano.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Sample
{
    class EditorComponent : Behaviour
    {
        bool viewState = false;
        float translateSpeed = 1.0f;
        float rotateSpeed = 3.5f;
        Vector3 lastMousePosition;
        Camera targetCamera;
        Text renderStatusText = null;
        int frameCount = 0;
        float lastFpsUpdateTimestamp = Time.time;

        void Awake()
        {
            lastMousePosition = Input.mousePosition;
            targetCamera = Camera.main;

            var canvas = new GameObject("Canvas");
            canvas.AddComponent<RectTransform>();
            canvas.transform.parent = this.transform;
            canvas.AddComponent<Canvas>();

            var renderStatus = new GameObject("RenderStatus");
            var rectTransform = renderStatus.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(128, 128);
            renderStatus.transform.parent = canvas.transform;
            renderStatus.AddComponent<CanvasRenderer>();
            renderStatusText = renderStatus.AddComponent<Text>();
            renderStatus.AddComponent<Shadow>();
            renderStatusText.font = Font.CreateDynamicFontFromOSFont("fonts/JetBrainsMono-Regular.ttf", 22);
        }

        void Update()
        {
            frameCount++;
            float duration = Time.time - lastFpsUpdateTimestamp;
            float threadhold = 1;
            if (duration > threadhold)
            {
                renderStatusText.text = string.Format("FPS: {0:F2}", (float)frameCount / duration);
                frameCount = 0;
                lastFpsUpdateTimestamp = Time.time;
            }

            Vector3 deltaMousePosition = Input.mousePosition - lastMousePosition;
            viewState = Input.GetMouseButton(1);
            if (viewState)
            {
                Vector3 direction =
                    ((Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0)) * Camera.main.transform.right +
                    ((Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0)) * Camera.main.transform.up +
                    ((Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0)) * Camera.main.transform.forward;
                if (direction.magnitude != 0)
                {
                    targetCamera.transform.localPosition += direction.normalized * this.translateSpeed * Time.deltaTime;
                }
                Vector3 deltaRotation = new Vector3(-deltaMousePosition.y, deltaMousePosition.x) * rotateSpeed * Time.deltaTime;
                if (deltaRotation.magnitude != 0)
                {
                    targetCamera.transform.localRotation = Quaternion.Euler(targetCamera.transform.localRotation.eulerAngles + deltaRotation);
                }
            }
            lastMousePosition = Input.mousePosition;
        }
    }

    public class Editor : ScriptablePrefab
    {
        public override void Construct(GameObject gameObject)
        {
            gameObject.AddComponent<Transform>();
            gameObject.AddComponent<EditorComponent>();
        }
    }
}
