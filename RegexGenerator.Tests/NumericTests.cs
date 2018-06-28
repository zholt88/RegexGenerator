using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RegexGenerator;
using RegexGenerator.Enum;

namespace CPS.Common.Tests.RegularExpression
{
    [Parallelizable]
    [TestFixture]
    public class NumericTests
    {
        #region Test Case Sources

        private static readonly CultureInfo TestCulture1 = new CultureInfo("en-US")
        {
            // The following are all the defaults of en-US, but are explicitly stated here for transparency.
            NumberFormat =
            {
                PositiveSign = "+",
                NegativeSign = "-",
                NumberGroupSeparator = ",",
                NumberGroupSizes = new[] {3},
                NumberDecimalSeparator = ".",
                NumberNegativePattern = 1
            }
        };
        private static readonly CultureInfo TestCulture2 = new CultureInfo("fr-CA")
        {
            // The following are NOT all the defaults of fr-CA, but are set here for testing purposes.
            NumberFormat =
            {
                PositiveSign = "~",
                NegativeSign = "_",
                NumberGroupSeparator = ".",
                NumberGroupSizes = new[] {4},
                NumberDecimalSeparator = ",",
                NumberNegativePattern = 3
            }
        };

        private static readonly (string value, bool expected)[] NumericTestCases =
        {
            ("", false),
            ("12345", true),
            ("0", true),
            ("89757335657278752", true),
            ("abcd", false),
            ("-1", false),
            ("12a34", false),
            ("1234423421s", false),
            (" 123", false),
            ("123 ", false),
            ("12 34", false)
        };

        private static readonly (string value, int digitCount, bool expected)[] DigitCountTestCases =
        {
            ("", 0, false),
            ("", 1, false),
            ("", 5, false),
            ("", 10, false),
            ("1", 0, true),
            ("1", 1, true),
            ("1", 5, false),
            ("1", 10, false),
            ("12345", 0, true),
            ("12345", 1, false),
            ("12345", 5, true),
            ("12345", 10, false),
            ("1234567890", 0, true),
            ("1234567890", 1, false),
            ("1234567890", 5, false),
            ("1234567890", 10, true),
        };

