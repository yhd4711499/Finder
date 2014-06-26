using System;
using System.Windows;
using System.Windows.Controls;

namespace Finder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Trie _trie = new Trie();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Folder_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Build();
        }

        private void Keyword_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            if(string.IsNullOrEmpty(Keyword.Text))
                return;
            var results = _trie.Search(Keyword.Text, MatchWholeWord.IsChecked == true);
            Results.ItemsSource = results;
            UpdateStatus("搜索完成，共有{0}项。", results.Length);
        }

        private void Recusive_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Build();
        }

        private void Build()
        {
            if(string.IsNullOrEmpty(Folder.Text))
                return;
            if(string.IsNullOrEmpty(Extensions.Text))
                return;

            var depth = 1;
            if (Recusive.IsChecked == true)
            {
                if (!int.TryParse(Depth.Text, out depth))
                    return;
            }
            UpdateStatus("正在索引，请稍候...");
            _trie.BuildAsync(Folder.Text, depth, Extensions.Text)
                .ContinueWith((t) =>
                {
                    Dispatcher.BeginInvoke(new Action(() => UpdateStatus("就绪，共索引{0}项", _trie.FileList.Count)));
                });
        }

        private void UpdateStatus(string msg, params object[] args)
        {
            Status.Text = String.Format(msg, args);
        }

        private void MatchWholeWord_OnCheckChanged(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void OnBuildConfigChanged(object sender, TextChangedEventArgs e)
        {
            Build();
        }
    }
}
