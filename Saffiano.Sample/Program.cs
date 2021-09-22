using System.Collections;

namespace Saffiano.Sample
{
    class Program
    {
        static IEnumerator Test()
        {
            yield return 1;
            yield return 2;
        }

        static void Main(string[] arguments)
        {
            Resources.SetRootDirectory("../../../../Resources");
            Application.Initialize();
            // Camera
            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Transform>();
            camera.AddComponent<Camera>().fieldOfView = 90.0f;
            camera.transform.localPosition = new Vector3(0, 0.1f, -0.5f);

            // ShadowMapping
            GameObject shadowMapping = Object.Instantiate(Resources.Load<ShadowMapping>()) as GameObject;
            //var shadowMappingTexture = shadowMapping.transform.Find("Camera").GetComponent<Camera>().TargetTexture;

            // Editor
            GameObject editor = Object.Instantiate(Resources.Load<Editor>()) as GameObject;

            // Lena
            //GameObject lena = Object.Instantiate(Resources.Load<Lena>()) as GameObject;

            // Bunny
            GameObject bunny = Object.Instantiate(Resources.Load<Bunny>()) as GameObject;

            GameObject.Find("Plane").GetComponent<MeshRenderer>().material = new ShadowMappingPhong();

            var material = GameObject.Find("Plane").GetComponent<MeshRenderer>().material as ShadowMappingPhong;

            Debug.Log(ScriptableMaterial.GetShaderSourceData(typeof(ShadowMappingPhong)).codes[ShaderType.FragmentShader]);

            var lightCamera = shadowMapping.GetComponent<Camera>();
            material.shadowMapTexture = lightCamera.TargetTexture;
            material.lightMVP = lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix;
            material.lightPosition = lightCamera.transform.position;

            Application.Run();
            Application.Uninitialize();
        }
    }
}
