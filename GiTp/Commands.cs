using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using GiTp.Objects;

namespace GiTp
{
    class Commands
    {
        
        static IDictionary<string, string> arguments = new Dictionary<string, string>();

        static Repository repo;
        static Boolean EndProgram = false;
        static String CurDir = Environment.CurrentDirectory;
        static List<Arquivo> Arquivos; // = new List<Arquivo>();
        static Connection Con = new Connection();
        static Config Config = new Config();
        static FTPConnection ftp;

        public Commands(String[] args)
        {
            if (args != null && args.Length > 0)
                RunCommand(String.Join(" ", args));
        }

        public Commands(Command cmd):base()
        {
            if (cmd != null) RunCommand(cmd);
        }

        public void Run()
        {
            do
            {
                Command cmd = ReadCommand();
                RunCommand(cmd);
            } while (!EndProgram);
        }

        static public Command ReadCommand()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(Help.Prefix);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(Environment.CurrentDirectory);
            Console.ResetColor();
            Console.Write(" >");
            string command = Console.ReadLine();
            return new Command(command) ;
        }

        static public void RunCommand(String command)
        {
            RunCommand(new Command(command));
        }

        static public void RunCommand(Command command)
        {
            if (command.Name != null)
            {
                bool invalido = false;
                switch (command.Name)
                {
                    case "cd":
                        if (command.Arguments.Count > 0)
                            SetDir(command);
                        else
                            invalido = true;
                        break;
                    case "rd":
                        ChangeRDir(command);
                        break;
                    case "dir":
                        ListDir();
                        break;
                    case "rdir":
                        if (ftp != null)
                            ftp.ListDir();
                        else
                            invalido = true;
                        break;
                    case "set":
                            Set(command);
                        break;
                    case "save":
                            Save(command);
                        break;
                    case "remove":
                        if (command.Arguments.Count == 1)
                            Remove(command);
                        else
                            invalido = true;
                        break;
                    case "connection":
                    case "con":
                        if (command.Arguments.Count > 0)
                            SetConnection(command);
                        else
                            Connection(command);
                            
                            //ConnectionInfo((args.Length > 1 ? args[1] : ""));
                        break;
                    case "connect":
                        ftp.ReCreate();
                        ftp.Connect();
                        if (ftp.IsConnected)
                            Util.Success("Conectado...");
                        else
                            Util.Error("Falha: " + ftp.Error);
                        break;
                    case "disconnect":
                        if (ftp.IsConnected)
                        {
                            ftp.Close();
                            Util.Error("Desconectado...");
                        }
                        else
                            Util.Warning("Nenhuma conexão aberta...");
                        break;
                    case "connections":
                    case "cons":
                        ListConnections();
                        break;
                    case "curdir":
                        Console.WriteLine(CurDir);
                        break;
                    case "repo":
                        RepoCommands(command);
                        break;
                    case "send":
                        SendFiles(command);
                        break;
                    case "help":
                        new Help(command);
                        break;
                    case "cls":
                    case "clear":
                        Console.Clear();
                        break;
                    case "path":
                        if (command.Params.Count > 0 && (command.Params.Has("-add") || command.Params.Has("-remove")))
                            new PathEnvironment(command.Params.First().Value);
                        else
                            Util.Error("Parâmetros inválidos, informe o método desejado -add ou -remove");
                        break;
                    case "exit":
                        if (ftp != null && ftp.IsConnected) RunCommand("disconnect");
                        EndProgram = true;
                        break;
                    default:
                        invalido = true;
                        break;
                }

                if (invalido)
                    InvalidCommand(command);
                else if (!invalido && command.Callback != null)
                    RunCommand(command.Callback);
            }

        }

        private static void Save(Command cmd)
        {
            if (cmd.Arguments.Count < 1)
            {
                cmd.Arguments.Add(new Param("con"));
                //InvalidCommand(cmd);
                //return;
            }

            switch (cmd.Arguments.First().Value)
            {
                case "connection":
                case "con":
                    SaveConnection();
                    break;
                default:
                    Util.Warning("Nenhuma ação tomada.");
                    break;
            }

        }

