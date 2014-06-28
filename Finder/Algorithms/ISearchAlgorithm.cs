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
        //string[] Search(string keyword, Dictionary<string, object> config);
        Task BuildAsync(CancellationToken ct);
        string[] Search(string keyword, Dictionary<string, object> config, CancellationToken token);
        int IndexedCount { get; }
    }
}