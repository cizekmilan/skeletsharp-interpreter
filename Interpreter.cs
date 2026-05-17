using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SkeletSharp
{
    /// <summary>
    /// Vykonává zdrojový kód skeletového jazyka.
    /// </summary>
    public class Interpreter
    {
        private readonly Preprocessor preprocessor;
        private readonly Lexer lexer;
        private readonly Stack<WhileLoopContext> whileLoopStack;
        private readonly Dictionary<string, RuntimeValue> variables;
        private readonly bool displayComments;

        private Token previousToken;
        private Token currentToken;
        private SourcePosition linePosition;
        private SourcePosition identifierPosition;
        private bool shouldExit;

        /// <summary>
        /// Vytvoří interpreter nad zadaným zdrojovým textem.
        /// </summary>
        public Interpreter(string input, bool displayComments)
        {
            preprocessor = new Preprocessor(input);
            lexer = new Lexer(preprocessor.Preprocessed);
            variables = new Dictionary<string, RuntimeValue>();
            this.displayComments = displayComments;
            whileLoopStack = new Stack<WhileLoopContext>();
        }

        /// <summary>
        /// Vrátí textový seznam všech známých proměnných a jejich hodnot.
        /// </summary>
        public string ListAllVariables()
        {
            List<string> variableNames = new List<string>(variables.Keys);
            variableNames.Sort(StringComparer.Ordinal);

            StringBuilder result = new StringBuilder();
            foreach (string variableName in variableNames)
                result.AppendLine(variableName + ": " + variables[variableName].IntegerValue.ToString(CultureInfo.InvariantCulture));

            return result.ToString();
        }

        /// <summary>
        /// Vrátí hodnotu proměnné podle jejího názvu.
        /// </summary>
        public RuntimeValue GetVariable(string name)
        {
            if (!variables.ContainsKey(name))
                throw new SkeletonRuntimeException("Variable '" + name + "' does not exist.");

            return variables[name];
        }

        /// <summary>
        /// Nastaví hodnotu proměnné, případně proměnnou vytvoří.
        /// </summary>
        public void SetVariable(string name, RuntimeValue value)
        {
            variables[name] = value;
        }

        /// <summary>
        /// Spustí interpretaci programu.
        /// </summary>
        public void Execute()
        {
            shouldExit = false;
            ReadNextToken();
            while (!shouldExit)
                ExecuteLine();
        }

        /// <summary>
        /// Vyhodí syntaktickou chybu doplněnou o aktuální řádek.
        /// </summary>
        private void ThrowSyntaxError(string text)
        {
            throw new SkeletonSyntaxException(text, linePosition);
        }

        /// <summary>
        /// Ověří, že aktuální token odpovídá očekávanému tokenu.
        /// </summary>
        private void Expect(Token expectedToken)
        {
            if (currentToken != expectedToken)
                ThrowSyntaxError("Expected " + expectedToken + ", found " + currentToken);
        }

        /// <summary>
        /// Načte další token z lexeru.
        /// </summary>
        private Token ReadNextToken()
        {
            previousToken = currentToken;
            currentToken = lexer.GetToken();

            if (currentToken == Token.EndOfFile && previousToken == Token.EndOfFile)
                ThrowSyntaxError("Unexpected end of file.");

            return currentToken;
        }

        /// <summary>
        /// Zpracuje jeden řádek zdrojového programu.
        /// </summary>
        private void ExecuteLine()
        {
            while (currentToken == Token.NewLine)
                ReadNextToken();

            if (currentToken == Token.EndOfFile)
            {
                shouldExit = true;
                return;
            }

            linePosition = lexer.TokenPosition;
            identifierPosition = lexer.IdentifierPosition;

            ExecuteStatement();

            if (currentToken != Token.NewLine && currentToken != Token.EndOfFile)
                ThrowSyntaxError("Expected new line, found " + currentToken);
        }

        /// <summary>
        /// Zpracuje komentářový řádek.
        /// </summary>
        private void ExecuteComment()
        {
            if (displayComments)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(lexer.CommentText);
                Console.ResetColor();
            }

            while (currentToken != Token.NewLine && currentToken != Token.EndOfFile)
                ReadNextToken();
        }

        /// <summary>
        /// Rozpozná a vykoná příkaz podle aktuálního klíčového slova.
        /// </summary>
        private void ExecuteStatement()
        {
            Token keyword = currentToken;
            ReadNextToken();

            switch (keyword)
            {
                case Token.Input: ExecuteInput(); break;
                case Token.Decrement: ExecuteDecrement(); break;
                case Token.Increment: ExecuteIncrement(); break;
                case Token.Output: ExecuteOutput(); break;
                case Token.While: ExecuteWhile(); break;
                case Token.End: ExecuteEnd(); break;
                case Token.Comment: ExecuteComment(); break;
                case Token.EndOfFile:
                    shouldExit = true;
                    break;
                default:
                    ThrowSyntaxError("Expected keyword, found " + keyword);
                    break;
            }
        }

        /// <summary>
        /// Vykoná příkaz incr.
        /// </summary>
        private void ExecuteIncrement()
        {
            while (true)
            {
                Expect(Token.Identifier);
                if (!variables.ContainsKey(lexer.Identifier))
                {
                    variables.Add(lexer.Identifier, RuntimeValue.CreateRandom());
                }
                else
                {
                    UInt32 oldValue = variables[lexer.Identifier].IntegerValue;
                    if (oldValue != UInt32.MaxValue)
                        SetVariable(lexer.Identifier, new RuntimeValue(++oldValue));
                }

                if (ReadNextToken() == Token.NewLine)
                    break;
            }
        }

        /// <summary>
        /// Vykoná příkaz decr.
        /// </summary>
        private void ExecuteDecrement()
        {
            while (true)
            {
                Expect(Token.Identifier);
                if (!variables.ContainsKey(lexer.Identifier))
                {
                    variables.Add(lexer.Identifier, RuntimeValue.CreateRandom());
                }
                else
                {
                    UInt32 oldValue = variables[lexer.Identifier].IntegerValue;
                    if (oldValue != 0)
                        SetVariable(lexer.Identifier, new RuntimeValue(--oldValue));
                }

                if (ReadNextToken() == Token.NewLine)
                    break;
            }
        }

        /// <summary>
        /// Vykoná příkaz output.
        /// </summary>
        private void ExecuteOutput()
        {
            while (true)
            {
                Expect(Token.Identifier);
                Console.WriteLine(lexer.Identifier + ": " + GetVariable(lexer.Identifier));
                if (ReadNextToken() == Token.NewLine)
                    break;
            }
        }

        /// <summary>
        /// Vykoná příkaz input.
        /// </summary>
        private void ExecuteInput()
        {
            while (true)
            {
                Expect(Token.Identifier);

                if (!variables.ContainsKey(lexer.Identifier))
                    variables.Add(lexer.Identifier, new RuntimeValue());

                Console.Write(lexer.Identifier + ": ");
                string input = Console.ReadLine();

                if (input != null && UInt32.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint parsedValue))
                    SetVariable(lexer.Identifier, new RuntimeValue(parsedValue));
                else
                    ThrowSyntaxError("Unrecognized or unsupported value.");

                if (ReadNextToken() == Token.NewLine)
                    break;
            }
        }

        /// <summary>
        /// Zpracuje konec cyklu while.
        /// </summary>
        private void ExecuteEnd()
        {
            if (whileLoopStack.Count == 0)
                ThrowSyntaxError("Unexpected end without matching while.");

            string variableName = whileLoopStack.Peek().VariableName;
            if (variables[variableName].IntegerValue == 0)
            {
                whileLoopStack.Pop();
                while (currentToken != Token.NewLine && currentToken != Token.EndOfFile)
                    ReadNextToken();
            }
            else
            {
                SourcePosition whilePosition = whileLoopStack.Peek().WhilePosition;
                lexer.LastChar = '\r';
                // Skok vrací lexer těsně před uložené while, aby další čtení začalo znovu na stejném příkazu.
                if (whilePosition.Pointer > -1)
                    lexer.MoveTo(new SourcePosition(whilePosition.Pointer - 1, whilePosition.Line, whilePosition.Column));
                else
                    lexer.MoveTo(whilePosition);
                currentToken = Token.NewLine;
            }
        }

        /// <summary>
        /// Zpracuje začátek cyklu while.
        /// </summary>
        private void ExecuteWhile()
        {
            Expect(Token.Identifier);
            string variableName = lexer.Identifier;

            ReadNextToken();
            Expect(Token.NotEqual);
            ReadNextToken();
            Expect(Token.Zero);
            ReadNextToken();
            Expect(Token.Do);
            ReadNextToken();

            if (whileLoopStack.Count == 0 || whileLoopStack.Peek().WhilePosition.Pointer != identifierPosition.Pointer)
            {
                WhileLoopContext loopContext = new WhileLoopContext(identifierPosition, variableName);

                if (!variables.ContainsKey(variableName))
                    variables.Add(variableName, RuntimeValue.CreateRandom());

                SourcePosition currentPosition = lexer.TokenPosition;
                int nestedWhileCount = 1;

                // Při prvním průchodu cyklem najdeme odpovídající end, včetně vnořených while cyklů.
                while (currentToken != Token.End || nestedWhileCount != 0)
                {
                    ReadNextToken();
                    if (currentToken == Token.EndOfFile)
                        ThrowSyntaxError("While loop does not have matching end.");
                    if (currentToken == Token.While)
                        nestedWhileCount++;
                    if (currentToken == Token.End && nestedWhileCount > 0)
                        nestedWhileCount--;
                }

                Expect(Token.End);
                loopContext.EndPosition = lexer.IdentifierPosition;

                whileLoopStack.Push(loopContext);
                lexer.MoveTo(currentPosition);
                currentToken = Token.NewLine;
            }

            if (variables[variableName].Compare(new RuntimeValue(0), Token.Equal).IntegerValue == 1)
            {
                SourcePosition endPosition = whileLoopStack.Peek().EndPosition;
                // Pokud je řídicí proměnná nulová, přeskočí se celé tělo cyklu.
                if (endPosition.Pointer > -1)
                    lexer.MoveTo(new SourcePosition(endPosition.Pointer - 1, endPosition.Line, endPosition.Column));
                else
                    lexer.MoveTo(endPosition);
                currentToken = Token.NewLine;
            }
        }
    }
}
