using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public ChangeScoreAction(SDConnection connection, InitialPayload payload) : base(connection, payload) { }

        public override void KeyPressed(KeyPayload payload)
        {
            try
            {
                dynamic data = new
                {
                    operation = this.Settings.Operation,
                    team = this.Settings.TeamIndex,
                    type = "score"
                };

                this.WsClient.EmitAsync("Update", data).Wait();
                Connection.ShowOk();
            }
            catch (Exception e)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, e.Message);
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