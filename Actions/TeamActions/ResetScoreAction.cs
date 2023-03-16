using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.resetscoreaction")]
    public class ResetScoreAction : BaseAction<BaseSettings>
    {
        protected override bool RequiresWebsocket { get; } = false;

        public ResetScoreAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            connection.SetTitleAsync("Reset\nScores");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            try
            {
                dynamic data1 = new
                {
                    operation = "reset",
                    team = 0,
                    type = "score"
                };

                dynamic data2 = new
                {
                    operation = "reset",
                    team = 1,
                    type = "score"
                };

                this.WsClient.EmitAsync("Update", data1).Wait();
                this.WsClient.EmitAsync("Update", data2).Wait();
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