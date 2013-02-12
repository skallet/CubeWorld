using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using CubeWorldTrees.Server;

namespace CubeWorldTrees.Controllers
{
    class UserController
    {

        protected Session session;

        protected HttpListenerContext context;

        public UserController(HttpListenerContext Context)
        {
            context = Context;

            Cookie sessid = context.Request.Cookies["sessid"];
            string sid = (sessid != null) ? sessid.Value.ToString() : "";

            if (sid.Length == 0 || sid.Equals("0"))
            {
                sid = Guid.NewGuid().ToString();
            }

            session = new Session(sid);

            if (!session.isEmpty() && !session.isValid())
            {
                session.unset();
                sid = Guid.NewGuid().ToString();
                session = new Session(sid);
            }

            if (session.isEmpty())
            {
                session.set("user", "Anonymous");
            }

            context.Response.Headers.Add("Set-Cookie", "sessid=" + sid + ";Path=/;Expires=" + session.getExpiration().ToString("ddd, dd-MMM-yyyy H:mm:ss") + " GMT");
        }

        public bool isLoggedIn()
        {
            return (session.isValid() && session.get("user") != "" && !session.get("user").Equals("Anonymous"));
        }

        public void logout()
        {
            if (this.isLoggedIn())
            {
                session.unset();
                context.Response.Headers.Add("Set-Cookie", "sessid=" + 0 + ";Path=/;Expires=" + DateTime.Now.ToString("ddd, dd-MMM-yyyy H:mm:ss") + " GMT");
            }
        }

        public void login(string username, string password)
        {
            if (password.Equals("test"))
            {
                session.set("user", "Registred");
                session.set("username", username);
            }
        }

    }
}
