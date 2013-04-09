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

    }
}
