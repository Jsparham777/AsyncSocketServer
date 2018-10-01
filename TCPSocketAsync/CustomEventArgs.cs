using System;

namespace TCPSocketAsync
{

    public class ConnectionDisconnectedEventArgs : EventArgs
    {
        public ConnectionDisconnectedEventArgs()
        {
            //Create RaiseClientDisconnectedEvent
            //Create RaiseServerDisconnectedEvent
        }
    }

    public class ConnectionConnectedEventArgs : EventArgs
    {
        public ConnectionConnectedEventArgs()
        {
            //Create RaiseServerConnectedEvent
        }
    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public string NewClient { get; set; }

        public ClientConnectedEventArgs(string _newClient)
        {
            NewClient = _newClient;
        }
    }

    public class TextReceivedEventArgs : EventArgs
    {
        public string ClientWhoSentText { get; set; }
        public string TextReceived { get; set; }

        public TextReceivedEventArgs(string _clientWhoSentText, string _textReceived)
        {
            ClientWhoSentText = _clientWhoSentText;
            TextReceived = _textReceived;
        }
    }


    
}
