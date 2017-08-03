using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DbConnector.Tools
{
    public class ReturnObject
    {
        public string KeyValue { get; set; }

        public List<string> ColumnNames { get; set; }

        public System.Reflection.PropertyInfo[] Properties { get; set; }
    }
}
