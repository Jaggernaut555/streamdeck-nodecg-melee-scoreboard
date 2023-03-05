using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SocketIOClient;
using System;
using System.Threading.Tasks;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.cancelpredictionaction")]
    public class CancelPredictionAction: KeypadBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.HTTPAddress = "localhost:9090";
                instance.WebsocketAddress = "localhost:9091";
                return instance;
            }


            [JsonProperty(PropertyName = "httpAddress")]
            public string HTTPAddress { get; set; }

            [JsonProperty(PropertyName = "websocketAddress")]
            public string WebsocketAddress { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public CancelPredictionAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                SaveSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
            connection.SetTitleAsync("Cancel\nPrediction");
        }

        public override void Dispose() { }

        public override void KeyPressed(KeyPayload payload) {
            var client = new RestClient($"http://{this.settings.HTTPAddress}");
            var request = new RestRequest("/api/v1/prediction");
            request.AddParameter("operation", "cancel");
            client.Post(request);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion
    }
}