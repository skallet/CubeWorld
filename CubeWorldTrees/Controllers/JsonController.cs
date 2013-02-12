using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CubeWorldTrees.Controllers
{
    class JsonController : BaseController
    {

        protected Trees.QuadTree.QuadTree<Map.Block> quadTree;

        public JsonController(HttpListenerContext Context, Trees.QuadTree.QuadTree<Map.Block> QuadTree)
            : base(Context)
        {
            quadTree = QuadTree;
        }

        public override void Render()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{");
            Map.Rectangle partZero;
            Map.Rectangle dot = new Map.Rectangle(0, 0, 1);
            int[,] parts = quadTree.GetSpace(dot, 3, out partZero);
            int counter = 0;
            if (parts != null)
            {
                for (int x = 0; x < parts.GetLength(0); x++)
                {
                    for (int y = 0; y < parts.GetLength(1); y++)
                    {
                        counter++;
                        Random rand = new Random();
                        sb.Append("\"block" + counter + "\": {\"x\": " + (partZero.x + x) + ", \"y\": " + (partZero.y + y) + ", \"value\": \"" + parts[x, y] + ".png\"}");
                        if (counter < parts.Length)
                        {
                            sb.Append(",");
                        }
                    }
                }
            }
            sb.Append("}");

            context.Response.ContentType = "application/json";
            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            context.Response.ContentLength64 = b.Length;

            try
            {
                context.Response.OutputStream.Write(b, 0, b.Length);
                context.Response.OutputStream.Close();
            }
            catch { }
        }

    }
}
