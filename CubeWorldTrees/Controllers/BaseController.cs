using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CubeWorldTrees.Controllers
{
    abstract class BaseController
    {

        protected HttpListenerContext context;

        public BaseController(HttpListenerContext Context)
        {
            context = Context;
        }

    }
}
