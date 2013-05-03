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

        public static void WorldDataTest(int tyles = 256, int treeChaining = 2)
        {
            MySqlConnection connection = new MySqlConnection("Database=cubeworld;DataSource=localhost;UserId=root;Password=root");
            Models.TilesModel model = new Models.TilesModel(connection, tyles);
            Map.Map map = new Map.Map(model, tyles, treeChaining);

            Map.Rectangle coord = new Map.Rectangle(0, 0, 1), coords = new Map.Rectangle(0, 0, 1);
            Map.Block blockA, blockB;
            Trees.QuadTree.QuadTree<Map.Block> tree;

            int totalBlocks = (int)Math.Pow(tyles, treeChaining + 1);

            int window = 8, tests = 0, errors = 0;

            Map.Rectangle positionA = new Map.Rectangle(19, 8, 0);
            Map.Rectangle positionB = new Map.Rectangle(25, 8, 0);

            Map.Rectangle spaceA = new Map.Rectangle(positionA.x - window, positionA.y - window, 2 * window);
            Map.Rectangle spaceB = new Map.Rectangle(positionB.x - window, positionB.y - window, 2 * window);

            List<Map.Block> partsA = map.getIntersect(spaceA);
            List<Map.Block> partsB = map.getIntersect(spaceB);

            foreach (Map.Block pA in partsA)
            {
                foreach (Map.Block pB in partsB)
                {
                    if (pA.location.x == pB.location.x
                        && pA.location.y == pB.location.y)
                    {
                        tests++;

                        if (pA.val != pB.val)
                        {
                            //Console.Write("| Chyba: x={0}, y={1}", pA.location.x, pB.location.y);
                            errors++;
                        }
                    }
                }
            }

            Console.WriteLine("Testování světa o šířce {0} dlaždic.", totalBlocks);

            if (tests != 0)
                Console.WriteLine("Výsledek testu: {0}% ({1} ok, {2} bad)", (100 * (tests - errors)) / tests, tests - errors, errors);
            else
                Console.WriteLine("Neproběhl žádný test!");
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

            /*Program.WorldDataTest(16, 5);

            Console.ReadKey();

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

            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
            {
                Trees.QuadTree.QuadTree<Map.Block>.freeAllTree();
            };

            AppDomain.CurrentDomain.ProcessExit += ProcessExitHandler;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Server.Server server = new Server.Server();
            return 0;
        }

        static void ProcessExitHandler(object sender, EventArgs e)
        {
            Trees.QuadTree.QuadTree<Map.Block>.freeAllTree();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trees.QuadTree.QuadTree<Map.Block>.freeAllTree();
        }
    }
}
