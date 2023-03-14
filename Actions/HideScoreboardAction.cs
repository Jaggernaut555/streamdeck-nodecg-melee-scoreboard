using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.hidescoreboardaction")]
    public class HideScoreboardAction : BaseAction<BaseSettings>
    {
        private class HideScoreboradMessage
        {
            [JsonProperty(PropertyName = "HideScoreboard")]
            public bool HideScoreboard { get; set; }
        }

        protected override bool RequiresWebsocket { get; } = true;

        protected override bool RequiresHttpClient { get; } = true;

        public HideScoreboardAction(SDConnection connection, InitialPayload payload) : base(connection, payload) { }

        public override void KeyPressed(KeyPayload payload)
        {
            var request = new RestRequest("/api/v1/scoreboard");
            request.AddParameter("operation", "toggle");
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
            this.WsClient.EmitAsync("join", "HideScoreboard").Wait();
            this.WsClient.EmitAsync("HideScoreboard");
        }

        protected override void InitializeWebsocket()
        {
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
        }

        #endregion
    }
}