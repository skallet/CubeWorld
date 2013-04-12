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

        public Models.UserModel model;

        protected Map.Map world;

        public UserControler(HttpListenerContext Context, Models.UserModel Model, Map.Map World)
        {
            context = Context;
            model = Model;
            world = World;

            Cookie sessid = context.Request.Cookies["sessid"];
            string sid = (sessid != null && !sessid.Expired) ? sessid.Value.ToString() : "";
            //Console.WriteLine("Try to connect with {0} session.", sid);

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
            //Console.WriteLine("Creating session {0}", sid);
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
            System.Collections.Hashtable userData = model.login(username, password);

            if (userData != null)
            {
                session.set("user", "Registred", true);
                session.set("username", userData["username"].ToString());
                session.set("id", userData["id"].ToString());

                session.set("pos-x", userData["x"].ToString());
                session.set("pos-y", userData["y"].ToString());

                session.forceValidFile();

                int x = Convert.ToInt32(session.get("pos-x"));
                int y = Convert.ToInt32(session.get("pos-y"));
            }
        }

        public Map.Rectangle getPosition()
        {
            int x = Convert.ToInt32(session.get("pos-x"));
            int y = Convert.ToInt32(session.get("pos-y"));

            if (x == 0 && y == 0)
            {
                session.set("pos-x", "5");
                session.set("pos-y", "5");
                x = 5;
                y = 5;
            }

            return new Map.Rectangle(x, y, 1);
        }

        public void setPosition(Map.Rectangle position)
        {
            int id = Convert.ToInt32(session.get("id"));
            model.updateUserPosition(id, position);

            session.set("pos-x", Convert.ToString(position.x));
            session.set("pos-y", Convert.ToString(position.y));
        }

    }
}
