using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using MySql.Data.MySqlClient;

namespace CubeWorldTrees.Server
{
    class Server
    {
        #region parameters

        public static MySqlConnection connection;

        public Models.UserModel users;

        public static HttpListener listener = new HttpListener();
        public Map.Map world;

        #endregion parameters

        #region constructors

        public Server()
        {
            int width = 256;
            world = new Map.Map(width);

            listener.Prefixes.Add("http://*:40000/");
            listener.Start();

            Server.connection = new MySqlConnection("Database=cubeworld;DataSource=localhost;UserId=root;Password=root");

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
