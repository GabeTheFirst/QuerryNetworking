using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PostAttribute : Attribute
    {
        // The URL for this method
        public string Url;
        // The method that returns the value to the client
        public MethodInfo GetResult;

        public PostAttribute(string Url)
        {
            // set the url when creating the attribute, like [Post("/api")] for example
            this.Url = Url;
        }
    }
}
