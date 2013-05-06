using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace CubeWorldTrees.Controlers
{
    class LoginControler : BaseControler
    {

        protected System.Collections.Hashtable loginForm = new System.Collections.Hashtable();

        protected List<string> loginFormErrors = new List<string>();

        public LoginControler(HttpListenerContext Context, Map.Map World)
            : base(Context, World)
        {
        }

        public override void  Action()
        {
 	        base.Action();
            loginForm["send"] = false;

            if (context.Request.HasEntityBody)
            {
                using (System.IO.Stream body = context.Request.InputStream)
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(body, context.Request.ContentEncoding))
                    {
                        loginForm["send"] = true;

                        string formData = reader.ReadToEnd();
                        string[] values = formData.Split('&');
                        string formRegex = @"[\s]*(.*)[\s]*[=][\s]*(.*)";
                        Regex formMatch = new Regex(formRegex);

                        foreach (string value in values)
                        {
                            Match match = formMatch.Match(value);
                            if (match.Success)
                            {
                                //Console.WriteLine("{0}: {1}", match.Groups[1].Value, match.Groups[2].Value);
                                if (match.Groups[2].Value != "")
                                    loginForm[match.Groups[1].Value] = match.Groups[2].Value;
                            }
                        }
                    }
                }
            }

            if ((bool)loginForm["send"] == true)
            {
                if (loginForm.Contains("username"))
                {
                    template.setParameter("loginName", loginForm["username"].ToString());
                }

                if (loginForm.Contains("username") && loginForm.Contains("password"))
                {
                    user.login(loginForm["username"].ToString(), loginForm["password"].ToString());

                    if (!user.isLoggedIn())
                    {
                        loginFormErrors.Add("Invalid username or password!");
                    }
                }
                else
                {
                    loginFormErrors.Add("Insert username and password!");
                }
            }

            if (user.isLoggedIn())
            {
                Redirect("http://" + context.Request.Url.Host + ":" + context.Request.Url.Port + "/");
                return;
            }

            string errors = "";
            if (loginFormErrors.Count > 0)
            {
                errors += "<ul class='errors'>\n";
                foreach (string error in loginFormErrors)
                {
                    errors += "<li>" + error + "</li>\n";
                }
                errors += "</ul>\n";
            }

            //Hash is only for testing purposes! In real application it will be deleted!
            if (context.Request.QueryString.Count == 2)
            {
                String pass = context.Request.QueryString.Get(0);
                String salt = context.Request.QueryString.Get(1);
                template.setParameter("hash", pass.Length != 0 && salt.Length != 0 ? user.hash(pass, salt) : "");
            }

            template.setParameter("errors", errors);
        }

    }
}
