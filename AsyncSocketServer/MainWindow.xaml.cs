using System;
using System.Windows;
using TCPSocketAsync;

namespace AsyncSocketServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TCPSocketServer _Server;

        public MainWindow()
        {
            InitializeComponent();
            _Server = new TCPSocketServer();

            _Server.RaiseClientConnectedEvent += HandleClientConnected;
            _Server.RaiseTextReceivedEvent += HandleTextReceived;
        }

        private void ButtonAcceptIncomingConnections_Click(object sender, RoutedEventArgs e)
        {
            _Server.StartListeningForIncomingConnectionsAsync();
            ListViewClientConnections.Items.Add("Server started. Accepting incoming connections...");
        }
               
        private void ButtonBroadcast_Click(object sender, RoutedEventArgs e)
        {
            _Server.SendToAllAsync(TextBoxMessage.Text.Trim());
            ListViewClientConnections.Items.Add($"Broadcasting \"{TextBoxMessage.Text.Trim()}\".");
        }

        private void ButtonStopServer_Click(object sender, RoutedEventArgs e)
        {
            _Server.StopServer();
            ListViewClientConnections.Items.Add("Server stopped.");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Server.StopServer();
        }

        private void HandleClientConnected(object sender, ClientConnectedEventArgs e)
        {
            ListViewClientConnections.Items.Add($"{DateTime.Now} - New client connected: {e.NewClient}");
        }

        private void HandleTextReceived(object sender, TextReceivedEventArgs e)
        {
            ListViewClientConnections.Items.Add($"{DateTime.Now} - Received from {e.ClientWhoSentText}: {e.TextReceived}");
        }

    }
}
