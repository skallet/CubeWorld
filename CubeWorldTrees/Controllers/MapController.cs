﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

namespace CubeWorldTrees.Controllers
{
    class MapController : BaseController
    {

        protected Trees.QuadTree.QuadTree<Map.Block> quadTree;

        public MapController(HttpListenerContext Context, Trees.QuadTree.QuadTree<Map.Block> QuadTree)
            : base(Context)
        {
            quadTree = QuadTree;
        }

        public override void Render()
        {
            Uri url = context.Request.Url;
            
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("<script src='http://code.jquery.com/jquery-1.8.3.min.js'></script>");
            sb.Append("<script src='/content/js/main.js'></script>");
            sb.Append("</head>");

            sb.Append("<body>");
            sb.Append("</body>");
            sb.Append("</html>");

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
