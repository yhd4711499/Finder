using System;
using System.Collections.Generic;
using Finder.Algorithms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Finder.Test
{
    [TestClass]
    public class BoyerMoore
    {
        const string Alphabet = "1234567890~!@#$%^&*()-=_+[]{};':abcdefghijklmnopqrstuvwxyz并且直接调用静态库里面的方法完成实";
        static readonly int AlphabetSize = Alphabet.Length;
        const int LenMin = 1, LenMax = 100;
       
        private const int Times = 500000 * 4;

        static readonly string Pattern;
        static readonly List<string> Sources = new List<string>();

        static BoyerMoore()
        {
            var indexRandom = new Random();
            Pattern = GenerateRandomString(LenMin, LenMax);
            for (var i = 0; i < Times; i++)
            {
                var source = GenerateRandomString(0, LenMax);
                Sources.Add(source.Insert(indexRandom.Next(0, source.Length), Pattern));
            }
        }

        private static string GenerateRandomString(int lengthMin, int lengthMax)
        {
            var randomChar = new Random();
            var randomLength = new Random();
            var len = randomLength.Next(lengthMin, lengthMax);
            var chars = new char[len];
            for (var i = 0; i < len; i++)
            {
                chars[i] = (Alphabet[randomChar.Next(0, AlphabetSize - 1)]);
            }
            return new string(chars);
        }

        [TestMethod]
        public void TestMethod_BoyerMooreSearch()
        {
            var deltaMap = BoyerMooreSearch.CreateDeltaMap(Pattern);
            foreach (var source in Sources)
            {
                Assert.AreEqual(BoyerMooreSearch.Match(source, Pattern, deltaMap), true);
            }
        }

        [TestMethod]
        public void TestMethod_NativeBoyerMooreSearch()
        {
            var deltaMap = NativeBoyerMooreSearch.CreateDeltaMap(Pattern);
            foreach (var source in Sources)
            {
                Assert.AreEqual(NativeBoyerMooreSearch.Match(source, Pattern, deltaMap), true);
            }
        }

        [TestMethod]
        public void TestMethod_StringContains()
        {
            foreach (var source in Sources)
            {
                Assert.AreEqual(source.Contains(Pattern), true);
            }
        }
    }
}
