using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace CubeWorldTrees.Models
{
    class UserModel : BaseModel
    {

        public UserModel(MySqlConnection Connection)
            : base(Connection)
        {

        }

        public string encrypt(string password, string salt)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(password + salt);
            HashAlgorithm algorithm = SHA256.Create();

            return BitConverter.ToString(algorithm.ComputeHash(inputBytes));
        }

        public System.Collections.Hashtable login(string username, string password)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = String.Format("SELECT * FROM `players` WHERE `username` = '{0}'", username);
            cmd.Connection = connection;

            connection.Open();
            MySqlDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                if (read.GetString("password") != encrypt(password, read.GetString("salt")))
                {
                    //throw auth exception
                    Debug.WriteLine("Password error");
                    Debug.WriteLine(encrypt(password, read.GetString("salt")));
                    connection.Close();
                    return null;
                }

                System.Collections.Hashtable data = new System.Collections.Hashtable();
                data.Add("username", read.GetString("username"));

                connection.Close();
                return data;
            }
            read.Close();
            connection.Close();

            return null;
        }

    }
}
