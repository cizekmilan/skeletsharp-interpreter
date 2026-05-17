
namespace SkeletSharp
{
    /// <summary>
    /// Druhy lexikálních tokenů, které lexer vrací interpretu.
    /// </summary>
    public enum Token
    {
        /// <summary>Neznámý nebo nepodporovaný znak.</summary>
        Unknown,
        /// <summary>Identifikátor proměnné.</summary>
        Identifier,
        /// <summary>Číselný literál odlišný od nuly.</summary>
        Number,
        /// <summary>Klíčové slovo input.</summary>
        Input,
        /// <summary>Klíčové slovo output.</summary>
        Output,
        /// <summary>Klíčové slovo incr.</summary>
        Increment,
        /// <summary>Klíčové slovo decr.</summary>
        Decrement,
        /// <summary>Klíčové slovo while.</summary>
        While,
        /// <summary>Klíčové slovo do.</summary>
        Do,
        /// <summary>Komentářový řádek začínající znakem #.</summary>
        Comment,
        /// <summary>Číselný literál 0.</summary>
        Zero,
        /// <summary>Klíčové slovo end.</summary>
        End,
        /// <summary>Konec řádku.</summary>
        NewLine,
        /// <summary>Operátor rovnosti.</summary>
        Equal,
        /// <summary>Operátor nerovnosti &lt;&gt;.</summary>
        NotEqual,
        /// <summary>Konec zdrojového textu.</summary>
        EndOfFile = -1
    }
}
