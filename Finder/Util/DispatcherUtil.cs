using System;
using System.Windows;

namespace Finder.Util
{
    static class DispatcherUtil
    {
        public static void BeginInvoke(this FrameworkElement fe, Action ac)
        {
            fe.Dispatcher.BeginInvoke(ac);
        }
    }
}
