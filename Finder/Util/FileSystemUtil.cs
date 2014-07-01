using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Finder.Util
{
    static class FileSystemUtil
    {
        public static void Walkthrough(List<string> result, ILookup<string, string> patternDic, string folder, int depth)
        {
            if (depth == 0)
                return;
            try
            {
                result.AddRange(Directory.GetFiles(folder).Where(filePath => patternDic.Contains(Path.GetExtension(filePath).ToLower())));
                foreach (var directory in Directory.GetDirectories(folder))
                {
                    Walkthrough(result, patternDic, directory, depth - 1);
                }
            }
            catch (Exception)
            {
                
            }
        } 
    }
}
