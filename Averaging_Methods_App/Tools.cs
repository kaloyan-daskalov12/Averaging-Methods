using System;
using System.Collections.Generic;
using System.Text;

namespace Averaging_Methods_App
{
    internal class Tools
    {
        /// <summary>
        /// Parses expression of type arr[*args*]
        /// </summary>
        /// <param name="input">The string to be parsed</param>
        /// <returns>A pair of the keyword before the brackets (not null), and a list of the arguments (possibly null)</returns>
        public static (string KeyWord, List<string>? Args) ParseSquareBrackets(string input)
        {
            if (input == null || input == "") return ("", null);
            if (!input.Contains('[')) return (input, null);
            int i = 0;
            while (i < input.Length && input[i] != '[') i++;
            string keyword = input.Substring(0, i);
            int j = i + 1;
            while (j < input.Length && input[j] != ']') j++;
            string indices_str = input.Substring(i + 1, j - i - 1);
            List<string> indices = indices_str.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            return (keyword, indices);
        }
    }
}
