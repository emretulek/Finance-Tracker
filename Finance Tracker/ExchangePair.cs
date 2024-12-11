using Newtonsoft.Json;
using System.ComponentModel;

namespace Finance_Tracker
{
    public class ExchangePair : INotifyPropertyChanged
    {
        private float _ch;
        public float Ch
        {
            get { return _ch; }
            set
            {
                if (_ch != value)
                {
                    _ch = value;
                    OnPropertyChanged(nameof(Ch));
                    OnPropertyChanged(nameof(Status)); // Status'u güncellemek için
                }
            }
        }

        private float _chp;
        public float Chp
        {
            get { return _chp; }
            set
            {
                if (_chp != value)
                {
                    _chp = value;
                    OnPropertyChanged(nameof(Chp));
                    OnPropertyChanged(nameof(ChpString)); // ChpString'i güncellemek için
                }
            }
        }

        private float _lp;
        public float Lp
        {
            get { return _lp; }
            set
            {
                if (_lp != value)
                {
                    _lp = value;
                    OnPropertyChanged(nameof(Lp));
                }
            }
        }

        private float _ask;
        public float Ask
        {
            get { return _ask; }
            set
            {
                if (_ask != value)
                {
                    _ask = value;
                    OnPropertyChanged(nameof(Ask));
                }
            }
        }

        private float _bid;
        public float Bid
        {
            get { return _bid; }
            set
            {
                if (_bid != value)
                {
                    _bid = value;
                    OnPropertyChanged(nameof(Bid));
                }
            }
        }

        private float _openPrice;
        [JsonProperty("open_price")]
        public float OpenPrice
        {
            get { return _openPrice; }
            set
            {
                if (_openPrice != value)
                {
                    _openPrice = value;
                    OnPropertyChanged(nameof(OpenPrice));
                }
            }
        }

        private float _highPrice;
        [JsonProperty("high_price")]
        public float HighPrice
        {
            get { return _highPrice; }
            set
            {
                if (_highPrice != value)
                {
                    _highPrice = value;
                    OnPropertyChanged(nameof(HighPrice));
                }
            }
        }

        private float _lowPrice;
        [JsonProperty("low_price")]
        public float LowPrice
        {
            get { return _lowPrice; }
            set
            {
                if (_lowPrice != value)
                {
                    _lowPrice = value;
                    OnPropertyChanged(nameof(LowPrice));
                }
            }
        }

        private float _prevClosePrice;
        [JsonProperty("prev_close_price")]
        public float PrevClosePrice
        {
            get { return _prevClosePrice; }
            set
            {
                if (_prevClosePrice != value)
                {
                    _prevClosePrice = value;
                    OnPropertyChanged(nameof(PrevClosePrice));
                }
            }
        }

        private int _spread;
        public int Spread
        {
            get { return _spread; }
            set
            {
                if (_spread != value)
                {
                    _spread = value;
                    OnPropertyChanged(nameof(Spread));
                }
            }
        }

        private float _volume;
        public float Volume
        {
            get { return _volume; }
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        private string? _shortName;
        [JsonProperty("short_name")]
        public string? ShortName
        {
            get { return _shortName; }
            set
            {
                if (_shortName != value)
                {
                    _shortName = value;
                    OnPropertyChanged(nameof(ShortName));
                }
            }
        }

        private string? _exchange;
        public string? Exchange
        {
            get { return _exchange; }
            set
            {
                if (_exchange != value)
                {
                    _exchange = value;
                    OnPropertyChanged(nameof(Exchange));
                }
            }
        }

        private string? _description;
        public string? Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(FirstLetter)); // FirstLetter'ı güncellemek için
                }
            }
        }

        private string? _searchName;
        public string? SearchName
        {
            get { return _searchName; }
            set
            {
                if (_searchName != value)
                {
                    _searchName = value;
                    OnPropertyChanged(nameof(SearchName));
                }
            }
        }

        // Status ve diğer özellikler
        public int Status => Ch < 0 ? -1 : (Ch > 0 ? 1 : 0);

        public string ChpString => Chp.ToString() + "%";

        public string? FirstLetter => Description?[..1];

        // INotifyPropertyChanged implementasyonu
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
