using Saffiano.Gallery.Assets.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Gallery.Assets.Objects
{
    class SceneRoamingComponent : Behaviour
    {
        private bool viewState = false;

        private Vector3 lastMousePosition;

        public float translateSpeed { get; set; } = 1.0f;

        public float rotateSpeed { get; set; } = 3.5f;

        public Camera targetCamera { get; set; }

        void Update()
        {
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

    public class SceneRoaming : SingletonGameObject<SceneRoaming>
    {
        private SceneRoamingComponent sceneRoamingComponent = null;

        public Camera targetCamera
        {
            get => sceneRoamingComponent.targetCamera;
            set
            {
                sceneRoamingComponent.targetCamera = value;
            }
        }

        public SceneRoaming()
        {
            this.AddComponent<Transform>();
            sceneRoamingComponent = this.AddComponent<SceneRoamingComponent>();
        }
    }
}
