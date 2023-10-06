using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Core
{
    public class ClientRequest
    {
        public HttpListenerContext Context;
        public string GetPostString()
        {
            using (Stream body = Context.Request.InputStream)
            {
                using (var reader = new StreamReader(body, Context.Request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
