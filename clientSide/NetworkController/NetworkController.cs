//Lee Neuschwander and Alex Aekle
// PS7 and PS8 for CS 3500
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NetworkController
{
    //Declaration of the type of Delegate.
    public delegate void EventProcessor(SocketState ss);
     
    /// <summary>
    /// This class holds all the necessary state to handle a client connection
    /// Note that all of its fields are public because we are using it like a "struct"
    /// It is a simple collection of fields
    /// </summary>
    public class SocketState
    {
        public Socket theSocket;
        public int ID;

        public EventProcessor EventCallback;
        public bool isDisconnected = false;

        // This is the buffer where we will receive message data from the client
        public byte[] messageBuffer = new byte[1024];

        // This is a larger (growable) buffer, in case a single receive does not contain the full message.
        public StringBuilder sb = new StringBuilder();

        public SocketState(Socket s, int id)
        {
            theSocket = s;
            ID = id;
        }
    }

    /// <summary>
    /// Socket State based class that will create a listener for the network to pass around for the clients and server to use. 
    /// </summary>
    public class TCPSocketState
    {        
        public EventProcessor EventCallback;

        public TcpListener listener;
    }

    public static class Networking
    {

        public const int DEFAULT_PORT = 11000;


        /// <summary>
        /// Start attempting to connect to the server
        /// </summary>
        /// <param name="host_name"> server to connect to </param>
        /// <returns></returns>
        public static SocketState ConnectToServer(EventProcessor CallMe, string hostName)
        {
            System.Diagnostics.Debug.WriteLine("connecting  to " + hostName);
        
            // Connect to a remote device.
            try
            {        
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;
                IPAddress ipAddress = IPAddress.None;
        
                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        MessageBox.Show("Invalid addres: " + hostName);
                        return null;
                    }
                }
                catch (Exception e1)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }
        
                // Create a TCP/IP socket.
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        
                SocketState ss = new SocketState(socket, -1);
        
                ss.EventCallback = CallMe;
                ss.theSocket.BeginConnect(ipAddress,DEFAULT_PORT, ConnectedToServer, ss);
                return ss;
            }
            catch (Exception e)
            {
                MessageBox.Show ("Unable to connect to server. Error occured: " + e);
                return null;
            }
        }
        
        /// <summary>
        /// Ends the connection with server and then starts up a loop to recieve from server.
        /// </summary>
        /// <param name="ar"></param>
        public static void ConnectedToServer(IAsyncResult ar)
        {            
            SocketState ss = (SocketState)ar.AsyncState;
            ss.theSocket.EndConnect(ar);
            ss.EventCallback(ss);
            ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
            
        }

        /// <summary>
        /// This creates a system to get data from the server and then jump to the event callback.
        /// </summary>
        /// <param name="ar"></param>
        public static void ReceiveCallback(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;
            try
            {                
                int bytesRead = ss.theSocket.EndReceive(ar);

                // If the socket is still open
                if (bytesRead > 0)
                {
                    //Needs possible work done we need to clear the string at one point ss.sb.Clear(); ****************************************
                    string theMessage = Encoding.UTF8.GetString(ss.messageBuffer, 0, bytesRead);
                    // Append the received data to the growable buffer.
                    // It may be an incomplete message, so we need to start building it up piece by piece
                    ss.sb.Append(theMessage);

                    ss.EventCallback(ss);
                }
            }
            catch (SocketException)
            {
                ss.isDisconnected = true;
                ss.theSocket.Disconnect(false);
            }
}

        /// <summary>
        /// Asks for more data from the server. 
        /// </summary>
        /// <param name="ss"></param>
        public static void GetData(SocketState ss)
        {            
            ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
        }

        /// <summary>
        /// Will take data that was sent and send it back to the server. 
        /// </summary>
        /// <param name="ss"></param>
        /// <param name="_data"></param>
        public static void Send(SocketState ss, string _data)
        {
            try
            {
                string Data = _data;
                byte[] messageBytes = Encoding.UTF8.GetBytes(Data);

                ss.theSocket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, ss);

            }
            catch(SocketException e)
            {
                ss.isDisconnected = true;
                ss.theSocket.Disconnect(false);
            }

        }
        
        /// <summary>
        /// A callback invoked when a send operation completes
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;
            // Nothing much to do here, just conclude the send operation so the socket is happy.

            ss.theSocket.EndSend(ar);            
        }

        //--------------------------------------------------------Server Networking Code----------------------------------------------------------------------------------

        /// <summary>
        /// Creates a TCP listener for the server to listen for any new socket being sent in.
        /// </summary>
        /// <param name="CallMe"></param>
        public static void ServerAwatingClientLoop(EventProcessor CallMe)
        {
            TCPSocketState TCPss = new TCPSocketState();
            TCPss.EventCallback = CallMe;
            TCPss.listener = new TcpListener(IPAddress.Any, DEFAULT_PORT);
            TCPss.listener.Start(); 
            
            TCPss.listener.BeginAcceptSocket(AcceptNewClient, TCPss);

        }

        /// <summary>
        /// Accept new client and create the socket state for that client. 
        /// Restarts the loop to look for new Clients. 
        /// </summary>
        /// <param name="ar"></param>
        public static void AcceptNewClient(IAsyncResult ar)
        {
            TCPSocketState TCPss = (TCPSocketState)ar.AsyncState;

            //Creates a socket from the TCP Listener results. 
            Socket socket = TCPss.listener.EndAcceptSocket(ar);
           
            SocketState ss = new SocketState(socket, -1);

            //Gives the eventcallback from the TCPSocketState to the new SocketState.
            ss.EventCallback = TCPss.EventCallback;
            ss.EventCallback(ss);

            TCPss.listener.BeginAcceptSocket(AcceptNewClient, TCPss);     
            
        }

    }
}
