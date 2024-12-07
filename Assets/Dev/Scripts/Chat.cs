using System;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using ExitGames.Client.Photon;
//using Photon.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Dev
{
    public class Chat : NetworkContext//, IChatClientListener
    {
        /*[SerializeField] private string _mainChannelKey;

        [SerializeField] private ChatAppSettings _chatAppSettings;
        [SerializeField] private TMP_InputField _chatInputField;
        [SerializeField] private TMP_Text _chatText;


        private ChatClient _chatClient;

        public override void Spawned()
        {
           // Debug.Log($"Connecting to chat");
           // Connect();
        }

        private void Connect()
        {
            string nickname = string.Empty;

            if (nickname == String.Empty)
            {
                nickname = $"Player{Random.Range(0, 99)}";
            }

            _chatClient = new ChatClient(this);
            _chatClient.UseBackgroundWorkerForSending = true;
            _chatClient.AuthValues = new AuthenticationValues(nickname);
            _chatClient.ConnectUsingSettings(_chatAppSettings);

            Debug.Log("Connecting as: " + nickname);
        }

        private void Update()
        {
            if (_chatClient != null)
            {
                _chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!

                OnEnterSend();


                if (Input.GetKeyDown(KeyCode.P))
                {
                    if (_chatClient != null)
                    {
                        _chatClient.Disconnect();
                    }
                }
                
            }
        }

        public void OnEnterSend()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                _chatClient.PublishMessage(_mainChannelKey, _chatInputField.text);
                _chatInputField.text = "";
            }
        }

        public void DebugReturn(DebugLevel level, string message) { }

        public void OnDisconnected()
        {
            Debug.Log($"On Chat disconnected {Runner.LocalPlayer}");
        }

        public void OnConnected()
        {
            Debug.Log($"On Chat connected {Runner.LocalPlayer}");

            _chatClient.Subscribe(_mainChannelKey);

            ShowChannel(_mainChannelKey);
            
            _chatClient.SetOnlineStatus(ChatUserStatus.Online);
        }

        public void OnChatStateChange(ChatState state) { }

        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            string message = messages.ToString();

            Debug.Log($"Receiver msg from channel {channelName}: {message}");

            ShowChannel(channelName);
        }

        public void ShowChannel(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
            {
                return;
            }

            ChatChannel channel = null;
            bool hasThatChat = _chatClient.TryGetChannel(channelName, out channel);

            if (hasThatChat == false)
            {
                Debug.Log("ShowChannel failed to find channel: " + channelName);
                return;
            }

            Debug.Log("ShowChannel: " + channelName);

            _chatText.text = channel.ToStringMessages();
        }

        public void OnPrivateMessage(string sender, object message, string channelName) { }

        public void OnSubscribed(string[] channels, bool[] results) { }

        public void OnUnsubscribed(string[] channels) { }

        public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }

        public void OnUserSubscribed(string channel, string user) { }

        public void OnUserUnsubscribed(string channel, string user) { }

        public void OnApplicationQuit()
        {
            if (_chatClient != null)
            {
                _chatClient.Disconnect();
                Debug.Log($"OnApplicationQuit disconnect from chat");
            }
        }*/
    }
}