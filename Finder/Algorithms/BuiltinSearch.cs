using System.Collections.Generic;
using System.Linq;

namespace Finder.Algorithms
{
    class BuiltinSearch : SearchBase
    {
        protected override void Build(System.Threading.CancellationToken token)
        {
        }

        public override List<SearchResult> Search(string keyword, Dictionary<Configs, object> config, System.Threading.CancellationToken token)
        {
            var matchAll = config.ContainsKey(Configs.MatchAll) && (bool)config[Configs.MatchAll];
            var results = new List<SearchResult>();

            FileList.AsParallel().ForAll(filePath =>
            {
                token.ThrowIfCancellationRequested();
                var lines = ReadContents(filePath);
                var lineIndex = 0;
                foreach (var line in lines.TakeWhile(line => line != null))
                {
                    if (line.Contains(keyword))
                    {
                        results.Add(new SearchResult(FileList.IndexOf(filePath), lineIndex));
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
