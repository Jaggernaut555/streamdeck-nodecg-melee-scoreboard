using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace StreamDeck_Scoreboard
{
    public abstract class NoTeamAction<T> : KeypadBase where T : BaseSettings, new()
    {
        protected SocketIO WsClient { get; set; }

        protected RestClient RestClient { get; set; }

        protected abstract bool RequiresWebsocket { get; }
        protected abstract bool RequiresHttpClient { get; }

        #region Private Members
        private string CurrentWebsocketUrl { get; set; }

        protected T Settings { get; set; }

        #endregion
        public NoTeamAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.Settings = new T();
                SaveSettings();
            }
            else
            {
                this.Settings = payload.Settings.ToObject<T>();
            }
            if (this.RequiresWebsocket)
            {
                InitializeBaseWebsocket();
            }
            if (this.RequiresHttpClient && !string.IsNullOrEmpty(this.Settings.HTTPAddress))
            {
                this.RestClient = new RestClient($"http://{this.Settings.HTTPAddress}");
            }
            this.UpdateInfo();
        }

        public override async void Dispose()
        {
            if (this.WsClient != null)
            {
                await this.WsClient.DisconnectAsync();
                this.WsClient.Dispose();
            }
        }

        public override void KeyPressed(KeyPayload payload) { }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            var oldHttpAddress = this.Settings.HTTPAddress;
            Tools.AutoPopulateSettings(Settings, payload.Settings);
            SaveSettings();

            if (this.RequiresWebsocket)
            {
                InitializeBaseWebsocket();
            }

            if (this.RequiresHttpClient && oldHttpAddress != this.Settings.HTTPAddress && !String.IsNullOrEmpty(this.Settings.HTTPAddress))
            {
                if (this.RestClient != null)
                {
                    this.RestClient.Dispose();
                }
                this.RestClient = new RestClient($"http://{this.Settings.HTTPAddress}");
            }
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Protected Methods

        protected Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(Settings));
        }

        protected async void InitializeBaseWebsocket()
        {
            if (String.IsNullOrEmpty(this.Settings.WebsocketAddress) || (this.WsClient != null && this.CurrentWebsocketUrl == this.Settings.WebsocketAddress))
            {
                return;
            }

            if (this.WsClient != null)
            {
                await this.WsClient.DisconnectAsync();
            }

            var url = new Uri($"ws://{this.Settings.WebsocketAddress}");

            this.WsClient = new SocketIO(url, new SocketIOOptions
            {
                ConnectionTimeout = TimeSpan.FromSeconds(5),
                ReconnectionDelay = 3000
            });

            InitializeWebsocket();

            this.WsClient.OnConnected += (sender, e) =>
            {
                OnWebsocketConnection();
                this.UpdateConnectedStatus();
            };

            this.WsClient.OnReconnected += (sender, e) =>
            {
                this.UpdateConnectedStatus();
            };

            this.WsClient.OnDisconnected += (sender, e) =>
            {
                this.UpdateConnectedStatus();
            };

            try
            {
                await this.WsClient.ConnectAsync();
            }
            catch (ConnectionException)
            {
                this.UpdateConnectedStatus();
            }
            this.CurrentWebsocketUrl = this.Settings.WebsocketAddress;
        }

        protected virtual void OnWebsocketConnection() { }

        protected virtual void InitializeWebsocket() { }
        #endregion

        protected virtual void UpdateConnectedStatus()
        {
            if (this.RequiresWebsocket && (this.WsClient == null || !this.WsClient.Connected))
            {
                using (Image image = Image.FromFile("images/neutralPluginIconError.png"))
                {
                    Connection.SetTitleAsync("Connec-\ntion\nError");
                    Connection.SetImageAsync(image);
                }
            }
            else
            {
                using (Image image = Image.FromFile("images/neutralPluginIcon.png"))
                {
                    Connection.SetImageAsync(image);
                }
            }
        }

        protected virtual void UpdateInfo() { }
    }
}