using Newtonsoft.Json;

namespace Finance_Tracker
{
    public class SearchResultItem
    {
        [JsonProperty("full_name")]
        public string? FullName { get; set; }

        public string? Symbol {  get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Ticker { get; set; }
        public string? Exchange { get; set; }
    }
}
