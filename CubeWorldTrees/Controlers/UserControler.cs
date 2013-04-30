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
                DateTime dt = DateTime.Parse(userData["mtime"].ToString());
                session.set("last-move", dt.ToString("yyyyMMddHHmmssfff"));

                session.forceValidFile();

                int x = Convert.ToInt32(session.get("pos-x"));
                int y = Convert.ToInt32(session.get("pos-y"));
            }
        }

        public int getId()
        {
            return Int32.Parse(session.get("id").ToString());
        }

        public Map.Rectangle getPosition()
        {
            int x = Convert.ToInt32(session.get("pos-x"));
            int y = Convert.ToInt32(session.get("pos-y"));

            if (x < 0 || y < 0)
            {
                session.set("pos-x", "5");
                session.set("pos-y", "5");
            }

            return new Map.Rectangle(x, y, 1);
        }

        public String getLastMoveTimestamp()
        {
            String stamp = session.get("last-move");

            if (stamp == null)
            {
                stamp = "0";
            }

            return stamp;
        }

        public String setPosition(Map.Rectangle position)
        {
            DateTime now = DateTime.Now;
            String stamp = now.ToString("yyyyMMddHHmmssfff");

            int id = Convert.ToInt32(session.get("id"));
            model.updateUserPosition(id, position, now.ToString("yyyy-MM-dd HH:mm:ss"));

            session.set("last-move", stamp);
            session.set("pos-x", Convert.ToString(position.x));
            session.set("pos-y", Convert.ToString(position.y));

            return stamp;
        }

    }
}
