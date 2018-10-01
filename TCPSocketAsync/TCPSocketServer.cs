using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPSocketAsync
{
    public class TCPSocketServer
    {
        #region Private members
        /// <summary>
        /// The IP Address
        /// </summary>
        private IPAddress _IP;

        /// <summary>
        /// The port number
        /// </summary>
        private int _Port;

        /// <summary>
        /// The TCP Listener
        /// </summary>
        private TcpListener _TCPListener;

        /// <summary>
        /// The TCP clients list
        /// </summary>
        private List<TcpClient> _Clients;
        #endregion

        #region Events
        /// <summary>
        /// 
        /// </summary>
        public EventHandler<ClientConnectedEventArgs> RaiseClientConnectedEvent;

        /// <summary>
        /// 
        /// </summary>
        public EventHandler<TextReceivedEventArgs> RaiseTextReceivedEvent;
        #endregion

        #region Properties
        public bool KeepRunning { get; set; }

        /// <summary>
        /// Gets a list of all connected TCP clients
        /// </summary>
        public List<TcpClient> ClientList { get { return _Clients; } }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TCPSocketServer()
        {
            _Clients = new List<TcpClient>();
        }

        protected virtual void OnRaisedClientConnectedEvent(ClientConnectedEventArgs e)
        {
            EventHandler<ClientConnectedEventArgs> handler = RaiseClientConnectedEvent;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRaisedTextReceivedEvent(TextReceivedEventArgs e)
        {
            EventHandler<TextReceivedEventArgs> handler =RaiseTextReceivedEvent;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Starts to listen for incoming connections on the specified IP address and port number
        /// </summary>
        /// <param name="ipaddr">IP address</param>
        /// <param name="port">Port number</param>
        public async void StartListeningForIncomingConnectionsAsync(IPAddress ipaddr = null, int port = 23000)
        {
            if (ipaddr == null)
                ipaddr = IPAddress.Any;

            if (port <= 0)
                port = 23000;

            _IP = ipaddr;
            _Port = port;

            Debug.WriteLine($"IP Address: {_IP.ToString()} - Port: {_Port.ToString()}");

            _TCPListener = new TcpListener(_IP, _Port);

            try
            {
                _TCPListener.Start();

                KeepRunning = true;

                while (KeepRunning)
                {
                    var returnedByAccept = await _TCPListener.AcceptTcpClientAsync();

                    _Clients.Add(returnedByAccept);

                    Debug.WriteLine($"Client connected successfully, count: {_Clients.Count} - {returnedByAccept.Client.RemoteEndPoint}");

                    HandleTCPClientAsync(returnedByAccept);

                    ClientConnectedEventArgs eaClientConnected = new ClientConnectedEventArgs(returnedByAccept.Client.RemoteEndPoint.ToString());
                    OnRaisedClientConnectedEvent(eaClientConnected);
                }                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Stops listening for incoming connecttion and shuts down the server
        /// </summary>
        public void StopServer()
        {
            try
            {
                if (_TCPListener != null)
                    _TCPListener.Stop();

                foreach (TcpClient client in _Clients)
                    client.Close();

                _Clients.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Handles any communication between the server and client
        /// </summary>
        /// <param name="paramClient">The TCP Client</param>
        private async Task HandleTCPClientAsync(TcpClient paramClient)
        {
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = paramClient.GetStream();
                reader = new StreamReader(stream);

                char[] buffer = new char[64];

                while (KeepRunning)
                {
                    Debug.WriteLine("*** Ready to read");

                    int nRet = await reader.ReadAsync(buffer, 0, buffer.Length);

                    Debug.WriteLine($"Returned: {nRet}");

                    if (nRet == 0)
                    {
                        RemoveClient(paramClient);

                        Debug.WriteLine("Socket disconnected");
                        break;
                    }

                    string receivedText = new string(buffer);

                    Debug.WriteLine($"*** Recieved: {receivedText}");

                    OnRaisedTextReceivedEvent(new TextReceivedEventArgs(paramClient.Client.RemoteEndPoint.ToString(), receivedText));

                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                RemoveClient(paramClient);
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Removes a client from the client list
        /// </summary>
        /// <param name="paramClient">The TCP client to remove</param>
        private void RemoveClient(TcpClient paramClient)
        {
            if(_Clients.Contains(paramClient))
            {
                _Clients.Remove(paramClient);
                Debug.WriteLine($"Client removed, count: {_Clients.Count}");
            }
        }

        /// <summary>
        /// Broadcasts a message to all connected clients
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        public async void SendToAllAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            try
            {
                byte[] bufferMessage = Encoding.ASCII.GetBytes(message);

                foreach (TcpClient client in _Clients)
                {
                    client.GetStream().WriteAsync(bufferMessage, 0, bufferMessage.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
