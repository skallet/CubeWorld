﻿using System;
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
            cmd.CommandText = String.Format("SELECT *, X( `position` ) AS x, Y( `position` ) AS y FROM `players` WHERE `username` = '{0}'", username);
            cmd.Connection = connection;

            mutex.WaitOne();
            connection.Open();
            MySqlDataReader read = cmd.ExecuteReader();
            while (read.Read())
            {
                if (read.GetString("password") != encrypt(password, read.GetString("salt")))
                {
                    //throw auth exception
                    Debug.WriteLine("Password error");
                    Debug.WriteLine(encrypt(password, read.GetString("salt")));
                    read.Close();
                    connection.Close();
                    mutex.ReleaseMutex();
                    return null;
                }

                System.Collections.Hashtable data = new System.Collections.Hashtable();
                data.Add("username", read.GetString("username"));
                data.Add("id", read.GetInt32("id"));

                data.Add("x", read.GetInt32("x"));
                data.Add("y", read.GetInt32("y"));
                data.Add("mtime", read.GetString("mtime"));

                read.Close();
                connection.Close();
                mutex.ReleaseMutex();
                return data;
            }
            read.Close();
            connection.Close();
            mutex.ReleaseMutex();

            return null;
        }

        public void updateUserPosition(int id, Map.Rectangle position, String mtime)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = String.Format("UPDATE `players` SET `position` = POINT({0}, {1}), `mtime` = '{2}' WHERE `id`={3}", position.x, position.y, mtime, id);
            cmd.Connection = connection;

            mutex.WaitOne();
            connection.Open();

            cmd.ExecuteNonQuery();

            connection.Close();
            mutex.ReleaseMutex();
        }

        public List<System.Collections.Hashtable> getUsers(Map.Rectangle position = null)
        {
            List<System.Collections.Hashtable> list = new List<System.Collections.Hashtable>();
            System.Collections.Hashtable user;

            MySqlCommand cmd = new MySqlCommand();
            if (position != null)
            {
                cmd.CommandText = String.Format("SELECT `id` AS id, X( `position` ) AS x, Y( `position` ) AS y  FROM `players` WHERE INTERSECTS(`position`, {0}) LIMIT 10", getSearchPolygonString(position.x - 8, position.y - 8, 16));
            }
            else
            {
                cmd.CommandText = String.Format("SELECT `id` AS id, X( `position` ) AS x, Y( `position` ) AS y  FROM `players`");
            }
            cmd.Connection = connection;

            mutex.WaitOne();
            connection.Open();
            MySqlDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                user = new System.Collections.Hashtable();
                user.Add("id", read.GetInt32("id"));
                user.Add("x", read.GetInt32("x"));
                user.Add("y", read.GetInt32("y"));

                list.Add(user);
            }

            read.Close();
            connection.Close();
            mutex.ReleaseMutex();

            return list;
        }

    }
}
