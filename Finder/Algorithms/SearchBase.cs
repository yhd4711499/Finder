using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Finder.Util;

namespace Finder.Algorithms
{
    abstract class SearchBase : ISearchAlgorithm
    {
        public int CodePage { get; set; }
        public string Patterns { get; set; }
        public int Depth { get; set; }
        public string FolderPath { get; set; }
        protected List<string> FileList { get; private set; }
        protected List<string> GetFileList()
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
                GC.Collect();
                FileList = GetFileList();
                Build(ct);
            }, ct);
        }

        public abstract string[] Search(string keyword, Dictionary<string, object> config, CancellationToken token);

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

        protected string ReadContent(string filePath)
        {
            Encoding encode;
            if (CodePage == 0)
            {
                using (var srtest = new StreamReader(filePath, Encoding.Default))
                {
                    var p = utf8_probability(Encoding.Default.GetBytes(srtest.ReadToEnd()));
                    encode = p > 80 ? Encoding.GetEncoding(65001) : Encoding.Default;
                }
            }
            else
            {
                encode = Encoding.GetEncoding(CodePage);
            }
            string content;
            using (var reader = new StreamReader(filePath, encode))
            {
                content = reader.ReadToEnd();
            }
            return content;
        }

        public int IndexedCount
        {
            get { return FileList == null ? 0 : FileList.Count; }
        }
    }
}
