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
    public class PredictionStatusAction : BaseAction<BaseSettings>
    {
        private class PredictionMessage
        {
            [JsonProperty(PropertyName = "PredictionStatus")]
            public string PredictionStatus { get; set; }
        }

        protected override bool RequiresWebsocket { get; } = true;

        public PredictionStatusAction(SDConnection connection, InitialPayload payload) : base(connection, payload) { }

        #region Protected Methods

        protected override void OnWebsocketConnection()
        {
            this.WsClient.EmitAsync("join", "Prediction").Wait();
            this.WsClient.EmitAsync("Prediction");
        }

        protected override void InitializeWebsocket()
        {
            this.WsClient.On("PredictionUpdate", async response =>
            {
                var msg = response.GetValue<PredictionMessage>();
                if (msg != null)
                {
                    await Connection.SetTitleAsync($"{msg.PredictionStatus}");
                }
            });
        }

        #endregion
    }
}