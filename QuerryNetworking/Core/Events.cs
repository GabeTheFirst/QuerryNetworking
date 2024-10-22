using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Core
{
    public class Events
    {
        public delegate void RequestEvent(string? URL, Api Location, int StatusCode);
    }
}
