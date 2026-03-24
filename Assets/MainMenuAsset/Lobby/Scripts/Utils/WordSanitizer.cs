using System;
using System.Text.RegularExpressions;

public class WordSanitizer
{
    public static string SanitizeWords(string input)
    {
        // Split the input into lines using commas and newlines as separators
        string[] lines = input.Split(new char[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Initialize an empty result string
        string result = "";

        // Loop through each line (word)
        foreach (string line in lines)
        {
            // Remove ":1" from the end of the word (if present)
            string cleanedWord = Regex.Replace(line, @":1$", "");

            // Add the cleaned word, enclosed in double quotation marks, to the result
            result += $"\"{cleanedWord}\",\n";
        }

        // Remove the trailing comma and newline
        result = result.TrimEnd(',', '\n');

        return result;
    }
}

