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

            String absolutePath = context.Request.Url.AbsolutePath;

            if (absolutePath == "/logout")
            {
                user.logout();
                Redirect("http://" + context.Request.Url.Host + ":" + context.Request.Url.Port + "/");
                return;
            }

            template.scripts.Add("/content/js/main.js");
        }

        public override void Render()
        {
            base.Render();
        }

    }
}
