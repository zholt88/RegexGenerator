using System;

namespace RegexGenerator.Enum
{
    [Flags]
    public enum WhiteSpaceOption
    {
        /// <summary>
        /// No leading or trailing white space allowed
        /// </summary>
        None = 0,
        /// <summary>
        /// Leading white space optional
        /// </summary>
        Leading = 1,
        /// <summary>
        /// Trailing white space optional
        /// </summary>
        Trailing = 2,
        /// <summary>
        /// Leading and trailing white space optional
        /// </summary>
        LeadingOrTrailing = Leading | Trailing
    }
}
