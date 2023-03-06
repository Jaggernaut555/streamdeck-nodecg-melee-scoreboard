using Newtonsoft.Json;

namespace StreamDeck_Scoreboard
{
    public class BaseSettings
    {
        public BaseSettings()
        {
            HTTPAddress = "localhost:9090";
            WebsocketAddress = "localhost:9091";
        }

        [JsonProperty(PropertyName = "httpAddress")]
        public string HTTPAddress { get; set; }

        [JsonProperty(PropertyName = "websocketAddress")]
        public string WebsocketAddress { get; set; }
    }
}