using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace Finder.Algorithms
{
    /// <summary>
    /// An implemention of Boyer-Moore algorithm.
    /// <para/>author : Ornithopter
    /// </summary>
    public class BoyerMooreSearch : SearchBase
    {
        const int AlphabetSize = 0xffff;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <param name="token"></param>
        /// <returns>An array of matched index</returns>
        public int[] Search(string source, string pattern, CancellationToken token)
        {
            var matchIndexes = new List<int>();

            // step increasment.
            int delta;

            // prepare a map providing delta for each char in pattern string.
            var deltaMap = CreateDeltaMap(pattern);

            // start searching.
            for (var i = pattern.Length - 1; i < source.Length; i += delta)
            {
                token.ThrowIfCancellationRequested();
                // find next match and update delta.
                if (FindNext(source, pattern, i, deltaMap, token, out delta))
                {
                    // add to result list if found.
                    matchIndexes.Add(i - (pattern.Length - 1));
                }
            }
            return matchIndexes.ToArray();
        }

        public static bool Match(string source, string pattern)
        {
            var deltaMap = CreateDeltaMap(pattern);
            return Match(source, deltaMap, pattern, new CancellationToken());
        }

        private static bool Match(string source, int[] deltaMap, string pattern, CancellationToken token)
        {
            // step increasment.
            var delta = 0;

            // for display.
            var indent = pattern.Length;
            Debug.WriteLine(source);

            // start searching.
            for (var i = pattern.Length - 1; i < source.Length; i += delta)
            {
                // for display.
                indent += delta;
                Debug.Write(String.Format("{0}({1})", pattern.PadLeft(indent, '.'), delta));

                token.ThrowIfCancellationRequested();
                // find next match and update delta.
                if (FindNext(source, pattern, i, deltaMap, token, out delta))
                {
                    Debug.Write("√");
                    return true;
                }
                if(delta <= 0)
                    throw new Exception();
                // new line.
                Debug.WriteLine("");
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
        private static bool FindNext(string source, string pattern, int start, int[] deltaMap, CancellationToken token, out int delta)
        {
            int i = pattern.Length - 1,
                index = 0;

            // start comparing from the last char in pattern.
            while (source[start - index] == pattern[i - index])
            {
                token.ThrowIfCancellationRequested();
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

            delta = c >= AlphabetSize ? 1 : deltaMap[c];

            if (delta <= index)
            {
                // this means the source[start] char is the last char in pattern
                // and only appears once. So delta should be the length of pattern.
                delta = pattern.Length;
            }
            else
            {
                delta = delta - index;
            }
            return false;
        }

        static int[] CreateDeltaMap(string pattern)
        {
            
            var patternLength = pattern.Length;
            var deltaMap = new int[AlphabetSize];

            // initialize the map.
            for (var i = 0; i < AlphabetSize; i++)
            {
                deltaMap[i] = patternLength;
            }

            // start from 0, which means any duplicated char will only have
            // the index nearest to the end.
            for (var i = 0; i < patternLength; i++)
            {
                var index = pattern[i];
                if(index >= AlphabetSize) throw new ArgumentException("搜索关键词包含非法的字符");
                deltaMap[index] = patternLength - i - 1;
            }
            return deltaMap;
        }

        protected override void Build(CancellationToken token)
        {
            //throw new NotImplementedException();
        }

        public override List<SearchResult> Search(string keyword, Dictionary<Configs, object> config, CancellationToken token)
        {
            var fileList = FileList;

            var deltaMap = CreateDeltaMap(keyword);

            var matchAll = config.ContainsKey(Configs.MatchAll) && (bool)config[Configs.MatchAll];

            var results = new List<SearchResult>();
            for (var fileIndex = 0; fileIndex < fileList.Count; fileIndex++)
            {
                var filePath = fileList[fileIndex];
                var lines = ReadContents(filePath);
                var lineIndex = 0;
                foreach (var line in lines.TakeWhile(line => line != null))
                {
                    if (Match(line, deltaMap, keyword, token))
                    {
                        results.Add(new SearchResult(fileIndex, lineIndex));
                        if(!matchAll)
                            break;
                    }
                    lineIndex++;
                }
            }
            return results;
        }
    }
}