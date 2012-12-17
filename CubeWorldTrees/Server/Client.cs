using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace CubeWorldTrees.Server
{
    class Client
    {

        #region parameters

        HttpListenerContext context;
        Server server;

        #endregion parameters

        #region constructs

        public Client(Server Server, HttpListenerContext Context)
        {
            context = Context;
            server = Server;
        }

        #endregion constructs

        #region request

        public void processRequest()
        {
            Console.WriteLine("> Client access from {0}", context.Request.RemoteEndPoint.Address);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string msg = "Welcome!";

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><body><h1>" + msg + "</h1>");
            sb.Append("</body></html>");
  
            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            context.Response.ContentLength64 = b.Length;
            context.Response.OutputStream.Write(b, 0, b.Length);
            context.Response.OutputStream.Close();

            sw.Stop();
            Console.WriteLine("> Request from {0} was completed for {1} ms", context.Request.RemoteEndPoint.Address, sw.ElapsedMilliseconds);
        }

        #endregion request

    }
}
