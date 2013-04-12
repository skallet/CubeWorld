using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

namespace CubeWorldTrees.Controlers
{
    class MapControler : BaseControler
    {

        public MapControler(HttpListenerContext Context, Map.Map World)
            : base(Context, World)
        {
        }

        public override void BeforeRender()
        {
            base.BeforeRender();

            template.scripts.Add("/content/js/main.js");
        }

        public override void Render()
        {
            base.Render();
        }

    }
}
