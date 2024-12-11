using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Widgets.Common;

namespace Finance_Tracker
{
    public partial class MainWindow : Window, IWidgetWindow
    {
        public readonly static string WidgetName = "Finance Tracker";
        public readonly static string SettingsFile = "settings.financetracker.json";

        private readonly Config config = new(SettingsFile);
        public FinanceViewModel FinanceViewModel;
        private FinanceViewModel.SettingsStruct Settings = FinanceViewModel.Default;

        public ObservableCollection<ExchangePair> ExchangeGridRows { get; set; }

        private readonly Schedule schedule = new();
        private readonly List<string> scheduleIds = [];
        private readonly DispatcherTimer _debounceTimer;

        public MainWindow()
        {
            InitializeComponent();

            LoadSettings();
            FinanceViewModel = new()
            {
                Settings = Settings
            };
  
            ExchangeGridRows = FinanceViewModel.ExchangeGridRows;

            Logger.Info($"{WidgetName} is started");
            foreach (ExchangePair pair in ExchangeGridRows)
            {
                Logger.Info("Symbol: " + pair.SearchName);
            }

            DataContext = FinanceViewModel;

            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _debounceTimer.Tick += DebounceTimer_Tick;
            Loaded += MainWindow_Loaded;
        }

        // Window to WidgetWindow
        public WidgetWindow WidgetWindow()
        {
            return new WidgetWindow(this, WidgetDefaultStruct());
        }

        //WidgetWindow default settings
        public static WidgetDefaultStruct WidgetDefaultStruct()
        {
            return new()
            {
                Height = 600,
                Width = 400,
                SizeToContent = SizeToContent.Height
            };
        }

        //Load internal settings
        public void LoadSettings()
        {
            try
            {
                string searchRecords = PropertyParser.ToString(config.GetValue("ExchangeGridRows"));
                Settings.ExchangeGridRows = JsonConvert.DeserializeObject<List<ExchangePair>>(searchRecords) ?? [];

                Settings.UpdateTime = PropertyParser.ToInt(config.GetValue("update_time"));
                Settings.FontSize = PropertyParser.ToFloat(config.GetValue("fontsize"));
                Settings.FontColor = PropertyParser.ToColorBrush(config.GetValue("fontcolor"), Settings.FontColor.ToString());
                Settings.HeaderFontColor = PropertyParser.ToColorBrush(config.GetValue("header_fontcolor"), Settings.HeaderFontColor.ToString());
                Settings.Background = PropertyParser.ToColorBrush(config.GetValue("background"), Settings.Background.ToString());
                Settings.DividerColor = PropertyParser.ToColorBrush(config.GetValue("divider_color"), Settings.DividerColor.ToString());
            }
            catch (Exception ex)
            {
                config.Add("update_time", Settings.UpdateTime);
                config.Add("fontsize", Settings.FontSize);
                config.Add("fontcolor", Settings.FontColor);
                config.Add("header_fontcolor", Settings.HeaderFontColor);
                config.Add("background", Settings.Background);
                config.Add("divider_color", Settings.DividerColor);
                config.Add("ExchangeGridRows", Settings.ExchangeGridRows);
                config.Save();
                Logger.Info(ex.Message);
            }
        }


