﻿using System;
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
                Debug.Log(Time.time);
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

            Application.Run();
            Application.Uninitialize();
        }
    }
}
