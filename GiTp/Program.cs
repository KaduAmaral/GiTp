using System;
using System.Reflection;
using System.Diagnostics;
using GiTp;

namespace GiTp
{
    class Program {
        private static Commands Z;
        
        private static Assembly assembly = Assembly.GetExecutingAssembly();
        private static FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        private static String Version = fileVersionInfo.ProductVersion;
        private static String SolutionName = Assembly.GetCallingAssembly().GetName().Name;

        static void Main(string[] args) {
            Welcome();
            Z = new Commands( args.Length > 0 ? new Command( String.Join(" ", args) ) : null );
            Z.Run();
        }

        static void Welcome()
        {
            Console.WriteLine(SolutionName + " ~ [Versão " + Version + "]");
            Console.WriteLine();

        }

    }
}
