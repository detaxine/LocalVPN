using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalVPN
{
    class Program
    {
        static void Main(string[] args)
        {
        
            try
            {
                Console.Title = "Local VPN - SERVER CORE";
            }
            catch (IOException)
            {
                
                StreamWriter sw = new StreamWriter(Stream.Null);
                Console.SetOut(sw);
                Console.SetError(sw);
            }

            
            UdpClient udpServer = new UdpClient(9999);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                
                byte[] receivedBytes = udpServer.Receive(ref remoteEP);
                string incomingMessage = Encoding.UTF8.GetString(receivedBytes);
                
                Console.WriteLine($"Received: {incomingMessage}");
            }
        }
    }
}