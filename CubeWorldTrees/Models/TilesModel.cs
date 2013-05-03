using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading;

namespace CubeWorldTrees.Models
{
    class TilesModel : BaseModel
    {

        protected int tiles;

        public TilesModel(MySqlConnection Connection, int Tiles)
            : base(Connection)
        {
            tiles = Tiles;
        }

        protected String getSearchPolygonString(int x, int y, int width)
        {
            return String.Format("PolyFromText('Polygon(({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))')", x + 1, y + 1, x + width - 1, y + width - 1);
        }

        protected String getPolygonString(int x, int y, int width)
        {
            return String.Format("PolyFromText('Polygon(({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))')", x, y, x + width, y + width);
        }

        public Boolean treeExist(int x, int y)
        {
            Boolean found = false;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = String.Format("SELECT COUNT(*) AS count FROM `tiles` WHERE INTERSECTS(coord, {0})", getSearchPolygonString(x, y, tiles));
            cmd.Connection = connection;

            mutex.WaitOne();
            connection.Open();
            MySqlDataReader read = cmd.ExecuteReader();

            if (read.Read())
            {
                //Console.WriteLine("Realy {0}", read.GetInt32("count"));
                if (read.GetInt32("count") >= Math.Pow((double)tiles, 2.0))
                {
                    found = true;
                }
            }

            read.Close();
            connection.Close();
            mutex.ReleaseMutex();

            return found;
        }

        public void updateTile(int x, int y, int owner, int value)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = String.Format("UPDATE `tiles` SET `image` = {0}, `owner` = {1} WHERE `coord` = {2}", value, owner, getPolygonString(x, y, 1));
            cmd.Connection = connection;

            mutex.WaitOne();
            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();
            mutex.ReleaseMutex();
        }

        public void insertTree(Trees.QuadTree.QuadTree<Map.Block> tree, Map.Rectangle location)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = String.Format("INSERT INTO `tiles` (`coord`, `image`) VALUES");
            cmd.Connection = connection;
            int count = 0;

            for (int x = 0; x < tiles; x++)
            {
                for (int y = 0; y < tiles; y++)
                {
                    count++;
                    Map.Block block = tree.Get(new Map.Rectangle(x, y, 1));
                    cmd.CommandText += String.Format("({0}, {1})", getPolygonString(x + location.x, y + location.y, block.location.width), block.val);

                    if (x != tiles - 1 || y != tiles - 1)
                    {
                        cmd.CommandText += ",";
                    }
                }
            }

            Program.TotalInserted += count;

            mutex.WaitOne();
            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();
            mutex.ReleaseMutex();

            //Console.WriteLine("Inserted {0}", count);
            //treeExist(location.x, location.y);
        }

        public Trees.QuadTree.QuadTree<Map.Block> loadTree(Map.Rectangle coord)
        {
            Trees.QuadTree.QuadTree<Map.Block> tree = Trees.QuadTree.QuadTree<Map.Block>.getFreeTree(new Map.Rectangle(coord.x, coord.y, coord.width), 0);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = String.Format("SELECT `image` AS value, X( POINTN( EXTERIORRING(  `coord` ) , 1 ) ) AS x, Y( POINTN( EXTERIORRING(  `coord` ) , 1 ) ) AS y, owner FROM `tiles` WHERE INTERSECTS(`coord`, {0})", getSearchPolygonString(coord.x * coord.width, coord.y * coord.width, coord.width));
            cmd.Connection = connection;

            mutex.WaitOne();
            connection.Open();
            MySqlDataReader read = cmd.ExecuteReader();

            int x, y, value, player, count = 0;
            Map.Block block = null;

            while (read.Read())
            {
                count++;
                x = read.GetInt32("x") % coord.width;
                y = read.GetInt32("y") % coord.width;
                value = read.GetInt32("value");
                player = read.GetInt32("owner");

                block = new Map.Block(value, new Map.Rectangle(x, y, 1));
                block.player = player;

                tree.Insert(block);
            }

            read.Close();
            connection.Close();

            /*
            cmd.CommandText = String.Format("SELECT `id` AS id, X( `position` ) AS x, Y( `position` ) AS y  FROM `players` WHERE INTERSECTS(`position`, {0})", getSearchPolygonString(coord.x * coord.width, coord.y * coord.width, coord.width));
            connection.Open();
            read = cmd.ExecuteReader();
            Map.Block b;

            while (read.Read())
            {
                b = tree.Get(new Map.Rectangle(read.GetInt32("x"), read.GetInt32("y"), 1));
                if (b != null)
                    b.player = read.GetInt32("id");
            }

            connection.Close();
            */
              
            mutex.ReleaseMutex();

            return tree;
        }

    }
}