        private static void Remove(Command cmd)
        {
            if (cmd.Arguments.Count == 0)
                cmd.Arguments.Add(new Param("connection"));

            switch (cmd.Arguments.First().Value)
            {
                case "connection":
                case "con":
                    RemoveConnection();
                    break;
                default:
                    Util.Warning("Nenhuma ação tomada.");
                    break;
            }
        }

        private static void RemoveConnection()
        {
            if (Config.DeleteConnection(Con))
            {
                Con = null;
                Util.Success("Conexão removida com sucesso.");
            }

        }

        private static void SaveConnection()
        {
            if (Con != null && Con.Host != null && Con.Host.Length > 0)
                Config.SaveConnection(Con);
            else
                Util.Error("Nenhuma conexão ativa");
        }

        private static void SetConnection(Command cmd)
        {

            if (cmd.Arguments.First().Value.Substring(0, 1) == "@")
            {
                var c = Config.GetConnection(cmd.Arguments.First().Value.Substring(1));

                if (c != null)
                {
                    Con = c;
                    if (Con.LocalDir.Length > 0 && Directory.Exists(Con.LocalDir))
                        RunCommand("cd "+Con.LocalDir);

                    ftp = new FTPConnection(Con);
                    RunCommand("connect | connection");
                }
                else
                    Util.Error("Não foi possível localizar o conexão denominada '" + cmd.Arguments.First().Value.Substring(1) + "'.");

                return;
            }

            if (!SetHost(cmd.Arguments.i(0).Value))
            {
                Util.Error("Host inválido.");

                Console.WriteLine("Informe o Host ou pressione Enter para continuar.");
                string host = Console.ReadLine();

                if (host != "" || !SetHost(host))
                    return;
            }

            if (cmd.Arguments.Count > 1)
                SetUser(cmd.Arguments.i(1).Value);
            else
            {
                Console.Write("Usuário: ");
                string user = Console.ReadLine();
                SetUser(user);
            }

            if (cmd.Arguments.Count > 2)
                SetPass(cmd.Arguments.i(2).Value);
            else
            {
                Console.Write("Senha: ");
                string user = Console.ReadLine();
                SetPass(user);
            }

            if (cmd.Arguments.Count > 3)
                SetPort(cmd.Arguments.i(3).Value);
            else
            {
                if (Con.Port == null)
                {
                    Console.Write("Porta: ");
                    string porta = Console.ReadLine();
                    SetPort(porta);
                }
            }

            ftp = new FTPConnection(Con);
            RunCommand("connect");
        }

        public static void Connection(Command cmd)
        {
            if (cmd.Params.Has("-test"))
            {
                FTPConnection FTPc = new FTPConnection(Con);

                if (!FTPc.TestConnection())
                {
                    Util.Warning("Não foi possível conectar com os dados informados...");
                }
                else
                {
                    Util.Success("Exito no teste de conexão.");
                }
            }
            else
            {
                ConnectionInfo(cmd.Params.Has("-p") ? "-p" : "");
            }
        }

        public static void ConnectionInfo(string p = "")
        {
            if (Con.Host == null)
            {
                Util.Error("Nenhuma conexão informada...");
                return;
            }
            Util.Echo(Util.Hr(), ConsoleColor.Gray);
            Console.WriteLine("Conexão Selecionada");
            Util.Echo(Util.Hr(0.5), ConsoleColor.Gray);
            Console.WriteLine("Nome: " + Con.Name);
            Console.WriteLine("Host: " + Con.Host);
            Console.WriteLine("Porta: " + Con.Port);
            Console.WriteLine("Usuário: " + (Con.User == null ? "" : Con.User));
            if (p == "-p") Console.WriteLine("Senha:  " + (Con.Pass == null ? "" : Con.Pass));
            Console.WriteLine("Diretório Remoto:  " + (Con.RemoteDir == null ? "" : Con.RemoteDir));
            Console.WriteLine("Diretório Local:  " + (Con.LocalDir == null ? "" : Con.LocalDir));
            Console.Write("Status: ");
            if (ftp.IsConnected)
                Util.Success("Conectado");
            else
                Util.Error("Desconectado");
            Util.Echo(Util.Hr(), ConsoleColor.Gray);
        }

