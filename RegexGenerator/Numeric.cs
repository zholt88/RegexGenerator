using System;
using System.Globalization;
using System.Text.RegularExpressions;
using RegexGenerator.Enum;

namespace RegexGenerator
{
    public static class Numeric
    {
        public static readonly string Pattern = @"^\d+$";
        public static readonly Regex Regex = new Regex(Pattern);

        #region Predefined

        public static readonly string LeadingAndTrailingWhiteSpacePattern = BuildPattern(whiteSpaceOption: WhiteSpaceOption.LeadingOrTrailing);
        public static readonly string SixDigitsPattern = BuildPattern(6);
        public static readonly string OptionalMathematicalSignOptionalNumberGroupSeparatorTwoRequiredDecimalDigitsPattern = BuildPattern(
            mathematicalSignOption: MathematicalSignOption.Any,
            numberGroupSeparatorOption: NumberGroupSeparatorOption.Optional,
            decimalDigits: 2,
            decimalDigitRequired: true);
        public static readonly string TwoOptionalDecimalDigitsPattern = BuildPattern(decimalDigits: 2);

        public static readonly Regex LeadingAndTrailingWhiteSpaceRegex = new Regex(LeadingAndTrailingWhiteSpacePattern);
        public static readonly Regex SixDigitsRegex = new Regex(SixDigitsPattern);
        public static readonly Regex OptionalMathematicalSignOptionalNumberGroupSeparatorTwoRequiredDecimalDigitsRegex = new Regex(OptionalMathematicalSignOptionalNumberGroupSeparatorTwoRequiredDecimalDigitsPattern);
        public static readonly Regex TwoOptionalDecimalDigitsRegex = new Regex(TwoOptionalDecimalDigitsPattern);

        #endregion

        /// <summary>
        /// Builds a numeric regular expression pattern.
        /// </summary>
        /// <param name="digitCount">Number of digits to the left of the decimal point to accept. A value of zero accepts any number of digits.</param>
        /// <param name="mathematicalSignOption">Determines how mathematical signs are handled.</param>
        /// <param name="numberGroupSeparatorOption">Determines how number group separators are handled.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point to accept.</param>
        /// <param name="decimalDigitRequired">Determines if the <see cref="decimalDigits"/> is required or optional.</param>
        /// <param name="whiteSpaceOption">Determines how white space is handled.</param>
        /// <param name="culture">The culture for which the regular expression is intended to be used. Defaults to the current culture.</param>
        public static string BuildPattern(
            int digitCount = 0,
            MathematicalSignOption mathematicalSignOption = MathematicalSignOption.None,
            NumberGroupSeparatorOption numberGroupSeparatorOption = NumberGroupSeparatorOption.None,
            int decimalDigits = 0,
            bool decimalDigitRequired = false,
            WhiteSpaceOption whiteSpaceOption = WhiteSpaceOption.None,
            CultureInfo culture = null)
        {
            culture = culture ?? CultureInfo.CurrentCulture;
            return Pattern
                .AddDigitCount(digitCount)
                .AddMathematicalSignOption(mathematicalSignOption, culture.NumberFormat)
                .AddNumberGroupSeparatorOption(numberGroupSeparatorOption, culture.NumberFormat)
                .AddDecimalDigits(decimalDigits, decimalDigitRequired, culture.NumberFormat)
                .AddWhiteSpaceOption(whiteSpaceOption);
        }

        /// <summary>
        /// Builds a <see cref="System.Text.RegularExpressions.Regex"/> object with a numeric pattern.
        /// </summary>
        /// <param name="digitCount">Number of digits to the left of the decimal point to accept. A value of zero accepts any number of digits.</param>
        /// <param name="mathematicalSignOption">Determines how mathematical signs are handled.</param>
        /// <param name="numberGroupSeparatorOption">Determines how number group separators are handled.</param>
        /// <param name="decimalDigits">Number of digits to the right of the decimal point to accept.</param>
        /// <param name="decimalDigitRequired">Determines if the <see cref="decimalDigits"/> is required or optional.</param>
        /// <param name="whiteSpaceOption">Determines how white space is handled.</param>
        /// <param name="culture">The culture for which the regular expression is intended to be used. Defaults to the current culture.</param>
        public static Regex BuildRegex(
            int digitCount = 0,
            MathematicalSignOption mathematicalSignOption = MathematicalSignOption.None,
            NumberGroupSeparatorOption numberGroupSeparatorOption = NumberGroupSeparatorOption.None,
            int decimalDigits = 0,
            bool decimalDigitRequired = false,
            WhiteSpaceOption whiteSpaceOption = WhiteSpaceOption.None,
            CultureInfo culture = null)
        {
            return new Regex(BuildPattern(
                digitCount,
                mathematicalSignOption,
                numberGroupSeparatorOption,
                decimalDigits,
                decimalDigitRequired,
                whiteSpaceOption,
                culture ?? CultureInfo.CurrentCulture));
        }

        private static string AddDigitCount(this string numericPattern, int digitCount)
        {
            return digitCount > 0 ? numericPattern.Replace("+", $"{{{digitCount}}}") : numericPattern;
        }

