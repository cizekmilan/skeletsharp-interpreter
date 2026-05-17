using System;
using System.Globalization;

namespace SkeletSharp
{
    /// <summary>
    /// Převádí zdrojový text skeletového jazyka na tokeny.
    /// </summary>
    public class Lexer
    {
        private readonly string source;
        private SourcePosition sourcePosition;

        /// <summary>
        /// Poslední přečtený znak ze zdrojového textu.
        /// </summary>
        public char LastChar { get; set; }

        /// <summary>
        /// Pozice aktuálního tokenu.
        /// </summary>
        public SourcePosition TokenPosition { get; private set; }

        /// <summary>
        /// Pozice aktuálního identifikátoru nebo klíčového slova.
        /// </summary>
        public SourcePosition IdentifierPosition { get; private set; }

        /// <summary>
        /// Text aktuálního identifikátoru nebo klíčového slova.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Hodnota aktuálního číselného literálu.
        /// </summary>
        public RuntimeValue LiteralValue { get; private set; }

        /// <summary>
        /// Text aktuálního komentáře.
        /// </summary>
        public string CommentText { get; private set; }

        /// <summary>
        /// Vytvoří lexer nad zadaným zdrojovým textem.
        /// </summary>
        public Lexer(string input)
        {
            source = input ?? string.Empty;
            sourcePosition = new SourcePosition(0, 1, 1);
            LastChar = source.Length == 0 ? (char)0 : source[0];
        }

        /// <summary>
        /// Přesune lexer na uloženou pozici ve zdrojovém textu.
        /// </summary>
        public void MoveTo(SourcePosition position)
        {
            sourcePosition = position;
        }

        /// <summary>
        /// Načte další znak a aktualizuje řádek/sloupec.
        /// </summary>
        private char ReadNextChar()
        {
            sourcePosition = new SourcePosition(sourcePosition.Pointer + 1, sourcePosition.Line, sourcePosition.Column + 1);

            if (sourcePosition.Pointer >= source.Length)
                return LastChar = (char)0;

            LastChar = source[sourcePosition.Pointer];
            if (LastChar == '\n')
                sourcePosition = new SourcePosition(sourcePosition.Pointer, sourcePosition.Line + 1, 1);

            return LastChar;
        }

        /// <summary>
        /// Vrátí další token ze zdrojového textu.
        /// </summary>
        public Token GetToken()
        {
            while (LastChar == ' ' || LastChar == '\t' || LastChar == '\r')
                ReadNextChar();

            TokenPosition = sourcePosition;

            if (char.IsLetter(LastChar) || LastChar == '#')
            {
                Identifier = LastChar.ToString();
                IdentifierPosition = sourcePosition;

                // Identifikátor končí prvním znakem, který už není písmeno ani číslice.
                while (char.IsLetterOrDigit(ReadNextChar()))
                    Identifier += LastChar;

                switch (Identifier.ToUpperInvariant())
                {
                    case "INCR": return Token.Increment;
                    case "DECR": return Token.Decrement;
                    case "INPUT": return Token.Input;
                    case "OUTPUT": return Token.Output;
                    case "WHILE": return Token.While;
                    case "DO": return Token.Do;
                    case "END": return Token.End;
                    case "#":
                        CommentText = "#";
                        // Komentář se čte až do konce řádku nebo souboru.
                        while (LastChar != '\n' && LastChar != (char)0)
                        {
                            CommentText += LastChar;
                            ReadNextChar();
                        }
                        return Token.Comment;
                    default:
                        return Token.Identifier;
                }
            }

            if (char.IsDigit(LastChar))
            {
                string number = string.Empty;
                // Číselný literál je souvislá posloupnost číslic.
                do
                {
                    number += LastChar;
                }
                while (char.IsDigit(ReadNextChar()));

                if (!UInt32.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint parsedValue))
                    throw new SkeletonSyntaxException("ERROR while parsing number", TokenPosition);

                LiteralValue = new RuntimeValue(parsedValue);
                return LiteralValue.IntegerValue == 0 ? Token.Zero : Token.Number;
            }

            Token token = Token.Unknown;
            switch (LastChar)
            {
                case '\n':
                    token = Token.NewLine;
                    break;
                case '<':
                    ReadNextChar();
                    if (LastChar == '>')
                        token = Token.NotEqual;
                    break;
                case (char)0:
                    return Token.EndOfFile;
            }

            ReadNextChar();
            return token;
        }
    }
}
