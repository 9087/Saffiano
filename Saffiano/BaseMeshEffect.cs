using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public abstract class BaseMeshEffect : Behaviour
    {
        protected Graphic graphic => this.GetComponent<Graphic>();

        public abstract void ModifyMesh(Mesh mesh);
    }
}
