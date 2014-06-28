using Finder.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finder.Algorithms
{
    class Trie : SearchBase
    {
        class Node
        {
            public Dictionary<char, Node> Next { get; private set; }
            public Dictionary<int, bool> FileIndex { get; private set; }

            public Node()
            {
                FileIndex = new Dictionary<int, bool>();
                Next = new Dictionary<char, Node>();
            }
        }

        private static readonly HashSet<char> Ignorance = new HashSet<char>
        {
            '[',
            '(',
            '{',
        };

        private static readonly HashSet<char> Spliters = new HashSet<char>
        {
            ' ',
            ',',
            '\n',
            '\r',
            ';',
            ']',
            ')',
            '}',
        };

        private Node _root;

        public List<string> FileList { get; private set; }

        public override Task BuildAsync()
        {
            return Task.Factory.StartNew(Build);
        }

        public void Build()
        {
            var folderPath = FolderPath;
            _root = new Node();

            GC.Collect();

            var index = 0;
            var patternDic = Patterns.Split('|').ToLookup(_ => _);

            FileList = new List<string>();

            FileSystemUtil.Walkthrough(FileList, patternDic, folderPath, Depth);

            foreach (var filePath in FileList)
            {
                Build(index++, ReadContent(filePath));
            }
        }

        private void Build(int fileIndex, string content)
        {
            var node = _root;
            foreach (var t in content)
            {
                var c = t;

                if (Ignorance.Contains(c))
                {
                    continue;
                }
                if (Spliters.Contains(c))
                {
                    node.FileIndex[fileIndex] = true;
                    node = _root;
                    continue;
                }

                /*if (!Char.IsLetter(c))
                    continue;*/

                if (Char.IsUpper(c))
                    c = Char.ToLower(c);

                Node next;
                if (!node.Next.TryGetValue(c, out next))
                {
                    next = new Node();
                    node.Next[c] = next;
                }
                node = next;
                node.FileIndex[fileIndex] = false;
            }
            node.FileIndex[fileIndex] = true;
        }

        private string[] Search(string keyword, bool matchWholeWord)
        {
            var node = _root;
            foreach (var t in keyword)
            {
                var c = t;
                if (Char.IsUpper(c))
                    c = Char.ToLower(c);

                Node next;
                if (!node.Next.TryGetValue(c, out next))
                {
                    return new string[0];
                }
                node = next;
            }

            if (matchWholeWord)
            {
                return node.FileIndex.Where(_ => _.Value).Select(_ => FileList[_.Key]).ToArray();
            }
            return node.FileIndex.Select(_ => FileList[_.Key]).ToArray();
        }

        public const string ConfigMatchWholeWord = "ConfigMatchWholeWord";

        public string[] Search(string keyword, Dictionary<string, object> config)
        {
            var matchWholeWord = (bool)config[ConfigMatchWholeWord];
            return Search(keyword, matchWholeWord);
        }


        public override int IndexedCount
        {
            get { return FileList.Count; }
        }


        public override bool SupportBuild
        {
            get { return true; }
        }

        public override Task<string[]> SearchAsync(string keyword, Dictionary<string, object> config)
        {
            return Task<string[]>.Factory.StartNew(() => Search(keyword, config));
        }
    }
}