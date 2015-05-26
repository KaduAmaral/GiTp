using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using GiTp.Objects;
using GiTp;

namespace GiTp
{
    class Help
    {
        public const String Prefix = "GiTp ~ ";
        private const String ComandsXML = "Commands.xml";

        IDictionary<string, CommandWiki> Commands = new Dictionary<string, CommandWiki>();
        public String Command;
        public String Info;
        XmlDocument xml;

        public Help(Command cmd)
        {
            this.Command = cmd.Arguments.Count == 1 ? cmd.Arguments.First().Value : cmd.Name;
            this.Load();
            this.Print();
            //this.Wiki();
            
        }

        private void Load()
        {
            try
            {
                xml = new XmlDocument();
                xml.Load(AppDomain.CurrentDomain.BaseDirectory + ComandsXML);
            }
            catch (Exception e)
            {
                Util.Error("Não foi possível abrir o dicionário de comandos.");
                Util.Warning(e.Message);
            }
            
        }

        private void PrintHelp()
        {
            String xPath = "/root/commands/command";
            XmlNodeList nodes = xml.SelectNodes(xPath);

            int ml = 3;
            int ns = 20;

            string sp = " " + Util.Hr(1) + " ";

            Util.EmptyLine();
            Util.EmptyLine();

            foreach (XmlNode node in nodes)
            {

                String summary = node.SelectSingleNode("summary").InnerText; //xml.SelectSingleNode(xPath + "/summary").InnerText;
                String name = node.Attributes["name"].Value;

                String line = name.MarginLeft(ml).PadRight(ns) + sp;

                int sw = Console.BufferWidth - 1 - line.Length;

                if (sw < 1) sw = 10;

                Util.Echo(line, false);

                if (summary.Length < sw)
                    Util.Echo(summary);
                else
                {

                    string tmps = summary.Substring(0, sw);
                    Util.Echo(tmps);

                    summary = summary.Substring(sw);

                    do
                    {
                        int s = summary.Length < sw ? summary.Length : sw;
                        tmps = summary.Substring(0, s);
                        Util.Echo(tmps.MarginLeft(line.Length));

                        summary = summary.Length > s ? summary.Substring(s) : null;
                    } while (summary != null);
                }
                

            }

            Util.EmptyLine();

            Util.Echo("Para mais detalhes sobre um comando específico, informe 'help' + nome do comando.", ConsoleColor.Gray);
            Util.Echo("Por exemplo: help connection", ConsoleColor.Gray);

            Util.EmptyLine();
            Util.EmptyLine();
        }

        private void Print()
        {

            if (this.Command == "help")
            {
                this.PrintHelp();
                return;
            }

            String xPath = "/root/commands/command[@name='" + this.Command + "']";
            XmlNode C = xml.SelectSingleNode(xPath);
            Util.EmptyLine();
            if (C == null)
            {
                Util.Echo("O comando '"+this.Command+"' não existe ou não foi documentado ainda.", ConsoleColor.Magenta);
                Util.EmptyLine();
                return;
            }
                

            String summary = xml.SelectSingleNode(xPath + "/summary").InnerText;
            XmlNodeList argumentlist = xml.SelectNodes(xPath + "/arguments/argument");
            XmlNodeList paramlist = xml.SelectNodes(xPath + "/params/param");
            XmlNodeList examples = xml.SelectNodes(xPath + "/examples/example");


            Util.EmptyLine();
            Util.Echo("Comando: ", ConsoleColor.Gray, false);
            int size = 10 + this.Command.Length;
            Util.Echo(this.Command, ConsoleColor.Cyan);
            Util.Echo(summary.MarginLeft(3), ConsoleColor.Yellow);

            if (argumentlist.Count > 0)
            {
                Util.EmptyLine();
                Util.Echo("Argumentos", ConsoleColor.Gray);

                foreach (XmlNode arg in argumentlist)
                    Util.Echo(("> " + arg.InnerText).MarginLeft(3));

                    
            }

            if (paramlist.Count > 0)
            {
                Util.EmptyLine();
                Util.Echo("Parâmetros:", ConsoleColor.Gray);

                foreach (XmlNode arg in paramlist) {
                    string p = arg.Attributes["name"].Value;
                    p =  p.Trim().PadRight(10) + " " + Util.Hr(1) + " " + 
                         arg.Attributes["summary"].Value.Trim();

                    Util.Echo(("> " + p).MarginLeft(3));
                }

            }

            if (examples.Count > 0)
            {
                Util.EmptyLine();
                Util.Echo("Exemplos:", ConsoleColor.Gray);

                foreach (XmlNode arg in examples)
                    Util.Echo(arg.InnerText.MarginLeft(3));

                Util.EmptyLine();
            }

            Util.EmptyLine();
        }

        private void Wiki()
        {
            
            // Comando HELP
            Commands["help"] = new CommandWiki() { 
                Name = "help",
                Summary = "Lista de comandos.",
                Example = "help"
            };
            
            // Comando SET
            Commands["set"] = new CommandWiki() {
                Name = "set",
                Summary = "Seta o valor de uma variável de execução.",
                Example = "set repository c:/caminho/do/repositorio"
            };
            Commands["set"].Params.Add(new ParamWiki()
            {
                Name = "var",
                Summary = "Nome da variável (repository, host, user, pass...);"
            });
            Commands["set"].Params.Add(new ParamWiki()
            {
                Name = "value",
                Summary = "Novo valor da variável;"
            });



            // Comando GET
            Commands["get"] = new CommandWiki()
            {
                Name = "get",
                Summary = "Retorna o valor da variável informada.",
                Example = "get repository"
            };
            Commands["get"].Params.Add(new ParamWiki()
            {
                Name = "var",
                Summary = "Nome da variável (repository, host, user...);"
            });

            // Comando EXIT
            Commands["exit"] = new CommandWiki()
            {
                Name = "exit",
                Summary = "Encerra a execução do programa.",
                Example = "exit"
            };

        }

        public string Draw()
        {
            Info = Prefix + "Wiki\n------------------\n";
            
            if (!Commands.ContainsKey(Command))
            {
                Info += "Desculpe, não sei nada sobre '" + Command + "'. :(\nTente outro comando.";
                return Info;
            }

            CommandWiki C = Commands[Command];

            int size = C.Name.Length + 5;

            Info += C.Name + "   - " + C.Summary + "\n";


            int psize;
            foreach (ParamWiki P in C.Params)
            {
                psize = 10 - P.Name.Length;
                if (psize < 1) psize = 3;

                Info +=  " > ".PadLeft(size) + P.Name + " - ".PadRight(psize) + P.Summary + "\n";
            }

            return Info;
        }
    }
}
