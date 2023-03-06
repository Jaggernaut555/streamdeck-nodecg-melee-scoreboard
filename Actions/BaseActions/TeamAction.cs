using BarRaider.SdTools;
using StreamDeck_Scoreboard.Actions.BaseActions;
using System.Drawing;
using System.Runtime;

namespace StreamDeck_Scoreboard
{
    abstract class TeamAction<T> : BaseAction<T> where T : TeamActionSettings, new()
    {
        public TeamAction(SDConnection connection, InitialPayload payload) : base(connection, payload) { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            var oldTeamIndex = this.Settings.TeamIndex;
            
            base.ReceivedSettings(payload);

            if (this.Settings.TeamIndex != oldTeamIndex)
            {
                this.UpdateInfo();
            }
        }

        protected override void UpdateInfo()
        {
            if (!this.RequiresWebsocket)
            {
                if (this.Settings.TeamIndex == 0)
                {
                    using (Image image = Image.FromFile("images/player1PluginIcon.png"))
                    {
                        Connection.SetImageAsync(image);
                    }
                }
                else
                {
                    using (Image image = Image.FromFile("images/player2PluginIcon.png"))
                    {
                        Connection.SetImageAsync(image);
                    }
                }
            }
        }

        protected override void UpdateConnectedStatus()
        {
            if (this.RequiresWebsocket && (this.WsClient == null || !this.WsClient.Connected))
            {
                Connection.SetTitleAsync("Connec-\ntion\nError");

                if (this.Settings.TeamIndex == 0)
                {
                    using (Image image = Image.FromFile("images/player1PluginIconError.png"))
                    {

                        Connection.SetImageAsync(image);
                    }
                }
                else
                {
                    using (Image image = Image.FromFile("images/player2PluginIconError.png"))
                    {
                        Connection.SetImageAsync(image);
                    }
                }
                return;
            }

            if (this.Settings.TeamIndex == 0)
            {
                using (Image image = Image.FromFile("images/player1PluginIcon.png"))
                {
                    Connection.SetImageAsync(image);
                }
            }
            else
            {
                using (Image image = Image.FromFile("images/player2PluginIcon.png"))
                {
                    Connection.SetImageAsync(image);
                }
            }
        }
    }
}