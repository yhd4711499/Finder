using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Finder
{
    class Trie
    {
        class Node
        {
            public Dictionary<char,Node> Next { get; private set; }
            public HashSet<int> FileIndex { get; private set; }
            public bool End { get; set; }

            public Node()
            {
                FileIndex = new HashSet<int>();
                Next = new Dictionary<char, Node>();
            }
        }

        private static readonly HashSet<char> Spliters = new HashSet<char>
        {
            ' ',
            ',',
            '.',
            '\n',
            '\r',
            ';',
            '\\',
            '/',
            '_',
            '-',
        };

        private Node _root;

        public List<string> FileList { get; private set; }

        public Task BuildAsync(string folderPath, int depth, string patterns)
        {
            return Task.Factory.StartNew(() => Build(folderPath, depth, patterns));
        }

        public void Build(string folderPath, int depth, string patterns)
        {
            _root = new Node();

            GC.Collect();

            var index = 0;
            var patternDic = patterns.Split('|').ToLookup(_ => _);

            FileList = new List<string>();

            Walkthrough(FileList, patternDic, folderPath, depth);

            foreach (var filePath in FileList)
            {
                using (TextReader reader = new StreamReader(filePath))
                {
                    Build(index++, reader.ReadToEnd());
                }
            }
        }

        private static void Walkthrough(List<string> result, ILookup<string, string> patternDic, string folder, int depth)
        {
            if(depth == 0)
                return;
            result.AddRange(Directory.GetFiles(folder).Where(filePath => patternDic.Contains(Path.GetExtension(filePath).ToLower())));

            foreach (var directory in Directory.GetDirectories(folder))
            {
                Walkthrough(result, patternDic, directory, depth-1);
            }
        } 

        private void Build(int fileIndex, string content)
        {
            var node = _root;
            foreach (var t in content)
            {
                var c = t;

                if (Spliters.Contains(c))
                {
                    node.End = true;
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
                node.FileIndex.Add(fileIndex);
            }
            node.End = true;
        }

        public string[] Search(string keyword, bool matchWholeWord)
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
            var strings = node.FileIndex.Select(_ => FileList[_]).ToArray();
            if (matchWholeWord)
            {
                return node.End ? strings : new string[0];
            }
            return strings;
        }
    }
}
