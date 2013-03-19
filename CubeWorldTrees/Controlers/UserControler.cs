using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using CubeWorldTrees.Server;

namespace CubeWorldTrees.Controlers
{
    class UserControler
    {

        protected Session session;

        protected HttpListenerContext context;

        public UserControler(HttpListenerContext Context)
        {
            context = Context;

            Cookie sessid = context.Request.Cookies["sessid"];
            string sid = (sessid != null && !sessid.Expired) ? sessid.Value.ToString() : "";
            Console.WriteLine("Try to connect with {0} session.", sid);

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
            Console.WriteLine("Creating session {0}", sid);
            Cookie cookie = new Cookie("sessid", sid);
            cookie.Expires = session.getExpiration();
            context.Response.Cookies.Add(cookie);
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
                Cookie cookie = new Cookie("sessid", "0");
                cookie.Expires = DateTime.Now;
                context.Response.Cookies.Add(cookie);
            }
        }

        public void login(string username, string password)
        {
            if (password.Equals("test"))
            {
                session.set("user", "Registred", true);
                session.set("username", username);
                session.forceValidFile();
            }
        }

    }
}
