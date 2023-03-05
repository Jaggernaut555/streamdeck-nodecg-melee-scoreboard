using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.predictionaction")]
    public class PredictionStatusAction: KeypadBase
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
        public PredictionStatusAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

        public override void KeyPressed(KeyPayload payload) { }

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
                    await Connection.SetTitleAsync($"{msg.PredictionStatus}");
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