using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiTp
{
    class Connection
    {
        public string   Name { get; set; }
        public string   Host { get; set; }
        public string   User { get; set; }
        public string   Pass { get; set; }
        public string   LocalDir { get; set; }
        public string   RemoteDir { get; set; }
        public string   Port { get; set; }
        public bool     Connected { get; set; }


        public Connection()
        {
            this.Connected = false;
        }
    }
}
