﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finder.Util;
using System.ComponentModel;

namespace Finder.Algorithms
{
    public abstract class SearchBase : ISearchAlgorithm
    {
        public bool RequestBuild { get; protected set; }
        public virtual int CodePage { get; set; }
        public string Patterns { get; set; }
        public int Depth { get; set; }
        public string FolderPath { get; set; }
        public List<string> FileList { get; private set; }

        public event Action OnBuildStarted, OnSearchStarted;
        public event Action<RunWorkerCompletedEventArgs> OnBuildStopped, OnSearchStopped;

        private List<string> GetFileList()
        {
            var patternDic = Patterns.Split('|').ToLookup(_ => _);
            var folderPath = FolderPath;
            var fileList = new List<string>();
            FileSystemUtil.Walkthrough(fileList, patternDic, folderPath, Depth);
            return fileList;
        }

        protected abstract void Build(CancellationToken token);

        public Task BuildAsync(CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                if (OnBuildStarted != null)
                    OnBuildStarted();
                GC.Collect();
                FileList = GetFileList();
                Build(ct);
            }, ct).ContinueWith(t=>{
                RaiseBuildStopped(t.Exception, t.IsCanceled);
            });
        }

        public Task<List<SearchResult>> SearchAsync(string keyword, Dictionary<Configs, object> config,
            CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    return Search(keyword, config, token);
                }
                catch (AggregateException ex)
                {
                    if(ex.InnerException is OperationCanceledException)
                        throw ex.InnerException;
                    throw ex;
                }
            }, token);
        }

        public abstract List<SearchResult> Search(string keyword, Dictionary<Configs, object> config, CancellationToken token);

        static int utf8_probability(byte[] rawtext)
        {
            int i;
            int goodbytes = 0, asciibytes = 0;

            // Maybe also use UTF8 Byte Order Mark:  EF BB BF

            // Check to see if characters fit into acceptable ranges
            var rawtextlen = rawtext.Length;
            for (i = 0; i < rawtextlen - 2; i++)
            {
                if ((rawtext[i] & (byte)0x7F) == rawtext[i])
                {  // One byte
                    asciibytes++;
                    // Ignore ASCII, can throw off count
                }
                else
                {
                    int mRawInt0 = Convert.ToInt16(rawtext[i]);
                    int mRawInt1 = Convert.ToInt16(rawtext[i + 1]);
                    int mRawInt2 = Convert.ToInt16(rawtext[i + 2]);

                    if (256 - 64 <= mRawInt0 && mRawInt0 <= 256 - 33 && // Two bytes
                     i + 1 < rawtextlen &&
                     256 - 128 <= mRawInt1 && mRawInt1 <= 256 - 65)
                    {
                        goodbytes += 2;
                        i++;
                    }
                    else if (256 - 32 <= mRawInt0 && mRawInt0 <= 256 - 17 && // Three bytes
                     i + 2 < rawtextlen &&
                     256 - 128 <= mRawInt1 && mRawInt1 <= 256 - 65 &&
                     256 - 128 <= mRawInt2 && mRawInt2 <= 256 - 65)
                    {
                        goodbytes += 3;
                        i += 2;
                    }
                }
            }

            if (asciibytes == rawtextlen) { return 0; }

            var score = (int)(100 * ((float)goodbytes / (float)(rawtextlen - asciibytes)));

            // If not above 98, reduce to zero to prevent coincidental matches
            // Allows for some (few) bad formed sequences
            if (score > 98)
            {
                return score;
            }
            if (score > 95 && goodbytes > 30)
            {
                return score;
            }
            return 0;
        }

        protected IEnumerable<string> ReadContents(string filePath)
        {
            Encoding encode;

            if (CodePage == 0)
            {
                try
                {
                    using (var srtest = new StreamReader(filePath, Encoding.Default))
                    {
                        var p = utf8_probability(Encoding.Default.GetBytes(srtest.ReadToEnd()));
                        encode = p > 80 ? Encoding.GetEncoding(65001) : Encoding.Default;
                    }
                }
                catch (Exception)
                {
                    encode = Encoding.Default;
                }
                
            }
            else
            {
                encode = Encoding.GetEncoding(CodePage);
            }
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(filePath, encode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't read file {0}. cause:{1}", filePath, ex.Message);
            }

            if (reader == null)
                yield return null;
            else
            {
                using (reader)
                {
                    while (!reader.EndOfStream)
                    {
                        yield return reader.ReadLine();
                    }
                }
            }
            
        }

        public int IndexedCount
        {
            get { return FileList == null ? 0 : FileList.Count; }
        }

        protected void RaiseBuildStopped(Exception error, bool cancelled)
        {
            if (OnBuildStopped != null)
            {
                OnBuildStopped(new RunWorkerCompletedEventArgs(null, error, cancelled));
            }
        }

        protected void RaiseSearchStopped(object results ,Exception error, bool cancelled)
        {
            if (OnSearchStopped != null)
            {
                OnSearchStopped(new RunWorkerCompletedEventArgs(results, error, cancelled));
            }
        }
    }

    public class SearchResult
    {
        public SearchResult(int fileIndex, int line)
        {
            Line = line;
            FileIndex = fileIndex;
        }

        public int FileIndex { get; private set; }
        public int Line { get; private set; }
    }

    public enum Configs
    {
        MatchAll,
        MatchWholeWord,
    }
}
