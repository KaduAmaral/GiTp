using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiTp.Objects
{
    class CommandWiki
    {
        public String Name { get; set; }
        public Action Caller { get; set; }
        public String Summary { get; set; }
        public List<ParamWiki> Params { get; private set; }
        public List<ArgumentWiki> Arguments { get; set; }
        public String Example { get; set; }

        public CommandWiki ()
	    {
            this.Params = new List<ParamWiki>();
	    }
    }

    class ParamWiki
    {
        public String Name { get; set; }
        public String Summary { get; set; }
    }

    class ArgumentWiki
    {
        public String Name { get; set; }
        public String Summary { get; set; }
    }
}
