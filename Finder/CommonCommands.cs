using System;
using System.IO;
using System.Windows.Controls;
using Finder.Util;

namespace Finder
{
    public static class CommonCommands
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

        private static RelayCommand<TextBox> _selectFolderCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public static RelayCommand<TextBox> SelectFolderCommand
        {
            get
            {
                return _selectFolderCommand
                    ?? (_selectFolderCommand = new RelayCommand<TextBox>(
                                          p =>
                                          {
                                              var d = new VistaFolderBrowserDialog();
                                              if (d.ShowDialog() != true) return;
                                              p.Text = d.SelectedPath;
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
