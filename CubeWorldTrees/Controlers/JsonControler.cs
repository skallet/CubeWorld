using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CubeWorldTrees.Controlers
{
    class JsonControler : BaseControler
    {

        public JsonControler(HttpListenerContext Context, Map.Map World)
            : base(Context, World)
        {
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
                int window = 8;
                Map.Rectangle position = user.getPosition();
                Map.Rectangle space = new Map.Rectangle(position.x - window, position.y - window, 2 * window);
                List<Map.Block> parts = world.getIntersect(space);
                int counter = 0;

                sb.Append("\"position\": {\"x\": " + position.x + ", \"y\": " + position.y + ", \"utime\": " + (user.getLastMoveTimestamp()) + ", \"id\": " + (user.getId()) + "}");

                List<System.Collections.Hashtable> users = user.model.getUsers(position);
                if (users.Count() > 0)
                {
                    sb.Append(",");

                    foreach (System.Collections.Hashtable u in users)
                    {
                        counter++;
                        sb.Append("\"user" + counter + "\": {\"x\": " + (u["x"]) + ", \"y\": " + (u["y"]) + ", \"id\": \"" + (u["id"]) + "\"}");
                    }
                }

                counter = 0;
                if (parts.Count() > 0)
                {
                    sb.Append(",");
                }

                foreach (Map.Block part in parts)
                {
                    counter++;
                    sb.Append("\"block" + counter +
                        "\": {\"x\": " + (part.location.x) +
                        ", \"y\": " + (part.location.y) +
                        ", \"value\": \"" + (part.val) +
                        ".png\", \"open\": " + (part.isSolid() ? "0" : "1") +
                        ", \"owner\": " + (part.player) +
                        "}");

                    if (counter < parts.Count)
                    {
                        sb.Append(",");
                    }
                }
            }
            else if (absolutePath == "/position")
            {
                if (context.Request.QueryString.Count == 2
                    && context.Request.QueryString.GetKey(0) == "x"
                    && context.Request.QueryString.GetKey(1) == "y")
                {
                    int x = Convert.ToInt32(context.Request.QueryString.Get(0));
                    int y = Convert.ToInt32(context.Request.QueryString.Get(1));

                    Map.Rectangle pos = new Map.Rectangle(x, y, 1);
                    Trees.QuadTree.QuadTree<Map.Block> tree = world.getIntersectTree(pos);

                    if (tree != null)
                    {
                        Map.Block moveBlock = tree.Get(pos);

                        if (moveBlock != null && !moveBlock.isSolid())
                        {
                            UInt64 lm = UInt64.Parse(user.getLastMoveTimestamp());
                            UInt64 now = UInt64.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));

                            if ((now - lm) >= 100)
                            {
                                Map.Rectangle position = user.getPosition();

                                if (Math.Abs(x - position.x) <= 2
                                    && Math.Abs(y - position.y) <= 2)
                                {
                                    String ut = user.setPosition(pos);
                                    position = user.getPosition();

                                    sb.Append("\"status\": \"ok\", \"utime\": " + ut + ", \"x\": " + position.x + ", \"y\": " + position.y);
                                }
                                else
                                {
                                    sb.Append("\"status\": \"error\", \"msg\": \"Try to move too far from previous position!\"");
                                }
                            }
                            else
                            {
                                sb.Append("\"status\": \"error\", \"msg\": \"Move before time limit!\"");
                            }
                        }
                        else
                        {
                            sb.Append("\"status\": \"error\", \"msg\": \"This block is solid!\"");
                        }
                    }
                    else
                    {
                        sb.Append("\"status\": \"error\", \"msg\": \"Out of range!\"");
                    }
                }
                else
                {
                    sb.Append("\"status\": \"error\"");
                }
            }
            else if (absolutePath == "/update")
            {
                Map.Rectangle pos = user.getPosition();
                Trees.QuadTree.QuadTree<Map.Block> tree = world.getIntersectTree(pos);

                if (tree != null)
                {
                    Map.Block newBlock = new Map.Block(1, pos);

                    if (tree.Update(newBlock, user.getId(), 2))
                    {
                        Map.Block updated = tree.Get(pos);
                        sb.Append(
                            "\"status\": \"ok\"" +
                            ",\"x\": " + (pos.x) +
                            ", \"y\": " + (pos.y) +
                            ", \"value\": \"" + (updated.val) +
                            ".png\", \"open\": " + (updated.isSolid() ? "0" : "1") +
                            ", \"owner\": " + (updated.player)
                        );
                    }
                    else
                    {
                        sb.Append("\"status\": \"error\", \"msg\": \"Can't update block!\"");
                    }
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
