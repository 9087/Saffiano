using System.Collections.Generic;

namespace Saffiano
{
    public enum LightType
    {
        Directional = 1,
    }

    public sealed class Light : Behaviour
    {
        static internal Dictionary<LightType, List<Light>> lights = new Dictionary<LightType, List<Light>>();

        private LightType _type = LightType.Directional;

        public LightType type
        { 
            get => _type;
            
            set
            {
                Unregister();
                _type = value;
                Register();
            }
        }

        static internal List<Light> directionLights => lights[LightType.Directional];

        public Color color { get; set; } = new Color(1, 0.956863f, 0.839216f);

        static Light()
        {
            foreach (LightType lightType in typeof(LightType).GetEnumValues())
            {
                lights.Add(lightType, new List<Light>());
            }
        }

        internal override void OnGameObjectActiveInHierarchyChanged(bool old, bool current)
        {
            base.OnGameObjectActiveInHierarchyChanged(old, current);
            Register(); /* or */ Unregister();
        }

        void OnEnable()
        {
            Register(); /* or */ Unregister();
        }

        void OnDisable()
        {
            Register(); /* or */ Unregister();
        }

        private bool Register()
        {
            if (isActiveAndEnabled && !lights[this.type].Contains(this))
            {
                lights[this.type].Add(this);
                return true;
            }
            return false;
        }

        private bool Unregister()
        {
            if (!isActiveAndEnabled && lights[this.type].Contains(this))
            {
                lights[this.type].Remove(this);
                return true;
            }
            return false;
        }
    }
}
