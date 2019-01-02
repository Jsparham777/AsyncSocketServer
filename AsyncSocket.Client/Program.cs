using System;
using TCPSocketAsync;

namespace AsyncSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPSocketClient client = new TCPSocketClient();

            client.RaiseTextReceivedEvent += HandleTextReceived;

            Console.WriteLine("**** Socket Client Initialised ***");
            Console.WriteLine("Enter the server IP address and press enter to continue.");

            string IPAddress = Console.ReadLine();

            Console.WriteLine("Enter the server port number (0 - 65535) and press enter to continue.");

            string portNumber = Console.ReadLine();

            if(!client.SetServerIPAddress(IPAddress) || !client.SetPortNumber(portNumber))
            {
                Console.WriteLine($"Wrong IP address or Port number - {IPAddress}:{portNumber}. Press a key to exit.");

                Console.ReadLine();
                return;
            }

            client.ConnectToServer();

            string inputUser = null;

            do
            {
                inputUser = Console.ReadLine();

                if (inputUser.Trim() != "<EXIT>")
                {
                    client.SendToServer(inputUser);
                }
                else if (inputUser.Equals("<EXIT>"))
                    client.CloseAndDisconnect();
                
            } while (inputUser != "<EXIT>");
        }

        private static void HandleTextReceived(object sender, TextReceivedEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now} - Received: {e.TextReceived}{Environment.NewLine}");
        }
    }
}
