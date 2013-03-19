using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Diagnostics;
using System.Web;

namespace CubeWorldTrees.Server
{
    class Client
    {

        #region parameters

        HttpListenerContext context;
        Server server;
        Map.MapGenerator generator = new Map.MapGenerator(256, 256);

        #endregion parameters

        #region constructs

        public Client(Server Server, HttpListenerContext Context)
        {
            context = Context;
            server = Server;
            generator.Generate();
        }

        #endregion constructs

        #region request

        public void processRequest()
        {
            Console.WriteLine("> Client access from {0}", context.Request.RemoteEndPoint.Address);
            Console.WriteLine(context.Request.Headers.ToString());

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Uri url = context.Request.Url;
            string regex = @"^(/[a-zA-Z0-9]*)*[.](png|jpg|js|css)$";
            Regex imgRegex = new Regex(regex);

            //image requested
            if (imgRegex.IsMatch(url.AbsolutePath))
            {
                string imgFile = "." + url.AbsolutePath;

                if (!File.Exists(imgFile))
                {
                    //TODO: 404 file not found
                    context.Response.OutputStream.Close();
                }
                else
                {
                    FileInfo fInfo = new FileInfo(imgFile);
                    
                    long numBytes = fInfo.Length;
                    FileStream fStream = new FileStream(imgFile, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fStream);
                    byte[] bOutput = br.ReadBytes((int)numBytes);
                    br.Close();
                    fStream.Close();

                    Match match = imgRegex.Match(url.AbsolutePath);  

                    switch (match.Groups[2].Value)
                    {
                        case "png":
                            context.Response.ContentType = "image/png";
                            break;
                        case "jpg":
                            context.Response.ContentType = "image/jpg";
                            break;
                        case "js":
                            context.Response.ContentType = "application/javascript";
                            break;
                        case "css":
                            context.Response.ContentType = "text/css";
                            break;
                    }

                    context.Response.ContentLength64 = bOutput.Length;

                    Stream OutputStream = context.Response.OutputStream;
                    OutputStream.Write(bOutput, 0, bOutput.Length);
                    OutputStream.Close();
                }
            }
            else
            {
                if (url.AbsolutePath.Equals("/"))
                {
                    Controllers.MapController controler = new Controllers.MapController(context, server.quadTree);
                    controler.Render();
                }
                else if (url.AbsolutePath.Equals("/initialize"))
                {
                    //TODO: find Controller
                    Controllers.MapController controler = new Controllers.MapController(context, server.quadTree);
                    controler.JSONTest();
                }

                sw.Stop();
                Console.WriteLine("> Map block requested from {0} was completed for {1} ms", context.Request.RemoteEndPoint.Address, sw.ElapsedMilliseconds);
            }
        }

        #endregion request

    }
}
