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
    [PluginActionId("ca.jaggernaut.scoreboard.cancelpredictionaction")]
    public class CancelPredictionAction : NoTeamAction<BaseSettings>
    {
        protected override bool RequiresWebsocket { get; } = false;
        protected override bool RequiresHttpClient { get; } = true;

        public CancelPredictionAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            connection.SetTitleAsync("Cancel\nPrediction");
        }

        public override void KeyPressed(KeyPayload payload)
        {
            var request = new RestRequest("/api/v1/prediction");
            request.AddParameter("operation", "cancel");
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