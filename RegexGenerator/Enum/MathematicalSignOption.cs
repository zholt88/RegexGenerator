using System;

namespace RegexGenerator.Enum
{
    [Flags]
    public enum MathematicalSignOption
    {
        /// <summary>
        /// No mathematical sign allowed
        /// </summary>
        None = 1,
        /// <summary>
        /// Positive mathematical sign required
        /// </summary>
        Positive = 2,
        /// <summary>
        /// Negative mathematical sign required
        /// </summary>
        Negative = 4,
        /// <summary>
        /// Positive mathematical sign optional
        /// </summary>
        PositiveOrNone = Positive | None,
        /// <summary>
        /// Negative mathematical sign optional
        /// </summary>
        NegativeOrNone = Negative | None,
        /// <summary>
        /// Positive or negative mathematical sign required
        /// </summary>
        PositiveOrNegative = Positive | Negative,
        /// <summary>
        /// Positive or negative mathematical sign optional
        /// </summary>
        Any = Positive | Negative | None
    }
}