using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SkeletSharp
{
    /// <summary>
    /// Přepisuje rozšířenou syntaxi na základní příkazy skeletového jazyka.
    /// </summary>
    public class Preprocessor
    {
        private static readonly Regex IdentifierRegex = new Regex(
            @"\b[a-zA-Z][a-zA-Z0-9]*\b",
            RegexOptions.Compiled);

        private static readonly Regex ClearCommandRegex = new Regex(
            @"^\s*clear\s+([a-zA-Z][a-zA-Z0-9]*)\s*$",
            RegexOptions.Compiled);

        private static readonly Regex AssignCommandRegex = new Regex(
            @"^\s*([a-zA-Z][a-zA-Z0-9]*)\s*=\s*([a-zA-Z][a-zA-Z0-9]*)\s*$",
            RegexOptions.Compiled);

        private readonly string input;
        private string preprocessedSource;

        /// <summary>
        /// Zdrojový text po aplikování všech preprocesorových přepisů.
        /// </summary>
        public string Preprocessed => preprocessedSource ?? (preprocessedSource = DoPreprocess());

        /// <summary>
        /// Vytvoří preprocesor nad původním zdrojovým textem.
        /// </summary>
        public Preprocessor(string input)
        {
            this.input = input ?? string.Empty;
        }

        /// <summary>
        /// Přepíše příkaz clear var na cyklus, který proměnnou vynuluje.
        /// </summary>
        private static string ReplaceClear(string source)
        {
            return RewriteLines(source, line =>
            {
                if (IsCommentLine(line))
                    return line;

                Match match = ClearCommandRegex.Match(line);
                return match.Success ? CreateClearCode(match.Groups[1].Value) : line;
            });
        }

        /// <summary>
        /// Přepíše přiřazení var1 = var2 na základní příkazy jazyka.
        /// </summary>
        private static string ReplaceAssign(string source)
        {
            HashSet<string> usedVariableNames = CollectIdentifiers(source);
            int generatedVariableCounter = 1;

            return RewriteLines(source, line =>
            {
                if (IsCommentLine(line))
                    return line;

                Match match = AssignCommandRegex.Match(line);
                return match.Success
                    ? CreateAssignCode(
                        match.Groups[1].Value,
                        match.Groups[2].Value,
                        CreateInternalVariableName(usedVariableNames, ref generatedVariableCounter))
                    : line;
            });
        }

        /// <summary>
        /// Najde identifikátory použité v původním zdrojovém textu.
        /// </summary>
        private static HashSet<string> CollectIdentifiers(string source)
        {
            HashSet<string> identifiers = new HashSet<string>(StringComparer.Ordinal);
            foreach (Match match in IdentifierRegex.Matches(source))
                identifiers.Add(match.Value);

            return identifiers;
        }

        /// <summary>
        /// Vygeneruje interní pomocnou proměnnou, která nekoliduje se zdrojovým programem.
        /// </summary>
        private static string CreateInternalVariableName(HashSet<string> usedVariableNames, ref int generatedVariableCounter)
        {
            string variableName;
            do
            {
                variableName = "auxAssign" + generatedVariableCounter.ToString(System.Globalization.CultureInfo.InvariantCulture);
                generatedVariableCounter++;
            }
            while (usedVariableNames.Contains(variableName));

            usedVariableNames.Add(variableName);
            return variableName;
        }

        /// <summary>
        /// Určí, zda řádek začíná komentářem.
        /// </summary>
        private static bool IsCommentLine(string line)
        {
            return line.TrimStart().StartsWith("#", StringComparison.Ordinal);
        }

        /// <summary>
        /// Aplikuje transformační funkci na jednotlivé řádky zdrojového textu.
        /// </summary>
        private static string RewriteLines(string source, Func<string, string> rewriteLine)
        {
            string[] lines = source.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            StringBuilder result = new StringBuilder();

            foreach (string line in lines)
                result.AppendLine(rewriteLine(line));

            return result.ToString();
        }

        /// <summary>
        /// Vytvoří základní skeletový kód pro vynulování proměnné.
        /// </summary>
        private static string CreateClearCode(string variableName)
        {
            return $$"""
                while {{variableName}}<> 0 do
                    decr {{variableName}}
                end
                """;
        }

        /// <summary>
        /// Vytvoří základní skeletový kód pro kopii hodnoty mezi proměnnými.
        /// </summary>
        private static string CreateAssignCode(string leftVariableName, string rightVariableName, string helperVariableName)
        {
            if (leftVariableName == rightVariableName)
                return CreateSelfAssignCode(leftVariableName, helperVariableName);

            // Pomocná proměnná musí mít unikátní interní název. Původní pevné "aux" mohlo
            // kolidovat s proměnnou uživatele a změnit výsledek interpretovaného programu.
            return $$"""

                while {{helperVariableName}}<> 0 do
                    decr {{helperVariableName}}
                end

                while {{leftVariableName}}<> 0 do
                    decr {{leftVariableName}}
                end

                while {{rightVariableName}}<> 0 do
                    incr {{helperVariableName}}
                    decr {{rightVariableName}}
                end

                while {{helperVariableName}} <> 0 do
                    incr {{leftVariableName}}
                    incr {{rightVariableName}}
                    decr {{helperVariableName}}
                end
                """;
        }

        /// <summary>
        /// Vytvoří kód pro přiřazení proměnné do sebe sama bez ztráty její hodnoty.
        /// </summary>
        private static string CreateSelfAssignCode(string variableName, string helperVariableName)
        {
            // U var = var nesmíme použít běžné přiřazení, protože to nejdřív nulovalo cílovou
            // proměnnou. Hodnotu proto jen dočasně přesuneme do pomocné proměnné a zase zpět.
            return $$"""

                while {{helperVariableName}}<> 0 do
                    decr {{helperVariableName}}
                end

                while {{variableName}}<> 0 do
                    incr {{helperVariableName}}
                    decr {{variableName}}
                end

                while {{helperVariableName}} <> 0 do
                    incr {{variableName}}
                    decr {{helperVariableName}}
                end
                """;
        }

        /// <summary>
        /// Provede všechny podporované preprocesorové přepisy.
        /// </summary>
        private string DoPreprocess()
        {
            return ReplaceClear(ReplaceAssign(input));
        }
    }
}
