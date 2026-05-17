using System;
using System.Globalization;

namespace SkeletSharp
{
    /// <summary>
    /// Typ hodnoty uložené v proměnné interpretu.
    /// </summary>
    public enum RuntimeValueType
    {
        /// <summary>
        /// Nezáporné celé číslo.
        /// </summary>
        Integer
    }

    /// <summary>
    /// Reprezentuje hodnotu proměnné při běhu interpretovaného programu.
    /// </summary>
    public struct RuntimeValue
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Typ uložené hodnoty.
        /// </summary>
        public RuntimeValueType Type { get; set; }

        /// <summary>
        /// Číselná hodnota proměnné.
        /// </summary>
        public UInt32 IntegerValue { get; set; }

        /// <summary>
        /// Vytvoří hodnotu z konkrétního nezáporného celého čísla.
        /// </summary>
        public RuntimeValue(UInt32 integerValue)
            : this()
        {
            Type = RuntimeValueType.Integer;
            IntegerValue = integerValue;
        }

        /// <summary>
        /// Vytvoří náhodnou počáteční hodnotu pro nově zavedenou proměnnou.
        /// </summary>
        public static RuntimeValue CreateRandom()
        {
            lock (Random)
                return new RuntimeValue((UInt32)Random.Next(1, 1000));
        }

        /// <summary>
        /// Porovná dvě hodnoty a vrátí výsledek ve tvaru hodnoty 0 nebo 1.
        /// </summary>
        public RuntimeValue Compare(RuntimeValue other, Token comparisonOperator)
        {
            if (comparisonOperator == Token.Equal)
                return new RuntimeValue(IntegerValue == other.IntegerValue ? (UInt32)1 : 0);

            if (comparisonOperator == Token.NotEqual)
                return new RuntimeValue(IntegerValue == other.IntegerValue ? (UInt32)0 : 1);

            throw new SkeletonRuntimeException("Unknown comparison operator.");
        }

        /// <summary>
        /// Vrátí textovou podobu číselné hodnoty.
        /// </summary>
        public override string ToString()
        {
            return IntegerValue.ToString(CultureInfo.InvariantCulture);
        }
    }
}
