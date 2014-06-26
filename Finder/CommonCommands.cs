using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Finder
{
    public class CommonCommands
    {
        private static RelayCommand<object> _shellExecuteCommand;

        /// <summary>
        /// Gets the ShellExecuteCommand.
        /// </summary>
        public static RelayCommand<object> ShellExecuteCommand
        {
            get
            {
                return _shellExecuteCommand
                       ?? (_shellExecuteCommand = new RelayCommand<object>(
                           p =>
                           {
                               var cmd = p as string;
                               if (cmd == null) return;
                               ShellUtil.ShellExecute(IntPtr.Zero, "open", cmd, null, null,
                                   (int)ShellUtil.ShowWindowCommands.SW_NORMAL);
                           }));
            }
        }

        private static RelayCommand<object> _browseCommand;

        /// <summary>
        /// Gets the BrowseCommand.
        /// </summary>
        public static RelayCommand<object> BrowseCommand
        {
            get
            {
                return _browseCommand
                       ?? (_browseCommand = new RelayCommand<object>(
                           p =>
                           {
                               var path = p as string;
                               if (path == null) return;
                               var folder = Path.GetDirectoryName(path);
                               ShellUtil.ShellExecute(IntPtr.Zero, "open", folder, null, null,
                                   (int) ShellUtil.ShowWindowCommands.SW_NORMAL);
                           }));
            }
        }
    }
}
