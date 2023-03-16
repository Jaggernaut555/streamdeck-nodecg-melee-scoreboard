using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.cancelpredictionaction")]
    public class CancelPredictionAction : BaseAction<BaseSettings>
    {
        protected override bool RequiresWebsocket { get; } = false;

        public CancelPredictionAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            connection.SetTitleAsync("Cancel\nPrediction");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            try
            {
                dynamic data = new
                {
                    operation = "cancel",
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
    }
}