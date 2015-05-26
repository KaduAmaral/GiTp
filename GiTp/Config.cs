using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using EnCryptDecrypt;

namespace GiTp
{
    class Config
    {
        public static String SecurityKey = "#Na(¨$3O0uhA-ka+s&¨bL:";
        static XmlDocument xml;
        static String filedata = "";
        static String appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static String appfolder = appdata+"/GiTp/";
        static String filepath = appfolder+"sys.gtd";

        public Config()
        {
            if (!Directory.Exists(appfolder))
                Directory.CreateDirectory(appfolder);

            if (!File.Exists(@filepath))
                CreateConfigFile();

            if (ReadConfig())
                ParseXml();
        }

        private static bool ReadConfig()
        {
            try
            {
                String filetext = File.ReadAllText(@filepath);
                String textdecrypted = CryptorEngine.Decrypt(filetext, true);
                filedata = Util.HexToStr( textdecrypted );
                return true;
            }
            catch (Exception e)
            {
                Util.Error("O arquivo de configurações não foi encontrado... ");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static void ParseXml()
        {
            xml = new XmlDocument();
            xml.LoadXml(filedata); // suppose that myXmlString contains "<Names>...</Names>"
        }

        private static bool SaveConfigXML()
        {
            Config.filedata = xml.OuterXml;
            return SaveConfigFile();
        }

        private static bool CreateConfigFile()
        {
            Config.filedata = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
                              "<root>\n"+
                              "<Connections></Connections>\n" +
                              "</root>";
            return SaveConfigFile();
        }

        private static bool SaveConfigFile()
        {
            String hexText = Util.StrToHex(Config.filedata);
            String filedata = CryptorEngine.Encrypt(hexText, true);
            Boolean ret = false;
            try
            {
                File.WriteAllText(@filepath, filedata);
                ret = true;
            }
            catch (Exception e)
            {
                Util.Error("Não foi possível salvar o arquivo de configuração...");
                Util.Warning(e.Message);
            }

            if (ReadConfig())
                ParseXml();

            return ret;
        }

        public bool DeleteConnection(Connection con)
        {
            XmlNode ConnectionNode = xml.SelectSingleNode("root/Connections/Connection[@name='" + con.Name + "']");
            if (ConnectionNode != null) {
                xml.SelectSingleNode("root/Connections").RemoveChild(ConnectionNode);
                return SaveConfigXML();
            }
            else
            {
                return Util.Error("A conexão '" + con.Name + "' não foi localizada.");
            }

            
        }

        public void SaveConnection(Connection con)
        {

            FTPConnection FTPc = new FTPConnection(con);

            if (!FTPc.TestConnection())
            {
                Util.Warning("Não foi possível conectar com os dados informados...");
                if (!Util.GetYesNo("Deseja salvar assim mesmo?"))
                    return;
            }


            XmlNode ConnectionNode = xml.SelectSingleNode("root/Connections/Connection[@name='" + con.Name + "']");

            Boolean nova = (ConnectionNode == null);

            XmlNode HostNode;
            XmlNode UserNode;
            XmlNode PassNode;
            XmlNode PortNode;
            XmlNode FolderNode;
            XmlNode LocalNode;
            XmlNode RemoteNode;


            if (nova){
                ConnectionNode = xml.CreateNode(XmlNodeType.Element, "Connection", null);
                XmlAttribute ConName = xml.CreateAttribute("name");
                if (con.Name == null)
                {
                    Console.Write("Informe um nome único para esta conexão: ");
                    con.Name = Console.ReadLine();
                }
                ConName.Value = con.Name;
                
                ConnectionNode.Attributes.Append(ConName);

                ConnectionNode.AppendChild(xml.CreateNode(XmlNodeType.Element, "host", null));
                ConnectionNode.AppendChild(xml.CreateNode(XmlNodeType.Element, "user", null));
                ConnectionNode.AppendChild(xml.CreateNode(XmlNodeType.Element, "pass", null));
                ConnectionNode.AppendChild(xml.CreateNode(XmlNodeType.Element, "port", null));
                
                FolderNode = xml.CreateNode(XmlNodeType.Element, "folder", null);
                LocalNode = xml.CreateNode(XmlNodeType.Element, "local", null);
                RemoteNode = xml.CreateNode(XmlNodeType.Element, "remote", null);

                FolderNode.AppendChild(LocalNode);
                FolderNode.AppendChild(RemoteNode);

                ConnectionNode.AppendChild(FolderNode);

                
            }
            else
            {
                if (!Util.GetYesNo("Confirma a atualização da conexão com o nome '" + con.Name+ "'?"))
                    return;
            }

            HostNode = ConnectionNode.SelectSingleNode("host");
            UserNode = ConnectionNode.SelectSingleNode("user");
            PassNode = ConnectionNode.SelectSingleNode("pass");
            PortNode = ConnectionNode.SelectSingleNode("port");
            LocalNode = ConnectionNode["folder"]["local"];
            RemoteNode = ConnectionNode["folder"]["remote"];

            HostNode.InnerText   = con.Host;
            UserNode.InnerText   = con.User;
            PassNode.InnerText   = CryptorEngine.Encrypt(con.Pass, true);
            PortNode.InnerText   = con.Port;
            LocalNode.InnerText  = con.LocalDir;
            RemoteNode.InnerText = con.RemoteDir;

            xml.SelectSingleNode("/root/Connections").AppendChild(ConnectionNode);

            if (SaveConfigXML())
                Util.Success("Configurações salvas com sucesso.");
            else
                Util.Error("Não foi possível salvar as configurações");

        }


        public Connection GetConnection(String name)
        {
            Connection connect = new Connection();

            XmlNode ConnectionNode = xml.SelectSingleNode("root/Connections/Connection[@name='" + name + "']");

            if (ConnectionNode == null) return null;

            connect.Name = name;
            connect.Host = ConnectionNode.SelectSingleNode("host").InnerText;
            connect.User = ConnectionNode.SelectSingleNode("user").InnerText;
            connect.Pass = CryptorEngine.Decrypt(ConnectionNode.SelectSingleNode("pass").InnerText, true);
            connect.Port = ConnectionNode.SelectSingleNode("port").InnerText;
            connect.LocalDir = ConnectionNode["folder"]["local"].InnerText;
            connect.RemoteDir = ConnectionNode["folder"]["remote"].InnerText;

            return connect;
        }

        public List<string> ListConnectionName()
        {
            List<string> cons = new List<string>(); ;

            XmlNodeList aNodes = xml.SelectNodes("/root/Connections/Connection");
            foreach (XmlNode node in aNodes)
            {
                cons.Add(node.Attributes["name"].Value);
            }

            return cons;
        }
    }
}
