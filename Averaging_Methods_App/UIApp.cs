using System;
using System.Collections.Generic;
using System.Text;
using AveragingMethods;

namespace Averaging_Methods_App
{
    internal class UIApp
    {
        public UIApp()
        {
            Tool = new AveragingTool();
        }

        AveragingTool Tool;

        public void Run()
        {
            Form form = new Form();
            while (true)
            {
                try
                {
                    Console.Write(form.GetMessage());
                    string message = Console.ReadLine();
                    if (message == "") continue;
                    if (message == "?" || message == "help")
                    {
                        Form.GetHelp(Tool.GetMethods());
                        continue;
                    }
                    if (message == "exit") break;
                    if (message == "stop")
                    {
                        form = new Form();
                        continue;
                    }
                    if (form.Next(message)) ExecuteCommand(form);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        void ExecuteCommand(Form form)
        {
            string path = Path.GetFullPath(form.Inputs[1]);
            string output_path = Path.GetFullPath(form.Inputs[3]);
            List<int> indices = Tools.ParseSquareBrackets(form.Inputs[4]).Args.Select(int.Parse).ToList();
            if (form.Inputs[0] == "d")
            {               
                List<(string Path, List<Value> Values)> data = ParseDirectory(path); 
                if (form.Inputs[2] == "d")
                {
                    foreach (var d in data)
                    {
                        string final_path = Path.Combine(Path.GetFullPath(output_path), Path.GetRelativePath(path, d.Path));
                        Directory.CreateDirectory(Path.GetDirectoryName(final_path));
                        if (d.Values.Count != 0) File.WriteAllLines(final_path, Tool.GetResult(d.Values, indices));
                    }
                }
                else
                {
                    Console.WriteLine($"Entered and output path: {output_path}");
                    List<string> lines = new List<string>();
                    foreach (var d in data)
                    {
                        lines.Add("Source: " + Path.GetRelativePath(path, d.Path));
                        if (d.Values.Count != 0) lines.AddRange(Tool.GetResult(d.Values, indices));
                        lines.Add("\n");
                    }
                    File.WriteAllLines(Path.GetFullPath(output_path), lines.ToArray());
                }
            }
            else
            {
                (string Path, List<Value> Values) data = ParseFile(path);
                File.WriteAllLines(Path.GetFullPath(output_path), Tool.GetResult(data.Values, indices));
            }

            Console.WriteLine("The results are ready and saved successfully!");
        }

        List<(string Path, List<Value> Values)> ParseDirectory(string path)
        {
            List<(string Path, List<Value> Values)> values = new();

            foreach (var file in Directory.GetFiles(path))
            {
                values.Add(ParseFile(file));
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                values.AddRange(ParseDirectory(dir));
            }

            return values;
        }
        (string Path, List<Value> Values) ParseFile(string path)
        {
            return (Path.GetFullPath(path), ParseValues(File.ReadAllLines(path)));
        }
        List<Value> ParseValues(string[] rows)
        {
            List<Value> values = new List<Value>();
            foreach (string row in rows)
            {
                string[] splitted = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (splitted.Length != 2) continue;
                double val, dval;
                if (!double.TryParse(splitted[0], out val)) continue;
                if (!double.TryParse(splitted[1].Replace("{", "").Replace("}", ""), out dval)) continue;
                values.Add(new Value() { Val = val, DVal = dval });
            }
            return values;
        }
    }
}
