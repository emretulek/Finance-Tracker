using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.ComponentModel;
using System;

namespace Finance_Tracker
{
    public class FinanceViewModel:INotifyPropertyChanged
    {
        private readonly static string _baseUrl = "https://tvc4.investing.com";

        public struct SettingsStruct
        {
            public List<ExchangePair> ExchangeGridRows { get; set; }
            public int UpdateTime { get; set; }
            public float FontSize { get; set; }
            public SolidColorBrush FontColor { get; set; }
            public SolidColorBrush HeaderFontColor { get; set; }
            public SolidColorBrush Background { get; set; }
            public SolidColorBrush DividerColor { get; set; }
            public readonly float IconSize => FontSize * 2;
        }

        public static SettingsStruct Default => new()
        {
            ExchangeGridRows = [],
            UpdateTime = 300,
            FontSize = 14,
            FontColor = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
            HeaderFontColor = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Colors.Black),
            DividerColor = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
        };

        private ObservableCollection<SearchResultItem>? _searchResultItems = [];
        public ObservableCollection<SearchResultItem>? SearchResultItems
        {
            get { return _searchResultItems; }
            set { 
                _searchResultItems = value;
                OnPropertyChanged(nameof(SearchResultItems));
            }
        }

        private SettingsStruct _settings;

        public required SettingsStruct Settings
        {
            get { return _settings; }
            set
            {
                if (!Equals(_settings, value))
                {
                    _settings = value;
                    foreach (var pairs in _settings.ExchangeGridRows)
                    {
                        ExchangeGridRows.Add(pairs);
                    }
                    OnPropertyChanged(nameof(Settings));
                }
            }
        }

        private ObservableCollection<ExchangePair> _exchangeGridRows = [];
        public ObservableCollection<ExchangePair> ExchangeGridRows
        {
            get { return _exchangeGridRows; }
            set
            {
                _exchangeGridRows = value;
                OnPropertyChanged(nameof(ExchangeGridRows));
            }
        }

        // Search financial instrument
        public async Task SearchRequest(string _query)
        {
            try
            {
                var uiCulture = CultureInfo.CurrentUICulture;
                var headers = new Dictionary<string, string>
                    {
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36" },
                        { "Accept", "*/*" },
                        { "Referer", "https://tvc-invdn-cf-com.investing.com/" },
                        { "Origin", "https://tvc-invdn-cf-com.investing.com" },
                        { "Accept-Language", $"{uiCulture},en;q=0.9" },
                        { "Accept-Encoding", "gzip, deflate, br" },
                        { "Upgrade-Insecure-Requests", "1" },
                        { "Sec-Fetch-Dest", "document" },
                        { "Sec-Fetch-Mode", "navigate" },
                        { "Sec-Fetch-Site", "same-site" },
                        { "Sec-ch-ua-platform", "Windows" },
                        { "Sec-ch-ua", @"Google Chrome"";v=""131"", ""Chromium"";v=""131"", ""Not_A Brand"";v=""24" },
                        { "Pragma", "no-cache" },
                        { "Cache-Control", "no-cache" }
                    };

                string token = Guid.NewGuid().ToString("N");
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                string url = $"{_baseUrl}/{token}/{timestamp}/1/1/8/search?limit=10&query={_query}&type=&exchange=";

                TLSSession Client = new(headers);
                var response = await Client.Get(url);

                if (response.Body != null)
                {
                    SearchResultItems = JsonConvert.DeserializeObject<ObservableCollection<SearchResultItem>>(response.Body);
                }
                else
                {
                    Debug.WriteLine($"Empty response");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.InnerException);
            }
        }

        // Symbol request single
        public static async Task<ExchangePair> SymbolRequest(SearchResultItem searchResaultItem)
        {
            List<SearchResultItem>  searchResaultItems = [searchResaultItem];
            var exchangeGridRows = await SymbolsRequest(searchResaultItems);

            return exchangeGridRows.First();
        }

        // Symbol request mutiple
        public static async Task<List<ExchangePair>> SymbolsRequest(List<SearchResultItem> searchResaultItems)
        {
            List<ExchangePair> exchangeGridRows = [];

            if (searchResaultItems.Count == 0)
            {
                return exchangeGridRows;
            }

            try
            {
                string parameters = "";

                foreach (var searchResponse in searchResaultItems)
                {
                    parameters += searchResponse.FullName + ",";
                }

                parameters = parameters.TrimEnd(',');

                var uiCulture = CultureInfo.CurrentUICulture;
                var headers = new Dictionary<string, string>
                    {
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36" },
                        { "Accept", "*/*" },
                        { "Referer", "https://tvc-invdn-cf-com.investing.com/" },
                        { "Origin", "https://tvc-invdn-cf-com.investing.com" },
                        { "Accept-Language", $"{uiCulture},en;q=0.9" },
                        { "Accept-Encoding", "gzip, deflate, br" },
                        { "Upgrade-Insecure-Requests", "1" },
                        { "Sec-Fetch-Dest", "document" },
                        { "Sec-Fetch-Mode", "navigate" },
                        { "Sec-Fetch-Site", "same-site" },
                        { "Sec-ch-ua-platform", "Windows" },
                        { "Sec-ch-ua", @"Google Chrome"";v=""131"", ""Chromium"";v=""131"", ""Not_A Brand"";v=""24" },
                        { "Pragma", "no-cache" },
                        { "Cache-Control", "no-cache" }
                    };

                string token = Guid.NewGuid().ToString("N");
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                string url = $"{_baseUrl}/{token}/{timestamp}/1/1/8/quotes?symbols={parameters}";
                Debug.WriteLine(url);

                TLSSession Client = new (headers);
                var response = await Client.Get(url);

                Debug.WriteLine(response.Body);

                if (response.Body == null)
                {
                    throw new Exception("response.Body is Empty");
                }

                var responseObj = JsonConvert.DeserializeObject<JObject>(response.Body);

                if (responseObj?["d"] is not JArray d)
                {
                    throw new Exception("Item d is not JArray");
                }

                foreach (var item in d)
                {
                    ExchangePair? exchangePair = item["v"]?.ToObject<ExchangePair>();

                    if (exchangePair != null)
                    {
                        exchangePair.SearchName = item["n"]?.ToString();
                        exchangeGridRows.Add(exchangePair);
                    }
                    else
                    {
                        throw new Exception("Empty v item");
                    }
                }               
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return exchangeGridRows;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
