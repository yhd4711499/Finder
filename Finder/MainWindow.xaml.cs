using Finder.Algorithms;
using Finder.Annotations;
using Finder.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Finder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Constants
        private const long _configSettingDelay = 300;
        #endregion

        #region Fields
        private ISearchAlgorithm _searchAlgorithm;
        private long _delay;
        private CancellationTokenSource _cuurentTaskToken = new CancellationTokenSource();
        private TextChangedEventHandler _keywordChangedEvent;
        private TextBox _keywordTextBox;
        #endregion

        #region Properties
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
        #endregion

        public MainWindow()
        {
            Keyboard.AddKeyDownHandler(this, OnKeydown);
            InitializeComponent();
            _keywordChangedEvent = new TextChangedEventHandler(Keyword_OnTextChanged);
            Keyword.AddHandler(TextBox.TextChangedEvent, _keywordChangedEvent);

            _keywordTextBox = (TextBox)typeof(ComboBox).InvokeMember(
                    "EditableTextBoxSite",
                    BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, Keyword, null);

            DataContext = this;
            LoadSettings();
        }

        #region Methods

        private void Search()
        {
            if (string.IsNullOrEmpty(Keyword.Text))
                return;
            if (!CheckFolderParams())
                return;

            SaveSettings();
            LoadSettings();

            UpdateStatus("正在搜索...");
            
            var keyword = Keyword.Text;
            var matchAll = MatchAll.IsChecked == true;

            var config = new Dictionary<Configs, object>
            {
                {Configs.MatchWholeWord, false},
                {Configs.MatchAll, matchAll},
            };
            var st = Stopwatch.StartNew();
            Action<List<SearchResult>> update = (result) =>
            {
                st.Stop();
                Results.ItemsSource = result.Where(_=>_!=null)
                    .Select(_=> _searchAlgorithm.FileList[_.FileIndex].Substring(Folder.Text.Length))
                    .OrderBy(_=>_);
                UpdateStatus("搜索完成，找到{0}项，用时{1} ms。", result.Count, st.ElapsedMilliseconds);
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
                    IsBusy = false;
                    if (t.IsCanceled)
                    {
                        this.BeginInvoke(() => UpdateStatus("已取消索引。"));
                    }
                    else if (t.IsFaulted)
                    {
                        this.BeginInvoke(() => UpdateStatus("索引发生错误：{0}。", t.Exception));
                    }
                    else
                    {
                        this.BeginInvoke(() =>
                        {
                            UpdateStatus("就绪，共索引{0}项", _searchAlgorithm.IndexedCount);
                            Search();
                        });
                    }
                });
        }

        private void UpdateStatus(string msg, params object[] args)
        {
            Status.Text = String.Format(msg, args);
        }

        private void SaveSettings()
        {
            MySettings.Instance.Add(Folder.Text, Keyword.Text, Extensions.Text);
        }

        private void LoadSettings()
        {
            Folder.ItemsSource = MySettings.Instance.HistoryFolders;
            Extensions.ItemsSource = MySettings.Instance.HistoryExtensions;
            Keyword.ItemsSource = MySettings.Instance.HistoryKeywords;
        }

        #endregion

        #region Commands
        private void ResultItem_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommonCommands.ShellExecuteCommand.Execute(Folder.Text + ((FrameworkElement)sender).DataContext);
        }

        private void MenuItemBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var cmd = menuItem.Command;
            if (cmd == null) return;
            var absPath = (string)menuItem.DataContext;
            cmd.Execute(Folder.Text + absPath);
        }

        private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            BuildAndSearch();
        }

        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            _cuurentTaskToken.Cancel();
        }

        private void OnKeydown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Escape)
            {
                _cuurentTaskToken.Cancel();
            }
        }
        #endregion

        #region Search Setting Changings

        private void Keyword_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Unicode.IsChecked == true)
            {
                Keyword.RemoveHandler(TextBox.TextChangedEvent, _keywordChangedEvent);

                Keyword.Text = Unicode.IsChecked == true ? Keyword.Text.ToUnicode() : Keyword.Text.ToGB2312();

                _keywordTextBox.SelectionStart = Keyword.Text.Length;

                Keyword.AddHandler(TextBox.TextChangedEvent, _keywordChangedEvent);
            }
            this.Relay(Search, _delay);
        }

        private void SearchDelay_Checked(object sender, RoutedEventArgs e)
        {
            var delayStr = (string)((FrameworkElement)sender).Tag;
            _delay = long.Parse(delayStr);
        }

        private void Unicode_CheckChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Keyword.Text = Unicode.IsChecked == true ? Keyword.Text.ToUnicode() : Keyword.Text.ToGB2312();
                Search();
            }
            catch (Exception ex)
            {
                UpdateStatus("字符串转换错误：{0}", ex.Message);
            }
        }
        #endregion

        #region Build Setting Changings

        private void OnBuildConfigChanged(object sender, TextChangedEventArgs e)
        {
            this.Relay(BuildAndSearch, _configSettingDelay);
        }

        private void Recusive_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            this.Relay(BuildAndSearch, _configSettingDelay);
        }

        private void SearchMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void Encoding_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_searchAlgorithm == null)
                return;
            var encodingStr = (string)((FrameworkElement)EncodingComboBox.SelectedItem).Tag;
            _searchAlgorithm.CodePage = int.Parse(encodingStr);

            if (_searchAlgorithm.RequestBuild)
                BuildAndSearch();
            else
                Search();
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}