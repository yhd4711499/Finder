using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

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


        public override int CodePage
        {
            get
            {
                return base.CodePage;
            }
            set
            {
                if (base.CodePage == value) return;
                base.CodePage = value;
                RequestBuild = true;
            }
        }

        protected override void Build(CancellationToken token)
        {
            _root = new Node();

            var index = 0;
            foreach (var filePath in FileList)
            {
                token.ThrowIfCancellationRequested();
                Build(index++, String.Concat(ReadContents(filePath)));
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

        private List<SearchResult> Search(string keyword, bool matchWholeWord, CancellationToken token)
        {
            var node = _root;
            foreach (var t in keyword)
            {
                token.ThrowIfCancellationRequested();
                var c = t;
                if (Char.IsUpper(c))
                    c = Char.ToLower(c);

                Node next;
                if (!node.Next.TryGetValue(c, out next))
                {
                    return new List<SearchResult>(0);
                }
                node = next;
            }

            if (matchWholeWord)
            {
                return node.FileIndex.Where(_ => _.Value).Select(_ => new SearchResult(_.Key, 0)).ToList();
            }
            return node.FileIndex.Select(_ => new SearchResult(_.Key, 0)).ToList();
        }

        public override List<SearchResult> Search(string keyword, Dictionary<Configs, object> config, CancellationToken token)
        {
            var matchWholeWord = (bool)config[Configs.MatchWholeWord];
            return Search(keyword, matchWholeWord, token);
        }
    }
}