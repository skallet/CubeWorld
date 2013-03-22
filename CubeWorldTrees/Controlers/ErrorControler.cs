using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CubeWorldTrees.Controlers
{
    class ErrorControler : BaseControler
    {

        public ErrorControler(HttpListenerContext Context)
            : base(Context)
        {
        }

        public override void Render()
        {
        }

    }
}
