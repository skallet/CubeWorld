using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CubeWorldTrees.Controlers
{
    class JsonControler : BaseControler
    {

        protected Map.Map world;

        public JsonControler(HttpListenerContext Context, Map.Map World)
            : base(Context)
        {
            world = World;
            renderTemplate = false;
        }

        public override void Render()
        {
            base.Render();

            if (!user.isLoggedIn())
                return;

            StringBuilder sb = new StringBuilder();

            string absolutePath = context.Request.Url.AbsolutePath;

            sb.Append("{");
            if (absolutePath == "/initialize")
            {
                int window = 6;
                Map.Rectangle position = user.getPosition();
                Map.Rectangle space = new Map.Rectangle(position.x - window, position.y - window, 2 * window);
                List<Map.Block> parts = world.getIntersect(space);
                int counter = 0;

                

                sb.Append("\"position\": {\"x\": " + position.x + ", \"y\": " + position.y + "}");
                if (parts.Count > 0)
                {
                    sb.Append(",");
                }

                foreach (Map.Block part in parts)
                {
                    counter++;
                    sb.Append("\"block" + counter + "\": {\"x\": " + (part.location.x) + ", \"y\": " + (part.location.y) + ", \"value\": \"" + (part.val) + ".png\"}");
                    if (counter < parts.Count)
                    {
                        sb.Append(",");
                    }
                }
            }
            else if (absolutePath == "/position")
            {
                if (context.Request.QueryString.GetKey(0) == "x"
                    && context.Request.QueryString.GetKey(1) == "y")
                {
                    user.setPosition(new Map.Rectangle(Convert.ToInt32(context.Request.QueryString.Get(0)), Convert.ToInt32(context.Request.QueryString.Get(1)), 1));
                    sb.Append("\"status\": \"ok\"");
                }
                else
                {
                    sb.Append("\"status\": \"error\"");
                }
            }
            sb.Append("}");           

            sendJsonResponse(sb.ToString());
        }

    }
}
