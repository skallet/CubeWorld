using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace CubeWorldTrees.Controllers
{
    class LoginController : BaseController
    {

        protected System.Collections.Hashtable loginForm = new System.Collections.Hashtable();

        protected List<string> loginFormErrors = new List<string>();

        public LoginController(HttpListenerContext Context)
            : base(Context)
        {
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
                                Console.WriteLine("{0}: {1}", match.Groups[1].Value, match.Groups[2].Value);
                                loginForm[match.Groups[1].Value] = match.Groups[2].Value;
                            }
                        }
                    }
                }
            }

            if ((bool)loginForm["send"] == true)
            {
                if (loginForm.Contains("username") && loginForm.Contains("password"))
                {
                    user.login(loginForm["username"].ToString(), loginForm["password"].ToString());
                }
                else
                {
                    loginFormErrors.Add("Insert username and password!");
                }
            }

            if (user.isLoggedIn())
            {
                context.Response.Redirect("http://" + context.Request.Url.Host + ":" + context.Request.Url.Port + "/");
                context.Response.StatusCode = 302;
                context.Response.OutputStream.Close();
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("</head>");

            sb.Append("<body>");
            sb.Append("<h1>Login</h1>");

            if (loginFormErrors.Count > 0)
            {
                sb.Append("<ul class='errors'>");
                foreach (string error in loginFormErrors)
                {
                    sb.Append("<li>" + error + "</li>");
                }
                sb.Append("</ul>");
            }

            sb.Append("<form method='post' action='?do=loginForm-send'>");
            sb.Append(" <label for='username'>Username:</label>");
            sb.Append(" <input type='text' name='username' value='" + loginForm["username"] + "' />");
            sb.Append(" <br />");
            sb.Append(" <label for='password'>Password:</label>");
            sb.Append(" <input type='password' name='password' />");
            sb.Append(" <br />");
            sb.Append(" <input type='submit' value='Login' />");
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            context.Response.ContentLength64 = b.Length;

            try
            {
                context.Response.OutputStream.Write(b, 0, b.Length);
                context.Response.OutputStream.Close();
            }
            catch { }
        }

    }
}
