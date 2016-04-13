﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace System.Text.RegularExpressions.Tests
{
    public class RegexMatchTests
    {
        public static IEnumerable<object[]> Match_Basic_TestData()
        {
            // Testing octal sequence matches: "\\060(\\061)?\\061"
            // Octal \061 is ASCII 49 ('1')
            yield return new object[] { @"\060(\061)?\061", "011", RegexOptions.None, 0, 3, true, "011" };

            // Testing hexadecimal sequence matches: "(\\x30\\x31\\x32)"
            // Hex \x31 is ASCII 49 ('1')
            yield return new object[] { @"(\x30\x31\x32)", "012", RegexOptions.None, 0, 3, true, "012" };

            // Testing control character escapes???: "2", "(\u0032)"
            yield return new object[] { "(\u0034)", "4", RegexOptions.None, 0, 1, true, "4", };

            // Using *, +, ?, {}: Actual - "a+\\.?b*\\.?c{2}"
            yield return new object[] { @"a+\.?b*\.+c{2}", "ab.cc", RegexOptions.None, 0, 5, true, "ab.cc" };

            // Using [a-z], \s, \w: Actual - "([a-zA-Z]+)\\s(\\w+)"
            yield return new object[] { @"([a-zA-Z]+)\s(\w+)", "David Bau", RegexOptions.None, 0, 9, true, "David Bau" };

            // \\S, \\d, \\D, \\W: Actual - "(\\S+):\\W(\\d+)\\s(\\D+)"
            yield return new object[] { @"(\S+):\W(\d+)\s(\D+)", "Price: 5 dollars", RegexOptions.None, 0, 16, true, "Price: 5 dollars" };

            // \\S, \\d, \\D, \\W: Actual - "[^0-9]+(\\d+)"
            yield return new object[] { @"[^0-9]+(\d+)", "Price: 30 dollars", RegexOptions.None, 0, 17, true, "Price: 30" };

            // Zero-width negative lookahead assertion: Actual - "abc(?!XXX)\\w+"
            yield return new object[] { @"abc(?!XXX)\w+", "abcXXXdef", RegexOptions.None, 0, 9, false, string.Empty };

            // Zero-width positive lookbehind assertion: Actual - "(\\w){6}(?<=XXX)def"
            yield return new object[] { @"(\w){6}(?<=XXX)def", "abcXXXdef", RegexOptions.None, 0, 9, true, "abcXXXdef" };

            // Zero-width negative lookbehind assertion: Actual - "(\\w){6}(?<!XXX)def"
            yield return new object[] { @"(\w){6}(?<!XXX)def", "XXXabcdef", RegexOptions.None, 0, 9, true, "XXXabcdef" };

            // Nonbacktracking subexpression: Actual - "[^0-9]+(?>[0-9]+)3"
            // The last 3 causes the match to fail, since the non backtracking subexpression does not give up the last digit it matched
            // for it to be a success. For a correct match, remove the last character, '3' from the pattern
            yield return new object[] { "[^0-9]+(?>[0-9]+)3", "abc123", RegexOptions.None, 0, 6, false, string.Empty };

            // Using beginning/end of string chars \A, \Z: Actual - "\\Aaaa\\w+zzz\\Z"
            yield return new object[] { @"\Aaaa\w+zzz\Z", "aaaasdfajsdlfjzzz", RegexOptions.None, 0, 17, true, "aaaasdfajsdlfjzzz" };

            // Using beginning/end of string chars \A, \Z: Actual - "\\Aaaa\\w+zzz\\Z"
            yield return new object[] { @"\Aaaa\w+zzz\Z", "aaaasdfajsdlfjzzza", RegexOptions.None, 0, 18, false, string.Empty };

            // Using beginning/end of string chars \A, \Z: Actual - "\\Aaaa\\w+zzz\\Z"
            yield return new object[] { @"\A(line2\n)line3\Z", "line2\nline3\n", RegexOptions.Multiline, 0, 12, true, "line2\nline3" };

            // Using beginning/end of string chars ^: Actual - "^b"
            yield return new object[] { "^b", "ab", RegexOptions.None, 0, 2, false, string.Empty };

            // Actual - "(?<char>\\w)\\<char>"
            yield return new object[] { @"(?<char>\w)\<char>", "aa", RegexOptions.None, 0, 2, true, "aa" };

            // Actual - "(?<43>\\w)\\43"
            yield return new object[] { @"(?<43>\w)\43", "aa", RegexOptions.None, 0, 2, true, "aa" };

            // Actual - "abc(?(1)111|222)"
            yield return new object[] { "(abbc)(?(1)111|222)", "abbc222", RegexOptions.None, 0, 7, false, string.Empty };

            // "x" option. Removes unescaped white space from the pattern: Actual - " ([^/]+) ","x"
            yield return new object[] { "            ((.)+)      ", "abc", RegexOptions.IgnorePatternWhitespace, 0, 3, true, "abc" };

            // "x" option. Removes unescaped white space from the pattern. : Actual - "\x20([^/]+)\x20","x"
            yield return new object[] { "\x20([^/]+)\x20\x20\x20\x20\x20\x20\x20", " abc       ", RegexOptions.IgnorePatternWhitespace, 0, 10, true, " abc      " };

            // Turning on case insensitive option in mid-pattern : Actual - "aaa(?i:match this)bbb"
            if ("i".ToUpper() == "I")
            {
                yield return new object[] { "aaa(?i:match this)bbb", "aaaMaTcH ThIsbbb", RegexOptions.None, 0, 16, true, "aaaMaTcH ThIsbbb" };
            }

            // Turning off case insensitive option in mid-pattern : Actual - "aaa(?-i:match this)bbb", "i"
            yield return new object[] { "aaa(?-i:match this)bbb", "AaAmatch thisBBb", RegexOptions.IgnoreCase, 0, 16, true, "AaAmatch thisBBb" };

            // Turning on/off all the options at once : Actual - "aaa(?imnsx-imnsx:match this)bbb", "i"
            yield return new object[] { "aaa(?-i:match this)bbb", "AaAmatcH thisBBb", RegexOptions.IgnoreCase, 0, 16, false, string.Empty };

            // Actual - "aaa(?#ignore this completely)bbb"
            yield return new object[] { "aaa(?#ignore this completely)bbb", "aaabbb", RegexOptions.None, 0, 6, true, "aaabbb" };

            // Trying empty string: Actual "[a-z0-9]+", ""
            yield return new object[] { "[a-z0-9]+", "", RegexOptions.None, 0, 0, false, string.Empty };

            // Numbering pattern slots: "(?<1>\\d{3})(?<2>\\d{3})(?<3>\\d{4})"
            yield return new object[] { @"(?<1>\d{3})(?<2>\d{3})(?<3>\d{4})", "8885551111", RegexOptions.None, 0, 10, true, "8885551111" };
            yield return new object[] { @"(?<1>\d{3})(?<2>\d{3})(?<3>\d{4})", "Invalid string", RegexOptions.None, 0, 14, false, string.Empty };

            // Not naming pattern slots at all: "^(cat|chat)"
            yield return new object[] { "^(cat|chat)", "cats are bad", RegexOptions.None, 0, 12, true, "cat" };

            yield return new object[] { "abc", "abc", RegexOptions.None, 0, 3, true, "abc" };
            yield return new object[] { "abc", "aBc", RegexOptions.None, 0, 3, false, string.Empty };
            yield return new object[] { "abc", "aBc", RegexOptions.IgnoreCase, 0, 3, true, "aBc" };

            // Using *, +, ?, {}: Actual - "a+\\.?b*\\.?c{2}"
            yield return new object[] { @"a+\.?b*\.+c{2}", "ab.cc", RegexOptions.None, 0, 5, true, "ab.cc" };

            // RightToLeft
            yield return new object[] { @"\s+\d+", "sdf 12sad", RegexOptions.RightToLeft, 0, 9, true, " 12" };
            yield return new object[] { @"\s+\d+", " asdf12 ", RegexOptions.RightToLeft, 0, 6, false, string.Empty };
            yield return new object[] { "aaa", "aaabbb", RegexOptions.None, 3, 3, false, string.Empty };

            yield return new object[] { @"foo\d+", "0123456789foo4567890foo         ", RegexOptions.RightToLeft, 10, 3, false, string.Empty };
            yield return new object[] { @"foo\d+", "0123456789foo4567890foo         ", RegexOptions.RightToLeft, 11, 21, false, string.Empty };

            // IgnoreCase
            yield return new object[] { "AAA", "aaabbb", RegexOptions.IgnoreCase, 0, 6, true, "aaa" };

            // "\D+"
            yield return new object[] { @"\D+", "12321", RegexOptions.None, 0, 5, false, string.Empty };

            // Groups
            yield return new object[] { "(?<first_name>\\S+)\\s(?<last_name>\\S+)", "David Bau", RegexOptions.None, 0, 9, true, "David Bau" };

            // "^b"
            yield return new object[] { "^b", "abc", RegexOptions.None, 0, 3, false, string.Empty };

            // RightToLeft
            yield return new object[] { @"foo\d+", "0123456789foo4567890foo         ", RegexOptions.RightToLeft, 0, 32, true, "foo4567890" };
            yield return new object[] { @"foo\d+", "0123456789foo4567890foo         ", RegexOptions.RightToLeft, 10, 22, true, "foo4567890" };
            yield return new object[] { @"foo\d+", "0123456789foo4567890foo         ", RegexOptions.RightToLeft, 10, 4, true, "foo4" };

            // Trim leading and trailing white spaces
            yield return new object[] { @"\s*(.*?)\s*$", " Hello World ", RegexOptions.None, 0, 13, true, " Hello World " };

            // < in group
            yield return new object[] { @"(?<cat>cat)\w+(?<dog-0>dog)", "cat_Hello_World_dog", RegexOptions.None, 0, 19, false, string.Empty };

            // Atomic Zero-Width Assertions \A \Z \z \G \b \B
            yield return new object[] { @"\A(cat)\s+(dog)", "cat   \n\n\ncat     dog", RegexOptions.None, 0, 20, false, string.Empty };
            yield return new object[] { @"\A(cat)\s+(dog)", "cat   \n\n\ncat     dog", RegexOptions.Multiline, 0, 20, false, string.Empty };
            yield return new object[] { @"\A(cat)\s+(dog)", "cat   \n\n\ncat     dog", RegexOptions.ECMAScript, 0, 20, false, string.Empty };

            yield return new object[] { @"(cat)\s+(dog)\Z", "cat   dog\n\n\ncat", RegexOptions.None, 0, 15, false, string.Empty };
            yield return new object[] { @"(cat)\s+(dog)\Z", "cat   dog\n\n\ncat     ", RegexOptions.Multiline, 0, 20, false, string.Empty };
            yield return new object[] { @"(cat)\s+(dog)\Z", "cat   dog\n\n\ncat     ", RegexOptions.ECMAScript, 0, 20, false, string.Empty };

            yield return new object[] { @"(cat)\s+(dog)\z", "cat   dog\n\n\ncat", RegexOptions.None, 0, 15, false, string.Empty };
            yield return new object[] { @"(cat)\s+(dog)\z", "cat   dog\n\n\ncat     ", RegexOptions.Multiline, 0, 20, false, string.Empty };
            yield return new object[] { @"(cat)\s+(dog)\z", "cat   dog\n\n\ncat     ", RegexOptions.ECMAScript, 0, 20, false, string.Empty };
            yield return new object[] { @"(cat)\s+(dog)\z", "cat   \n\n\n   dog\n", RegexOptions.None, 0, 16, false, string.Empty };
            yield return new object[] { @"(cat)\s+(dog)\z", "cat   \n\n\n   dog\n", RegexOptions.Multiline, 0, 16, false, string.Empty };
            yield return new object[] { @"(cat)\s+(dog)\z", "cat   \n\n\n   dog\n", RegexOptions.ECMAScript, 0, 16, false, string.Empty };

            yield return new object[] { @"\b@cat", "123START123;@catEND", RegexOptions.None, 0, 19, false, string.Empty };
            yield return new object[] { @"\b<cat", "123START123'<catEND", RegexOptions.None, 0, 19, false, string.Empty };
            yield return new object[] { @"\b,cat", "satwe,,,START',catEND", RegexOptions.None, 0, 21, false, string.Empty };
            yield return new object[] { @"\b\[cat", "`12START123'[catEND", RegexOptions.None, 0, 19, false, string.Empty };

            yield return new object[] { @"\B@cat", "123START123@catEND", RegexOptions.None, 0, 18, false, string.Empty };
            yield return new object[] { @"\B<cat", "123START123<catEND", RegexOptions.None, 0, 18, false, string.Empty };
            yield return new object[] { @"\B,cat", "satwe,,,START,catEND", RegexOptions.None, 0, 20, false, string.Empty };
            yield return new object[] { @"\B\[cat", "`12START123[catEND", RegexOptions.None, 0, 18, false, string.Empty };

            // Lazy operator Backtracking
            yield return new object[] { @"http://([a-zA-z0-9\-]*\.?)*?(:[0-9]*)??/", "http://www.msn.com", RegexOptions.IgnoreCase, 0, 18, false, string.Empty };

            // Grouping Constructs Invalid Regular Expressions
            yield return new object[] { "(?!)", "(?!)cat", RegexOptions.None, 0, 7, false, string.Empty };
            yield return new object[] { "(?<!)", "(?<!)cat", RegexOptions.None, 0, 8, false, string.Empty };

            // Alternation construct
            yield return new object[] { "(?(cat)|dog)", "oof", RegexOptions.None, 0, 3, false, string.Empty };

            //No Negation
            yield return new object[] { "[abcd-[abcd]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { "[1234-[1234]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            //All Negation
            yield return new object[] { "[^abcd-[^abcd]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { "[^1234-[^1234]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            //No Negation        
            yield return new object[] { "[a-z-[a-z]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { "[0-9-[0-9]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            //All Negation
            yield return new object[] { "[^a-z-[^a-z]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { "[^0-9-[^0-9]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            // No Negation
            yield return new object[] { @"[\w-[\w]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\W-[\W]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\s-[\s]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\S-[\S]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\d-[\d]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\D-[\D]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            // All Negation
            yield return new object[] { @"[^\w-[^\w]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\W-[^\W]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\s-[^\s]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\S-[^\S]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\d-[^\d]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\D-[^\D]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            // MixedNegation
            yield return new object[] { @"[^\w-[\W]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\w-[^\W]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\s-[\S]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\s-[^\S]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\d-[\D]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\d-[^\D]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            //No Negation
            yield return new object[] { @"[\p{Ll}-[\p{Ll}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\P{Ll}-[\P{Ll}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\p{Lu}-[\p{Lu}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\P{Lu}-[\P{Lu}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\p{Nd}-[\p{Nd}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\P{Nd}-[\P{Nd}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            // All Negation
            yield return new object[] { @"[^\p{Ll}-[^\p{Ll}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\P{Ll}-[^\P{Ll}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\p{Lu}-[^\p{Lu}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\P{Lu}-[^\P{Lu}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\p{Nd}-[^\p{Nd}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\P{Nd}-[^\P{Nd}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            // MixedNegation
            yield return new object[] { @"[^\p{Ll}-[\P{Ll}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\p{Ll}-[^\P{Ll}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\p{Lu}-[\P{Lu}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\p{Lu}-[^\P{Lu}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[^\p{Nd}-[\P{Nd}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };
            yield return new object[] { @"[\p{Nd}-[^\P{Nd}]]+", "abcxyzABCXYZ`!@#$%^&*()_-+= \t\n", RegexOptions.None, 0, 30, false, string.Empty };

            // Character Class Substraction
            yield return new object[] { @"[ab\-\[cd-[-[]]]]", "[]]", RegexOptions.None, 0, 3, false, string.Empty };
            yield return new object[] { @"[ab\-\[cd-[-[]]]]", "-]]", RegexOptions.None, 0, 3, false, string.Empty };
            yield return new object[] { @"[ab\-\[cd-[-[]]]]", "`]]", RegexOptions.None, 0, 3, false, string.Empty };
            yield return new object[] { @"[ab\-\[cd-[-[]]]]", "e]]", RegexOptions.None, 0, 3, false, string.Empty };

            yield return new object[] { @"[ab\-\[cd-[[]]]]", "']]", RegexOptions.None, 0, 3, false, string.Empty };
            yield return new object[] { @"[ab\-\[cd-[[]]]]", "e]]", RegexOptions.None, 0, 3, false, string.Empty };

            yield return new object[] { @"[a-[a-f]]", "abcdefghijklmnopqrstuvwxyz", RegexOptions.None, 0, 26, false, string.Empty };
        }

        [Theory]
        [MemberData(nameof(Match_Basic_TestData))]
        public void Match(string pattern, string input, RegexOptions options, int beginning, int length, bool expectedSuccess, string expectedValue)
        {
            bool isDefaultStart = RegexHelpers.IsDefaultStart(input, options, beginning);
            bool isDefaultCount = RegexHelpers.IsDefaultCount(input, options, length);
            if (options == RegexOptions.None)
            {
                if (isDefaultStart && isDefaultCount)
                {
                    // Use Match(string) or Match(string, string)
                    VerifyMatch(new Regex(pattern).Match(input), expectedSuccess, expectedValue);
                    VerifyMatch(Regex.Match(input, pattern), expectedSuccess, expectedValue);

                    Assert.Equal(expectedSuccess, new Regex(pattern).IsMatch(input));
                    Assert.Equal(expectedSuccess, Regex.IsMatch(input, pattern));
                }
                if (beginning + length == input.Length)
                {
                    // Use Match(string, int)
                    VerifyMatch(new Regex(pattern).Match(input, beginning), expectedSuccess, expectedValue);

                    Assert.Equal(expectedSuccess, new Regex(pattern).IsMatch(input, beginning));
                }
                // Use Match(string, int, int)
                VerifyMatch(new Regex(pattern).Match(input, beginning, length), expectedSuccess, expectedValue);
            }
            if (isDefaultStart && isDefaultCount)
            {
                // Use Match(string) or Match(string, string, RegexOptions)
                VerifyMatch(new Regex(pattern, options).Match(input), expectedSuccess, expectedValue);
                VerifyMatch(Regex.Match(input, pattern, options), expectedSuccess, expectedValue);

                Assert.Equal(expectedSuccess, Regex.IsMatch(input, pattern, options));
            }
            if (beginning + length == input.Length && (options & RegexOptions.RightToLeft) == 0)
            {
                // Use Match(string, int)
                VerifyMatch(new Regex(pattern, options).Match(input, beginning), expectedSuccess, expectedValue);
            }
            // Use Match(string, int, int)
            VerifyMatch(new Regex(pattern, options).Match(input, beginning, length), expectedSuccess, expectedValue);
        }

        public static void VerifyMatch(Match match, bool expectedSuccess, string expectedValue)
        {
            Assert.Equal(expectedSuccess, match.Success);
            Assert.Equal(expectedValue, match.Value);
        }

        public static IEnumerable<object[]> Match_Advanced_TestData()
        {
            // \B special character escape: ".*\\B(SUCCESS)\\B.*"
            yield return new object[]
            {
                @".*\B(SUCCESS)\B.*", "adfadsfSUCCESSadsfadsf", RegexOptions.None, 0, 22,
                new Capture[]
                {
                    new Capture("adfadsfSUCCESSadsfadsf", 0, 22),
                    new Capture("SUCCESS", 7, 7)
                }
            };

            // Using |, (), ^, $, .: Actual - "^aaa(bb.+)(d|c)$"
            yield return new object[]
            {
                "^aaa(bb.+)(d|c)$", "aaabb.cc", RegexOptions.None, 0, 8,
                new Capture[]
                {
                    new Capture("aaabb.cc", 0, 8),
                    new Capture("bb.c", 3, 4),
                    new Capture("c", 7, 1)
                }
            };

            // Using greedy quantifiers: Actual - "(a+)(b*)(c?)"
            yield return new object[]
            {
                "(a+)(b*)(c?)", "aaabbbccc", RegexOptions.None, 0, 9,
                new Capture[]
                {
                    new Capture("aaabbbc", 0, 7),
                    new Capture("aaa", 0, 3),
                    new Capture("bbb", 3, 3),
                    new Capture("c", 6, 1)
                }
            };

            // Using lazy quantifiers: Actual - "(d+?)(e*?)(f??)"
            // Interesting match from this pattern and input. If needed to go to the end of the string change the ? to + in the last lazy quantifier
            yield return new object[]
            {
                "(d+?)(e*?)(f??)", "dddeeefff", RegexOptions.None, 0, 9,
                new Capture[]
                {
                    new Capture("d", 0, 1),
                    new Capture("d", 0, 1),
                    new Capture(string.Empty, 1, 0),
                    new Capture(string.Empty, 1, 0)
                }
            };

            // Noncapturing group : Actual - "(a+)(?:b*)(ccc)"
            yield return new object[]
            {
                "(a+)(?:b*)(ccc)", "aaabbbccc", RegexOptions.None, 0, 9,
                new Capture[]
                {
                    new Capture("aaabbbccc", 0, 9),
                    new Capture("aaa", 0, 3),
                    new Capture("ccc", 6, 3),
                }
            };

            // Zero-width positive lookahead assertion: Actual - "abc(?=XXX)\\w+"
            yield return new object[]
            {
                @"abc(?=XXX)\w+", "abcXXXdef", RegexOptions.None, 0, 9,
                new Capture[]
                {
                    new Capture("abcXXXdef", 0, 9)
                }
            };

            // Backreferences : Actual - "(\\w)\\1"
            yield return new object[]
            {
                @"(\w)\1", "aa", RegexOptions.None, 0, 2,
                new Capture[]
                {
                    new Capture("aa", 0, 2),
                    new Capture("a", 0, 1),
                }
            };

            // Alternation constructs: Actual - "(111|aaa)"
            yield return new object[]
            {
                "(111|aaa)", "aaa", RegexOptions.None, 0, 3,
                new Capture[]
                {
                    new Capture("aaa", 0, 3),
                    new Capture("aaa", 0, 3)
                }
            };

            // Actual - "(?<1>\\d+)abc(?(1)222|111)"
            yield return new object[]
            {
                @"(?<MyDigits>\d+)abc(?(MyDigits)222|111)", "111abc222", RegexOptions.None, 0, 9,
                new Capture[]
                {
                    new Capture("111abc222", 0, 9),
                    new Capture("111", 0, 3)
                }
            };

            // Using "n" Regex option. Only explicitly named groups should be captured: Actual - "([0-9]*)\\s(?<s>[a-z_A-Z]+)", "n"
            yield return new object[]
            {
                @"([0-9]*)\s(?<s>[a-z_A-Z]+)", "200 dollars", RegexOptions.ExplicitCapture, 0, 11,
                new Capture[]
                {
                    new Capture("200 dollars", 0, 11),
                    new Capture("dollars", 4, 7)
                }
            };

            // Single line mode "s". Includes new line character: Actual - "([^/]+)","s"
            yield return new object[]
            {
                "(.*)", "abc\nsfc", RegexOptions.Singleline, 0, 7,
                new Capture[]
                {
                    new Capture("abc\nsfc", 0, 7),
                    new Capture("abc\nsfc", 0, 7),
                }
            };

            // "([0-9]+(\\.[0-9]+){3})"
            yield return new object[]
            {
                @"([0-9]+(\.[0-9]+){3})", "209.25.0.111", RegexOptions.None, 0, 12,
                new Capture[]
                {
                    new Capture("209.25.0.111", 0, 12),
                    new Capture("209.25.0.111", 0, 12),
                    new Capture(".111", 8, 4, new Capture[]
                    {
                        new Capture(".25", 3, 3),
                        new Capture(".0", 6, 2),
                        new Capture(".111", 8, 4),
                    }),
                }
            };

            // Groups and captures
            yield return new object[]
            {
                @"(?<A1>a*)(?<A2>b*)(?<A3>c*)", "aaabbccccccccccaaaabc", RegexOptions.None, 0, 21,
                new Capture[]
                {
                    new Capture("aaabbcccccccccc", 0, 15),
                    new Capture("aaa", 0, 3),
                    new Capture("bb", 3, 2),
                    new Capture("cccccccccc", 5, 10)
                }
            };

            // Using |, (), ^, $, .: Actual - "^aaa(bb.+)(d|c)$"
            yield return new object[]
            {
                "^aaa(bb.+)(d|c)$", "aaabb.cc", RegexOptions.None, 0, 8,
                new Capture[]
                {
                    new Capture("aaabb.cc", 0, 8),
                    new Capture("bb.c", 3, 4),
                    new Capture("c", 7, 1)
                }
            };

            // Actual - ".*\\b(\\w+)\\b"
            yield return new object[]
            {
                @".*\b(\w+)\b", "XSP_TEST_FAILURE SUCCESS", RegexOptions.None, 0, 24,
                new Capture[]
                {
                    new Capture("XSP_TEST_FAILURE SUCCESS", 0, 24),
                    new Capture("SUCCESS", 17, 7)
                }
            };

            // Mutliline
            yield return new object[]
            {
                "(line2$\n)line3", "line1\nline2\nline3\n\nline4", RegexOptions.Multiline, 0, 24,
                new Capture[]
                {
                    new Capture("line2\nline3", 6, 11),
                    new Capture("line2\n", 6, 6)
                }
            };

            // Mutliline
            yield return new object[]
            {
                "(line2\n^)line3", "line1\nline2\nline3\n\nline4", RegexOptions.Multiline, 0, 24,
                new Capture[]
                {
                    new Capture("line2\nline3", 6, 11),
                    new Capture("line2\n", 6, 6)
                }
            };

            // Mutliline
            yield return new object[]
            {
                "(line3\n$\n)line4", "line1\nline2\nline3\n\nline4", RegexOptions.Multiline, 0, 24,
                new Capture[]
                {
                    new Capture("line3\n\nline4", 12, 12),
                    new Capture("line3\n\n", 12, 7)
                }
            };

            // Mutliline
            yield return new object[]
            {
                "(line3\n^\n)line4", "line1\nline2\nline3\n\nline4", RegexOptions.Multiline, 0, 24,
                new Capture[]
                {
                    new Capture("line3\n\nline4", 12, 12),
                    new Capture("line3\n\n", 12, 7)
                }
            };

            // Mutliline
            yield return new object[]
            {
                "(line2$\n^)line3", "line1\nline2\nline3\n\nline4", RegexOptions.Multiline, 0, 24,
                new Capture[]
                {
                    new Capture("line2\nline3", 6, 11),
                    new Capture("line2\n", 6, 6)
                }
            };

            // RightToLeft
            yield return new object[]
            {
                "aaa", "aaabbb", RegexOptions.RightToLeft, 3, 3,
                new Capture[]
                {
                    new Capture("aaa", 0, 3)
                }
            };
        }

        [Theory]
        [MemberData(nameof(Match_Advanced_TestData))]
        public void Match(string pattern, string input, RegexOptions options, int beginning, int length, Capture[] expected)
        {
            bool isDefaultStart = RegexHelpers.IsDefaultStart(input, options, beginning);
            bool isDefaultCount = RegexHelpers.IsDefaultStart(input, options, length);
            if (options == RegexOptions.None)
            {
                if (isDefaultStart  && isDefaultCount)
                {
                    // Use Match(string) or Match(string, string)
                    VerifyMatch(new Regex(pattern).Match(input), true, expected);
                    VerifyMatch(Regex.Match(input, pattern), true, expected);

                    Assert.True(new Regex(pattern).IsMatch(input));
                }
                if (beginning + length == input.Length)
                {
                    // Use Match(string, int)
                    VerifyMatch(new Regex(pattern).Match(input, beginning), true, expected);

                    Assert.True(new Regex(pattern).IsMatch(input, beginning));
                }
                else
                {
                    // Use Match(string, int, int)
                    VerifyMatch(new Regex(pattern).Match(input, beginning, length), true, expected);
                }
            }
            if (isDefaultStart && isDefaultCount)
            {
                // Use Match(string) or Match(string, string, RegexOptions)
                VerifyMatch(new Regex(pattern, options).Match(input), true, expected);
                VerifyMatch(Regex.Match(input, pattern, options), true, expected);

                Assert.True(Regex.IsMatch(input, pattern, options));
            }
            if (beginning + length == input.Length)
            {
                // Use Match(string, int)
                VerifyMatch(new Regex(pattern, options).Match(input, beginning), true, expected);
            }
            if ((options & RegexOptions.RightToLeft) == 0)
            {
                // Use Match(string, int, int)
                VerifyMatch(new Regex(pattern, options).Match(input, beginning, length), true, expected);
            }
        }

        public static void VerifyMatch(Match match, bool expectedSuccess, Capture[] expected)
        {
            Assert.Equal(expectedSuccess, match.Success);

            Assert.Equal(expected[0].Value, match.Value);
            Assert.Equal(expected[0].Index, match.Index);
            Assert.Equal(expected[0].Length, match.Length);

            Assert.Equal(1, match.Captures.Count);
            Assert.Equal(expected[0].Value, match.Captures[0].Value);
            Assert.Equal(expected[0].Index, match.Captures[0].Index);
            Assert.Equal(expected[0].Length, match.Captures[0].Length);

            Assert.Equal(expected.Length, match.Groups.Count);
            for (int i = 0; i < match.Groups.Count; i++)
            {
                Assert.Equal(expected[i].Value, match.Groups[i].Value);
                Assert.Equal(expected[i].Index, match.Groups[i].Index);
                Assert.Equal(expected[i].Length, match.Groups[i].Length);

                Assert.Equal(expected[i].Captures.Length, match.Groups[i].Captures.Count);
                for (int j = 0; j < match.Groups[i].Captures.Count; j++)
                {
                    Assert.Equal(expected[i].Captures[j].Value, match.Groups[i].Captures[j].Value);
                    Assert.Equal(expected[i].Captures[j].Index, match.Groups[i].Captures[j].Index);
                    Assert.Equal(expected[i].Captures[j].Length, match.Groups[i].Captures[j].Length);
                }
            }
        }

        [Theory]
        [InlineData(@"(?<1>\d{1,2})/(?<2>\d{1,2})/(?<3>\d{2,4})\s(?<time>\S+)", "08/10/99 16:00", "${time}", "16:00")]
        [InlineData(@"(?<1>\d{1,2})/(?<2>\d{1,2})/(?<3>\d{2,4})\s(?<time>\S+)", "08/10/99 16:00", "${1}", "08")]
        [InlineData(@"(?<1>\d{1,2})/(?<2>\d{1,2})/(?<3>\d{2,4})\s(?<time>\S+)", "08/10/99 16:00", "${2}", "10")]
        [InlineData(@"(?<1>\d{1,2})/(?<2>\d{1,2})/(?<3>\d{2,4})\s(?<time>\S+)", "08/10/99 16:00", "${3}", "99")]
        public void Result(string pattern, string input, string replacement, string expected)
        {
            Assert.Equal(expected, new Regex(pattern).Match(input).Result(replacement));
        }

        [Fact]
        public void Match_SpecialUnicodeCharacters()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            CultureInfo enUSCulture = new CultureInfo("en-US");
            try
            {
                CultureInfo.CurrentCulture = enUSCulture;
                Match("\u0131", "\u0049", RegexOptions.IgnoreCase, 0, 1, false, string.Empty);
                Match("\u0131", "\u0069", RegexOptions.IgnoreCase, 0, 1, false, string.Empty);

                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                Match("\u0131", "\u0049", RegexOptions.IgnoreCase, 0, 1, false, string.Empty);
                Match("\u0131", "\u0069", RegexOptions.IgnoreCase, 0, 1, false, string.Empty);
                Match("\u0130", "\u0049", RegexOptions.IgnoreCase, 0, 1, false, string.Empty);
                Match("\u0130", "\u0069", RegexOptions.IgnoreCase, 0, 1, false, string.Empty);
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
            }
        }
    }
}