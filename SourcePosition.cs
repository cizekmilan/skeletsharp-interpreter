namespace SkeletSharp
{
    /// <summary>
    /// Popisuje pozici znaku ve zdrojovém textu.
    /// </summary>
    public readonly struct SourcePosition
    {
        /// <summary>
        /// Absolutní index znaku ve zdrojovém textu, počítaný od nuly.
        /// </summary>
        public int Pointer { get; }

        /// <summary>
        /// Číslo řádku, počítané od jedné.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Číslo sloupce, počítané od jedné.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Vytvoří novou pozici ve zdrojovém textu.
        /// </summary>
        public SourcePosition(int pointer, int line, int column)
        {
            Pointer = pointer;
            Line = line;
            Column = column;
        }
    }
}
