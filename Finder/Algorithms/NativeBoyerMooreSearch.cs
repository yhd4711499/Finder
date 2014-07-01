using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Finder.Algorithms
{
    /// <summary>
    /// An wrapper for unmanaged Boyer-Moore algorithm.
    /// <para/>author : Ornithopter
    /// </summary>
    public class NativeBoyerMooreSearch : SearchBase
    {
        private const string DllPath = "lib.dll";

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void createDeltaMap(
            [MarshalAs(UnmanagedType.LPWStr)]string pattern, 
            int[] outDeltaMap);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int getDeltaMapSize();

        [DllImport(DllPath,CallingConvention = CallingConvention.Cdecl)]
        private static extern int contains(
            [MarshalAs(UnmanagedType.LPWStr)]string source,
            int sourceLen,
            [MarshalAs(UnmanagedType.LPWStr)]string pattern,
            int patternLen,
            int[] deltaMap);

        #region Algorithm
        
        public static bool Match(string source, string pattern, int[] deltaMap)
        {
            return contains(source, source.Length, pattern, pattern.Length, deltaMap) == 1;
        }

        public static int[] CreateDeltaMap(string pattern)
        {
            var size = getDeltaMapSize();
            var deltaMap = new int[size];
            createDeltaMap(pattern, deltaMap);
            return deltaMap;
        }
        #endregion

        protected override void Build(CancellationToken token)
        {
        }

        public override List<SearchResult> Search(string keyword, Dictionary<Configs, object> config, CancellationToken token)
        {
            var fileList = FileList;

            var deltaMap = CreateDeltaMap(keyword);

            var matchAll = config.ContainsKey(Configs.MatchAll) && (bool)config[Configs.MatchAll];

            var results = new List<SearchResult>();

            fileList.AsParallel().ForAll(filePath =>
                {
                    token.ThrowIfCancellationRequested();
                    var lines = ReadContents(filePath);
                    var lineIndex = 0;
                    foreach (var line in lines.TakeWhile(line => line != null))
                    {
                        var match = Match(line, keyword, deltaMap);
                        if (match)
                        {
                            results.Add(new SearchResult(fileList.IndexOf(filePath), lineIndex));
                            if (!matchAll)
                                break;
                        }
                        lineIndex++;
                    }
                });
            return results;
        }
    }
}