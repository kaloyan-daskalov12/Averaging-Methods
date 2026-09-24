using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Averaging_Methods_App
{
    internal class Form
    {
        static string[] Messages = new string[] {
                "Input type (file or directory) [f/d]: ",
                "Input path: ",
                "Output type (single file ot directory) [f/d]: ",
                "Output path: ",
                "Methods (separated with ',' / type '?' for more info): "
            };
        static Dictionary<byte, Func<string, bool>> Validate = new Dictionary<byte, Func<string, bool>>
        {
            {0, (string m) => (m == "f" || m == "d") },
            {1, (string m) => (Path.Exists(m)) },
            {2, (string m) => (m == "f" || m == "d") },
            {3, (string m) => (true) },
            {4, (string m) => Tools.ParseSquareBrackets(m).Args != null }
        };
        public static void GetHelp(string tools)
        {
            Console.WriteLine(
                "[Input type] This app takes two types of arguments: text file with inner format: \"value {uncertainty}\", or a directory, containing folders and subfolders, cntaining files with this format.\n" +
                "[Input path] This is the path to the imput values. Make sure the path leads to the right type specified in the previous prompt.\n" +
                "[Output type] Specifies whether you want the output to be a directory of a single file. If the input is from a single file, it doesn't matter.\n" +
                "[Output path] The path where you want to save the result. Can be a directory or file.\n" +
                "[Methods] A set of indices '[]', separated with ',', specifying which averaging methods to be used from the following list:\n" +
                tools
                );
        }

        public Form() 
        {
            Inputs = new string[Messages.Length];

            i = 0;
        }

        public string[] Inputs;

        byte i;
        public string GetMessage()
        {
            return Messages[i];
        }
        public bool Next(string message)
        {
            message = message.Replace("\"", "");
            if (i == Inputs.Length - 1)
            {
                if (Validate[i](message)) Inputs[i] = message;
                else throw new Exception("Invalid input!");
                return true;
            }
            if (Validate[i](message)) Inputs[i++] = message;
            else throw new Exception("Invalid input!");
            return false;
        }
    }
}
