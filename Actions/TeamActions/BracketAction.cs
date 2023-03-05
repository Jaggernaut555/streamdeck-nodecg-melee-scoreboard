using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.bracketaction")]
    public class BracketAction: KeypadBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.HTTPAddress = "localhost:9090";
                instance.WebsocketAddress = "localhost:9091";
                instance.TeamIndex = 0;
                return instance;
            }


            [JsonProperty(PropertyName = "httpAddress")]
            public string HTTPAddress { get; set; }

            [JsonProperty(PropertyName = "websocketAddress")]
            public string WebsocketAddress { get; set; }

            [JsonProperty(PropertyName = "teamIndex")]
            public int TeamIndex { get; set; }
        }

        private class BracketMessage
        {
            [JsonProperty(PropertyName = "TeamIndex")]
            public int TeamIndex { get; set; }
            [JsonProperty(PropertyName = "Bracket")]
            public string Bracket { get; set; }
        }

        private SocketIO WsClient { get; set; }

        private string CurrentWebsocketUrl { get; set; }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public BracketAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            var request = new RestRequest("/api/v1/bracket");
            request.AddParameter("bracket", "toggle");
            request.AddParameter("team", this.settings.TeamIndex);
            client.Post(request);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            var oldTeamIndex = this.settings.TeamIndex;
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();

            InitializeWebsocket();
            if (this.settings.TeamIndex != oldTeamIndex)
            {
                this.UpdateInfo();
            }
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

            this.WsClient.On("BracketUpdate", async response =>
            {
                var msg = response.GetValue<BracketMessage>();
                if (msg != null)
                {
                    if (msg.TeamIndex == this.settings.TeamIndex)
                    {
                        await Connection.SetTitleAsync($"{msg.Bracket}");
                    }
                }
            });

            this.WsClient.OnConnected += async (sender, e) =>
            {
                await this.WsClient.EmitAsync("join", "Bracket");
                this.UpdateInfo();
            };

            await this.WsClient.ConnectAsync();
            this.CurrentWebsocketUrl = this.settings.WebsocketAddress;
        }

        private void UpdateInfo()
        {
            if (this.WsClient != null)
            {
                this.WsClient.EmitAsync("Bracket", this.settings.TeamIndex);
            }

            if (this.settings.TeamIndex == 0)
            {
                using (Image image = Image.FromFile("images/player1PluginIcon.png"))
                {
                    Connection.SetImageAsync(image);
                }
            }
            else
            {
                using (Image image = Image.FromFile("images/player2PluginIcon.png"))
                {
                    Connection.SetImageAsync(image);
                }
            }
        }

        #endregion
    }
}