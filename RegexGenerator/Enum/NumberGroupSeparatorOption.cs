using System;

namespace RegexGenerator.Enum
{
    [Flags]
    public enum NumberGroupSeparatorOption
    {
        /// <summary>
        /// No number group separator allowed
        /// </summary>
        None = 1,
        /// <summary>
        /// Number group separator required
        /// </summary>
        Required = 2,
        /// <summary>
        /// Number group separator optional
        /// </summary>
        Optional = None | Required
    }
}