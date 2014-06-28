using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Finder.Algorithms
{
    abstract class SearchBase : ISearchAlgorithm
    {
        public int CodePage { get; set; }
        public string Patterns { get; set; }
        public int Depth { get; set; }
        public string FolderPath { get; set; }
        public abstract Task BuildAsync();

        public abstract Task<string[]> SearchAsync(string keyword, Dictionary<string, object> config);

        static int utf8_probability(byte[] rawtext)
        {
            int score = 0;
            int i, rawtextlen = 0;
            int goodbytes = 0, asciibytes = 0;

            // Maybe also use UTF8 Byte Order Mark:  EF BB BF

            // Check to see if characters fit into acceptable ranges
            rawtextlen = rawtext.Length;
            for (i = 0; i < rawtextlen; i++)
            {
                if ((rawtext[i] & (byte)0x7F) == rawtext[i])
                {  // One byte
                    asciibytes++;
                    // Ignore ASCII, can throw off count
                }
                else
                {
                    int m_rawInt0 = Convert.ToInt16(rawtext[i]);
                    int m_rawInt1 = Convert.ToInt16(rawtext[i + 1]);
                    int m_rawInt2 = Convert.ToInt16(rawtext[i + 2]);

                    if (256 - 64 <= m_rawInt0 && m_rawInt0 <= 256 - 33 && // Two bytes
                     i + 1 < rawtextlen &&
                     256 - 128 <= m_rawInt1 && m_rawInt1 <= 256 - 65)
                    {
                        goodbytes += 2;
                        i++;
                    }
                    else if (256 - 32 <= m_rawInt0 && m_rawInt0 <= 256 - 17 && // Three bytes
                     i + 2 < rawtextlen &&
                     256 - 128 <= m_rawInt1 && m_rawInt1 <= 256 - 65 &&
                     256 - 128 <= m_rawInt2 && m_rawInt2 <= 256 - 65)
                    {
                        goodbytes += 3;
                        i += 2;
                    }
                }
            }

            if (asciibytes == rawtextlen) { return 0; }

            score = (int)(100 * ((float)goodbytes / (float)(rawtextlen - asciibytes)));

            // If not above 98, reduce to zero to prevent coincidental matches
            // Allows for some (few) bad formed sequences
            if (score > 98)
            {
                return score;
            }
            else if (score > 95 && goodbytes > 30)
            {
                return score;
            }
            else
            {
                return 0;
            }

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

        public abstract int IndexedCount { get; }
        public abstract bool SupportBuild { get; }
    }
}