        private static string AddMathematicalSignOption(this string numericPattern, MathematicalSignOption mathematicalSignOption, NumberFormatInfo numberFormat)
        {
            if (numberFormat.NumberNegativePattern == 0) throw new NotImplementedException($"{nameof(numberFormat.NumberNegativePattern)} of value {numberFormat.NumberNegativePattern} has not yet been implemented.");
            bool before = numberFormat.NumberNegativePattern == 1 || numberFormat.NumberNegativePattern == 2;
            switch (mathematicalSignOption)
            {
                case MathematicalSignOption.None:
                    return numericPattern;
                case MathematicalSignOption.Positive when before:
                    return numericPattern.Replace("^", $"^[{numberFormat.PositiveSign}]");
                case MathematicalSignOption.Positive:
                    return numericPattern.Replace("$", $"[{numberFormat.PositiveSign}]$");
                case MathematicalSignOption.Negative when before:
                    return numericPattern.Replace("^", $"^[{numberFormat.NegativeSign}]");
                case MathematicalSignOption.Negative:
                    return numericPattern.Replace("$", $"[{numberFormat.NegativeSign}]$");
                case MathematicalSignOption.PositiveOrNone when before:
                    return numericPattern.Replace("^", $"^[{numberFormat.PositiveSign}]?");
                case MathematicalSignOption.PositiveOrNone:
                    return numericPattern.Replace("$", $"[{numberFormat.PositiveSign}]?$");
                case MathematicalSignOption.NegativeOrNone when before:
                    return numericPattern.Replace("^", $"^[{numberFormat.NegativeSign}]?");
                case MathematicalSignOption.NegativeOrNone:
                    return numericPattern.Replace("$", $"[{numberFormat.NegativeSign}]?$");
                case MathematicalSignOption.PositiveOrNegative when before:
                    return numericPattern.Replace("^", $"^[{numberFormat.PositiveSign}{numberFormat.NegativeSign}]");
                case MathematicalSignOption.PositiveOrNegative:
                    return numericPattern.Replace("$", $"[{numberFormat.PositiveSign}{numberFormat.NegativeSign}]$");
                case MathematicalSignOption.Any when before:
                    return numericPattern.Replace("^", $"^[{numberFormat.PositiveSign}{numberFormat.NegativeSign}]?");
                case MathematicalSignOption.Any:
                    return numericPattern.Replace("$", $"[{numberFormat.PositiveSign}{numberFormat.NegativeSign}]?$");
                default:
                    throw new ArgumentOutOfRangeException(nameof(mathematicalSignOption), mathematicalSignOption, null);
            }
        }

        private static string AddNumberGroupSeparatorOption(this string numericPattern, NumberGroupSeparatorOption numberGroupSeparatorOption, NumberFormatInfo numberFormat)
        {
            if (numberFormat.NumberGroupSizes.Length > 1) throw new NotImplementedException("Support for multiple number group sizes has not yet been implemented.");
            string numberGroupSize = numberFormat.NumberGroupSizes[0].ToString();
            string numberGroupSeparator = Regex.Escape(numberFormat.NumberGroupSeparator);
            switch (numberGroupSeparatorOption)
            {
                case NumberGroupSeparatorOption.None:
                    return numericPattern;
                case NumberGroupSeparatorOption.Required:
                    return Regex.Replace(numericPattern.Replace(@"\d", $@"[\d]{{1,{numberGroupSize}}}(?:{numberGroupSeparator}[\d]{{{numberGroupSize}}})"), @"(?<!\[)\+(?!\])", "*");
                case NumberGroupSeparatorOption.Optional:
                    return Regex.Replace(numericPattern.Replace(@"\d", $@"[\d]{{1,{numberGroupSize}}}(?:{numberGroupSeparator}?[\d]{{{numberGroupSize}}})"), @"(?<!\[)\+(?!\])", "*");
                default:
                    throw new ArgumentOutOfRangeException(nameof(numberGroupSeparatorOption), numberGroupSeparatorOption, null);
            }
        }

        private static string AddDecimalDigits(this string numericPattern, int decimalDigits, bool required, NumberFormatInfo numberFormat)
        {
            string decimalSeparator = Regex.Escape(numberFormat.NumberDecimalSeparator);
            string requiredPattern = required ? "" : "?";
            string decimalDigitsPattern = required ? decimalDigits.ToString() : $"0,{decimalDigits}";
            return decimalDigits > 0
                ? numericPattern.Replace("$", $@"({decimalSeparator}{requiredPattern})[\d]{{{decimalDigitsPattern}}}$")
                : numericPattern;
        }

        private static string AddWhiteSpaceOption(this string pattern, WhiteSpaceOption whiteSpaceOption)
        {
            switch (whiteSpaceOption)
            {
                case WhiteSpaceOption.None:
                    return pattern;
                case WhiteSpaceOption.Leading:
                    return pattern.Replace("^", @"^\s*");
                case WhiteSpaceOption.Trailing:
                    return pattern.Replace("$", @"\s*$");
                case WhiteSpaceOption.LeadingOrTrailing:
                    return pattern.Replace("^", @"^\s*").Replace("$", @"\s*$");
                default:
                    throw new ArgumentOutOfRangeException(nameof(whiteSpaceOption), whiteSpaceOption, null);
            }
        }
    }
}
