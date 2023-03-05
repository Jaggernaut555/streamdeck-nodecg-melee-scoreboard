using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
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
    [PluginActionId("ca.jaggernaut.scoreboard.hidescoreboardaction")]
    public class HideScoreboardAction: KeypadBase
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

        private class HideScoreboradMessage
        {
            [JsonProperty(PropertyName = "HideScoreboard")]
            public bool HideScoreboard { get; set; }
        }

        private SocketIO WsClient { get; set; }

        private string CurrentWebsocketUrl { get; set; }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public HideScoreboardAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            var request = new RestRequest("/api/v1/scoreboard");
            client.Post(request);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();

            InitializeWebsocket();
            // TODO:
            // if team changed but websocket already initialized
            // then requeset the new team info
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

            this.WsClient.On("HideScoreboardUpdate", async response =>
            {
                var msg = response.GetValue<HideScoreboradMessage>();
                if (msg != null)
                {
                    if (msg.HideScoreboard)
                    {
                        await Connection.SetTitleAsync("OFF");
                    }
                    else
                    {
                        await Connection.SetTitleAsync("ON");
                    }
                }
            });

            this.WsClient.OnConnected += async (sender, e) =>
            {
                await this.WsClient.EmitAsync("join", "HideScoreboard");
                await this.WsClient.EmitAsync("HideScoreboard");
            };

            await this.WsClient.ConnectAsync();
            this.CurrentWebsocketUrl = this.settings.WebsocketAddress;
        }

        #endregion
    }
}