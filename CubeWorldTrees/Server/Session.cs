using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace CubeWorldTrees.Server
{
    class Session
    {

        #region Constants

        public const string SAVE_PATH = @"./content/temp/sessions/";

        public const string SESSION_EXPIRATION = "session_expiration";

        public const int DEFAULT_SESSION_EXPIRATION = 2 * 24;

        #endregion Constants

        #region parameters

        protected string key;

        protected bool valid = true;

        protected System.Collections.Hashtable data = new System.Collections.Hashtable();

        #endregion parameters

        #region Getters and Setters

        #endregion Getters and Setters

        #region constructors

        public Session(string Key)
        {
            key = Key;
            readData();
        }

        ~Session()
        {
            updateData();
        }

        #endregion constructors

        #region debug

        public void dump()
        {
            Console.WriteLine("== Dumping session " + key);
            Console.WriteLine("Valid: {0}", this.isValid());
            Console.WriteLine("Valid file: {0}", valid);
            Console.WriteLine("data:");
            foreach (string dkey in data.Keys)
            {
                if (dkey != "session-expiration")
                {
                    Console.WriteLine(" + " + dkey + ": " + data[dkey]);
                }
            }
            Console.WriteLine("== end");
        }

        #endregion debug

        #region methods

        protected void readData()
        {
            if (!File.Exists(Session.SAVE_PATH + key + ".session"))
            {
                return;
            }

            Boolean readed = false;

            while (!readed)
            {
                try
                {
                    StreamReader sr = new StreamReader(Session.SAVE_PATH + key + ".session");
                    string parameterRegex = @"^[\s]*([A-Za-z]*[a-zA-Z0-9\-]*)[:][\s]*(.*)";
                    Regex parameterFormat = new Regex(parameterRegex);
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] parameters = line.Split(';');

                        foreach (string parameter in parameters)
                        {
                            if (parameterFormat.IsMatch(parameter))
                            {
                                Match match = parameterFormat.Match(parameter);
                                if (match.Success)
                                {
                                    data[match.Groups[1].Value] = match.Groups[2].Value;
                                }
                            }
                        }
                    }

                    sr.Close();
                    readed = true;
                }
                catch (System.IO.IOException exception)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

            
        }

        protected void updateData()
        {
            if (!this.isValid() && File.Exists(Session.SAVE_PATH + key + ".session"))
            {
                try
                {
                    File.Delete(Session.SAVE_PATH + key + ".session");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (!valid)
            {
                valid = true;
                string input = "";

                foreach (string dkey in data.Keys)
                {
                    if (input.Length > 0)
                        input += ";";
                    input += dkey.ToString() + ": " + data[dkey].ToString();
                }

                Boolean writen = false;
                while (!writen)
                {
                    try
                    {
                        StreamWriter sw = new StreamWriter(Session.SAVE_PATH + key + ".session");
                        sw.WriteLine(input);
                        sw.Close();
                        writen = true;
                    }
                    catch (System.IO.IOException exception)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }

        public bool isValid()
        {
            if (data.Contains("session-expiration"))
            {
                DateTime dt = DateTime.Parse(data["session-expiration"].ToString());
                return dt >= DateTime.Now;
            }

            return false;
        }

        public bool isEmpty()
        {
            return data.Count == 0;
        }

        public DateTime getExpiration()
        {
            if (data.Contains("session-expiration"))
                return DateTime.Parse(data["session-expiration"].ToString());

            return DateTime.Now;
        }

        public string get(string Key)
        {
            Key = "data-" + Key.ToLower();
            if (this.isValid())
            {
                if (data.Contains(Key))
                {
                    return data[Key].ToString();
                }
            }

            return null;
        }

        public void setExpiration(int hours = 0)
        {
            if (hours != 0)
                data["session-expiration"] = DateTime.Now.AddHours(DEFAULT_SESSION_EXPIRATION).ToString();
            else
                data["session-expiration"] = DateTime.Now.AddHours(hours).ToString();
        }

        public void set(string Key, string Value, bool Sliding = false)
        {
            Key = "data-" + Key.ToLower();
            data[Key] = Value;

            if (Sliding)
            {
                data["session-expiration"] = DateTime.Now.AddHours(DEFAULT_SESSION_EXPIRATION).ToString();
            }
            else
            {
                if (!this.isValid())
                    data["session-expiration"] = DateTime.Now.AddMinutes(DEFAULT_SESSION_EXPIRATION).ToString();
            }

            valid = false;
        }

        public void forceValidFile()
        {
            if (!valid)
            {
                updateData();
                readData();
            }
        }

        public void unset()
        {
            data["session-expiration"] = DateTime.Now.AddHours(-DEFAULT_SESSION_EXPIRATION).ToString();
            valid = true;

            updateData();
        }

        #endregion methods

    }
}
