using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Finder.Algorithms;
using Finder.Annotations;
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
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ISearchAlgorithm _searchAlgorithm;
        private long _delay;
        private CancellationTokenSource _cuurentTaskToken = new CancellationTokenSource();


        /// <summary>
        /// IsBusy属性的名称
        /// </summary>
        public const string IsBusyPropertyName = "IsBusy";
        private bool _isBusy;
        /// <summary>
        /// 作者很懒，什么描述也没有
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged(IsBusyPropertyName);
                }
            }
        }

        public MainWindow()
        {
            Keyboard.AddKeyDownHandler(this, OnKeydown);
            InitializeComponent();
            DataContext = this;
        }

        private void OnKeydown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Escape)
            {
                _cuurentTaskToken.Cancel();
            }
        }

        private void Folder_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            BuildAndSearch();
        }

        private void Keyword_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            this.Relay(Search, _delay);
        }

        private void Search()
        {
            if (string.IsNullOrEmpty(Keyword.Text))
                return;
            if (!CheckFolderParams())
                return;

            UpdateStatus("正在搜索...");

            var keyword = Keyword.Text;
            var matchAll = MatchAll.IsChecked == true;
            var config = new Dictionary<Configs, object>
            {
                {Configs.MatchWholeWord, false},
                {Configs.MatchAll, matchAll}
            };

            Action<List<SearchResult>> update = (result) =>
            {
                Results.ItemsSource = result.Select(_=> _searchAlgorithm.FileList[_.FileIndex].Substring(Folder.Text.Length));
                UpdateStatus("搜索完成，找到{0}项。", result.Count);
            };

            if (_delay == -1)
            {
                var result = _searchAlgorithm.Search(keyword, config, _cuurentTaskToken.Token);
                update(result);
            }
            else
            {
                _cuurentTaskToken.Cancel();
                _cuurentTaskToken = new CancellationTokenSource();
                IsBusy = true;
                _searchAlgorithm.SearchAsync(keyword, config, _cuurentTaskToken.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled)
                        {
                            if (_cuurentTaskToken.IsCancellationRequested)
                            {
                                this.BeginInvoke(() => UpdateStatus("已取消搜索。"));
                                IsBusy = false;
                            }
                        }
                        else if (t.IsFaulted)
                        {
                            this.BeginInvoke(() => UpdateStatus("发生了错误：{0}", t.Exception.InnerExceptions[0].Message));
                            IsBusy = false;
                        }
                        else
                        {
                            this.BeginInvoke(() => update(t.Result));
                            IsBusy = false;
                        }
                        
                    });
            }

        }

        private void Recusive_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            BuildAndSearch();
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

        private void BuildAndSearch()
        {
            if (!CheckFolderParams())
                return;

            DeferUtil.StopAll();

            var depth = 1;
            if (Recusive.IsChecked == true)
            {
                if (!int.TryParse(Depth.Text, out depth))
                    return;
            }

            _searchAlgorithm.Depth = depth;
            _searchAlgorithm.Patterns = Extensions.Text;
            _searchAlgorithm.FolderPath = Folder.Text;

            UpdateStatus("正在索引，请稍候...");
            IsBusy = true;
            _cuurentTaskToken.Cancel();
            _cuurentTaskToken = new CancellationTokenSource();
            _searchAlgorithm.BuildAsync(_cuurentTaskToken.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        this.BeginInvoke(() => UpdateStatus("已取消索引。"));
                    }
                    else
                    {
                        this.BeginInvoke(() => UpdateStatus("就绪，共索引{0}项", _searchAlgorithm.IndexedCount));
                    }
                    IsBusy = false;

                })
                .ContinueWith(t =>
                {
                    if(t.IsCanceled)
                        return;
                    
                    this.BeginInvoke(Search);
                });
        }

        private void UpdateStatus(string msg, params object[] args)
        {
            Status.Text = String.Format(msg, args);
        }

        private void OnBuildConfigChanged(object sender, TextChangedEventArgs e)
        {
            BuildAndSearch();
        }

        private void SearchMethod_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            _cuurentTaskToken.Cancel();

            var shortName = (string)((FrameworkElement)SearchMethod.SelectedItem).Tag;
            var fullName = "Finder.Algorithms." + shortName;
            var s = Activator.CreateInstance(Type.GetType(fullName)) as ISearchAlgorithm;
            _searchAlgorithm = s;

            var encodingStr = (string)((FrameworkElement)EncodingComboBox.SelectedItem).Tag;
            _searchAlgorithm.CodePage = int.Parse(encodingStr);

            BuildAndSearch();
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

            BuildAndSearch();
        }

        private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            BuildAndSearch();
        }

        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            _cuurentTaskToken.Cancel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResultItem_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommonCommands.ShellExecuteCommand.Execute(Folder.Text + ((FrameworkElement)sender).DataContext);
        }

        private void MenuItemBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var cmd = menuItem.Command;
            if(cmd == null) return;
            var absPath = (string)menuItem.DataContext;
            cmd.Execute(Folder.Text + absPath);
        }
    }
}