﻿using QuerryNetworking.Data;
using QuerryNetworking.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace QuerryNetworking.Core
{
    public static class Api
    {
        // all requests that are valid
        static List<RequestBase> Requests;
        
        // the HttpListener used to get requests
        static HttpListener Listener;

        // set up the server so it can be started correctly
        static void SetupBase(string[]? Namespaces = null)
        {
            Log.Info("Setting up...");

            // make sure the requests list isn't null
            Requests = new List<RequestBase>();


            var Types = Assembly.GetEntryAssembly().GetTypes();

            if(Namespaces != null)
            {
                Log.Debug("Requests in namespace: " + JsonSerializer.Serialize(Namespaces));
                Types = Types.Where(A => Namespaces.Any(B => B == A.Namespace)).ToArray();
            }

            // find every function that has the Get Attribute (idk if this is the best way of doing that haha)
            var Gets = Types
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
                Log.Debug("[GET] Registering Request: " + url);

                // add to the list of valid requests, and add the method to the attribute
                Requests.Add(new RequestBase()
                {
                    Url = url,
                    Method = item.GetCustomAttribute<GetAttribute>().GetResult = item,
                    HttpMethod = QuerryHttpMethod.GET
                });
            }

            // find every function that has the Post Attribute (idk if this is the best way of doing that haha)
            var Posts = Types
                      .SelectMany(t => t.GetMethods())
                      .Where(m => m.GetCustomAttribute(typeof(PostAttribute)) != null);

            // for each request in the array just got
            foreach (var item in Posts)
            {
                // get the url so we can modify it
                string url = item.GetCustomAttribute<PostAttribute>().Url;


                if (!url.StartsWith("/"))
                {
                    // add a / to the start if it doesn't already have one, just for ease of use
                    url = "/" + url;
                }
                Log.Debug("[POST] Registering Request: " + url);

                // add to the list of valid requests, and add the method to the attribute
                Requests.Add(new RequestBase()
                {
                    Url = url,
                    Method = item.GetCustomAttribute<PostAttribute>().GetResult = item,
                    HttpMethod = QuerryHttpMethod.POST
                });
            }

            // make sure the HttpListener isn't null
            Listener = new HttpListener();
            // add the url to the prefixes
            Listener.Prefixes.Add(ServerData.BaseUrl);
            Log.Complete("Finished setting up!");
        }

        public static void StartServer(string[]? Namespaces = null)
        {
            Log.Info("starting...");

            // setup the base
            SetupBase(Namespaces);

            // start the server
            Listener.Start();
            Log.Complete("Started! Listening on: " + ServerData.BaseUrl);
            while (true)
            {
                // get the HttpListenerContext
                HttpListenerContext context = Listener.GetContext();
                // queue process request to run on a different thread
                ThreadPool.QueueUserWorkItem(ProcessRequest, context);
            }
        }

        public static async void ProcessRequest(object o)
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
            Log.Info("Request for: " + Url);

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
                if (ItemUrl == Url.Split("?")[0])
                {
                    // the Type that the method returns
                    Type ReturnType = item.Method.ReturnType;
                    
                    // not a 404
                    Found = true;


                    object Instance = Activator.CreateInstance(item.Method.DeclaringType);
                    if(Instance.GetType().IsSubclassOf(typeof(ClientRequest)))
                    {
                        ((ClientRequest)Instance).Context = context;

                        if(context.Request.ContentType == "application/x-www-form-urlencoded")
                        {
                            ((ClientRequest)Instance).Form = ReadFormPost(((ClientRequest)Instance).GetPostString());
                        }
                    }

                    // check the return type
                    if (ReturnType == typeof(string))
                    {
                        // if it returns a string then convert the string to bytes
                        Result = System.Text.Encoding.UTF8.GetBytes((string)item.Method.Invoke(Instance, Vars));
                    }
                    else if (ReturnType == typeof(byte[]))
                    {
                        // if it returns a byte array then just return that byte array
                        Result = (byte[])item.Method.Invoke(Instance, Vars);
                    }
                    else if (ReturnType.ToString().Contains("System.Threading.Tasks.Task"))
                    {
                        Task t = (Task)item.Method.Invoke(Instance, Vars);
                        await t.ConfigureAwait(false);

                        if (context.Request.IsWebSocketRequest)
                        {
                            // should be handled in the app that receives the request 🙏 (maybe I'll add full websocket support here eventually)
                            return;
                        }

                        switch (Settings.DataType)
                        {
                            case QuerryDataType.Json:
                                Result = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize((object)((dynamic)t).Result));
                                break;
                            case QuerryDataType.QuerryData:
                                Result = System.Text.Encoding.UTF8.GetBytes(QuerryData.Convert((object)((dynamic)t).Result));
                                break;
                        }
                    }
                    else
                    {
                        Log.Debug("Returning Type: " + ReturnType);
                        // serialize
                        switch(Settings.DataType)
                        {
                            case QuerryDataType.Json:
                                Result = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item.Method.Invoke(Instance, Vars)));
                                break;
                            case QuerryDataType.QuerryData:
                                Result = System.Text.Encoding.UTF8.GetBytes(QuerryData.Convert(item.Method.Invoke(Instance, Vars)));
                                break;
                        }
                    }
                }
            }

            if (context.Request.IsWebSocketRequest)
            {
                // should be handled in the app that receives the request 🙏 (maybe I'll add full websocket support here eventually)
                return;
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

        static NameValueCollection? ReadFormPost(string POST)
        {
            try
            {
                NameValueCollection Collection = new NameValueCollection();
                string[] Items = POST.Split("&");
                foreach (string Item in Items)
                {
                    string[] ItemValue = Item.Split('=');
                    
                    Collection.Add(ItemValue[0], Uri.UnescapeDataString(ItemValue[1]));
                }
                return Collection;
            }
            catch
            {
                Log.Error("Invalid form data! X3");
                return null;
            }
        }
    }
}