        // Add all Symbol from settings
        private async void AddSymbols(List<SearchResultItem> searchRecordItems)
        {
            var exchangePairs =  await FinanceViewModel.SymbolsRequest(searchRecordItems);

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (ExchangePair exchangePair in exchangePairs)
                {
                    var existingItem = ExchangeGridRows.FirstOrDefault(x => x.SearchName == exchangePair.SearchName);

                    if (existingItem != null)
                    {
                        existingItem.Ch = exchangePair.Ch;
                        existingItem.Chp = exchangePair.Chp;
                        existingItem.Lp = exchangePair.Lp;
                        existingItem.Ask = exchangePair.Ask;
                        existingItem.Bid = exchangePair.Bid;
                        existingItem.OpenPrice = exchangePair.OpenPrice;
                        existingItem.HighPrice = exchangePair.HighPrice;
                        existingItem.LowPrice = exchangePair.LowPrice;
                        existingItem.PrevClosePrice = exchangePair.PrevClosePrice;
                        existingItem.Spread = exchangePair.Spread;
                        existingItem.Volume = exchangePair.Volume;
                    }
                }
            });
        }

        // Add new Symbol to the DataGrid row
        private async void AddSymbol(SearchResultItem selectedSearchItem)
        {
            ExchangePair exchange = await FinanceViewModel.SymbolRequest(selectedSearchItem);

            ExchangeGridRows.Add(exchange);
            config.Add("ExchangeGridRows", ExchangeGridRows);
            config.Save();
        }

        // Datagrid delete row object
        private void RemoveSymbol(ExchangePair selectedItem)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ExchangeGridRows.Remove(selectedItem);
            });

            config.Add("ExchangeGridRows", ExchangeGridRows);
            config.Save();
        }


        #region Events Handled
        // window loaded
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<SearchResultItem> searchResultItems = [];

            foreach (var exchangePair in ExchangeGridRows.ToList())
            {
                searchResultItems.Add(new SearchResultItem { FullName = exchangePair.SearchName });
            }
 
            AddSymbols(searchResultItems);

            scheduleIds.Add(schedule.Secondly(() => {

                List<SearchResultItem> searchResultItems = [];

                foreach (var exchangePair in ExchangeGridRows.ToList())
                {
                    searchResultItems.Add(new SearchResultItem { FullName = exchangePair.SearchName });
                }

                AddSymbols(searchResultItems);

            }, Settings.UpdateTime));
        }

        //When window closing
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            foreach (var timerId in scheduleIds)
            {
                schedule.Stop(timerId);
            }
            scheduleIds.Clear();

            Logger.Info($"{WidgetName} is closed");
        }

        // DataGrid delete selected row
        public void DeleteDataGridRow(object sender, RoutedEventArgs e)
        {
            Label? clickedButton = sender as Label;
            DependencyObject? parent = clickedButton?.Parent;

            while (parent != null && parent is not DataGridRow)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is DataGridRow dataGridRow)
            {
                if (dataGridRow.Item is ExchangePair deletedRow)
                {
                    RemoveSymbol(deletedRow);
                }
            }
        }

        //lazy dispatcher
        private async void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            _debounceTimer.Stop();
            if (SearchBox.Text != "")
            {
                SearchPanel.Visibility = Visibility.Visible;
                await FinanceViewModel.SearchRequest(SearchBox.Text);
            }
        }

        // Search Crypto Symbol
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _debounceTimer.Stop();

            if (SearchBox.Text == "")
            {
                PlaceHolder.Opacity = 0.5;
                SearchPanel.Visibility = Visibility.Collapsed;
                return;
            }

            PlaceHolder.Opacity = 0;
            SearchPanel.Visibility = Visibility.Visible;

            _debounceTimer.Start();
        }

        // switch focus from searchbox to searchresults
        private void SearchBox_DownArrow(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && SearchResults.Items.Count > 0)
            {
                SearchResults.SelectedIndex = 0;
                SearchResults.Focus();
            }
        }

        // Select searchresult with mouse
        private void SearchResults_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SearchResults.SelectedItem != null)
            {
                if (SearchResults.SelectedItem is SearchResultItem selectedPair)
                {
                    AddSymbol(selectedPair);
                    SearchPanel.Visibility = Visibility.Collapsed;
                    SearchBox.Focus();
                    SearchBox.Text = "";
                }
            }
        }

        // Select searchresult with Enter
        private void SearchResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SearchResults.SelectedItem != null)
            {
                if (SearchResults.SelectedItem is SearchResultItem selectedPair)
                {
                    AddSymbol(selectedPair);
                    SearchPanel.Visibility = Visibility.Collapsed;
                    SearchBox.Focus();
                    SearchBox.Text = "";
                }
            }
        }

        // DataGridRow Hover start event
        private void DataGridRow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.Background = Settings.DividerColor;
                Label? targetButton = FindChildButtonWithTag(row, "DeleteButton");

                if (row.DataContext is ExchangePair && targetButton != null)
                {
                    targetButton.Visibility = Visibility.Visible;
                }
            }
        }

        // DataGridRow Hover end event
        private void DataGridRow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.Background = Brushes.Transparent;
                Label? targetButton = FindChildButtonWithTag(row, "DeleteButton");

                if (targetButton != null)
                {
                    targetButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        // DataGridRow restore sorting to original
        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortMemberPath == "ID")
            {
                e.Handled = true;
                var view = CollectionViewSource.GetDefaultView(ExchangeGridRows);
                view.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
                view.SortDescriptions.Clear();
                view.Refresh();
            }
            else
            {
                e.Handled = false;
            }
        }

        // DataGrid save when order changes with drag drop
        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.ItemsSource is ObservableCollection<ExchangePair>)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    config.Add("ExchangeGridRows", ExchangeGridRows);
                    config.Save();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // DataGrid DeleteButton finder
        private static Label? FindChildButtonWithTag(DependencyObject parent, string tag)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Label button && button.Tag?.ToString() == tag)
                {
                    return button;
                }

                // recursive
                var result = FindChildButtonWithTag(child, tag);
                if (result != null)
                    return result;
            }
            return null;
        }
        #endregion Events Handled End
    }
}