        private static void RepoCommands(Command cmd)
        {

            if (cmd.Params.Has("-here"))
            {
                RunCommand("set repository -here");
            }

            if (cmd.Arguments.Count == 0 && !cmd.Params.Has("-here"))
            {
                InvalidCommand(cmd);
                return;
            }

            if (cmd.Arguments.Count > 0)
            {
                switch (cmd.Arguments.First().Value)
                {
                    case "files":
                        RepoListFiles(cmd);
                        break;
                }
            }
        }

        private static void ListConnections()
        {
            List<string> cons = Config.ListConnectionName();

            if (cons.Count == 0)
            {
                Util.Warning("Nenhuma conexão cadastrada.");
                return;
            }

            Console.WriteLine("Conexões cadastradas:");
            int cont = 0;
            foreach (string c in cons)
            {
                cont++;
                Console.WriteLine(cont.ToString() + ": " + c);
            }

        }

        private static void RepoListFiles(Command cmd)
        {

            if (!arguments.ContainsKey("repository"))
            {
                Util.Error("O repositório ainda não foi informado.");
                return;
            }

            if (repo.Head.Commits.Count() < 1)
            {
                Util.Error("Nenhum Commit existente.");
                return;
            }

            Commit commit = repo.Commits.First();

            Tree tree = repo.Lookup<Commit>(commit.Sha).Tree;

            Util.Echo("\nSha: ", ConsoleColor.White, false);
            Util.Echo(commit.Sha, ConsoleColor.DarkYellow);
            if (commit.Author.Name != "unknown") Util.Echo("Autor: " + commit.Author.Name);
            if (commit.Committer.Name != "unknown") Util.Echo("Commiter: " + commit.Committer.Name);
            Util.Echo("Data: " + commit.Author.When); //Commit-Date
            Util.Echo("Mensagem: " + commit.Message);
            SetFiles();
            PrintFiles();

        }

        static void SetFiles()
        {
            Arquivos = new List<Arquivo>();

            if (repo == null)
            {
                Util.Error("Informe um repositório.");
                return;
            }


            Tree commitTree = repo.Head.Tip.Tree;
            Tree parentCommitTree = repo.Head.Tip.Parents.Single().Tree;

            var patch = repo.Diff.Compare<Patch>(parentCommitTree, commitTree);

            foreach (var ptc in patch)
            {
                Arquivo arquivo = new Arquivo();
                arquivo.FullPath= ptc.Path;
                arquivo.Status = ptc.Status;
                Arquivos.Add(arquivo);
            }
            
        }

        static void PrintFiles()
        {
            ConsoleColor cor;
            String pref;

            Util.Hr(true);

            foreach (Arquivo file in Arquivos)
            {

                switch (file.Status)
                {
                    case ChangeKind.Added:
                        cor = ConsoleColor.Green;
                        pref = "+ ";
                        break;
                    case ChangeKind.Deleted:
                        cor = ConsoleColor.Red;
                        pref = "- ";
                        break;
                    case ChangeKind.Unreadable:
                    case ChangeKind.Untracked:
                    case ChangeKind.Ignored:
                        cor = ConsoleColor.Gray;
                        pref = "  ";
                        break;
                    case ChangeKind.Unmodified:
                        cor = ConsoleColor.White;
                        pref = "  ";
                        break;
                    case ChangeKind.Copied:
                    case ChangeKind.Modified:
                    case ChangeKind.Renamed:
                    case ChangeKind.TypeChanged:
                    default:
                        cor = ConsoleColor.Yellow;
                        pref = "* ";
                        
                        break;
                }


                Util.Echo(pref + file.FullPath, cor);
            }

            Util.Hr(true);
            Util.Echo("Novo +", ConsoleColor.Green, false);
            Util.Echo(" | ", false);
            Util.Echo("Deletado -", ConsoleColor.Red, false);
            Util.Echo(" | ", false);
            Util.Echo("Ignorados  ", ConsoleColor.Gray, false);
            Util.Echo(" | ", false);
            Util.Echo("Não modificados  ", ConsoleColor.White, false);
            Util.Echo(" | ", false);
            Util.Echo("Modificados  ", ConsoleColor.Yellow);
            Util.EmptyLine();

            
        }

