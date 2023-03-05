using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SocketIOClient;
using System;
using System.Threading.Tasks;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.progresspredictionaction")]
    public class ProgressPredictionAction: KeypadBase
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

        private class PredictionMessage
        {
            [JsonProperty(PropertyName = "PredictionStatus")]
            public string PredictionStatus { get; set; }
        }

        private SocketIO WsClient { get; set; }

        private string CurrentWebsocketUrl { get; set; }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public ProgressPredictionAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            InitializeWebsocket();
        }

        public override async void Dispose() {
            if (this.WsClient != null)
            {
                await this.WsClient.DisconnectAsync();
                this.WsClient.Dispose();
            }
        }

        public override void KeyPressed(KeyPayload payload) {
            var client = new RestClient($"http://{this.settings.HTTPAddress}");
            var request = new RestRequest("/api/v1/prediction");
            request.AddParameter("operation", "progress");
            client.Post(request);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
            InitializeWebsocket();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private async void InitializeWebsocket()
        {
            if (this.settings.WebsocketAddress == null || (this.WsClient != null && this.CurrentWebsocketUrl == this.settings.WebsocketAddress))
            {
                return;
            }

            if (this.WsClient != null)
            {
                await this.WsClient.DisconnectAsync();
            }

            var url = new Uri($"ws://{this.settings.WebsocketAddress}");

            this.WsClient = new SocketIO(url);

            this.WsClient.On("PredictionUpdate", async response =>
            {
                var msg = response.GetValue<PredictionMessage>();
                if (msg != null)
                {
                    switch(msg.PredictionStatus)
                    {
                        case "Started":
                            await Connection.SetTitleAsync($"Lock\nPrediction");
                            break;
                        case "Stopped":
                            await Connection.SetTitleAsync($"Start\nPrediction");
                            break;
                        case "Locked":
                            await Connection.SetTitleAsync($"End\nPrediction");
                            break;
                        default:
                            await Connection.SetTitleAsync($"Progress\nPrediction");
                            break;
                    }
                }
            });

            this.WsClient.OnConnected += async (sender, e) =>
            {
                await this.WsClient.EmitAsync("join", "Prediction");
                await this.WsClient.EmitAsync("Prediction");
            };

            await this.WsClient.ConnectAsync();
            this.CurrentWebsocketUrl = this.settings.WebsocketAddress;
        }

        #endregion
    }
}