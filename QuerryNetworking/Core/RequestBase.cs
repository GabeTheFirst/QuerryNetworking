using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Core
{
    public class RequestBase
    {
        public string Url { get; set; }
        public MethodInfo Method { get; set; }
    }
}
