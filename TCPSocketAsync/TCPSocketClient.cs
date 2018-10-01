using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TCPSocketAsync
{
    public class TCPSocketClient
    {
        private IPAddress mServerIPAddress;
        private int mServerPort;
        private TcpClient mClient;

        public IPAddress ServerIPAddress { get { return mServerIPAddress; } }
        public int ServerPort { get { return mServerPort; } }

        public EventHandler<TextReceivedEventArgs> RaiseTextReceivedEvent;

        public TCPSocketClient()
        {
            mClient = null;
            mServerPort = -1;
            mServerIPAddress = null;
        }

        public bool SetServerIPAddress(string _IPAddressServer)
        {
            if (!IPAddress.TryParse(_IPAddressServer, out IPAddress ipaddr))
            {
                Console.WriteLine("Invalid server IP supplied.");
                return false;
            }

            mServerIPAddress = ipaddr;

            return true;
        }

        protected virtual void OnRaiseTextReceivedEvent(TextReceivedEventArgs e)
        {
            EventHandler<TextReceivedEventArgs> handler = RaiseTextReceivedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool SetPortNumber(string _ServerPort)
        {
            if (!int.TryParse(_ServerPort.Trim(), out int portNumber))
            {
                Console.WriteLine("Invalid port number supplied.");
                return false;
            }

            if (portNumber <= 0 || portNumber > 65535)
            {
                Console.WriteLine("Port number must be between 0 and 65535");
                return false;
            }

            mServerPort = portNumber;

            return true;
        }

        public void CloseAndDisconnect()
        {
            if (mClient != null)
            {
                if (mClient.Connected)
                {
                    mClient.Close();
                }
            }
        }

        public async Task SendToServer(string inputUser)
        {
            if (string.IsNullOrEmpty(inputUser))
            {
                Console.WriteLine("Empty string supplied to send");
                return;
            }

            if (mClient != null)
            {
                if (mClient.Connected)
                {
                    StreamWriter streamWriter = new StreamWriter(mClient.GetStream());
                    streamWriter.AutoFlush = true;

                    await streamWriter.WriteAsync(inputUser);
                    Console.WriteLine("Data sent...");
                }
            }
        }

        public async Task ConnectToServer()
        {
            if (mClient == null)
                mClient = new TcpClient();

            try
            {
                await mClient.ConnectAsync(mServerIPAddress, mServerPort);
                Console.WriteLine($"Connected to server IP/Port: {mServerIPAddress} / {mServerPort}");

                ReadDataAsync(mClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task ReadDataAsync(TcpClient mClient)
        {
            try
            {
                StreamReader clientStreamReader = new StreamReader(mClient.GetStream());
                char[] buffer = new char[64];
                int readByteCount = 0;

                while (true)
                {
                    readByteCount = await clientStreamReader.ReadAsync(buffer, 0, buffer.Length);

                    if (readByteCount == 0)
                    {
                        Console.WriteLine("Disconnected from server");
                        mClient.Close();
                        break;
                    }

                    Console.WriteLine($"Received bytes {readByteCount} - Message: {new string(buffer)}");

                    OnRaiseTextReceivedEvent(new TextReceivedEventArgs(mClient.Client.RemoteEndPoint.ToString(), new string(buffer)));
                    
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
