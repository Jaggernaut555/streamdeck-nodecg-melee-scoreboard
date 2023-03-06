using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SocketIOClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamDeck_Scoreboard
{
    [PluginActionId("ca.jaggernaut.scoreboard.swapteamsaction")]
    public class SwapTeamsAction : NoTeamAction<BaseSettings>
    {
        protected override bool RequiresWebsocket { get; } = false;
        protected override bool RequiresHttpClient { get; } = true;


        public SwapTeamsAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            connection.SetTitleAsync("Swap");
        }

        public override void Dispose() { }

        public override void KeyPressed(KeyPayload payload)
        {
            var request = new RestRequest("/api/v1/swap");
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
    }
}