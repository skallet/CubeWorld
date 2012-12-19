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
        public Trees.QuadTree.QuadTree<Map.Block> quadTree;

        #endregion parameters

        #region constructors

        public Server()
        {
            int width = 256;
            Map.MapGenerator generator = new Map.MapGenerator(width, width);
            generator.Generate();
            Map.Rectangle space = new Map.Rectangle(0, 0, width);
            Map.Block block;

            quadTree = new Trees.QuadTree.QuadTree<Map.Block>(space);
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        block = generator.GetBlock(x, y);
                        if (block != null)
                            quadTree.Insert(block);
                        else
                            Console.WriteLine("\tBLock not found!");
                    }
                }
            }

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
