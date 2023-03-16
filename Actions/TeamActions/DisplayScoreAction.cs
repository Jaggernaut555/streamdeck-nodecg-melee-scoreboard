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
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.displayscoreaction")]
    class DisplayScoreAction : TeamAction<TeamActionSettings>
    {
        protected override bool RequiresWebsocket { get; } = true;

        private class ScoreMessage
        {
            [JsonProperty(PropertyName = "TeamIndex")]
            public int TeamIndex { get; set; }
            [JsonProperty(PropertyName = "Score")]
            public int Score { get; set; }
        }

        public DisplayScoreAction(SDConnection connection, InitialPayload payload) : base(connection, payload) { }

        #region Protected Methods

        protected override void OnWebsocketConnection()
        {
            this.WsClient.EmitAsync("join", "Score").Wait();
            this.UpdateInfo();
        }

        protected override void InitializeWebsocket()
        {
            this.WsClient.On("ScoreUpdate", async response =>
            {
                var msg = response.GetValue<ScoreMessage>();
                if (msg != null)
                {
                    if (msg.TeamIndex == this.Settings.TeamIndex)
                    {
                        await Connection.SetTitleAsync($"{msg.Score}");
                    }
                }
            });
        }
        protected override void UpdateInfo()
        {
            base.UpdateInfo();

            if (this.WsClient != null && this.WsClient.Connected)
            {
                this.WsClient.EmitAsync("Score", this.Settings.TeamIndex);
            }

        }
        #endregion
    }
}