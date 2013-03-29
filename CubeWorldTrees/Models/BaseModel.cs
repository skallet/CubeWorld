using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace CubeWorldTrees.Models
{
    class BaseModel
    {

        protected MySqlConnection connection;

        public BaseModel(MySqlConnection Connection)
        {
            connection = Connection;
        }

    }
}
