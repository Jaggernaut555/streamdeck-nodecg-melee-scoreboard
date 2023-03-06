using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SocketIOClient;
using StreamDeck_Scoreboard.Actions.BaseActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace StreamDeck_Scoreboard
{
    class ChangeScoreSettings : TeamActionSettings
    {
        public ChangeScoreSettings() : base()
        {
            Operation = "add";
        }

        [JsonProperty(PropertyName = "operation")]
        public string Operation { get; set; }
    }

    [PluginActionId("ca.jaggernaut.scoreboard.changescoreaction")]
    class ChangeScoreAction : TeamAction<ChangeScoreSettings>
    {
        protected override bool RequiresWebsocket { get; } = false;
        protected override bool RequiresHttpClient { get; } = true;

        public ChangeScoreAction(SDConnection connection, InitialPayload payload) : base(connection, payload) { }

        public override void KeyPressed(KeyPayload payload)
        {
            var request = new RestRequest("/api/v1/score");
            request.AddParameter("operation", this.Settings.Operation);
            request.AddParameter("team", this.Settings.TeamIndex);
            try
            {
                this.RestClient.Post(request);
                Connection.ShowOk();
            }
            catch (HttpRequestException)
            {
                Connection.ShowAlert();
            }
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            var oldOperation = this.Settings.Operation;
            base.ReceivedSettings(payload);

            if (oldOperation != this.Settings.Operation)
            {
                this.UpdateInfo();
            }
        }

        #region Protected Methods

        protected override void UpdateInfo()
        {
            base.UpdateInfo();

            var change = this.Settings.Operation == "add" ? "+1" : "-1";
            Connection.SetTitleAsync($"P{this.Settings.TeamIndex + 1} {change}");
        }
        #endregion
    }
}