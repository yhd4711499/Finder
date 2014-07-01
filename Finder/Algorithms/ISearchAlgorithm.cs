using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        bool RequestBuild { get; }

        Task BuildAsync(CancellationToken ct);
        Task<List<SearchResult>> SearchAsync(string keyword, Dictionary<Configs, object> config, CancellationToken token);
        List<SearchResult> Search(string keyword, Dictionary<Configs, object> config, CancellationToken token);
        int IndexedCount { get; }
    }
}