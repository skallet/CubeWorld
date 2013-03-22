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

        public static void treeTest(int width)
        {
            
            Map.Rectangle space = new Map.Rectangle(0, 0, width);
            Map.Rectangle location;
            Map.Block block;

            Map.Rectangle partZero;
            Map.Rectangle dot = new Map.Rectangle(0, 0, 1);
            int[,] parts;
            long init, insert, readOne, readSpace, completed;

            Console.WriteLine("Tree test 1: {0}", width);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Trees.QuadTree.QuadTree<Map.Block> tree = new Trees.QuadTree.QuadTree<Map.Block>(space);

            sw.Stop();
            init = sw.ElapsedMilliseconds;
            sw.Start();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    location = new Map.Rectangle(x, y, 1);
                    block = new Map.Block(0, location);
                    tree.Insert(block);
                }
            }

            sw.Stop();
            insert = sw.ElapsedMilliseconds;
            sw.Start();

            location = new Map.Rectangle(10, 10, 1);
            tree.Get(location);

            sw.Stop();
            readOne = sw.ElapsedMilliseconds;
            sw.Start();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    location = new Map.Rectangle(x, y, 1);
                    block = tree.Get(location);
                }
            }

            sw.Stop();
            readSpace = sw.ElapsedMilliseconds;
            sw.Start();

            parts = tree.GetSpace(dot, 4, out partZero);


            sw.Stop();
            completed = sw.ElapsedMilliseconds;
            sw.Start();

            System.IO.StreamWriter file = System.IO.File.CreateText(@"C:\Users\Milan\Dokumenty\BI-BP\tests\test1-" + width + ".txt");
            file.WriteLine("== Test completed for {0}-width space", width);
            file.WriteLine("= Part 1. Initializing: {0}", init);
            file.WriteLine("= Part 2. Full inserting: {0}", insert - init);
            file.WriteLine("= Part 3. Reading one element: {0}", readOne - insert);
            file.WriteLine("= Part 3. Reading space (16x16): {0}", readSpace - readOne);
            file.WriteLine("= Part 3. Full reading: {0}", completed - readSpace);
            file.WriteLine("== Overall: {0}", completed);
            file.WriteLine("");
            file.Close();
        }

        public static void treeTest2(int width)
        {

            Map.Rectangle space = new Map.Rectangle(0, 0, width);
            Map.Rectangle location;
            Map.Block block;

            Map.Rectangle partZero;
            Map.Rectangle dot = new Map.Rectangle(0, 0, 1);
            int[,] parts;
            int worldx, worldy;
            long init, insert, readOne, readSpace, completed;
            Trees.QuadTree.QuadTree<Map.Block> part;

            Console.WriteLine("Tree test 2: {0}", width);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Trees.QuadTree.QuadTree<Map.Block> world = new Trees.QuadTree.QuadTree<Map.Block>(space);

            sw.Stop();
            init = sw.ElapsedMilliseconds;
            sw.Start();

            worldx = 10;
            worldy = 10;

            part = new Trees.QuadTree.QuadTree<Map.Block>(space);
            location = new Map.Rectangle(worldx, worldy, 1);
            block = new Map.Block(0, location);
            block.tree = part;
            world.Insert(block);
            part = world.Get(location).tree;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    location = new Map.Rectangle(x, y, 1);
                    block = new Map.Block(x + y, location);
                    part.Insert(block);
                }
            }

            sw.Stop();
            insert = sw.ElapsedMilliseconds;
            sw.Start();

            worldx = 15;
            worldy = 15;

            part = new Trees.QuadTree.QuadTree<Map.Block>(space);
            location = new Map.Rectangle(worldx, worldy, 1);
            block = new Map.Block(0, location);
            block.tree = part;
            world.Insert(block);
            part = world.Get(location).tree;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    location = new Map.Rectangle(x, y, 1);
                    block = new Map.Block(x + y, location);
                    part.Insert(block);
                }
            }

            location = new Map.Rectangle(worldx, worldy, 1);
            part = world.Get(location).tree;

            sw.Stop();
            readOne = sw.ElapsedMilliseconds;
            sw.Start();

            worldx = 15;
            worldy = 15;

            location = new Map.Rectangle(worldx, worldy, 1);

            part = world.Get(location).tree;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    location = new Map.Rectangle(x, y, 1);
                    block = part.Get(location);
                }
            }

            sw.Stop();
            readSpace = sw.ElapsedMilliseconds;
            sw.Start();

            location = new Map.Rectangle(10, 10, 1);
            part = world.Get(location).tree;
            parts = part.GetSpace(dot, 4, out partZero);


            sw.Stop();
            completed = sw.ElapsedMilliseconds;
            sw.Start();

            System.IO.StreamWriter file = System.IO.File.CreateText(@"C:\Users\Milan\Dokumenty\BI-BP\tests\test2-" + width + ".txt");
            file.WriteLine("== Test completed for {0}-width space (total: {0} blocks width)", width, Math.Pow(2, (double)width));
            file.WriteLine("= Part 1. Initializing: {0}", init);
            file.WriteLine("= Part 2. Creating new world part: {0}", insert - init);
            file.WriteLine("= Part 3. Reading new element: {0}", readOne - insert);
            file.WriteLine("= Part 3. Reading existing element: {0}", readSpace - readOne);
            file.WriteLine("= Part 3. Reading space (16x16) in existing element: {0}", completed - readSpace);
            file.WriteLine("== Overall: {0}", completed);
            file.WriteLine("");
            file.Close();
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

        public static void WorldTest(int tyles = 256)
        {
            Map.Map map = new Map.Map(tyles);

            Map.Rectangle coord = new Map.Rectangle(0, 0, 1);
            Map.Block block;
            Trees.QuadTree.QuadTree<Map.Block> tree;
            /*
            for (int i = 0; i < 10; i++)
            {
                coord.x = tyles * i;
                block = map.getBlock(coord);
                Console.WriteLine("Block X: {0}, Y: {1}, Value: {2}.", block.location.x, block.location.y, block.val);
            }

            for (int i = 0; i < 10; i++)
            {
                coord.x = i;
                tree = map.getTree(coord);
            }*/

            coord.x = 15;
            coord.y = 0;
            coord.width = 25;
            List<Map.Block> list = map.getIntersect(coord);
            Console.WriteLine("Retriev {0} map parts", list.Count);
            
        }

        static int Main(string[] args)
        {
            /* TESTY
            //Server.Server server = new Server.Server();
            double width = 0;
            for (int i = 4; i < 12; i++)
            {
                width = Math.Pow((double)2, (double)i);

                Program.treeTest((int)width);
                Program.treeTest2((int)width);
            }

            Console.WriteLine("press key to exit");
            Console.ReadKey();
             */

            /* World testy + implementace
            Program.WorldTest();
            Console.ReadKey();
             */
            
            Server.Server server = new Server.Server();
            return 0;
        }
    }
}