        private static readonly (string value, MathematicalSignOption mathSign, CultureInfo culture, bool expected)[] MathematicalSignTestCases =
        {
            ("", MathematicalSignOption.None,               TestCulture1, false),
            ("", MathematicalSignOption.Any,                TestCulture1, false),
            ("", MathematicalSignOption.Positive,           TestCulture1, false),
            ("", MathematicalSignOption.Negative,           TestCulture1, false),
            ("", MathematicalSignOption.PositiveOrNone,     TestCulture1, false),
            ("", MathematicalSignOption.NegativeOrNone,     TestCulture1, false),
            ("", MathematicalSignOption.PositiveOrNegative, TestCulture1, false),
            ("", MathematicalSignOption.None,               TestCulture2, false),
            ("", MathematicalSignOption.Any,                TestCulture2, false),
            ("", MathematicalSignOption.Positive,           TestCulture2, false),
            ("", MathematicalSignOption.Negative,           TestCulture2, false),
            ("", MathematicalSignOption.PositiveOrNone,     TestCulture2, false),
            ("", MathematicalSignOption.NegativeOrNone,     TestCulture2, false),
            ("", MathematicalSignOption.PositiveOrNegative, TestCulture2, false),

            ("+", MathematicalSignOption.None,               TestCulture1, false),
            ("+", MathematicalSignOption.Any,                TestCulture1, false),
            ("+", MathematicalSignOption.Positive,           TestCulture1, false),
            ("+", MathematicalSignOption.Negative,           TestCulture1, false),
            ("+", MathematicalSignOption.PositiveOrNone,     TestCulture1, false),
            ("+", MathematicalSignOption.NegativeOrNone,     TestCulture1, false),
            ("+", MathematicalSignOption.PositiveOrNegative, TestCulture1, false),
            ("+", MathematicalSignOption.None,               TestCulture2, false),
            ("+", MathematicalSignOption.Any,                TestCulture2, false),
            ("+", MathematicalSignOption.Positive,           TestCulture2, false),
            ("+", MathematicalSignOption.Negative,           TestCulture2, false),
            ("+", MathematicalSignOption.PositiveOrNone,     TestCulture2, false),
            ("+", MathematicalSignOption.NegativeOrNone,     TestCulture2, false),
            ("+", MathematicalSignOption.PositiveOrNegative, TestCulture2, false),

            ("-", MathematicalSignOption.None,               TestCulture1, false),
            ("-", MathematicalSignOption.Any,                TestCulture1, false),
            ("-", MathematicalSignOption.Positive,           TestCulture1, false),
            ("-", MathematicalSignOption.Negative,           TestCulture1, false),
            ("-", MathematicalSignOption.PositiveOrNone,     TestCulture1, false),
            ("-", MathematicalSignOption.NegativeOrNone,     TestCulture1, false),
            ("-", MathematicalSignOption.PositiveOrNegative, TestCulture1, false),
            ("-", MathematicalSignOption.None,               TestCulture2, false),
            ("-", MathematicalSignOption.Any,                TestCulture2, false),
            ("-", MathematicalSignOption.Positive,           TestCulture2, false),
            ("-", MathematicalSignOption.Negative,           TestCulture2, false),
            ("-", MathematicalSignOption.PositiveOrNone,     TestCulture2, false),
            ("-", MathematicalSignOption.NegativeOrNone,     TestCulture2, false),
            ("-", MathematicalSignOption.PositiveOrNegative, TestCulture2, false),

            ("~", MathematicalSignOption.None,               TestCulture1, false),
            ("~", MathematicalSignOption.Any,                TestCulture1, false),
            ("~", MathematicalSignOption.Positive,           TestCulture1, false),
            ("~", MathematicalSignOption.Negative,           TestCulture1, false),
            ("~", MathematicalSignOption.PositiveOrNone,     TestCulture1, false),
            ("~", MathematicalSignOption.NegativeOrNone,     TestCulture1, false),
            ("~", MathematicalSignOption.PositiveOrNegative, TestCulture1, false),
            ("~", MathematicalSignOption.None,               TestCulture2, false),
            ("~", MathematicalSignOption.Any,                TestCulture2, false),
            ("~", MathematicalSignOption.Positive,           TestCulture2, false),
            ("~", MathematicalSignOption.Negative,           TestCulture2, false),
            ("~", MathematicalSignOption.PositiveOrNone,     TestCulture2, false),
            ("~", MathematicalSignOption.NegativeOrNone,     TestCulture2, false),
            ("~", MathematicalSignOption.PositiveOrNegative, TestCulture2, false),

            ("_", MathematicalSignOption.None,               TestCulture1, false),
            ("_", MathematicalSignOption.Any,                TestCulture1, false),
            ("_", MathematicalSignOption.Positive,           TestCulture1, false),
            ("_", MathematicalSignOption.Negative,           TestCulture1, false),
            ("_", MathematicalSignOption.PositiveOrNone,     TestCulture1, false),
            ("_", MathematicalSignOption.NegativeOrNone,     TestCulture1, false),
            ("_", MathematicalSignOption.PositiveOrNegative, TestCulture1, false),
            ("_", MathematicalSignOption.None,               TestCulture2, false),
            ("_", MathematicalSignOption.Any,                TestCulture2, false),
            ("_", MathematicalSignOption.Positive,           TestCulture2, false),
            ("_", MathematicalSignOption.Negative,           TestCulture2, false),
            ("_", MathematicalSignOption.PositiveOrNone,     TestCulture2, false),
            ("_", MathematicalSignOption.NegativeOrNone,     TestCulture2, false),
            ("_", MathematicalSignOption.PositiveOrNegative, TestCulture2, false),

            ( "1", MathematicalSignOption.None,                 TestCulture1, true),
            ( "1", MathematicalSignOption.Any,                  TestCulture1, true),
            ( "1", MathematicalSignOption.Positive,             TestCulture1, false),
            ( "1", MathematicalSignOption.Negative,             TestCulture1, false),
            ( "1", MathematicalSignOption.PositiveOrNone,       TestCulture1, true),
            ( "1", MathematicalSignOption.NegativeOrNone,       TestCulture1, true),
            ( "1", MathematicalSignOption.PositiveOrNegative,   TestCulture1, false),
            ( "1", MathematicalSignOption.None,                 TestCulture2, true),
            ( "1", MathematicalSignOption.Any,                  TestCulture2, true),
            ( "1", MathematicalSignOption.Positive,             TestCulture2, false),
            ( "1", MathematicalSignOption.Negative,             TestCulture2, false),
            ( "1", MathematicalSignOption.PositiveOrNone,       TestCulture2, true),
            ( "1", MathematicalSignOption.NegativeOrNone,       TestCulture2, true),
            ( "1", MathematicalSignOption.PositiveOrNegative,   TestCulture2, false),

            ("+1", MathematicalSignOption.None,                 TestCulture1, false),
            ("+1", MathematicalSignOption.Any,                  TestCulture1, true),
            ("+1", MathematicalSignOption.Positive,             TestCulture1, true),
            ("+1", MathematicalSignOption.Negative,             TestCulture1, false),
            ("+1", MathematicalSignOption.PositiveOrNone,       TestCulture1, true),
            ("+1", MathematicalSignOption.NegativeOrNone,       TestCulture1, false),
            ("+1", MathematicalSignOption.PositiveOrNegative,   TestCulture1, true),
            ("+1", MathematicalSignOption.None,                 TestCulture2, false),
            ("+1", MathematicalSignOption.Any,                  TestCulture2, false),
            ("+1", MathematicalSignOption.Positive,             TestCulture2, false),
            ("+1", MathematicalSignOption.Negative,             TestCulture2, false),
            ("+1", MathematicalSignOption.PositiveOrNone,       TestCulture2, false),
            ("+1", MathematicalSignOption.NegativeOrNone,       TestCulture2, false),
            ("+1", MathematicalSignOption.PositiveOrNegative,   TestCulture2, false),

            ("-1", MathematicalSignOption.None,                 TestCulture1, false),
            ("-1", MathematicalSignOption.Any,                  TestCulture1, true),
            ("-1", MathematicalSignOption.Positive,             TestCulture1, false),
            ("-1", MathematicalSignOption.Negative,             TestCulture1, true),
            ("-1", MathematicalSignOption.PositiveOrNone,       TestCulture1, false),
            ("-1", MathematicalSignOption.NegativeOrNone,       TestCulture1, true),
            ("-1", MathematicalSignOption.PositiveOrNegative,   TestCulture1, true),
            ("-1", MathematicalSignOption.None,                 TestCulture2, false),
            ("-1", MathematicalSignOption.Any,                  TestCulture2, false),
            ("-1", MathematicalSignOption.Positive,             TestCulture2, false),
            ("-1", MathematicalSignOption.Negative,             TestCulture2, false),
            ("-1", MathematicalSignOption.PositiveOrNone,       TestCulture2, false),
            ("-1", MathematicalSignOption.NegativeOrNone,       TestCulture2, false),
            ("-1", MathematicalSignOption.PositiveOrNegative,   TestCulture2, false),

            ("1~", MathematicalSignOption.None,                 TestCulture1, false),
            ("1~", MathematicalSignOption.Any,                  TestCulture1, false),
            ("1~", MathematicalSignOption.Positive,             TestCulture1, false),
            ("1~", MathematicalSignOption.Negative,             TestCulture1, false),
            ("1~", MathematicalSignOption.PositiveOrNone,       TestCulture1, false),
            ("1~", MathematicalSignOption.NegativeOrNone,       TestCulture1, false),
            ("1~", MathematicalSignOption.PositiveOrNegative,   TestCulture1, false),
            ("1~", MathematicalSignOption.None,                 TestCulture2, false),
            ("1~", MathematicalSignOption.Any,                  TestCulture2, true),
            ("1~", MathematicalSignOption.Positive,             TestCulture2, true),
            ("1~", MathematicalSignOption.Negative,             TestCulture2, false),
            ("1~", MathematicalSignOption.PositiveOrNone,       TestCulture2, true),
            ("1~", MathematicalSignOption.NegativeOrNone,       TestCulture2, false),
            ("1~", MathematicalSignOption.PositiveOrNegative,   TestCulture2, true),

            ("1_", MathematicalSignOption.None,                 TestCulture1, false),
            ("1_", MathematicalSignOption.Any,                  TestCulture1, false),
            ("1_", MathematicalSignOption.Positive,             TestCulture1, false),
            ("1_", MathematicalSignOption.Negative,             TestCulture1, false),
            ("1_", MathematicalSignOption.PositiveOrNone,       TestCulture1, false),
            ("1_", MathematicalSignOption.NegativeOrNone,       TestCulture1, false),
            ("1_", MathematicalSignOption.PositiveOrNegative,   TestCulture1, false),
            ("1_", MathematicalSignOption.None,                 TestCulture2, false),
            ("1_", MathematicalSignOption.Any,                  TestCulture2, true),
            ("1_", MathematicalSignOption.Positive,             TestCulture2, false),
            ("1_", MathematicalSignOption.Negative,             TestCulture2, true),
            ("1_", MathematicalSignOption.PositiveOrNone,       TestCulture2, false),
            ("1_", MathematicalSignOption.NegativeOrNone,       TestCulture2, true),
            ("1_", MathematicalSignOption.PositiveOrNegative,   TestCulture2, true),

            ( "10", MathematicalSignOption.None,                TestCulture1, true),
            ( "10", MathematicalSignOption.Any,                 TestCulture1, true),
            ( "10", MathematicalSignOption.Positive,            TestCulture1, false),
            ( "10", MathematicalSignOption.Negative,            TestCulture1, false),
            ( "10", MathematicalSignOption.PositiveOrNone,      TestCulture1, true),
            ( "10", MathematicalSignOption.NegativeOrNone,      TestCulture1, true),
            ( "10", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ( "10", MathematicalSignOption.None,                TestCulture2, true),
            ( "10", MathematicalSignOption.Any,                 TestCulture2, true),
            ( "10", MathematicalSignOption.Positive,            TestCulture2, false),
            ( "10", MathematicalSignOption.Negative,            TestCulture2, false),
            ( "10", MathematicalSignOption.PositiveOrNone,      TestCulture2, true),
            ( "10", MathematicalSignOption.NegativeOrNone,      TestCulture2, true),
            ( "10", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("+10", MathematicalSignOption.None,                TestCulture1, false),
            ("+10", MathematicalSignOption.Any,                 TestCulture1, true),
            ("+10", MathematicalSignOption.Positive,            TestCulture1, true),
            ("+10", MathematicalSignOption.Negative,            TestCulture1, false),
            ("+10", MathematicalSignOption.PositiveOrNone,      TestCulture1, true),
            ("+10", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("+10", MathematicalSignOption.PositiveOrNegative,  TestCulture1, true),
            ("+10", MathematicalSignOption.None,                TestCulture2, false),
            ("+10", MathematicalSignOption.Any,                 TestCulture2, false),
            ("+10", MathematicalSignOption.Positive,            TestCulture2, false),
            ("+10", MathematicalSignOption.Negative,            TestCulture2, false),
            ("+10", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("+10", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("+10", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("-10", MathematicalSignOption.None,                TestCulture1, false),
            ("-10", MathematicalSignOption.Any,                 TestCulture1, true),
            ("-10", MathematicalSignOption.Positive,            TestCulture1, false),
            ("-10", MathematicalSignOption.Negative,            TestCulture1, true),
            ("-10", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("-10", MathematicalSignOption.NegativeOrNone,      TestCulture1, true),
            ("-10", MathematicalSignOption.PositiveOrNegative,  TestCulture1, true),
            ("-10", MathematicalSignOption.None,                TestCulture2, false),
            ("-10", MathematicalSignOption.Any,                 TestCulture2, false),
            ("-10", MathematicalSignOption.Positive,            TestCulture2, false),
            ("-10", MathematicalSignOption.Negative,            TestCulture2, false),
            ("-10", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("-10", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("-10", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("10~", MathematicalSignOption.None,                TestCulture1, false),
            ("10~", MathematicalSignOption.Any,                 TestCulture1, false),
            ("10~", MathematicalSignOption.Positive,            TestCulture1, false),
            ("10~", MathematicalSignOption.Negative,            TestCulture1, false),
            ("10~", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("10~", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("10~", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ("10~", MathematicalSignOption.None,                TestCulture2, false),
            ("10~", MathematicalSignOption.Any,                 TestCulture2, true),
            ("10~", MathematicalSignOption.Positive,            TestCulture2, true),
            ("10~", MathematicalSignOption.Negative,            TestCulture2, false),
            ("10~", MathematicalSignOption.PositiveOrNone,      TestCulture2, true),
            ("10~", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("10~", MathematicalSignOption.PositiveOrNegative,  TestCulture2, true),

            ("10_", MathematicalSignOption.None,                TestCulture1, false),
            ("10_", MathematicalSignOption.Any,                 TestCulture1, false),
            ("10_", MathematicalSignOption.Positive,            TestCulture1, false),
            ("10_", MathematicalSignOption.Negative,            TestCulture1, false),
            ("10_", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("10_", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("10_", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ("10_", MathematicalSignOption.None,                TestCulture2, false),
            ("10_", MathematicalSignOption.Any,                 TestCulture2, true),
            ("10_", MathematicalSignOption.Positive,            TestCulture2, false),
            ("10_", MathematicalSignOption.Negative,            TestCulture2, true),
            ("10_", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("10_", MathematicalSignOption.NegativeOrNone,      TestCulture2, true),
            ("10_", MathematicalSignOption.PositiveOrNegative,  TestCulture2, true),

            ( "7345243445", MathematicalSignOption.None,                TestCulture1, true),
            ( "7345243445", MathematicalSignOption.Any,                 TestCulture1, true),
            ( "7345243445", MathematicalSignOption.Positive,            TestCulture1, false),
            ( "7345243445", MathematicalSignOption.Negative,            TestCulture1, false),
            ( "7345243445", MathematicalSignOption.PositiveOrNone,      TestCulture1, true),
            ( "7345243445", MathematicalSignOption.NegativeOrNone,      TestCulture1, true),
            ( "7345243445", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ( "7345243445", MathematicalSignOption.None,                TestCulture2, true),
            ( "7345243445", MathematicalSignOption.Any,                 TestCulture2, true),
            ( "7345243445", MathematicalSignOption.Positive,            TestCulture2, false),
            ( "7345243445", MathematicalSignOption.Negative,            TestCulture2, false),
            ( "7345243445", MathematicalSignOption.PositiveOrNone,      TestCulture2, true),
            ( "7345243445", MathematicalSignOption.NegativeOrNone,      TestCulture2, true),
            ( "7345243445", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("+7345243445", MathematicalSignOption.None,                TestCulture1, false),
            ("+7345243445", MathematicalSignOption.Any,                 TestCulture1, true),
            ("+7345243445", MathematicalSignOption.Positive,            TestCulture1, true),
            ("+7345243445", MathematicalSignOption.Negative,            TestCulture1, false),
            ("+7345243445", MathematicalSignOption.PositiveOrNone,      TestCulture1, true),
            ("+7345243445", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("+7345243445", MathematicalSignOption.PositiveOrNegative,  TestCulture1, true),
            ("+7345243445", MathematicalSignOption.None,                TestCulture2, false),
            ("+7345243445", MathematicalSignOption.Any,                 TestCulture2, false),
            ("+7345243445", MathematicalSignOption.Positive,            TestCulture2, false),
            ("+7345243445", MathematicalSignOption.Negative,            TestCulture2, false),
            ("+7345243445", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("+7345243445", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("+7345243445", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("-7345243445", MathematicalSignOption.None,                TestCulture1, false),
            ("-7345243445", MathematicalSignOption.Any,                 TestCulture1, true),
            ("-7345243445", MathematicalSignOption.Positive,            TestCulture1, false),
            ("-7345243445", MathematicalSignOption.Negative,            TestCulture1, true),
            ("-7345243445", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("-7345243445", MathematicalSignOption.NegativeOrNone,      TestCulture1, true),
            ("-7345243445", MathematicalSignOption.PositiveOrNegative,  TestCulture1, true),
            ("-7345243445", MathematicalSignOption.None,                TestCulture2, false),
            ("-7345243445", MathematicalSignOption.Any,                 TestCulture2, false),
            ("-7345243445", MathematicalSignOption.Positive,            TestCulture2, false),
            ("-7345243445", MathematicalSignOption.Negative,            TestCulture2, false),
            ("-7345243445", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("-7345243445", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("-7345243445", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("7345243445~", MathematicalSignOption.None,                TestCulture1, false),
            ("7345243445~", MathematicalSignOption.Any,                 TestCulture1, false),
            ("7345243445~", MathematicalSignOption.Positive,            TestCulture1, false),
            ("7345243445~", MathematicalSignOption.Negative,            TestCulture1, false),
            ("7345243445~", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("7345243445~", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("7345243445~", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ("7345243445~", MathematicalSignOption.None,                TestCulture2, false),
            ("7345243445~", MathematicalSignOption.Any,                 TestCulture2, true),
            ("7345243445~", MathematicalSignOption.Positive,            TestCulture2, true),
            ("7345243445~", MathematicalSignOption.Negative,            TestCulture2, false),
            ("7345243445~", MathematicalSignOption.PositiveOrNone,      TestCulture2, true),
            ("7345243445~", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("7345243445~", MathematicalSignOption.PositiveOrNegative,  TestCulture2, true),

            ("7345243445_", MathematicalSignOption.None,                TestCulture1, false),
            ("7345243445_", MathematicalSignOption.Any,                 TestCulture1, false),
            ("7345243445_", MathematicalSignOption.Positive,            TestCulture1, false),
            ("7345243445_", MathematicalSignOption.Negative,            TestCulture1, false),
            ("7345243445_", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("7345243445_", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("7345243445_", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ("7345243445_", MathematicalSignOption.None,                TestCulture2, false),
            ("7345243445_", MathematicalSignOption.Any,                 TestCulture2, true),
            ("7345243445_", MathematicalSignOption.Positive,            TestCulture2, false),
            ("7345243445_", MathematicalSignOption.Negative,            TestCulture2, true),
            ("7345243445_", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("7345243445_", MathematicalSignOption.NegativeOrNone,      TestCulture2, true),
            ("7345243445_", MathematicalSignOption.PositiveOrNegative,  TestCulture2, true),

            ( "999999999999999999999999999999999999999999", MathematicalSignOption.None,                TestCulture1, true),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.Any,                 TestCulture1, true),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.Positive,            TestCulture1, false),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.Negative,            TestCulture1, false),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNone,      TestCulture1, true),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.NegativeOrNone,      TestCulture1, true),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.None,                TestCulture2, true),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.Any,                 TestCulture2, true),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.Positive,            TestCulture2, false),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.Negative,            TestCulture2, false),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNone,      TestCulture2, true),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.NegativeOrNone,      TestCulture2, true),
            ( "999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("+999999999999999999999999999999999999999999", MathematicalSignOption.None,                TestCulture1, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.Any,                 TestCulture1, true),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.Positive,            TestCulture1, true),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.Negative,            TestCulture1, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNone,      TestCulture1, true),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNegative,  TestCulture1, true),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.None,                TestCulture2, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.Any,                 TestCulture2, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.Positive,            TestCulture2, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.Negative,            TestCulture2, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("+999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("-999999999999999999999999999999999999999999", MathematicalSignOption.None,                TestCulture1, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.Any,                 TestCulture1, true),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.Positive,            TestCulture1, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.Negative,            TestCulture1, true),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.NegativeOrNone,      TestCulture1, true),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNegative,  TestCulture1, true),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.None,                TestCulture2, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.Any,                 TestCulture2, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.Positive,            TestCulture2, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.Negative,            TestCulture2, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("-999999999999999999999999999999999999999999", MathematicalSignOption.PositiveOrNegative,  TestCulture2, false),

            ("999999999999999999999999999999999999999999~", MathematicalSignOption.None,                TestCulture1, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.Any,                 TestCulture1, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.Positive,            TestCulture1, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.Negative,            TestCulture1, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.None,                TestCulture2, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.Any,                 TestCulture2, true),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.Positive,            TestCulture2, true),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.Negative,            TestCulture2, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.PositiveOrNone,      TestCulture2, true),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.NegativeOrNone,      TestCulture2, false),
            ("999999999999999999999999999999999999999999~", MathematicalSignOption.PositiveOrNegative,  TestCulture2, true),

            ("999999999999999999999999999999999999999999_", MathematicalSignOption.None,                TestCulture1, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.Any,                 TestCulture1, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.Positive,            TestCulture1, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.Negative,            TestCulture1, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.PositiveOrNone,      TestCulture1, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.NegativeOrNone,      TestCulture1, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.PositiveOrNegative,  TestCulture1, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.None,                TestCulture2, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.Any,                 TestCulture2, true),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.Positive,            TestCulture2, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.Negative,            TestCulture2, true),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.PositiveOrNone,      TestCulture2, false),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.NegativeOrNone,      TestCulture2, true),
            ("999999999999999999999999999999999999999999_", MathematicalSignOption.PositiveOrNegative,  TestCulture2, true),
        };

        private static readonly (string value, NumberGroupSeparatorOption NumberGroupSeparator, CultureInfo culture, bool expected)[] NumberGroupSeparatorTestCases =
        {
            ("", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("0", NumberGroupSeparatorOption.None, TestCulture1, true),
            ("0", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("0", NumberGroupSeparatorOption.Required, TestCulture1, true),
            ("0", NumberGroupSeparatorOption.None, TestCulture2, true),
            ("0", NumberGroupSeparatorOption.Optional, TestCulture2, true),
            ("0", NumberGroupSeparatorOption.Required, TestCulture2, true),

            ("10", NumberGroupSeparatorOption.None, TestCulture1, true),
            ("10", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("10", NumberGroupSeparatorOption.Required, TestCulture1, true),
            ("10", NumberGroupSeparatorOption.None, TestCulture2, true),
            ("10", NumberGroupSeparatorOption.Optional, TestCulture2, true),
            ("10", NumberGroupSeparatorOption.Required, TestCulture2, true),

            ("1000", NumberGroupSeparatorOption.None, TestCulture1, true),
            ("1000", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("1000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1000", NumberGroupSeparatorOption.None, TestCulture2, true),
            ("1000", NumberGroupSeparatorOption.Optional, TestCulture2, true),
            ("1000", NumberGroupSeparatorOption.Required, TestCulture2, true),

            ("1,000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1,000", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("1,000", NumberGroupSeparatorOption.Required, TestCulture1, true),
            ("1,000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1,000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("1,000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("1.000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1.000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("1.000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1.000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1.000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("1.000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("10000", NumberGroupSeparatorOption.None, TestCulture1, true),
            ("10000", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("10000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("10000", NumberGroupSeparatorOption.None, TestCulture2, true),
            ("10000", NumberGroupSeparatorOption.Optional, TestCulture2, true),
            ("10000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("1,0000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1,0000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("1,0000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1,0000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1,0000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("1,0000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("1.0000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1.0000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("1.0000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1.0000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1.0000", NumberGroupSeparatorOption.Optional, TestCulture2, true),
            ("1.0000", NumberGroupSeparatorOption.Required, TestCulture2, true),

            ("1,000,000,000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1,000,000,000", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("1,000,000,000", NumberGroupSeparatorOption.Required, TestCulture1, true),
            ("1,000,000,000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1,000,000,000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("1,000,000,000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("1,000000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1,000000000", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("1,000000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1,000000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1,000000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("1,000000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("1.0000.0000.0000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1.0000.0000.0000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("1.0000.0000.0000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1.0000.0000.0000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1.0000.0000.0000", NumberGroupSeparatorOption.Optional, TestCulture2, true),
            ("1.0000.0000.0000", NumberGroupSeparatorOption.Required, TestCulture2, true),

            ("1.000000000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1.000000000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("1.000000000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1.000000000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1.000000000000", NumberGroupSeparatorOption.Optional, TestCulture2, true),
            ("1.000000000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("10,0000000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("10,0000000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("10,0000000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("10,0000000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("10,0000000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("10,0000000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("100,000000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("100,000000000", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("100,000000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("100,000000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("100,000000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("100,000000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("1,0,0000000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1,0,0000000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("1,0,0000000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1,0,0000000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1,0,0000000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("1,0,0000000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("10,0,000000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("10,0,000000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("10,0,000000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("10,0,000000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("10,0,000000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("10,0,000000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("100,0,00000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("100,0,00000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("100,0,00000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("100,0,00000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("100,0,00000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("100,0,00000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("1,00,000000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1,00,000000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("1,00,000000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1,00,000000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1,00,000000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("1,00,000000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("10,00,00000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("10,00,00000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("10,00,00000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("10,00,00000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("10,00,00000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("10,00,00000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("100,00,0000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("100,00,0000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("100,00,0000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("100,00,0000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("100,00,0000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("100,00,0000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("1,000,00000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("1,000,00000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("1,000,00000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("1,000,00000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("1,000,00000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("1,000,00000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("10,000,0000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("10,000,0000000", NumberGroupSeparatorOption.Optional, TestCulture1, false),
            ("10,000,0000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("10,000,0000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("10,000,0000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("10,000,0000000", NumberGroupSeparatorOption.Required, TestCulture2, false),

            ("100,000,000000", NumberGroupSeparatorOption.None, TestCulture1, false),
            ("100,000,000000", NumberGroupSeparatorOption.Optional, TestCulture1, true),
            ("100,000,000000", NumberGroupSeparatorOption.Required, TestCulture1, false),
            ("100,000,000000", NumberGroupSeparatorOption.None, TestCulture2, false),
            ("100,000,000000", NumberGroupSeparatorOption.Optional, TestCulture2, false),
            ("100,000,000000", NumberGroupSeparatorOption.Required, TestCulture2, false),
        };

        private static readonly (string value, int decimalDigits, bool decimalDigitRequired, CultureInfo culture, bool expected)[] DecimalDigitsTestCases =
        {
            ("", 0,     false, TestCulture1, false),
            ("", 1,     false, TestCulture1, false),
            ("", 2,     false, TestCulture1, false),
            ("", 10,    false, TestCulture1, false),
            ("", 100,   false, TestCulture1, false),
            ("", 0,     true,  TestCulture1, false),
            ("", 1,     true,  TestCulture1, false),
            ("", 2,     true,  TestCulture1, false),
            ("", 10,    true,  TestCulture1, false),
            ("", 100,   true,  TestCulture1, false),
            ("", 0,     false, TestCulture2, false),
            ("", 1,     false, TestCulture2, false),
            ("", 2,     false, TestCulture2, false),
            ("", 10,    false, TestCulture2, false),
            ("", 100,   false, TestCulture2, false),
            ("", 0,     true,  TestCulture2, false),
            ("", 1,     true,  TestCulture2, false),
            ("", 2,     true,  TestCulture2, false),
            ("", 10,    true,  TestCulture2, false),
            ("", 100,   true,  TestCulture2, false),

            ("0", 0,     false, TestCulture1, true),
            ("0", 1,     false, TestCulture1, true),
            ("0", 2,     false, TestCulture1, true),
            ("0", 10,    false, TestCulture1, true),
            ("0", 100,   false, TestCulture1, true),
            ("0", 0,     true,  TestCulture1, true),
            ("0", 1,     true,  TestCulture1, false),
            ("0", 2,     true,  TestCulture1, false),
            ("0", 10,    true,  TestCulture1, false),
            ("0", 100,   true,  TestCulture1, false),
            ("0", 0,     false, TestCulture2, true),
            ("0", 1,     false, TestCulture2, true),
            ("0", 2,     false, TestCulture2, true),
            ("0", 10,    false, TestCulture2, true),
            ("0", 100,   false, TestCulture2, true),
            ("0", 0,     true,  TestCulture2, true),
            ("0", 1,     true,  TestCulture2, false),
            ("0", 2,     true,  TestCulture2, false),
            ("0", 10,    true,  TestCulture2, false),
            ("0", 100,   true,  TestCulture2, false),

            ("10", 0,     false, TestCulture1, true),
            ("10", 1,     false, TestCulture1, true),
            ("10", 2,     false, TestCulture1, true),
            ("10", 10,    false, TestCulture1, true),
            ("10", 100,   false, TestCulture1, true),
            ("10", 0,     true,  TestCulture1, true),
            ("10", 1,     true,  TestCulture1, false),
            ("10", 2,     true,  TestCulture1, false),
            ("10", 10,    true,  TestCulture1, false),
            ("10", 100,   true,  TestCulture1, false),
            ("10", 0,     false, TestCulture2, true),
            ("10", 1,     false, TestCulture2, true),
            ("10", 2,     false, TestCulture2, true),
            ("10", 10,    false, TestCulture2, true),
            ("10", 100,   false, TestCulture2, true),
            ("10", 0,     true,  TestCulture2, true),
            ("10", 1,     true,  TestCulture2, false),
            ("10", 2,     true,  TestCulture2, false),
            ("10", 10,    true,  TestCulture2, false),
            ("10", 100,   true,  TestCulture2, false),

            ("10000000", 0,     false, TestCulture1, true),
            ("10000000", 1,     false, TestCulture1, true),
            ("10000000", 2,     false, TestCulture1, true),
            ("10000000", 10,    false, TestCulture1, true),
            ("10000000", 100,   false, TestCulture1, true),
            ("10000000", 0,     true,  TestCulture1, true),
            ("10000000", 1,     true,  TestCulture1, false),
            ("10000000", 2,     true,  TestCulture1, false),
            ("10000000", 10,    true,  TestCulture1, false),
            ("10000000", 100,   true,  TestCulture1, false),
            ("10000000", 0,     false, TestCulture2, true),
            ("10000000", 1,     false, TestCulture2, true),
            ("10000000", 2,     false, TestCulture2, true),
            ("10000000", 10,    false, TestCulture2, true),
            ("10000000", 100,   false, TestCulture2, true),
            ("10000000", 0,     true,  TestCulture2, true),
            ("10000000", 1,     true,  TestCulture2, false),
            ("10000000", 2,     true,  TestCulture2, false),
            ("10000000", 10,    true,  TestCulture2, false),
            ("10000000", 100,   true,  TestCulture2, false),

            ("3.1", 0,     false, TestCulture1, false),
            ("3.1", 1,     false, TestCulture1, true),
            ("3.1", 2,     false, TestCulture1, true),
            ("3.1", 10,    false, TestCulture1, true),
            ("3.1", 100,   false, TestCulture1, true),
            ("3.1", 0,     true,  TestCulture1, false),
            ("3.1", 1,     true,  TestCulture1, true),
            ("3.1", 2,     true,  TestCulture1, false),
            ("3.1", 10,    true,  TestCulture1, false),
            ("3.1", 100,   true,  TestCulture1, false),
            ("3.1", 0,     false, TestCulture2, false),
            ("3.1", 1,     false, TestCulture2, false),
            ("3.1", 2,     false, TestCulture2, false),
            ("3.1", 10,    false, TestCulture2, false),
            ("3.1", 100,   false, TestCulture2, false),
            ("3.1", 0,     true,  TestCulture2, false),
            ("3.1", 1,     true,  TestCulture2, false),
            ("3.1", 2,     true,  TestCulture2, false),
            ("3.1", 10,    true,  TestCulture2, false),
            ("3.1", 100,   true,  TestCulture2, false),

            ("3.14", 0,     false, TestCulture1, false),
            ("3.14", 1,     false, TestCulture1, false),
            ("3.14", 2,     false, TestCulture1, true),
            ("3.14", 10,    false, TestCulture1, true),
            ("3.14", 100,   false, TestCulture1, true),
            ("3.14", 0,     true,  TestCulture1, false),
            ("3.14", 1,     true,  TestCulture1, false),
            ("3.14", 2,     true,  TestCulture1, true),
            ("3.14", 10,    true,  TestCulture1, false),
            ("3.14", 100,   true,  TestCulture1, false),
            ("3.14", 0,     false, TestCulture2, false),
            ("3.14", 1,     false, TestCulture2, false),
            ("3.14", 2,     false, TestCulture2, false),
            ("3.14", 10,    false, TestCulture2, false),
            ("3.14", 100,   false, TestCulture2, false),
            ("3.14", 0,     true,  TestCulture2, false),
            ("3.14", 1,     true,  TestCulture2, false),
            ("3.14", 2,     true,  TestCulture2, false),
            ("3.14", 10,    true,  TestCulture2, false),
            ("3.14", 100,   true,  TestCulture2, false),

            ("3.1415926535", 0,     false, TestCulture1, false),
            ("3.1415926535", 1,     false, TestCulture1, false),
            ("3.1415926535", 2,     false, TestCulture1, false),
            ("3.1415926535", 10,    false, TestCulture1, true),
            ("3.1415926535", 100,   false, TestCulture1, true),
            ("3.1415926535", 0,     true,  TestCulture1, false),
            ("3.1415926535", 1,     true,  TestCulture1, false),
            ("3.1415926535", 2,     true,  TestCulture1, false),
            ("3.1415926535", 10,    true,  TestCulture1, true),
            ("3.1415926535", 100,   true,  TestCulture1, false),
            ("3.1415926535", 0,     false, TestCulture2, false),
            ("3.1415926535", 1,     false, TestCulture2, false),
            ("3.1415926535", 2,     false, TestCulture2, false),
            ("3.1415926535", 10,    false, TestCulture2, false),
            ("3.1415926535", 100,   false, TestCulture2, false),
            ("3.1415926535", 0,     true,  TestCulture2, false),
            ("3.1415926535", 1,     true,  TestCulture2, false),
            ("3.1415926535", 2,     true,  TestCulture2, false),
            ("3.1415926535", 10,    true,  TestCulture2, false),
            ("3.1415926535", 100,   true,  TestCulture2, false),

            ("3,1", 0,     false, TestCulture1, false),
            ("3,1", 1,     false, TestCulture1, false),
            ("3,1", 2,     false, TestCulture1, false),
            ("3,1", 10,    false, TestCulture1, false),
            ("3,1", 100,   false, TestCulture1, false),
            ("3,1", 0,     true,  TestCulture1, false),
            ("3,1", 1,     true,  TestCulture1, false),
            ("3,1", 2,     true,  TestCulture1, false),
            ("3,1", 10,    true,  TestCulture1, false),
            ("3,1", 100,   true,  TestCulture1, false),
            ("3,1", 0,     false, TestCulture2, false),
            ("3,1", 1,     false, TestCulture2, true),
            ("3,1", 2,     false, TestCulture2, true),
            ("3,1", 10,    false, TestCulture2, true),
            ("3,1", 100,   false, TestCulture2, true),
            ("3,1", 0,     true,  TestCulture2, false),
            ("3,1", 1,     true,  TestCulture2, true),
            ("3,1", 2,     true,  TestCulture2, false),
            ("3,1", 10,    true,  TestCulture2, false),
            ("3,1", 100,   true,  TestCulture2, false),

            ("3,14", 0,     false, TestCulture1, false),
            ("3,14", 1,     false, TestCulture1, false),
            ("3,14", 2,     false, TestCulture1, false),
            ("3,14", 10,    false, TestCulture1, false),
            ("3,14", 100,   false, TestCulture1, false),
            ("3,14", 0,     true,  TestCulture1, false),
            ("3,14", 1,     true,  TestCulture1, false),
            ("3,14", 2,     true,  TestCulture1, false),
            ("3,14", 10,    true,  TestCulture1, false),
            ("3,14", 100,   true,  TestCulture1, false),
            ("3,14", 0,     false, TestCulture2, false),
            ("3,14", 1,     false, TestCulture2, false),
            ("3,14", 2,     false, TestCulture2, true),
            ("3,14", 10,    false, TestCulture2, true),
            ("3,14", 100,   false, TestCulture2, true),
            ("3,14", 0,     true,  TestCulture2, false),
            ("3,14", 1,     true,  TestCulture2, false),
            ("3,14", 2,     true,  TestCulture2, true),
            ("3,14", 10,    true,  TestCulture2, false),
            ("3,14", 100,   true,  TestCulture2, false),

            ("3,1415926535", 0,     false, TestCulture1, false),
            ("3,1415926535", 1,     false, TestCulture1, false),
            ("3,1415926535", 2,     false, TestCulture1, false),
            ("3,1415926535", 10,    false, TestCulture1, false),
            ("3,1415926535", 100,   false, TestCulture1, false),
            ("3,1415926535", 0,     true,  TestCulture1, false),
            ("3,1415926535", 1,     true,  TestCulture1, false),
            ("3,1415926535", 2,     true,  TestCulture1, false),
            ("3,1415926535", 10,    true,  TestCulture1, false),
            ("3,1415926535", 100,   true,  TestCulture1, false),
            ("3,1415926535", 0,     false, TestCulture2, false),
            ("3,1415926535", 1,     false, TestCulture2, false),
            ("3,1415926535", 2,     false, TestCulture2, false),
            ("3,1415926535", 10,    false, TestCulture2, true),
            ("3,1415926535", 100,   false, TestCulture2, true),
            ("3,1415926535", 0,     true,  TestCulture2, false),
            ("3,1415926535", 1,     true,  TestCulture2, false),
            ("3,1415926535", 2,     true,  TestCulture2, false),
            ("3,1415926535", 10,    true,  TestCulture2, true),
            ("3,1415926535", 100,   true,  TestCulture2, false),
        };

        private static readonly (string value, WhiteSpaceOption whiteSpace, bool expected)[] WhiteSpaceTestCases =
        {
            ("0", WhiteSpaceOption.None, true),
            (" 0", WhiteSpaceOption.None, false),
            ("\t0", WhiteSpaceOption.None, false),
            ("\r0", WhiteSpaceOption.None, false),
            ("\n0", WhiteSpaceOption.None, false),
            ("\r\n\t 0", WhiteSpaceOption.None, false),
            ("0 ", WhiteSpaceOption.None, false),
            ("0\t", WhiteSpaceOption.None, false),
            ("0\r", WhiteSpaceOption.None, false),
            ("0\n", WhiteSpaceOption.None, false),
            ("0\r\n\t ", WhiteSpaceOption.None, false),
            (" 0 ", WhiteSpaceOption.None, false),
            ("\t0\t", WhiteSpaceOption.None, false),
            ("\r0\r", WhiteSpaceOption.None, false),
            ("\n0\n", WhiteSpaceOption.None, false),
            ("\r\n\t 0\r\n\t ", WhiteSpaceOption.None, false),

            ("0", WhiteSpaceOption.Leading, true),
            (" 0", WhiteSpaceOption.Leading, true),
            ("\t0", WhiteSpaceOption.Leading, true),
            ("\r0", WhiteSpaceOption.Leading, true),
            ("\n0", WhiteSpaceOption.Leading, true),
            ("\r\n\t 0", WhiteSpaceOption.Leading, true),
            ("0 ", WhiteSpaceOption.Leading, false),
            ("0\t", WhiteSpaceOption.Leading, false),
            ("0\r", WhiteSpaceOption.Leading, false),
            ("0\n", WhiteSpaceOption.Leading, false),
            ("0\r\n\t ", WhiteSpaceOption.Leading, false),
            (" 0 ", WhiteSpaceOption.Leading, false),
            ("\t0\t", WhiteSpaceOption.Leading, false),
            ("\r0\r", WhiteSpaceOption.Leading, false),
            ("\n0\n", WhiteSpaceOption.Leading, false),
            ("\r\n\t 0\r\n\t ", WhiteSpaceOption.Leading, false),

            ("0", WhiteSpaceOption.Trailing, true),
            (" 0", WhiteSpaceOption.Trailing, false),
            ("\t0", WhiteSpaceOption.Trailing, false),
            ("\r0", WhiteSpaceOption.Trailing, false),
            ("\n0", WhiteSpaceOption.Trailing, false),
            ("\r\n\t 0", WhiteSpaceOption.Trailing, false),
            ("0 ", WhiteSpaceOption.Trailing, true),
            ("0\t", WhiteSpaceOption.Trailing, true),
            ("0\r", WhiteSpaceOption.Trailing, true),
            ("0\n", WhiteSpaceOption.Trailing, true),
            ("0\r\n\t ", WhiteSpaceOption.Trailing, true),
            (" 0 ", WhiteSpaceOption.Trailing, false),
            ("\t0\t", WhiteSpaceOption.Trailing, false),
            ("\r0\r", WhiteSpaceOption.Trailing, false),
            ("\n0\n", WhiteSpaceOption.Trailing, false),
            ("\r\n\t 0\r\n\t ", WhiteSpaceOption.Trailing, false),

            ("0", WhiteSpaceOption.LeadingOrTrailing, true),
            (" 0", WhiteSpaceOption.LeadingOrTrailing, true),
            ("\t0", WhiteSpaceOption.LeadingOrTrailing, true),
            ("\r0", WhiteSpaceOption.LeadingOrTrailing, true),
            ("\n0", WhiteSpaceOption.LeadingOrTrailing, true),
            ("\r\n\t 0", WhiteSpaceOption.LeadingOrTrailing, true),
            ("0 ", WhiteSpaceOption.LeadingOrTrailing, true),
            ("0\t", WhiteSpaceOption.LeadingOrTrailing, true),
            ("0\r", WhiteSpaceOption.LeadingOrTrailing, true),
            ("0\n", WhiteSpaceOption.LeadingOrTrailing, true),
            ("0\r\n\t ", WhiteSpaceOption.LeadingOrTrailing, true),
            (" 0 ", WhiteSpaceOption.LeadingOrTrailing, true),
            ("\t0\t", WhiteSpaceOption.LeadingOrTrailing, true),
            ("\r0\r", WhiteSpaceOption.LeadingOrTrailing, true),
            ("\n0\n", WhiteSpaceOption.LeadingOrTrailing, true),
            ("\r\n\t 0\r\n\t ", WhiteSpaceOption.LeadingOrTrailing, true)
        };

        private static TestCaseData[] _numericTestCases = NumericTestCases.Select(m => new TestCaseData(m.value) { ExpectedResult = m.expected }).ToArray();

        private static TestCaseData[] _digitCountTestCases = DigitCountTestCases.Select(m => new TestCaseData(m.value, m.digitCount) { ExpectedResult = m.expected }).ToArray();

        private static TestCaseData[] _mathematicalSignTestCases = MathematicalSignTestCases.Select(m => new TestCaseData(m.value, m.mathSign, m.culture) { ExpectedResult = m.expected }).ToArray();

        private static TestCaseData[] _numberGroupSeparatorTestCases = NumberGroupSeparatorTestCases.Select(m => new TestCaseData(m.value, m.NumberGroupSeparator, m.culture) { ExpectedResult = m.expected }).ToArray();

        private static TestCaseData[] _decimalDigitsTestCases = DecimalDigitsTestCases.Select(m => new TestCaseData(m.value, m.decimalDigits, m.decimalDigitRequired, m.culture) { ExpectedResult = m.expected }).ToArray();

        private static TestCaseData[] _whiteSpaceTestCases = WhiteSpaceTestCases.Select(m => new TestCaseData(m.value, m.whiteSpace) { ExpectedResult = m.expected }).ToArray();

        #region Predefined Test Case Sources

        private static readonly IEnumerable<TestCaseData> NumericTwoDecimalPlacesOptionalSignTestCases = new(string value, bool expected)[]
                {
                    ("+1.11", true),
                    ("-1.11", true),
                    ("+1.111", false),
                    ("-1.111", false),
                    ("1.00", true),
                    ("1.00", true),
                    ("+1,000,000,000,000.00", true),
                    ("1,000,000,000,000.00", true),
                    ("+1000000000000.00", true),
                    ("-1,00.00", false),
                    ("1,00.0", false),
                    ("1,00.0", false),
                    ("1,00", false),
                    ("1,00", false)
                }
                .Select(t => new TestCaseData(
                        t.value,
                        Numeric.OptionalMathematicalSignOptionalNumberGroupSeparatorTwoRequiredDecimalDigitsPattern)
                { ExpectedResult = t.expected });

        private static readonly IEnumerable<TestCaseData> NumericUpToTwoDecimalPlacesTestCases = new(string value, bool expected)[]
            {
                // No legacy tests exist. Below are test cases inferred from pattern and description.
                ("0", true),
                ("0.", true),
                ("0.0", true),
                ("0.00", true),
                ("0.000", false),
                ("100", true),
                ("100.", true),
                ("100.0", true),
                ("100.00", true),
                ("100.000", false),
                ("100000000000000000000", true),
                ("100000000000000000000.", true),
                ("100000000000000000000.0", true),
                ("100000000000000000000.00", true),
                ("100000000000000000000.000", false),
            }
            .Select(t => new TestCaseData(
                    t.value,
                    Numeric.TwoOptionalDecimalDigitsPattern)
            { ExpectedResult = t.expected });

        private static readonly IEnumerable<TestCaseData> NumericFormatAcceptLeadingAndTrailingSpacesTestCases = new(string value, bool expected)[]
            {
                (" 123", true),
                ("123 ", true),
                ("  123  ", true),
                (" 12 34 ", false),
                ("12345", true),
                (" a123", false),
                ("12 45", false),
                ("12.34", false),
                ("123b ", false)
            }
            .Select(t => new TestCaseData(
                    t.value,
                    Numeric.LeadingAndTrailingWhiteSpacePattern)
            { ExpectedResult = t.expected });

        private static readonly IEnumerable<TestCaseData> NumericFormatSixDigitsTestCases = new(string value, bool expected)[]
                {
                    ("123456", true),
                    ("1234567", false),
                    ("12345", false),
                    ("12345.0", false),
                    ("1234.0", false),
                    ("1234.0", false),
                    ("12345a", false),
                    ("123456a", false)
                }
                .Select(t => new TestCaseData(
                        t.value,
                        Numeric.SixDigitsPattern)
                { ExpectedResult = t.expected });

        private static readonly TestCaseData[] PredefinedPatternTestCases = NumericTwoDecimalPlacesOptionalSignTestCases
            .Concat(NumericUpToTwoDecimalPlacesTestCases)
            .Concat(NumericFormatAcceptLeadingAndTrailingSpacesTestCases)
            .Concat(NumericFormatSixDigitsTestCases)
            .ToArray();

        private static readonly TestCaseData[] PredefinedRegexTestCases = PredefinedPatternTestCases
            .Select(t => new TestCaseData(t.Arguments[0], new Regex((string)t.Arguments[1])) { ExpectedResult = t.ExpectedResult })
            .ToArray();

        #endregion

        #endregion

        [TestCaseSource(nameof(PredefinedPatternTestCases))]
        public bool PredefinedPatternTest(string value, string pattern)
        {
            Match match = new Regex(pattern).Match(value);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(PredefinedRegexTestCases))]
        public bool PredefinedRegexTest(string value, Regex regex)
        {
            Match match = regex.Match(value);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_numericTestCases))]
        public bool RegexTest(string value)
        {
            Match match = Numeric.Regex.Match(value);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_numericTestCases))]
        public bool PatternTest(string value)
        {
            Match match = Regex.Match(value, Numeric.Pattern);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_digitCountTestCases))]
        public bool DigitCountRegexTest(string value, int digitCount)
        {
            Match match = Numeric.BuildRegex(digitCount).Match(value);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_digitCountTestCases))]
        public bool DigitCountPatternTest(string value, int digitCount)
        {
            Match match = Regex.Match(value, Numeric.BuildPattern(digitCount));
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_mathematicalSignTestCases))]
        public bool MathematicalSignRegexTest(string value, MathematicalSignOption mathematicalSignOption, CultureInfo culture)
        {
            Match match = Numeric.BuildRegex(mathematicalSignOption: mathematicalSignOption, culture: culture).Match(value);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_mathematicalSignTestCases))]
        public bool MathematicalSignPatternTest(string value, MathematicalSignOption mathematicalSignOption, CultureInfo culture)
        {
            Match match = Regex.Match(value, Numeric.BuildPattern(mathematicalSignOption: mathematicalSignOption, culture: culture));
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_numberGroupSeparatorTestCases))]
        public bool NumberGroupSeparatorRegexTest(string value, NumberGroupSeparatorOption numberGroupSeparatorOption, CultureInfo culture)
        {
            Match match = Numeric.BuildRegex(numberGroupSeparatorOption: numberGroupSeparatorOption, culture: culture).Match(value);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_numberGroupSeparatorTestCases))]
        public bool NumberGroupSeparatorPatternTest(string value, NumberGroupSeparatorOption numberGroupSeparatorOption, CultureInfo culture)
        {
            Match match = Regex.Match(value, Numeric.BuildPattern(numberGroupSeparatorOption: numberGroupSeparatorOption, culture: culture));
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_decimalDigitsTestCases))]
        public bool DecimalDigitsRegexTest(string value, int decimalDigits, bool decimalDigitRequired, CultureInfo culture)
        {
            Match match = Numeric.BuildRegex(decimalDigits: decimalDigits, decimalDigitRequired: decimalDigitRequired, culture: culture).Match(value);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_decimalDigitsTestCases))]
        public bool DecimalDigitsPatternTest(string value, int decimalDigits, bool decimalDigitRequired, CultureInfo culture)
        {
            Match match = Regex.Match(value, Numeric.BuildPattern(decimalDigits: decimalDigits, decimalDigitRequired: decimalDigitRequired, culture: culture));
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_whiteSpaceTestCases))]
        public bool WhiteSpaceRegexTest(string value, WhiteSpaceOption whiteSpaceOption)
        {
            Match match = Numeric.BuildRegex(whiteSpaceOption: whiteSpaceOption).Match(value);
            return match.Success && match.Value == value;
        }

        [TestCaseSource(nameof(_whiteSpaceTestCases))]
        public bool WhiteSpacePatternTest(string value, WhiteSpaceOption whiteSpaceOption)
        {
            Match match = Regex.Match(value, Numeric.BuildPattern(whiteSpaceOption: whiteSpaceOption));
            return match.Success && match.Value == value;
        }
    }
}
