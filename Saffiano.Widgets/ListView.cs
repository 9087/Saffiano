using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Widgets
{
    public class ListView : Widget
    {
        private LinearLayoutGroup layoutGroup = null;

        public ListView()
        {
            layoutGroup = AddComponent<LinearLayoutGroup>();
        }

        public float itemsMargin
        {
            get => layoutGroup.spacing;
            set { layoutGroup.spacing = value; }
        }
    }
}
