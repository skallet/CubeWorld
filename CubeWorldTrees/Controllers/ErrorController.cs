using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CubeWorldTrees.Controllers
{
    class ErrorController : BaseController
    {

        public ErrorController(HttpListenerContext Context)
            : base(Context)
        {
        }

        public override void Render()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("</head>");

            sb.Append("<body>");
            sb.Append("<h1>Chyba! Stránka nenalezena</h1>");
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
