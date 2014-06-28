using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
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
            GC.Collect();

            if (string.IsNullOrEmpty(Keyword.Text))
                return;
            if (!CheckFolderParams())
                return;

            UpdateStatus("正在搜索...");

            var keyword = Keyword.Text;
            Func<string[]> searchAction = () => _searchAlgorithm.Search(keyword, new Dictionary<string, object> { { Trie.ConfigMatchWholeWord, false } }, _cuurentTaskToken.Token);
            Action<string[]> update = (result) =>
            {
                Results.ItemsSource = result;
                UpdateStatus("搜索完成，找到{0}项。", result.Length);
            };

            if (_delay == -1)
            {
                var result = searchAction();
                update(result);
            }
            else
            {
                _cuurentTaskToken.Cancel();
                _cuurentTaskToken = new CancellationTokenSource();
                IsBusy = true;
                Task.Factory.StartNew(searchAction, _cuurentTaskToken.Token)
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

            DeferUtil.StopAll();
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResultItem_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommonCommands.ShellExecuteCommand.Execute(((FrameworkElement)sender).DataContext.ToString());
        }
    }
}