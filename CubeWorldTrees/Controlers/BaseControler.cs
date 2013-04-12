using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CubeWorldTrees.Controlers
{
    abstract class BaseControler
    {

        protected string[] ANONYMOUS_RIGHT = {"LoginControler", "ErrorControler"};

        protected HttpListenerContext context;

        protected Controlers.UserControler user;

        protected Templates.Template template;

        protected bool renderTemplate = true;

        public bool responseSent = false;

        protected Map.Map world;

        public BaseControler(HttpListenerContext Context, Map.Map World)
        {
            context = Context;
            world = World;

            Models.UserModel usersModel = new Models.UserModel(Server.Server.connection);
            user = new Controlers.UserControler(context, usersModel, world);
        }

        public void Redirect(string url)
        {
            if (responseSent)
                return;

            try
            {
                responseSent = true;
                context.Response.Redirect(url);
                context.Response.StatusCode = 302;
                context.Response.OutputStream.Close();
            }
            catch { }
        }

        public void sendJsonResponse(string response)
        {
            if (responseSent)
                return;

            responseSent = true;
            context.Response.ContentType = "application/json";
            byte[] b = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = b.Length;

            try
            {
                context.Response.OutputStream.Write(b, 0, b.Length);
                context.Response.OutputStream.Close();
            }
            catch { }
        }

        public virtual void Auth()
        {
            string controler = this.ToString().Split('.').Last();
            Uri url = context.Request.Url;

            if (!ANONYMOUS_RIGHT.Contains(controler))
            {
                if (!user.isLoggedIn())
                {
                    Redirect("http://" + url.Host + ":" + url.Port + "/login");
                }
            }
        }

        public virtual void Action()
        {
        }

        public virtual void BeforeRender()
        {
        }

        public virtual void Render()
        {
        }

        public virtual void AfterRender()
        {
        }

        public void Run()
        {
            template = new Templates.Template(this);
            template.setParameter("user", this.user);

            this.Auth();
            this.Action();

            string controler = this.ToString().Split('.').Last().Replace("Controler", "").ToLower() + "/default.cw";
            template.setFile(controler);

            this.BeforeRender();
            this.Render();

            if (renderTemplate == true)
            {
                template.Render(context.Response);
            }

            this.AfterRender();
        }

    }
}
