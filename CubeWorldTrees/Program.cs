using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Web;
using MySql.Data.MySqlClient;

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
            Trees.QuadTree.QuadTree<Map.Block> quadTree = Trees.QuadTree.QuadTree<Map.Block>.getFreeTree(space, 0);
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

            Trees.QuadTree.QuadTree<Map.Block> tree = Trees.QuadTree.QuadTree<Map.Block>.getFreeTree(space, 0);

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

            Trees.QuadTree.QuadTree<Map.Block> world = Trees.QuadTree.QuadTree<Map.Block>.getFreeTree(space, 0);

            sw.Stop();
            init = sw.ElapsedMilliseconds;
            sw.Start();

            worldx = 10;
            worldy = 10;

            part = Trees.QuadTree.QuadTree<Map.Block>.getFreeTree(space, 0);
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

            part = Trees.QuadTree.QuadTree<Map.Block>.getFreeTree(space, 0);
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

        public static int TotalInserted = 0;

        public static Mutex mt = new Mutex(false, "console");

        public static void WorldTest(int tyles = 16, int treeChaining = 2)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            MySqlConnection connection = new MySqlConnection("Database=cubeworld;DataSource=localhost;UserId=root;Password=root");
            Models.TilesModel model = new Models.TilesModel(connection, tyles);
            Map.Map map = new Map.Map(model, tyles, treeChaining);

            Map.Rectangle coord = new Map.Rectangle(0, 0, 1), coords = new Map.Rectangle(0, 0, 1);
            Map.Block blockA, blockB;
            Trees.QuadTree.QuadTree<Map.Block> tree;

            int totalBlocks = (int)Math.Pow(tyles, treeChaining + 1) * (int)Math.Pow(tyles, treeChaining + 1);
            
            /*
            for (int i = 0; i < 16; i++)
            {
                coord.x = tyles * i;
                block = map.getBlock(coord);
                Console.WriteLine("Block X: {0}, Y: {1}, Value: {2}.", block.location.x, block.location.y, block.val);
            }
             */

            int test = 0, failsTiles = 0, failsValues = 0, valuesTest = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int x = 0; x < (int)Math.Pow(tyles, treeChaining); x += tyles)
                {
                    for (int y = 0; y < (int)Math.Pow(tyles, treeChaining); y += tyles)
                    {
                        test++;
                        //Console.WriteLine("{0}. iteration, Tree {1}, {2}", tyles * x + y, x, y);
                        coord.x = x;
                        coord.y = y;

                        tree = map.getTree(coord);
                        Trees.QuadTree.QuadTree<Map.Block> dbtree = model.loadTree(new Map.Rectangle(coord.x, coord.y, tyles));

                        for (int ix = 0; ix < tyles; ix++)
                        {
                            for (int iy = 0; iy < tyles; iy++)
                            {
                                valuesTest++;
                                coords.x = ix + coord.x * tyles;
                                coords.y = iy + coord.y * tyles;

                                //Console.WriteLine("Try find {0} {1} {2}", coords.x, coords.y, dbtree.DumpCount());

                                blockA = tree.Get(coords);
                                blockB = dbtree.Get(coords);

                                //Console.WriteLine("Found A:{0}, B:{1}", blockA != null, blockB != null);

                                if (blockA != null && blockB != null)
                                {
                                    if (blockA.val != blockB.val)
                                    {
                                        failsValues++;
                                    }
                                }
                                else if (blockA == null ^ blockB == null)
                                {
                                    failsValues++;
                                }

                                //System.Threading.Thread.Sleep(1000);
                            }
                        }

                        //Console.WriteLine("Test: {0} valid: {1}", tree.DumpCount(), tree.DumpCount() == tyles * tyles);

                        if (tree.DumpCount() != tyles * tyles)
                        {
                            //Console.WriteLine("Test: {0} valid: {1}; x {2} y {3}", tree.DumpCount(), tree.DumpCount() == tyles * tyles, x, y);
                            tree.Dump();
                            dbtree.Dump();
                            failsTiles += 1;
                            //Console.ReadKey();
                        }

                        Trees.QuadTree.QuadTree<Map.Block>.unsetAllTree();
                    }
                }
            }

            sw.Stop();

            mt.WaitOne();
            Console.WriteLine("Očekávaný počet mapových bloků {0}.", totalBlocks);
            Console.WriteLine("Hodnoceni počtu dlaždic: {0}% ({1} ok, {2} bad)", (100 * (test - failsTiles)) / test, test - failsTiles, failsTiles);
            Console.WriteLine("Hodnoceni hodnot bloků : {0}% ({1} ok, {2} bad)", (100 * (valuesTest - failsValues)) / valuesTest, valuesTest - failsValues, failsValues);
            Console.WriteLine("Celkem uloženo bloků {0}", Program.TotalInserted);
            Console.WriteLine("Test zabral {0} s", sw.ElapsedMilliseconds / 1000);
            mt.ReleaseMutex();
        }

        public static void WorldTestBlock(int tyles = 16, int treeChaining = 2)
        {
            MySqlConnection connection = new MySqlConnection("Database=cubeworld;DataSource=localhost;UserId=root;Password=root");
            Models.TilesModel model = new Models.TilesModel(connection, tyles);
            Map.Map map = new Map.Map(model, tyles, treeChaining);

            Map.Rectangle coord = new Map.Rectangle(0, 0, 1), coords = new Map.Rectangle(0, 0, 1);
            Map.Block blockA, blockB;
            Trees.QuadTree.QuadTree<Map.Block> tree;

            int totalBlocks = (int)Math.Pow(tyles, treeChaining + 1) * (int)Math.Pow(tyles, treeChaining + 1);

            List<Map.Block> list, list2;
            int errors = 0, tests = 0;

            tree = map.getTree(new Map.Rectangle(0, 0, 16));


            return;

            for (int i = 1; i < 15; i++)
            {
                list = map.getIntersect(new Map.Rectangle(8, 0, 16));
                list2 = map.getIntersect(new Map.Rectangle(i, 0, 16));

                foreach (Map.Block b in list)
                {
                    foreach (Map.Block b2 in list2)
                    {
                        if (b.location.x == b2.location.x)
                        {
                            if (b.location.y == b2.location.y)
                            {
                                tests++;
                                if (b.val != b2.val)
                                {
                                    errors++;
                                }
                            }
                        }
                    }
                }

                //System.Threading.Thread.Sleep(1000);
            }

            Console.WriteLine("Hodnoceni počtu dlaždic: {0}% ({1} ok, {2} bad)", (100 * (tests - errors)) / tests, tests - errors, errors);
        }

        public static void WorldSizeTest(int tyles = 256, int trees = 10)
        {
            Map.Rectangle coord = new Map.Rectangle(0, 0, 1);
            Map.Block block;

            long GC_MemStart = System.GC.GetTotalMemory(true);

            MySqlConnection connection = new MySqlConnection("Database=cubeworld;DataSource=localhost;UserId=root;Password=root");
            Map.Map map = new Map.Map(new Models.TilesModel(connection, tyles), tyles);

            for (int i = 0; i < trees; i++)
            {
                coord.x = tyles * i;
                block = map.getBlock(coord);
            }

            long GC_MemEnd = System.GC.GetTotalMemory(true);

            Console.WriteLine("{0} trees, {1}mb", trees, (GC_MemEnd - GC_MemStart) / 8000000);
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
            Program.WorldTestBlock(16, 2);
            Console.ReadKey();
             */

            /* MultiThreading world test
            var numThreads = 10;
            var countdownEvent = new CountdownEvent(numThreads);

            // Start workers.
            for (var i = 0; i < numThreads; i++)
            {
                new Thread(delegate()
                {
                    Program.WorldTest(16, 1);
                    // Signal the CountdownEvent.
                    countdownEvent.Signal();
                }).Start();
            }

            // Wait for workers.
            countdownEvent.Wait();
            Console.WriteLine("Finished.");
            Console.ReadKey();
             * */



            /* World size testy
            for (int i = 10; i < 11; i ++)
            {
                Program.WorldSizeTest(256, 10 * i);
                Console.WriteLine("Finish {0}", i * 10);
                //Console.ReadKey();
            }

            Console.WriteLine("Finish all");
            Console.ReadKey();
             */
            
            
            Server.Server server = new Server.Server();
            return 0;
        }
    }
}
