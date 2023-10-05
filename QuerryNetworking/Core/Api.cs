﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Core
{
    public static class Api
    {
        // all requests that are valid
        public static List<RequestBase> Requests;
        
        // the HttpListener used to get requests
        public static HttpListener Listener;

        // set up the server so it can be started correctly
        public static void SetupBase()
        {
            Console.WriteLine("Setting up...");

            // make sure the requests list isn't null
            Requests = new List<RequestBase>();

            // find every function that has the Get Attribute (idk if this is the best way of doing that haha)
            var Gets = Assembly.GetEntryAssembly().GetTypes()
                      .SelectMany(t => t.GetMethods())
                      .Where(m => m.GetCustomAttribute(typeof(GetAttribute)) != null);

            // for each request in the array just got
            foreach (var item in Gets)
            {
                // get the url so we can modify it
                string url = item.GetCustomAttribute<GetAttribute>().Url;


                if (!url.StartsWith("/"))
                {
                    // add a / to the start if it doesn't already have one, just for ease of use
                    url = "/" + url;
                }
                Console.WriteLine("Registering Request: " + url);

                // add to the list of valid requests, and add the method to the attribute
                Requests.Add(new RequestBase()
                {
                    Url = url,
                    Method = item.GetCustomAttribute<GetAttribute>().GetResult = item
                });
            }

            // make sure the HttpListener isn't null
            Listener = new HttpListener();
            // add the url to the prefixes
            Listener.Prefixes.Add(ServerData.BaseUrl);
            Console.WriteLine("Finished setting up!");
        }

        public static void StartServer()
        {
            Console.WriteLine("starting...");
            // start the server
            Listener.Start();
            Console.WriteLine("Started! Listening on: " + ServerData.BaseUrl);
            while (true)
            {
                // get the HttpListenerContext
                HttpListenerContext context = Listener.GetContext();
                // queue process request to run on a different thread
                ThreadPool.QueueUserWorkItem(ProcessRequest, context);
            }
        }

        public static void ProcessRequest(object o)
        {
            // convert the object (o) bacl to an HttpListenerContext
            HttpListenerContext context = o as HttpListenerContext;

            // the bytes to send back to the client
            byte[] Result = new byte[0];

            // if or if it's not a 404
            bool Found = false;

            // the url from the request
            string Url = context.Request.RawUrl;

            // log the request received, might remove this for performance
            Console.WriteLine("Request for: " + Url);

            // check the valid requests to find the matching one
            foreach (var item in Requests)
            {
                // the url of the current request it's checking
                string ItemUrl = item.Url;

                // a list of variables
                List<string> VarsReceived = new List<string>();
                // the list of variables but as an array, I probably don't need both of these but it was faster when I was writing it haha 🐱
                object[] Vars = null;
                // each part of the url so a url with variables can still be found as valid
                string[] ItemSplits = ItemUrl.Split("/");

                // note: I probably shouldn't put so many ifs inside each other like this haha, I might change this
                // if the urls are the same length, continue to check if they match
                if (Url.Split("/").Length == ItemSplits.Length)
                {
                    // for each split in the path
                    for (int i = 0; i < ItemSplits.Length; i++)
                    {
                        //check if the splits match, or if it's a {var}
                        if (ItemSplits[i] == Url.Split("/")[i] || ItemSplits[i].Contains("{var}"))
                        {
                            // only continue if it's a {var}
                            if (ItemSplits[i].Contains("{var}"))
                            {
                                // set the found url to match the requested one so they match
                                ItemSplits[i] = Url.Split("/")[i];

                                // confirm vars isn't null
                                if (Vars == null)
                                {
                                    Vars = new object[] { };
                                }
                                // add this var to the list so we can pass it through
                                VarsReceived.Add(Url.Split("/")[i]);
                                // make the vars into an array
                                Vars = VarsReceived.ToArray();
                            }
                        }
                    }
                }
                // make url empty so we can rebuild it
                ItemUrl = "";

                foreach (var Split in ItemSplits)
                {
                    // add the / then the part of the url
                    ItemUrl += "/";
                    ItemUrl += Split;
                }
                // without this it would start with '//'
                ItemUrl = ItemUrl.Substring(1);

                // if the urls match
                if (ItemUrl == Url)
                {
                    // the Type that the method returns
                    Type ReturnType = item.Method.ReturnType;
                    
                    // not a 404
                    Found = true;




                    // check the return type
                    if (ReturnType == typeof(string))
                    {
                        // if it returns a string then convert the string to bytes
                        Result = System.Text.Encoding.UTF8.GetBytes((string)item.Method.Invoke(Activator.CreateInstance(item.Method.DeclaringType), Vars));
                    }
                    else if (ReturnType == typeof(byte[]))
                    {
                        // if it returns a byte array then just return that byte array
                        Result = (byte[])item.Method.Invoke(Activator.CreateInstance(item.Method.DeclaringType), Vars);
                    }
                    else
                    {
                        // no other return types are supported yet! I probably can do it more easily once I add the data system (or decide to just use JSON)
                        Result = System.Text.Encoding.UTF8.GetBytes("The requested API has an invalid return type! (this is not user error, it's an error in the api server.) Status code: 500");
                        context.Response.StatusCode = 500;
                    }
                }
            }

            // if it's a 404
            if (!Found)
            {
                // return a 404
                Result = System.Text.Encoding.UTF8.GetBytes("The requested API was not found on the server! Status code: 404");
                context.Response.StatusCode = 404;
            }

            // set the content length
            context.Response.ContentLength64 = Result.Length;
            // write the response
            context.Response.OutputStream.Write(Result, 0, Result.Length);
            // close
            context.Response.Close();
        }
    }
}