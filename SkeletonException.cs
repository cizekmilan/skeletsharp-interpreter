using System;

namespace SkeletSharp
{
    /// <summary>
    /// Základní výjimka pro chyby vzniklé při zpracování skeletového programu.
    /// </summary>
    public abstract class SkeletonException : Exception
    {
        protected SkeletonException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Výjimka pro syntaktické chyby ve zdrojovém textu.
    /// </summary>
    public class SkeletonSyntaxException : SkeletonException
    {
        /// <summary>
        /// Řádek, na kterém byla chyba detekována.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Sloupec, na kterém byla chyba detekována.
        /// </summary>
        public int Column { get; }

        public SkeletonSyntaxException(string message, SourcePosition position)
            : base(message + " line: " + position.Line + ", column: " + position.Column)
        {
            Line = position.Line;
            Column = position.Column;
        }
    }

    /// <summary>
    /// Výjimka pro chyby za běhu interpretovaného programu.
    /// </summary>
    public class SkeletonRuntimeException : SkeletonException
    {
        public SkeletonRuntimeException(string message)
            : base(message)
        {
        }
    }
}
