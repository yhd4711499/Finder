using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Finder.Algorithms
{
    interface ISearchAlgorithm
    {
        int CodePage { get; set; }
        string Patterns { get; set; }
        int Depth { get; set; }
        string FolderPath { get; set; }
        List<string> FileList { get; }
        //string[] Search(string keyword, Dictionary<string, object> config);
        Task BuildAsync(CancellationToken ct);
        Task<List<SearchResult>> SearchAsync(string keyword, Dictionary<Configs, object> config, CancellationToken token);
        List<SearchResult> Search(string keyword, Dictionary<Configs, object> config, CancellationToken token);
        int IndexedCount { get; }
    }
}