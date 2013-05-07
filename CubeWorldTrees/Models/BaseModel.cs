using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading;

namespace CubeWorldTrees.Models
{
    class BaseModel
    {

        protected MySqlConnection connection;

        protected Mutex mutex = new Mutex(false, "databaseMutex");

        public BaseModel(MySqlConnection Connection)
        {
            connection = Connection;
        }

        protected String getSearchPolygonString(int x, int y, int width)
        {
            return String.Format("PolyFromText('Polygon(({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))')", x + 1, y + 1, x + width - 1, y + width - 1);
        }

        protected String getPolygonString(int x, int y, int width)
        {
            return String.Format("PolyFromText('Polygon(({0} {1}, {2} {1}, {2} {3}, {0} {3}, {0} {1}))')", x, y, x + width, y + width);
        }

    }
}
