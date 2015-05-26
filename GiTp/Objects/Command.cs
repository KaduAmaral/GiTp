using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GiTp.Objects;

namespace GiTp
{
    class Command
    {
        public String Name;
        public String String;
        public ParamList Params = new ParamList();
        public ParamList Arguments = new ParamList();
        private String[] Args;
        public Command Callback;

        public Command(String args)
        {
            if (args.IndexOf('|') > -1)
            {
                this.String = args.Substring(0, args.IndexOf('|') ).Trim();
                this.Callback = new Command(args.Substring( args.IndexOf('|') + 1 ).Trim());
            } else 
                this.String = args;

            args = this.String;
            do
            {
                args = args.Replace("  ", " ");
            } while (args.Contains("  ")); 

            this.Args = args.Split(' ');
            ParseCommand();
        }

        private void ParseCommand()
        {
            foreach (String arg in Args)
            {
                if (arg.Length == 0)
                    continue;

                if (this.Name == null)
                {
                    this.Name = arg.ToLower();
                    continue;
                }

                if (arg.Substring(0, 1) == "-")
                    this.Params.Add(arg);
                else
                    this.Arguments.Add(arg);
                    
            }
        }
    }
}
