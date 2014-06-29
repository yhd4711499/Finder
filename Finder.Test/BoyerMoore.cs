using System;
using System.Collections.Generic;
using Finder.Algorithms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Finder.Test
{
    [TestClass]
    public class BoyerMoore
    {
        [TestMethod]
        public void TestMethod1()
        {
            const int sourceLenMin = 3, sourceLenMax = 20;
            const string alphabet = "1234567890~!@#$%^&*()-=_+[]{};':abcdefghijklmnopqrstuvwxyz";
            var alphabetSize = alphabet.Length;
            var randomChar = new Random();
            var randomSourceLen = new Random();
            var randomPatternLen = new Random();
            var randomPatternStart = new Random();
            for (var time = 0; time < 10000; time++)
            {
                var sourceLen = randomSourceLen.Next(sourceLenMin, sourceLenMax);
                var chars = new char[sourceLen];
                for (var i = 0; i < sourceLen; i++)
                {
                    chars[i] = (alphabet[randomChar.Next(0, alphabetSize - 1)]);
                }
                var source = new string(chars);

                var patternStart = randomPatternStart.Next(0, source.Length - 2);
                var patternLen = randomPatternLen.Next(1, source.Length - patternStart - 2);
                var pattern = source.Substring(patternStart, patternLen);
                
                Assert.AreEqual(BoyerMooreSearch.Match(source, pattern), true);
            }
            
        }
    }
}
