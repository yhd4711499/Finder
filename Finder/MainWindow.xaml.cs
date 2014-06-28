using Finder.Algorithms;
using Finder.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Finder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ISearchAlgorithm _searchAlgorithm;
        private long _delay;
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
            this.Relay(Search, _delay);
        }

        private void Search()
        {
            GC.Collect();

            if (string.IsNullOrEmpty(Keyword.Text))
                return;
            if (!CheckFolderParams())
                return;

            UpdateStatus("正在搜索...");
            _searchAlgorithm.SearchAsync(Keyword.Text, new Dictionary<string, object> { { Trie.ConfigMatchWholeWord, false } })
                .ContinueWith(t => this.BeginInvoke(() =>
                {
                    Results.ItemsSource = t.Result;
                    UpdateStatus("搜索完成，找到{0}项。", t.Result.Length);
                }));

        }

        private void Recusive_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Build();
        }

        private bool CheckFolderParams()
        {
            if (string.IsNullOrEmpty(Folder.Text))
                return false;
            if (string.IsNullOrEmpty(Extensions.Text))
                return false;
            if (!Directory.Exists(Folder.Text))
            {
                UpdateStatus("指定目录不存在。");
                return false;
            }
            return true;
        }

        private void Build(Action callback = null)
        {
            if (!CheckFolderParams())
                return;

            var depth = 1;
            if (Recusive.IsChecked == true)
            {
                if (!int.TryParse(Depth.Text, out depth))
                    return;
            }

            _searchAlgorithm.Depth = depth;
            _searchAlgorithm.Patterns = Extensions.Text;
            _searchAlgorithm.FolderPath = Folder.Text;

            if (!_searchAlgorithm.SupportBuild)
            {
                if (callback != null)
                callback();
                return;
            }

            UpdateStatus("正在索引，请稍候...");

            _searchAlgorithm.BuildAsync()
                .ContinueWith(t => this.BeginInvoke(() => UpdateStatus("就绪，共索引{0}项", _searchAlgorithm.IndexedCount)))
                .ContinueWith(t =>
                {
                    if (callback != null)
                        callback();
                });
        }

        private void UpdateStatus(string msg, params object[] args)
        {
            Status.Text = String.Format(msg, args);
        }

        private void OnBuildConfigChanged(object sender, TextChangedEventArgs e)
        {
            Build();
        }

        private void SearchMethod_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var shortName = (string)((FrameworkElement)SearchMethod.SelectedItem).Tag;
            var fullName = "Finder.Algorithms." + shortName;
            var s = Activator.CreateInstance(Type.GetType(fullName)) as ISearchAlgorithm;
            _searchAlgorithm = s;

            var encodingStr = (string)((FrameworkElement)EncodingComboBox.SelectedItem).Tag;
            _searchAlgorithm.CodePage = int.Parse(encodingStr);

            DeferUtil.StopAll();
            Build(() => this.BeginInvoke(Search));
        }

        private void SearchDelay_Checked(object sender, RoutedEventArgs e)
        {
            var delayStr = (string)((FrameworkElement)sender).Tag;
            _delay = long.Parse(delayStr);
        }

        private void Encoding_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_searchAlgorithm == null)
                return;
            var encodingStr = (string)((FrameworkElement) EncodingComboBox.SelectedItem).Tag;
            _searchAlgorithm.CodePage = int.Parse(encodingStr);

            DeferUtil.StopAll();
            Build(() => this.BeginInvoke(Search));
        }
    }
}