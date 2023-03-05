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
    [PluginActionId("ca.jaggernaut.scoreboard.changescoreaction")]
    public class ChangeScoreAction: KeypadBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.HTTPAddress = "localhost:9090";
                instance.TeamIndex = 0;
                instance.Operation = "add";
                return instance;
            }


            [JsonProperty(PropertyName = "httpAddress")]
            public string HTTPAddress { get; set; }

            [JsonProperty(PropertyName = "teamIndex")]
            public int TeamIndex { get; set; }

            [JsonProperty(PropertyName = "operation")]
            public string Operation { get; set; }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public ChangeScoreAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

            this.UpdateInfo();
        }

        public override void Dispose() { }

        public override void KeyPressed(KeyPayload payload) {
            var client = new RestClient($"http://{this.settings.HTTPAddress}");
            var request = new RestRequest("/api/v1/score");
            request.AddParameter("operation", this.settings.Operation);
            request.AddParameter("team", this.settings.TeamIndex);
            client.Post(request);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
            this.UpdateInfo();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private Task UpdateInfo()
        {
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

            var change = this.settings.Operation == "add" ? "+1" : "-1";
            return Connection.SetTitleAsync($"P{this.settings.TeamIndex+1} {change}");
        }

        #endregion
    }
}