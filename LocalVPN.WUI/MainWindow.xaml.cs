using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LocalVPN.WUI
{
    public partial class MainWindow : Window
    {
        private UdpClient? _udpClient;
        private Thread? _tunnelThread;
        private bool _isConnected = false;
        private int _packetCounter = 1;

        public MainWindow()
        {
            InitializeComponent();
            TxtLog.Text = $"[{DateTime.Now:HH:mm:ss}] [SYSTEM] Security Core Ready. Waiting for activation...\n";
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            StopTunnel();

            try
            {
                Process[] serverProcesses = Process.GetProcessesByName("LocalVPN");
                foreach (var process in serverProcesses)
                {
                    process.Kill();
                }
            }
            catch (Exception) { }

            Environment.Exit(0);
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                StartTunnel();
            }
            else
            {
                StopTunnel();
            }
        }

        private void StartTunnel()
        {
            _isConnected = true;
            _packetCounter = 1;
            CboCountries.IsEnabled = false;

            string? selectedCountry = (CboCountries.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
            string countryDisplayName = selectedCountry != null ? selectedCountry.Split('(')[0].Trim().ToUpper() : "SECURE NODE";

            TxtStatus.Text = $"PROTECTED // NODE: {countryDisplayName}";
            TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(27, 94, 32));

            BtnConnect.Content = "TERMINATE CONNECTION";
            BtnConnect.Background = new SolidColorBrush(Color.FromRgb(21, 101, 192));
            BtnConnect.Foreground = new SolidColorBrush(Color.FromRgb(255, 229, 127));

            WriteLog("[SECURITY] Initializing loopback virtual tunnel interface...");
            WriteLog("[SECURITY] Capturing traffic and establishing AES-256 forwarders.");

            _udpClient = new UdpClient();
            _tunnelThread = new Thread(TunnelLoop) { IsBackground = true };
            _tunnelThread.Start();
        }

        private void StopTunnel()
        {
            _isConnected = false;
            CboCountries.IsEnabled = true;

            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }

            if (_tunnelThread != null && _tunnelThread.IsAlive)
            {
                _tunnelThread.Join(300);
                _tunnelThread = null;
            }

            TxtStatus.Text = "UNPROTECTED - DISCONNECTED";
            TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(183, 28, 28));

            BtnConnect.Content = "ACTIVATE THE SECURITY";
            BtnConnect.Background = new SolidColorBrush(Color.FromRgb(62, 39, 35));
            BtnConnect.Foreground = new SolidColorBrush(Color.FromRgb(255, 229, 127));

            WriteLog("[SECURITY] Secure tunnel collapsed. Virtual adapter disabled.");
        }

        private void TunnelLoop()
        {
            string serverIP = "127.0.0.1";
            int serverPort = 9999;

            try
            {
                while (_isConnected && _udpClient != null)
                {
                    string countryName = "";
                    Dispatcher.Invoke(() =>
                    {
                        var item = CboCountries.SelectedItem as System.Windows.Controls.ComboBoxItem;
                        countryName = item != null ? item.Content.ToString().Split('(')[0].Trim() : "Simulation";
                    });

                    string originalFakeData = $"[RAW_PACKET] Source: 10.0.0.2 | Dest: 8.8.8.8 | Payload_ID: {_packetCounter} | Node: {countryName.ToUpper()} | Protocol: TCP (AES-256)";
                    byte[] packetBytes = Encoding.UTF8.GetBytes(originalFakeData);

                    _udpClient.Send(packetBytes, packetBytes.Length, serverIP, serverPort);

                    Dispatcher.Invoke(() =>
                    {
                        WriteLog($"[CLIENT] Captured & Forwarded Packet No: {_packetCounter} ({packetBytes.Length} bytes)");
                    });

                    _packetCounter++;
                    Thread.Sleep(1500);
                }
            }
            catch { }
        }

        private void WriteLog(string message)
        {
            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            TxtLog.ScrollToEnd();
        }
    }
}