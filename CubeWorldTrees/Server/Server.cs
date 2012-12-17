using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace CubeWorldTrees.Server
{
    class Server
    {
        #region parameters

        public static HttpListener listener = new HttpListener();

        #endregion parameters

        #region constructors

        public Server()
        {
            listener.Prefixes.Add("http://*:40000/");
            listener.Start();

            Console.WriteLine("> Starting server ...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                new Thread(new Client(this, context).processRequest).Start();
            }
        }

        #endregion constructors

    }
}
