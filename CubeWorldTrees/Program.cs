using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Web;

namespace CubeWorldTrees
{
    class Program
    {

        public static void mapGenerator(int width)
        {
            Stopwatch sw = new Stopwatch();

            Console.WriteLine("Initializing map generator {0:D} x {0:D}...", width, width);
            Map.MapGenerator generator = new Map.MapGenerator(width, width);
            generator.Generate();
            Console.WriteLine("Map generated!");

            Map.Rectangle space = new Map.Rectangle(0, 0, width);
            Map.Block block;
            Map.Rectangle dot = new Map.Rectangle(0, 0, 1);

            Console.WriteLine("QuadTree - initialize & saving map data");
            sw.Start();
            Trees.QuadTree.QuadTree<Map.Block> quadTree = new Trees.QuadTree.QuadTree<Map.Block>(space);
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
            sw.Stop();
            Console.WriteLine("\tElapsed = {0}", sw.Elapsed);
            sw.Reset();
            Console.WriteLine("QuadTree - reading all");
            sw.Start();
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        dot.x = x;
                        dot.y = y;
                        block = quadTree.Get(dot);
                        if (block != null)
                        {
                            if (block.val != x * y)
                            {
                                Console.WriteLine("\tError while getting [{0:D}, {0:D}]!", x, y);
                            }
                        }
                        else
                        {
                            Console.WriteLine("\tNot found");
                        }
                    }
                }
            }
            sw.Stop();
            Console.WriteLine("\tElapsed = {0}", sw.Elapsed);
            sw.Reset();
            Console.WriteLine("QuadTree - reading one");
            sw.Start();
            {
                Random rand = new Random();
                int xs = rand.Next(0, width);
                int ys = rand.Next(0, width);
                dot.x = xs;
                dot.y = ys;
                block = quadTree.Get(dot);
                if (block != null)
                {
                    if (block.val != xs * ys)
                    {
                        Console.WriteLine("\tError while getting [{0:D}, {0:D}]!", xs, ys);
                    }
                }
                else
                {
                    Console.WriteLine("\tNot found");
                }
            }
            sw.Stop();
            Console.WriteLine("\tElapsed = {0}", sw.Elapsed);
            sw.Reset();
            Console.WriteLine("QuadTree - reading part");
            sw.Start();
            {
                dot.x = 5;
                dot.y = 5;
                Map.Rectangle partZero;
                int[,] parts = quadTree.GetSpace(dot, 2, out partZero);
                if (parts != null)
                {
                    for (int x = 0; x < parts.GetLength(0); x++)
                    {
                        for (int y = 0; y < parts.GetLength(1); y++)
                        {
                            Console.WriteLine("Part[{0:D}, {1:D}] = {2:D}", partZero.x + x, partZero.y + y, parts[x, y]);
                        }
                    }
                }
            }
            sw.Stop();
            Console.WriteLine("\tElapsed = {0}", sw.Elapsed);
            sw.Reset();
            Console.WriteLine("QuadTree - completed");
            Console.WriteLine("====================");
            Console.WriteLine("Finish!");
            Console.ReadKey();
        }

        public static void testSession()
        {
            Server.Session session = new Server.Session("test");
            session.dump();

            Console.WriteLine("Getting name: " + session.get("name"));

            session.set("name", "Test");
            session.dump();

            session.unset();
            session.dump();
        }

        static int Main(string[] args)
        {
            Server.Server server = new Server.Server();
            return 0;
        }
    }
}