        static void Get(Command cmd)
        {

            if (cmd.Arguments.Count < 1){
                InvalidCommand(cmd);
                return;
            }

            if (arguments.ContainsKey(cmd.Arguments.First().Value))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(" > ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(arguments[cmd.Arguments.First().Value]);
                Console.ResetColor();
            }
            else
                Util.Error("Parâmetro não encontrado...");
        }

        private static void SetArg(string arg, string val)
        {
            arguments[arg] = val;
        }

        static void Set(Command cmd)
        {
            if (cmd.Arguments.Count == 0)
            {
                InvalidCommand(cmd);
                return;
            }
                
            string var = cmd.Arguments.i(0).Value;
            
            string val = "";
            if (cmd.Arguments.Count > 1) val = cmd.Arguments.i(1).Value;
            switch (var)
            {
                case "repository":
                    SetRepository(cmd);
                    break;
                case "host":
                    SetHost(val);
                    break;
                case "user":
                    SetUser(val);
                    break;
                case "pass":
                    SetPass(val);
                    break;
                case "rdir":
                    SetRDir(val);
                    break;
                case "dir":
                    SetConDir(cmd);
                    break;
                case "port":
                    SetPort(val);
                    break;
                default:
                    Util.Error("O argumento '" + var + "' é inválido.");
                    break;
            }
        }

        

        private static void SetDir(Command cmd)
        {

            string dir = cmd.String.TrimStart(cmd.Name.ToCharArray()).Trim().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            string ndir = Util.GetDir(dir);

            if (ndir != null)
                Environment.CurrentDirectory = ndir;
            else
                Util.Error("Diretório inválido.");

        }

        private static void SetConDir(Command cmd)
        {

            string nodir = cmd.Name + ' ' + cmd.Arguments.First().Value;
            string dir = cmd.String.TrimStart(nodir.ToCharArray()).Trim().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string ndir = Util.GetDir(dir);

            if (ndir != null)
                Con.LocalDir = ndir;
            else
                Util.Error("Diretório inválido.");
        }

        private static void ListDir()
        {
            try
            {
                Util.Echo(Util.Hr(), ConsoleColor.Gray);
                var files = from file in Directory.EnumerateFiles(Environment.CurrentDirectory, "*.*", SearchOption.TopDirectoryOnly)
                            select new
                            {
                                File = file.Replace(Environment.CurrentDirectory, ""),
                                Info = new FileInfo(file)
                            };
                var dirs = from dir in Directory.EnumerateDirectories(Environment.CurrentDirectory, "*", SearchOption.TopDirectoryOnly)
                           select new
                           {
                               Name = dir.Replace(Environment.CurrentDirectory, "")
                           };

                foreach (var d in dirs)
                    Util.Echo(d.Name+Path.DirectorySeparatorChar, ConsoleColor.Green);


                int width = Console.BufferWidth-1;
                int pad;
                foreach (var f in files)
                {

                    string filesize = f.Info.Length.ToString() + " bytes";
                    int namelen = f.File.Length;
                    pad = width - filesize.Length;
                    if (pad < 1) pad = 10;

                    Util.Echo(f.File.PadRight(pad) + filesize, ConsoleColor.Magenta);
                }

                Util.Echo(Util.Hr(), ConsoleColor.Gray);

            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Console.WriteLine(PathEx.Message);
            }
        }

        private static void SetRDir(string rdir)
        {
            Con.RemoteDir = rdir;
        }

        private static void ChangeRDir(Command cmd)
        {
            if (cmd.Arguments.Count == 0 || ftp == null || !ftp.IsConnected)
            {
                InvalidCommand(cmd);
                return;
            }

            string dir = cmd.String.TrimStart((cmd.Name + " ").ToCharArray()).Trim();

            if (ftp.ChangeDir(dir))
                RunCommand("rdir");

        }

