using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoudaProxy.lib
{
    public abstract partial class BaseEntity
    {
        public int Id { get; set; }
    }

    public class ProxyServer : BaseEntity
    {
        public string Server { get; set; }
        public int LocalPort { get; set; }
    }
}