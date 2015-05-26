using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace GiTp
{
    class PathEnvironment
    {
        private static string Name = "PATH";

        public PathEnvironment(String type)
        {
            switch (type)
                {
                    case "-add":
                        Add();
                        break;
                    case "-remove":
                        Remove();
                        break;
                    default:
                        throw new System.ArgumentException("Informe o tipo de operação -add ou -remove.");
                }
            
        }

        private static void Add()
        {
            string curPath = GetPath();
            string newPath = AddPath(curPath, MyPath());

            if (curPath != newPath)
                SetPath(newPath);
        }

        private static void Remove()
        {
            SetPath(RemovePath(GetPath(), MyPath()));
        }

        private static string MyPath()
        {
            string myFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string myPath = System.IO.Path.GetDirectoryName(myFile);
            return myPath;
        }

        private static RegistryKey GetPathRegKey(bool writable)
        {
            // for the user-specific path...
            return Registry.CurrentUser.OpenSubKey("Environment", writable);

            // for the system-wide path...
            //return Registry.LocalMachine.OpenSubKey(
            //    @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", writable);
        }

        private static void SetPath(string value)
        {
            using (RegistryKey reg = GetPathRegKey(true))
            {
                reg.SetValue(Name, value, RegistryValueKind.ExpandString);
            }
        }

        private static string GetPath()
        {
            using (RegistryKey reg = GetPathRegKey(false))
            {
                return (string)reg.GetValue(Name, "", RegistryValueOptions.DoNotExpandEnvironmentNames);
            }
        }

        private static string AddPath(string list, string item)
        {
            List<string> paths = new List<string>(list.Split(';'));

            foreach (string path in paths)
                if (string.Compare(path, item, true) == 0)
                {
                    // already present
                    return list;
                }

            paths.Add(item);
            return string.Join(";", paths.ToArray());
        }

        private static string RemovePath(string list, string item)
        {
            List<string> paths = new List<string>(list.Split(';'));

            for (int i = 0; i < paths.Count; i++)
                if (string.Compare(paths[i], item, true) == 0)
                {
                    paths.RemoveAt(i);
                    return string.Join(";", paths.ToArray());
                }

            // not present
            return list;
        }
    }
}
