using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CubeWorldTrees.Controllers
{
    abstract class BaseController
    {

        protected string[] ANONYMOUS_RIGHT = {"LoginController", "ErrorController"};

        protected HttpListenerContext context;

        protected Controllers.UserController user;

        public BaseController(HttpListenerContext Context)
        {
            context = Context;
            user = new Controllers.UserController(context);
        }

        public virtual void Render()
        {
        }

        public void Run()
        {
            string controller = this.ToString().Split('.').Last();
            Uri url = context.Request.Url;

            if (!ANONYMOUS_RIGHT.Contains(controller))
            {
                if (!user.isLoggedIn())
                {
                    try
                    {
                        context.Response.Redirect("http://" + url.Host + ":" + url.Port + "/login");
                        context.Response.StatusCode = 302;
                        context.Response.OutputStream.Close();
                    }
                    catch { }

                    return;
                }
            }

            this.Render();
        }

    }
}
