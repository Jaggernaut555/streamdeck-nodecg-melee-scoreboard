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
    [PluginActionId("ca.jaggernaut.scoreboard.bracketaction")]
    class BracketAction : TeamAction<TeamActionSettings>
    {
        protected override bool RequiresWebsocket { get; } = true;
        protected override bool RequiresHttpClient { get; } = true;

        private class BracketMessage
        {
            [JsonProperty(PropertyName = "TeamIndex")]
            public int TeamIndex { get; set; }
            [JsonProperty(PropertyName = "Bracket")]
            public string Bracket { get; set; }
        }

        public BracketAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
        }

        public override void KeyPressed(KeyPayload payload)
        {
            var request = new RestRequest("/api/v1/bracket");
            request.AddParameter("bracket", "toggle");
            request.AddParameter("team", this.Settings.TeamIndex);
            try
            {
                this.RestClient.Post(request);
            }
            catch (HttpRequestException)
            {
                Connection.ShowAlert();
            }
        }

        #region Protected Methods

        protected override void OnWebsocketConnection()
        {
            this.WsClient.EmitAsync("join", "Bracket").Wait();
            this.UpdateInfo();
        }

        protected override void InitializeWebsocket()
        {
            this.WsClient.On("BracketUpdate", async response =>
            {
                var msg = response.GetValue<BracketMessage>();
                if (msg != null)
                {
                    if (msg.TeamIndex == this.Settings.TeamIndex)
                    {
                        await Connection.SetTitleAsync($"{msg.Bracket}");
                    }
                }
            });
        }
        protected override void UpdateInfo()
        {
            base.UpdateInfo();

            if (this.WsClient != null && this.WsClient.Connected)
            {
                this.WsClient.EmitAsync("Bracket", this.Settings.TeamIndex);
            }
        }

        #endregion
    }
}