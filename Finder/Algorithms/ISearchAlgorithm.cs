using System.Collections.Generic;
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
        Task BuildAsync();
        Task<string[]> SearchAsync(string keyword, Dictionary<string, object> config);
        int IndexedCount { get; }
        bool SupportBuild { get; }
    }
}