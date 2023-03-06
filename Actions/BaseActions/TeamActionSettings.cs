using Newtonsoft.Json;

namespace StreamDeck_Scoreboard.Actions.BaseActions
{
    internal class TeamActionSettings : BaseSettings
    {
        public TeamActionSettings() : base()
        {
            TeamIndex = 0;
        }
        [JsonProperty(PropertyName = "teamIndex")]
        public int TeamIndex { get; set; }
    }
}
