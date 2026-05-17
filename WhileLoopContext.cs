namespace SkeletSharp
{
    /// <summary>
    /// Ukládá stav jednoho aktivního cyklu while.
    /// </summary>
    internal class WhileLoopContext
    {
        public const int InvalidPointer = -10;

        /// <summary>
        /// Pozice začátku příkazu while.
        /// </summary>
        public SourcePosition WhilePosition { get; }

        /// <summary>
        /// Pozice odpovídajícího příkazu end.
        /// </summary>
        public SourcePosition EndPosition { get; set; }

        /// <summary>
        /// Název řídicí proměnné cyklu.
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// Vytvoří kontext cyklu podle pozice while a řídicí proměnné.
        /// </summary>
        public WhileLoopContext(SourcePosition whilePosition, string variableName)
        {
            WhilePosition = whilePosition;
            VariableName = variableName;
            EndPosition = new SourcePosition(InvalidPointer, 0, 0);
        }
    }
}
