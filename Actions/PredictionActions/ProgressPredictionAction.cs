using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.progresspredictionaction")]
    public class ProgressPredictionAction : BaseAction<BaseSettings>
    {
        private class PredictionMessage
        {
            [JsonProperty(PropertyName = "PredictionStatus")]
            public string PredictionStatus { get; set; }
        }

        protected override bool RequiresWebsocket { get; } = true;

        public ProgressPredictionAction(SDConnection connection, InitialPayload payload) : base(connection, payload) { }

        public override void KeyPressed(KeyPayload payload)
        {
            try
            {
                dynamic data = new
                {
                    operation = "progress",
                    type = "prediction"
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

        #region Protected methods

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
                    switch (msg.PredictionStatus)
                    {
                        case "Started":
                            await Connection.SetTitleAsync($"Lock\nPrediction");
                            break;
                        case "Stopped":
                            await Connection.SetTitleAsync($"Start\nPrediction");
                            break;
                        case "Locked":
                            await Connection.SetTitleAsync($"End\nPrediction");
                            break;
                        default:
                            await Connection.SetTitleAsync($"Progress\nPrediction");
                            break;
                    }
                }
            });
        }

        #endregion
    }
}