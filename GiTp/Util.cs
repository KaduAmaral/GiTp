using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace GiTp
{
    class Util
    {
        /// <summary>Pausa a execução através de um ReadKey</summary>
        /// <param name="message">Mostra uma mensagem para continuar</param>
        /// <returns></returns>
        public static void Pause(bool message = true)
        {
            if (message)
                Warning("Pressione qualquer tecla para continuar...");

            Console.ReadKey();
        }

        /// <summary>
        /// Escreve no console através do Console.Write/WriteLine na cor informada pelo parâmetro color
        /// </summary>
        /// <param name="msg">Mensagem a ser exebida</param>
        /// <param name="color">Cor da mensagem</param>
        /// <param name="newline">Nova linha</param>
        public static void Echo(String msg, ConsoleColor color = ConsoleColor.White, Boolean newline = true)
        {
            Console.ForegroundColor = color;
            if (newline) Console.WriteLine(msg);
            else Console.Write(msg);
            Console.ResetColor();
        }

        /// <summary>
        /// Escreve no console através do Console.Write/WriteLine na cor informada pelo parâmetro color
        /// </summary>
        /// <param name="msg">Mensagem a ser exebida</param>
        /// <param name="newline">Nova linha</param>
        public static void Echo(String msg, Boolean newline)
        {
            Echo(msg, ConsoleColor.White, newline);
        }

        /// <summary>Mostra uma mensagem de "alerta" em amarelo.</summary>
        /// <param name="msg">Mensagem a ser exibida</param>
        /// <returns></returns>
        public static void Warning(String msg)
        {
            Util.Echo(msg, ConsoleColor.Yellow);
        }

        /// <summary>
        /// Imprime uma linha vazia;
        /// </summary>
        public static void EmptyLine()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// Mensagem escrita em vermelho
        /// </summary>
        /// <param name="msg">Mensagem a ser exibida</param>
        /// <returns>Retorna sempre false</returns>
        public static bool Error(string msg)
        {
            Util.Echo(msg, ConsoleColor.Red);
            return false;
        }

        /// <summary>
        /// Mensagem em verde
        /// </summary>
        /// <param name="msg">Mensagem a ser exibida</param>
        /// <returns>Retorna sempre true</returns>
        public static bool Success(string msg)
        {
            Util.Echo(msg, ConsoleColor.Green);
            return true;
        }

        /// <summary>
        /// Captura opções S/N (Sim ou Não)
        /// </summary>
        /// <param name="msg">Pergunta para resposta de Sim ou Não</param>
        /// <returns>Retorna true caso usuário informe S e false caso N</returns>
        public static bool GetYesNo(string msg)
        {
            return ( Char.ToUpper( GetKeyPress(msg + " (S para Sim, N para Não) ", new Char[] { 'S', 'N' }) ) == 'S');
        }


        /// <summary>
        /// Captura o caracter informado
        /// </summary>
        /// <param name="msg">Mensagem / Pergunta</param>
        /// <param name="validChars">Array de caractéres válidos</param>
        /// <returns>Char</returns>
        public static Char GetKeyPress(String msg, Char[] validChars)
        {
            ConsoleKeyInfo keyPressed;
            bool valid = false;

            Console.WriteLine();
            do
            {
                Console.Write(msg);
                keyPressed = Console.ReadKey();
                Console.WriteLine();
                if (Array.Exists(validChars, ch => ch.Equals(Char.ToUpper(keyPressed.KeyChar))))
                    valid = true;

            } while (!valid);
            return keyPressed.KeyChar;
        }

        public static String StrToHex(string input)
        {
            string output = "";
            char[] values = input.ToCharArray();
            foreach (char letter in values)
            {
                // Get the integral value of the character.
                int value = Convert.ToInt32(letter);
                // Convert the decimal value to a hexadecimal value in string form.
                string hexOutput = String.Format("{0:X}", value);
                output += hexOutput + " ";
                //Console.WriteLine("Hexadecimal value of {0} is {1}", letter, hexOutput);
            }
            return output;
        }

        public static String HexToStr(string hexValues)
        {
            string output = "";
            string[] hexValuesSplit = hexValues.Split(' ');
            foreach (String hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer.
                if (IsHex(hex))
                {
                    int value = Convert.ToInt32(hex, 16);
                    // Get the character corresponding to the integral value.
                    string stringValue = Char.ConvertFromUtf32(value);
                    char charValue = (char)value;
                    //Console.WriteLine("hexadecimal value = {0}, int value = {1}, char value = {2} or {3}",
                    //                    hex, value, stringValue, charValue);
                    output += stringValue;
                }
            }

            return output;
        }

        public static bool IsHex(String inputString)
        {
            int dummy;
            bool isHex = int.TryParse(inputString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dummy);
            return isHex;
        }


        public static string Hr()
        {
            return new String('─', Console.BufferWidth);
        }

        public static void Hr(Boolean print)
        {
            Echo(Hr(), ConsoleColor.Gray, false);
        }

        public static string Hr(Int32 size)
        {
            return Hr().Substring(0, size);
        }

        public static string Hr(Double size)
        {
            string output = Hr();
            return output.Substring(0, Convert.ToInt32(output.Length * size));
        }

        public static void ConsoleUpLines(Int32 qtdLines = 1)
        {
            int Line = Console.CursorTop - qtdLines;
            if (Line < 0) Line = 0;
            Console.SetCursorPosition(0, Line);
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static string GetDir(String dir)
        {
            string cur = Environment.CurrentDirectory;
            string bdir = ".." + Path.DirectorySeparatorChar;
            if ('/' != Path.DirectorySeparatorChar)
                dir = dir.Replace('/', Path.DirectorySeparatorChar);
            else
                dir = dir.Replace('\\', Path.DirectorySeparatorChar);

            dir = dir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            string nov = null;
            if (dir == bdir){
                DirectoryInfo parent = Directory.GetParent(cur);
                if (parent != null)
                    nov = parent.FullName;
            }
            else if (dir.Contains(":" + Path.DirectorySeparatorChar) && Directory.Exists(dir))
                nov = @dir;
            else if (Directory.Exists(cur + Path.DirectorySeparatorChar + dir))
                nov = cur + Path.DirectorySeparatorChar + dir;

            return nov;
        }

    }
}
