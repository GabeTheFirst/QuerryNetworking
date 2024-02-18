using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuerryNetworking.Core
{
    public static class Settings
    {
        public static QuerryDataType DataType;
    }

    public enum QuerryDataType
    {
        Json,
        QuerryData
    }
}
