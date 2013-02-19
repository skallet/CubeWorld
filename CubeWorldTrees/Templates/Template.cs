using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using CubeWorldTrees.Controlers;

namespace CubeWorldTrees.Templates
{
    class Template
    {
        private string[] RESERVED_PARAMETERS = { "scripts", "styles", "content" };

        public const string BASE_PATH = @"./content/templates/";

        protected string layout = "@layout.cw";
        protected string file;
        protected System.Collections.Hashtable parameters = new System.Collections.Hashtable();
        protected BaseControler parent;

        public List<string> scripts = new List<string>();
        public List<string> styles = new List<string>();

        public Template(BaseControler Parent = null)
        {
            parent = Parent;
            scripts.Add("http://code.jquery.com/jquery-1.8.3.min.js");
        }

        public bool setLayout(string Layout)
        {
            if (File.Exists(BASE_PATH + Layout))
            {
                layout = Layout;
                return true;
            }

            return false;
        }

        public bool setFile(string Filename)
        {
            if (File.Exists(BASE_PATH + Filename))
            {
                file = Filename;
                return true;
            }

            return false;
        }

        public bool setParameter(string name, object value)
        {
            if (!RESERVED_PARAMETERS.Contains(name))
            {
                parameters[name] = value;
                return true;
            }

            return false;
        }

        protected string renderMacro(string macro)
        {
            if (RESERVED_PARAMETERS.Contains(macro))
            {
                string ret = "";
                if (macro == "scripts")
                {
                    foreach (string s in scripts)
                    {
                        ret = ret + "<script src='" + s + "'></script>\n";
                    }

                    return ret;
                }
                else if (macro == "styles")
                {
                    foreach (string s in styles)
                    {
                        ret = ret + "<style src='" + s + "'></style>\n";
                    }

                    return ret;
                }
                else if (macro == "content")
                {
                    StreamReader srFile = new StreamReader(BASE_PATH + file);
                    ret = srFile.ReadToEnd();
                    srFile.Close();
                    return process(ret);
                }
            }
            else
            {
                if (!parameters.Contains(macro))
                {
                    //throw new Exception("Parameter {" + macro + "} not defined!");
                }
                else
                {
                    switch (parameters[macro].GetType().ToString().ToLower())
                    {
                        case "system.string":
                            return parameters[macro].ToString();
                    }
                }
            }

            return "";
        }

        protected string process(string file)
        {
            string macroFinder = @".*[\s]*[{][$]([a-zA-Z0-9]+)[}].*";
            Regex macroRegex = new Regex(macroFinder);
            MatchCollection matches = macroRegex.Matches(file);
            string macro;

            foreach (Match match in matches)
            {
                macro = match.Groups[1].Value;
                file = file.Replace("{$" + macro + "}", renderMacro(macro));
            }
            
            return file;
        }

        public void Render(HttpListenerResponse response)
        {
            if (parent.responseSent)
                return;

            StringBuilder sb = new StringBuilder();

            StreamReader srLayout = new StreamReader(BASE_PATH + layout);
            string fileLayout = srLayout.ReadToEnd();
            srLayout.Close();

            fileLayout = process(fileLayout);

            sb.Append(fileLayout);
            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            response.ContentLength64 = b.Length;

            try
            {
                response.OutputStream.Write(b, 0, b.Length);
                response.OutputStream.Close();
            }
            catch { }
        }

    }
}
