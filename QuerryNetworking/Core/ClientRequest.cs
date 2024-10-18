using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Core
{
    public class ClientRequest
    {
        public HttpListenerContext Context;
        public NameValueCollection? Form;
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

        public byte[] GetPostBytes()
        {
            using (Stream body = Context.Request.InputStream)
            {
                MemoryStream destination = new MemoryStream();
                body.CopyTo(destination);
                byte[] Data = destination.ToArray();
                return Data;
            }
        }
    }
}
