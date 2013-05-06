using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Threading;

namespace CubeWorldTrees.Tests
{
    class BaseTest
    {

        MySqlConnection connection = new MySqlConnection("Database=cubeworld;DataSource=localhost;UserId=root;Password=root");
        public Models.TilesModel model;
        public Map.Map map;

        Stopwatch sw;
        public int tiles, chaining;

        public BaseTest(int Tiles, int Chaining)
        {
            tiles = Tiles;
            chaining = Chaining;

            model = new Models.TilesModel(connection, tiles);
            map = new Map.Map(model, tiles, chaining);
        }

        public virtual void Run()
        {
        }

        protected void StartTimer()
        {
            if (sw == null)
                sw = new Stopwatch();

            sw.Reset();
            sw.Start();
        }

        protected void StopTimer(String msg = "")
        {
            sw.Stop();
            Console.WriteLine("Test took {0} ms (Note: {1})", sw.ElapsedMilliseconds, msg);
        }

    }
}
