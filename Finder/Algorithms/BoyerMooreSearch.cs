using Finder.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finder.Algorithms
{
    /// <summary>
    /// An implemention of Boyer-Moore algorithm.
    /// <para/>author : Ornithopter
    /// </summary>
    class BoyerMooreSearch : SearchBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns>An array of matched index</returns>
        public int[] Search(string source, string pattern)
        {
            var matchIndexes = new List<int>();

            // step increasment.
            int delta;

            // prepare a map providing delta for each char in pattern string.
            var deltaMap = CreateDeltaMap(pattern);

            // start searching.
            for (var i = pattern.Length - 1; i < source.Length; i += delta)
            {
                // find next match and update delta.
                if (FindNext(source, pattern, i, deltaMap, out delta))
                {
                    // add to result list if found.
                    matchIndexes.Add(i - (pattern.Length - 1));
                }
            }
            return matchIndexes.ToArray();
        }

        private static bool Match(string source, int[] deltaMap, string pattern)
        {
            // step increasment.
            int delta;

            // start searching.
            for (var i = pattern.Length - 1; i < source.Length; i += delta)
            {
                // find next match and update delta.
                if (FindNext(source, pattern, i, deltaMap, out delta))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Find the next matched index and update delte at the same time.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <param name="start"></param>
        /// <param name="deltaMap"></param>
        /// <param name="delta"></param>
        /// <returns>true if found one, otherwise false.</returns>
        private static bool FindNext(string source, string pattern, int start, int[] deltaMap, out int delta)
        {
            int i = pattern.Length - 1,
                index = 0;

            // start comparing from the last char in pattern.
            while (source[start - index] == pattern[i - index])
            {
                if (index != pattern.Length - 1)
                {
                    index++;
                }
                else
                {
                    // matchs to the end. So it's a search result.
                    delta = pattern.Length;
                    return true;
                }
            }

            // found one dismatched char at (start - index), get delta from map.
            var c = source[start - index];
            delta = /*c > 128 ? 0 : */deltaMap[c];

            if (delta == 0)
            {
                // this means the source[start] char is the last char in pattern
                // and only appears once. So delta should be the length of pattern.
                delta = pattern.Length;
            }
            return false;
        }

        static int[] CreateDeltaMap(string pattern)
        {
            const int alphabetSize = 0xffff;
            var patternLength = pattern.Length;
            var deltaMap = new int[alphabetSize];

            // initialize the map.
            for (var i = 0; i < alphabetSize; i++)
            {
                deltaMap[i] = patternLength;
            }

            // start from 0, which means any duplicated char will only have
            // the index nearest to the end.
            for (var i = 0; i < patternLength; i++)
            {
                var index = pattern[i];
                deltaMap[index] = patternLength - i - 1;
            }
            return deltaMap;
        }

        public string[] Search(string keyword)
        {
            var patternDic = Patterns.Split('|').ToLookup(_ => _);
            var folderPath = FolderPath;

            var fileList = new List<string>();
            FileSystemUtil.Walkthrough(fileList, patternDic, folderPath, Depth);

            var deltaMap = CreateDeltaMap(keyword);

            return fileList.Where(filePath => Match(ReadContent(filePath), deltaMap, keyword)).ToArray();
        }

        public override Task BuildAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<string[]> SearchAsync(string keyword, Dictionary<string, object> config)
        {
            return Task<string[]>.Factory.StartNew(() => Search(keyword));
        }

        public override int IndexedCount
        {
            get { return 0; }
        }

        public override bool SupportBuild
        {
            get { return false; }
        }
    }
}