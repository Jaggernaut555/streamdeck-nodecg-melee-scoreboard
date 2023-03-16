using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.swapteamsaction")]
    public class SwapTeamsAction : BaseAction<BaseSettings>
    {
        protected override bool RequiresWebsocket { get; } = false;

        public SwapTeamsAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            connection.SetTitleAsync("Swap");
        }

        public override void Dispose() { }

        public override void KeyPressed(KeyPayload payload)
        {
            try
            {
                this.WsClient.EmitAsync("Swap").Wait();
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