        private static void SendFiles(Command cmd)
        {

            if (Con == null || ftp == null || !ftp.IsConnected)
            {
                Util.Error("Informe uma conexão antes de continuar.");
                return;
            }

            if (cmd.Params.Has("-file"))
            {
                if (cmd.Arguments.Count == 0)
                {
                    InvalidCommand(cmd);
                    return;
                }

                if (!File.Exists(cmd.Arguments.First().Value))
                {
                    Util.Error("Não foi possível encontrar o arquivo '" + cmd.Arguments.First().Value + "'.");
                    return;
                }

                string diretorio = Con.RemoteDir.Length == 0 ? "raíz" : "'" + Con.RemoteDir + "'";

                if (!Util.GetYesNo("Confirma o envio do arquivo para o diretório " + diretorio + "?"))
                    return;
                

                if (!ftp.SendFile(new Arquivo() { 
                    FullPath = cmd.Arguments.First().Value,
                    Status = ChangeKind.Added
                }))
                    Util.Error("Falha no envio.");
            }
            else
            {
                SetFiles();

                ProcessFiles(Arquivos);

            }
        }

        private static void ProcessFiles(List<Arquivo> files)
        {
            String Dir = Con.RemoteDir;
            List<Arquivo> reprocess = new List<Arquivo>();

            // Limite de tentativas de envio de um arquivo
            int Limite = 3;

            foreach (Arquivo file in files)
            {
                // Caminho do Arquivo
                //Con.RemoteDir = Dir + '/' + file.FullPath;
                ftp.ChangeDir(Dir + '/' + file.Path, true, true);


                // Incrementa uma tentativa
                file.Attempts++;

                Util.Echo("Tentativa " + file.Attempts.ToString(), ConsoleColor.Yellow);
                Util.Echo("Processando arquivo: " + file.Name);
                Util.Echo("Diretório de destino: " + file.Path);
                Util.Echo("Tipo de processamento: " + file.Status.ToString());

                bool success = false;

                // Tipo de Procedimento
                if (file.Status == ChangeKind.Deleted)
                {
                    success = ftp.RemoveFile(file.Name);

                    if (!success) { 
                        success = !ftp.FileExists(file.Name);
                        if (success)
                            Util.Warning("Arquivo já removido...");
                    }
                }
                else
                    success = ftp.SendFile(file);

                if (success)
                    Util.Success("Arquivo processado.");
                else
                {
                    Util.Error("Erro ao processar arquivo.");

                    if (file.Attempts <= Limite)
                    {
                        reprocess.Add(file);
                        Util.Warning("Arquivo adicionado à lista de reprocessamento.");
                    }
                    else
                    {
                        Util.Error("Excedido o limite de tentativas.");
                        Util.Error(ftp.Response.StatusDescription);
                    }
                }

                Util.EmptyLine();
            }

            Con.RemoteDir = Dir;

            if (reprocess.Count > 0)
                ProcessFiles(reprocess);
        }

        private static void SetUser(string user)
        {
            Con.User = user;
        }

        private static void SetPass(string pass)
        {
            Con.Pass = pass;
        }

        static bool SetHost(string host)
        {

            if (host.Contains(":"))
            {
                SetPort(host.Split(':')[1]);
                host = host.Split(':')[0];
            }

            if (Uri.CheckHostName(host) != UriHostNameType.Unknown)
            {
                Con.Host = host;
                return true;
            }
            else
                return false;
        }

        private static void SetPort(string port)
        {
            Con.Port = port;
        }

        static bool SetRepository(Command cmd)
        {
            string dir = "";
            if (cmd.Arguments.Count > 1)
                dir = cmd.Arguments.i(1).Value;
            else if (cmd.Arguments.Count == 1 && cmd.Params.Count == 1 && cmd.Params.i(0).Value == "-here")
                dir = Environment.CurrentDirectory;
            else
            {
                InvalidCommand(cmd);
                return false;
            }

            dir = dir.Replace('\\', '/').TrimEnd('/');
            if (!System.IO.Directory.Exists(dir.Trim()) || !Repository.IsValid(dir))
            {
                Util.Error("Diretório inválido.");
                Util.EmptyLine();
                return false;
            }
            else
            {
                SetArg("repository", dir);
                repo = new Repository(dir);
                return true;
            }
        }

        static private void InvalidCommand(Command cmd)
        {
            Util.Error("O comando '" + cmd.Name + "' é inválido.");
            Util.Echo("Para ajuda digite 'help' ou 'help' mais o nome do comando.", ConsoleColor.Gray);
            Util.Echo("Por exemplo: help " + cmd.Name, ConsoleColor.Gray);
        }

        
    }
}
