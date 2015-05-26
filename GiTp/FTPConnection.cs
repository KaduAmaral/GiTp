using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiTp
{
    class FTPConnection
    {
        private Connection Con;
        private FtpWebRequest Request;
        public FtpWebResponse Response;
        public Boolean IsConnected = false;

        public String Error;

        public FTPConnection()
        {
            Con = new Connection();

            Console.WriteLine("Informe os dados solicitado");
            Console.Write("Host: ");
            Con.Host = Console.ReadLine();
            Console.Write("Usuário: ");
            Con.User = Console.ReadLine();
            Console.Write("Senha: ");
            Con.Pass = Console.ReadLine();
            Console.Write("Porta (Enter para padrão): ");
            Con.Port = Console.ReadLine();
            Con.Port = (Con.Port == "" ? "21" : Con.Port);
            this.Create();
        }

        public FTPConnection(Connection c)
        {
            Con = c;
            this.Create();
        }

        private bool IsMethod(String Method)
        {
            return this.Request.Method == Method;
        }

        public void ChangeMethod(String Method)
        {
            if (this.IsConnected && this.IsMethod(Method))
                return;
            
            this.ReCreate();
            this.Request.Method = Method;
        }

        public void ReCreate()
        {
            if (this.IsConnected) this.Close();
            this.Create();
        }

        public void Refresh()
        {
            this.ReCreate();
            this.Connect();
        }

        public void Create()
        {
            string host = "ftp://" + Con.Host;
            if (Con.Port != null) host += ":" + Con.Port;
            if (Con.RemoteDir != null) host += '/' + Con.RemoteDir.Trim('/');
            try
            {
                this.Request = FtpWebRequest.Create( @host ) as FtpWebRequest;
                this.Request.Credentials = new NetworkCredential(Con.User, Con.Pass);
                this.Request.Method = WebRequestMethods.Ftp.ListDirectory; // Método Padrão
            }
            catch (Exception e)
            {
                Util.Error("Erro ao criar conexão: " + e.Message);
            }
        }

        public bool Connect()
        {
            try
            {
                this.Response = Request.GetResponse() as FtpWebResponse;
                this.IsConnected = true;
            }
            catch (Exception e)
            {
                //Util.Error("Erro ao tentar conexão: " + e.Message);
                this.Error = e.Message;
                this.IsConnected = false;
            }

            return this.IsConnected;
        }

        public void Close()
        {
            try
            {
                this.Response.Close();
                this.IsConnected = false;
            }
            catch (Exception e)
            {
                Util.Error(e.Message);
            }
        }

        public void ListDir()
        {

            StreamReader streamReader;

            this.ChangeMethod(WebRequestMethods.Ftp.ListDirectory);
            if (!this.Connect())
            {
                Util.Error(this.Error);
                return;
            }

            try
            {
                streamReader = new StreamReader(this.Response.GetResponseStream());
            }
            catch (Exception e)
            {
                Util.Error(e.Message);
                return;
            }
            

            List<string> directories = new List<string>();

            string line = streamReader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                directories.Add(line);
                line = streamReader.ReadLine();
            }

            streamReader.Close();

            string header;
            header = Con.Name + ": " + Con.Host + " | ";
            Util.Echo(header, false);
            
            string headerdir = Con.RemoteDir.Length > 0 ? Con.RemoteDir : "Diretório Raíz";
            Util.Echo(headerdir, ConsoleColor.DarkYellow, false);


            int pad = Console.BufferWidth - 10 - header.Length - headerdir.Length;

            Util.Echo("Conectado".PadLeft(pad), ConsoleColor.Green);
            Util.Hr(true);
            
            Util.Echo(Con.RemoteDir.Length == 0 ? "/" : "../");

            if (directories.Count > 0)
                foreach (string dir in directories)
                    Util.Echo(dir.TrimStart(Con.RemoteDir.TrimEnd('/').ToCharArray()));

            Util.Hr(true);

        }

        public bool ChangeDir(String dir, Boolean createdir = false, Boolean absolutePath = false)
        {
            // Diretório Atual
            string LastDir = "";

            // Diretório original informado
            string OrigDir = dir;

            // Limpando a string
            dir = dir.Replace("//", "/");

            // Verifica se está na pasta raíz
            if (Con.RemoteDir.Length > 0 && !absolutePath)
            {
                // Remove barras 
                Con.RemoteDir = Con.RemoteDir.Trim('/');

                // Seta o diretório atual
                LastDir = Con.RemoteDir;

                // Verifica se tem algum Backdir
                if (dir.IndexOf("../") != -1)
                {
                    // Verifica se tem algum backdir em um caminho, por exemplo "foobar/../../etc"
                    if (dir.IndexOf("../") > 0 || (dir.IndexOf("../") == 0 && dir.Length > 3))
                    {
                        Util.Error("Não é permitido o backdir em um caminho, para voltar o diretório informe apenas '../'");
                        return false;
                    }

                    // Verifica se o diretório atual é filho de outro "foo/bar" e volta para o anterior "foo"
                    if (Con.RemoteDir.LastIndexOf('/') > 0)
                        dir = Con.RemoteDir.Substring(0, Con.RemoteDir.LastIndexOf('/'));
                    else
                        dir = ""; // Senão volta pra raíz
                }
                else // Caso não tenha backdir vai para um subdiretório
                    dir = Con.RemoteDir.Trim('/') + '/' + dir.Trim('/');
            }
            else // Se estiver no diretório raíz
            {
                // Se estiver tentando acessar um diretório pai
                if (dir.IndexOf("../") != -1)
                {
                    Util.Warning("Não é possível voltar o diretório");
                    return false;
                }
            }

            Con.RemoteDir = dir.TrimEnd('/');
            this.Refresh();

            // Se falhar volta para o diretório anterior
            if (!this.IsConnected)
            {
                // Cria um diretório
                if (createdir)
                {
                    this.ChangeMethod(WebRequestMethods.Ftp.MakeDirectory);
                    this.Connect();

                    if (!this.IsConnected)
                    {
                        Util.Warning("Não foi possível criar o diretório '" + OrigDir + "'.");
                        return false;
                    }
                    else return true;
                }
                
                Util.Warning("Não foi possível acessar '" + OrigDir + "'.\nO diretório não existe, ou você não tem permissão para acessa-lo.");
                Con.RemoteDir = LastDir;
                this.Refresh();
                return false;
                
            }

            return true;
        }

        public bool RemoveFile(String fileName)
        {
            String LastPath = this.Con.RemoteDir;
            this.Con.RemoteDir = this.Con.RemoteDir.TrimEnd('/') + '/' + fileName;

            this.ChangeMethod(WebRequestMethods.Ftp.DeleteFile);

            return this.Connect();
        }

        public bool SendFile(Arquivo file)
        {

            FileInfo fileInf = new FileInfo(file.FullPath);
            FileStream fs;
            Stream strm;

            // The buffer size is set to 2kb
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];

            Int32 contentLen;
            Int64 contentTotal;
            Int16 percent;

            

            String LastPath = this.Con.RemoteDir;
            
            this.Con.RemoteDir = this.Con.RemoteDir.TrimEnd('/') + '/' + file.Name;
            this.ReCreate();
            
            // Coloca em modo de Upload
            this.ChangeMethod(WebRequestMethods.Ftp.UploadFile);
            
            this.Request.UseBinary = true;


            try
            {
                // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
                fs = fileInf.OpenRead();

                // Stream to which the file to be upload is written
                strm = this.Request.GetRequestStream();

                // Read from the file stream 2kb at a time
                contentLen = fs.Read(buff, 0, buffLength);
            }
            catch (Exception e)
            {
                Util.Error(e.Message);
                return false;
            }

            
            
            // Total size file
            contentTotal = Convert.ToInt64(fs.Length);
            Int64 contentSended = 0;
            percent = 0;
            string msg = "Enviado: ";

            Util.Echo("Diretório de destino: " + this.Con.RemoteDir);
            Util.Echo("Enviando arquivo: "+file.Name);
            Util.Echo(msg, ConsoleColor.Yellow, false);
            

            // Till Stream content ends
            while (contentLen != 0)
            {
                // Write Content from the file stream to the FTP Upload Stream
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);

                // Registra os bytes enviados para cálculo
                contentSended += contentLen;
                // Calcula porcentagem enviada
                percent = Convert.ToInt16((contentSended * 100) / contentTotal);
                
                // Imprime a porcentagem
                Console.SetCursorPosition(msg.Length, Console.CursorTop);
                Util.Echo(percent.ToString() + "%    ", ConsoleColor.Yellow, false);
                
            }
            Util.ConsoleUpLines(0);
            Util.Echo(msg + "100%    ", ConsoleColor.Green);
            // Close the file stream and the Request Stream
            strm.Close();
            fs.Close();

            // Volta para o diretório anterior
            this.Con.RemoteDir = LastPath;
            this.ReCreate();


            return this.Connect();
        }

        public bool FileExists(String fileName)
        {

            var host = "ftp://" + this.Con.Host.TrimEnd('/') + '/';
            if (Con.RemoteDir != null) host += Con.RemoteDir.Trim('/') + '/';

            var req = (FtpWebRequest)WebRequest.Create(host + fileName.TrimStart('/'));
            req.Credentials = new NetworkCredential(this.Con.User, this.Con.Pass);
            req.Method = WebRequestMethods.Ftp.GetDateTimestamp;// WebRequestMethods.Ftp.GetFileSize;

            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                return (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable);
            }
        }


        public bool TestConnection(){
            bool ret = true;
            try
            {
                this.Create();


                if (this.Connect())
                {
                    this.Close();
                    Util.Success("Conexão realizada com sucesso!");
                }
                else
                {
                    Util.Error(this.Error);
                    ret = false;
                }

                
            }
            catch (WebException e)
            {
                Util.Error("Falha na conexão: " + Con.Name);
                Util.Warning(e.Message);
                ret = false;
            }

            return ret;
        }
    }
}
