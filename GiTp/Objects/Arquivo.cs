using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GiTp
{
    class Arquivo
    {
        private String fullpath;
        public String FullPath {
            get { return fullpath; }
            set {
                fullpath = value.Replace('\\', '/').TrimEnd('/');
                if (this.fullpath.IndexOf('/') != -1)
                {
                    this.Name = this.fullpath.Substring(this.fullpath.LastIndexOf('/') + 1);
                    this.Path = this.fullpath.Substring(0, this.fullpath.LastIndexOf('/'));
                }
                else 
                {
                    this.Name = this.fullpath.TrimEnd('/');
                    this.Path = "";
                }
            } 
        }
        public String Name { get; set; }
        public String Path { get; set; }
        public ChangeKind Status { get; set; }
        public Boolean Success{ get; set; }
        public Int32 Attempts { get; set; }

        public Arquivo()
        {
            this.Attempts = 0;
            this.Success = false;
        }
    }
}
