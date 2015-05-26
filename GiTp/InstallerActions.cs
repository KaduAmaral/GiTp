using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace GiTp
{
    [RunInstaller(true)]
    public partial class InstallerActions : System.Configuration.Install.Installer
    {

        private string logfile = @"C:\gitplog.txt";
        private bool log = true;
        // asdasd StringDictionary parameters;

        public InstallerActions()
        {
            if (log && !File.Exists(logfile)) { 
                FileStream file = File.Create(logfile);
                file.Close();
            }

        }

        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);
            //Add custom code here

            //logArg(this.Context.Parameters);


        }


        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            //Add custom code here
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);

            if (this.Context.Parameters["cpath"] == "1")
                AddPath(this.Context.Parameters["targ"]);

            //logArg(this.Context.Parameters);

            //Add custom code here
        }

        private void AddPath(String path)
        {
            var originalPath = Environment.GetEnvironmentVariable("PATH").TrimEnd(';');
            Environment.SetEnvironmentVariable("PATH", originalPath + ';' + path.TrimEnd('\\'), EnvironmentVariableTarget.Machine);

            if (log) { 
                using (StreamWriter file = new StreamWriter(logfile))
                {
                    file.WriteLine("Add");
                    file.WriteLine("---");
                    file.WriteLine("  OldPath: " + originalPath);
                    file.WriteLine("  NewPath: " + originalPath + ';' + path);
                    file.WriteLine("  AtualPath: " + Environment.GetEnvironmentVariable("PATH"));
                    file.WriteLine();
                }
            }

        }

        private void RemovePath(String path)
        {
            var originalPath = Environment.GetEnvironmentVariable("PATH").TrimEnd(';');

            List<string> paths = new List<string>(originalPath.Split(';'));


            foreach (string p in paths)
            {
                if (String.Compare(p, path, true) == 0)
                {
                    paths.Remove(p);
                    break;
                }

            }

            Environment.SetEnvironmentVariable("PATH", String.Join(";", paths) );

            if (log)
            {
                using (StreamWriter file = new StreamWriter(logfile))
                {
                    file.WriteLine("Remove");
                    file.WriteLine("------");
                    file.WriteLine("  OldPath: " + originalPath);
                    file.WriteLine("  NewPath: " + String.Join(";", paths));
                    file.WriteLine("  AtualPath: " + Environment.GetEnvironmentVariable("PATH").TrimEnd(';'));
                    file.WriteLine();
                }
            }
        }

        /// <summary>
        /// Método para fins de Debug
        /// </summary>
        /// <param name="pars">Parâmetros</param>
        private void logArg(StringDictionary pars)
        {

            if (pars == null || pars.Count == 0)
                return;

            using (StreamWriter file = new StreamWriter(logfile) )
            {
                file.WriteLine("Parâmetros");
                file.WriteLine("----------");

                foreach (DictionaryEntry entry in pars)
                {

                    string key = entry.Key == null ? "n/a" : entry.Key.ToString();

                    string value = entry.Value == null ? "n/a" : entry.Value.ToString();

                    file.WriteLine("  " + key + " => " + value);
                }

                file.WriteLine();
            }

            
        }


        public override void Uninstall(IDictionary savedState)
        {
            Process application = null;
            foreach (var process in Process.GetProcesses())
            {
                if (!process.ProcessName.ToLower().Contains("gitp")) continue;
                application = process;
                break;
            }

            if (application != null && application.Responding)
                application.Kill();

            RemovePath(this.Context.Parameters["targ"]);

            base.Uninstall(savedState);
        }

    }
}
