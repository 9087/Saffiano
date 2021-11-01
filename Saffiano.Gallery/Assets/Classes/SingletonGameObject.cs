using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Gallery.Assets.Classes
{
    public class SingletonGameObject<T> : GameObject where T : GameObject, new()
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                instance = new T();
                return instance;
            }
        }
    }
}
