using System;
using System.Collections;

namespace Saffiano
{
    class Sample : Behaviour
    {
        void Start()
        {
            this.StartCoroutine(this.Count());
        }

        IEnumerator Count()
        {
            while (true)
            {
                Debug.LogFormat("Time.time = {0}", Time.time);
                yield return new WaitForSeconds(1);
            }
        }
    }

    class Program
    {
        static void Main(String[] arguments)
        {
            Application.Initialize();

            GameObject gameObject = new GameObject();
            gameObject.AddComponent<Transform>();
            gameObject.AddComponent<Sample>();
            var mesh = new Mesh("../../../../Resources/bunny/reconstruction/bun_zipper_res4.ply");

            Application.Run();
            Application.Uninitialize();
        }
    }
}
