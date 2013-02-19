using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Diagnostics;

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
            //Console.WriteLine("> Client access from {0}", context.Request.RemoteEndPoint.Address);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Uri url = context.Request.Url;
            string regex = @"^(/[a-zA-Z0-9]*)*[.](png|jpg|js|css)$";
            Regex fileRegex = new Regex(regex);

            //image requested
            if (fileRegex.IsMatch(url.AbsolutePath))
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

                    Match match = fileRegex.Match(url.AbsolutePath);  

                    switch (match.Groups[2].Value)
                    {
                        case "png":
                            //Console.WriteLine("> Request png!");
                            context.Response.ContentType = "image/png";
                            break;
                        case "jpg":
                            //Console.WriteLine("> Request jpg!");
                            context.Response.ContentType = "image/jpg";
                            break;
                        case "js":
                            //Console.WriteLine("> Request javascript!");
                            context.Response.ContentType = "application/javascript";
                            break;
                        case "css":
                            //Console.WriteLine("> Request css!");
                            context.Response.ContentType = "text/css";
                            break;
                    }

                    context.Response.ContentLength64 = bOutput.Length;

                    Stream OutputStream = context.Response.OutputStream;

                    try
                    {
                        OutputStream.Write(bOutput, 0, bOutput.Length);
                        OutputStream.Close();
                    }
                    catch { }
                }
            }
            else
            {
                Controlers.BaseControler controler = null;
                string absolutePath = url.AbsolutePath;

                switch (absolutePath)
                {
                    case "/":
                        controler = new Controlers.MapControler(context, server.quadTree);
                        break;
                    case "/initialize":
                        controler = new Controlers.JsonControler(context, server.quadTree);
                        break;
                    case "/login":
                        controler = new Controlers.LoginControler(context);
                        break;
                    default:
                        controler = new Controlers.ErrorControler(context);
                        break;
                }

                controler.Run();
            }

            

            sw.Stop();
            Console.WriteLine("> Request was completed for {1} ms", context.Request.RemoteEndPoint.Address, sw.ElapsedMilliseconds);
        }

        #endregion request

    }
}
