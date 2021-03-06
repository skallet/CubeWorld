﻿using System;
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
            //Console.WriteLine("> Client access from {0}", context.Request.RemoteEndPoint.Address);
            //Console.WriteLine(context.Request.Headers.ToString());

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Uri url = context.Request.Url;
            string regex = @"^(/[a-zA-Z0-9]*)*[.](png|jpg|js|css|ico)$";
            Regex fileRegex = new Regex(regex);

            //image requested
            if (fileRegex.IsMatch(url.AbsolutePath))
            {
                string imgFile = "." + url.AbsolutePath;

                if (!File.Exists(imgFile))
                {
                    //404 file not found
                    context.Response.StatusCode = 404;
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
                            context.Response.ContentType = "application/javascript";
                            break;
                        case "css":
                            //Console.WriteLine("> Request css!");
                            context.Response.ContentType = "text/css";
                            break;
                        case "ico":
                            context.Response.ContentType = "image/x-icon";
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
                //Console.WriteLine("Request path: {0}", absolutePath);

                switch (absolutePath)
                {
                    case "/":
                    case "/logout":
                        controler = new Controlers.MapControler(context, server.world);
                        break;
                    case "/update":
                    case "/initialize":
                    case "/position":
                        controler = new Controlers.JsonControler(context, server.world);
                        break;
                    case "/login":
                        controler = new Controlers.LoginControler(context, server.world);
                        break;
                    default:
                        controler = new Controlers.ErrorControler(context, server.world);
                        break;
                }

                controler.Run();

                Trees.QuadTree.QuadTree<Map.Block>.unsetAllTree();
            }

            sw.Stop();

            Server.maxClientsAtOnce.Release();
            Console.WriteLine("> Request was completed for {1} ms", context.Request.RemoteEndPoint.Address, sw.ElapsedMilliseconds);
        }

        #endregion request

    }
}
