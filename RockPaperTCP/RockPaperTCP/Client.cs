//RockPaperTCP
//Tilly Dewing Fall 2019 Networking Project
 
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace RockPaperTCP
{
    class Client
    {
        public static string serverIP = "127.0.0.1";
        public static int port = 25555;
        public static int bufferSize = 2048; //2KB buffer size for messages
        public static bool running = false;
        public static string playerName = "Player";

        private static NetworkStream msgStream = null;
        private static TcpClient tcpClient;
        private static bool isTurn = false;
        private static List<string> chatLog = new List<string>();
        private static List<string> serverLog = new List<string>();
        private static int chatlogLimit = 8; //number of messages to display in chat log
        private static int selectedOption = 0;
        private static string chatbox = "";

        public static void InitializeClient(string ip, int port, string playerName)
        {
            serverIP = ip;
            Client.port = port;
            Client.playerName = playerName;
            tcpClient = new TcpClient();
            tcpClient.SendBufferSize = bufferSize;
            tcpClient.ReceiveBufferSize = bufferSize;
            running = true;
            Connect();
        }
        private static void ShutDownClient()
        {
            Console.WriteLine("[Info]Client is shuting down");
            CleanUpNetResources();
            Console.WriteLine("Done...");
            Console.Clear();
            running = false;
            MainMenu.running = true;
        }

        public static void Update()
        {
            GetAvailableMessage();
            
            if (Input.GetKey(ConsoleKey.DownArrow))
            {
                selectedOption++;
            }
            else if (Input.GetKey(ConsoleKey.UpArrow))
            {
                selectedOption--;
            }
            else if (Input.GetKey(ConsoleKey.Enter))
            {
                if (isTurn)
                {
                    SendMessage("MOVE|" + (selectedOption + 1));
                    isTurn = false;
                }       
            }
            selectedOption = Math.Clamp(selectedOption, 0, 2);

            foreach (ConsoleKey key in Input.keysDown)
            {
                Console.WriteLine(key);
                if (key == ConsoleKey.Enter && !isTurn)
                {
                    Console.Clear();
                    chatLog.Add(playerName + "> " + chatbox);
                    SendMessage("CHAT|" + chatbox);
                    chatbox = "";
                }
                else if (key == ConsoleKey.Backspace)
                {
                    chatbox.Remove(chatbox.Length - 1);
                }
                else if (key == ConsoleKey.Spacebar)
                {
                    chatbox += " ";
                }
                else if (key != ConsoleKey.UpArrow && key != ConsoleKey.DownArrow)
                {
                    chatbox += key.ToString();
                }
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(isTurn);
            ConsoleExtentions.WriteLineColor("Chat Log:", ConsoleColor.Cyan);
            foreach (string chatMsg in chatLog)
            {
                Console.WriteLine(chatMsg);
            }
            Console.WriteLine("____________");
            ConsoleExtentions.WriteLineColor("Server Log:", ConsoleColor.White);
            foreach (string chatMsg in serverLog)
            {
                Console.WriteLine("> " + chatMsg);
            }
            Console.WriteLine("____________");

            if (isTurn == true)
            {
                switch (selectedOption)
                {
                    case 0:
                        ConsoleExtentions.WriteLineBackgroundColor("Rock", ConsoleColor.White, ConsoleColor.DarkCyan);
                        Console.WriteLine("Paper");
                        Console.WriteLine("Scissors");
                        break;
                    case 1:
                        Console.WriteLine("Rock");
                        ConsoleExtentions.WriteLineBackgroundColor("Paper", ConsoleColor.White, ConsoleColor.DarkCyan);
                        Console.WriteLine("Scissors");
                        break;
                    case 2:
                        Console.WriteLine("Rock");
                        Console.WriteLine("Paper");
                        ConsoleExtentions.WriteLineBackgroundColor("Scissors", ConsoleColor.White, ConsoleColor.DarkCyan);
                        break;
                }
            }
            if (isTurn == false)
            {
                foreach (ConsoleKey key in Input.keysDown)
                {
                    if (key == ConsoleKey.Enter)
                    {
                        Console.Clear();
                        chatLog.Add(playerName + "> " + chatbox);
                        SendMessage("CHAT|" + chatbox);
                        chatbox = "";
                    }
                    else if (key == ConsoleKey.Backspace)
                    {
                        chatbox.Remove(chatbox.Length - 1);
                    }
                    else if (key == ConsoleKey.Spacebar)
                    {
                        chatbox += " ";
                    }
                    else if (key != ConsoleKey.UpArrow && key != ConsoleKey.DownArrow)
                    {
                        chatbox += key.ToString();
                    }
                }

                Console.WriteLine("_____________________________________________________________________________________________");
                Console.WriteLine('>' + chatbox);
            }

            while (serverLog.Count > chatlogLimit)
            {
                serverLog.RemoveAt(0);
            }
            while (chatLog.Count > chatlogLimit)
            {
                chatLog.RemoveAt(0);
            }
        }
        public static void SendMessage(string message)
        {
            if (tcpClient != null && !IsDisconnected(tcpClient))
            {
                byte[] msgBuffer = Encoding.UTF8.GetBytes(message);
                msgStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
        }
        public static void GetAvailableMessage() //gets a message from server if available
        {
            if (IsDisconnected(tcpClient) || msgStream == null)
            {
                return; ;
            }
            int messageLength = tcpClient.Available;
            if (messageLength >= 0)
            {
                try
                {
                    byte[] msgBuffer = new byte[messageLength];
                    msgStream.Read(msgBuffer, 0, messageLength);
                    string msg = Encoding.UTF8.GetString(msgBuffer); //Converts the recevived byte array back into a string
                    if (msg.Length != 0)
                    {
                        string[] splitMsg = msg.Split('|');

                        switch (splitMsg[0])
                        {
                            case "CHAT":
                                chatLog.Add(splitMsg[1]);
                                break;
                            case "CMD":
                                if (splitMsg[1] == "ENDTURN")
                                {
                                    isTurn = false;
                                }
                                else if (splitMsg[1] == "STARTTURN")
                                {
                                    ;
                                    isTurn = true;
                                }
                                break;
                            case "INFO":
                                Console.Clear();
                                serverLog.Add(splitMsg[1]);
                                break;
                        }
                    }
                }
                catch (IOException exeption)
                {
                    ShutDownClient();
                }
                
            }
            return;
        }
        public static void Connect()
        {
            Console.WriteLine("Connecting to: " + serverIP + ":" + port);
            try //Try catch to handle when client is unable to connect
            {
                tcpClient.Connect(serverIP, port);
                EndPoint endPoint = tcpClient.Client.RemoteEndPoint;
            }
            catch (SocketException exeption)
            {
                ConsoleExtentions.WriteLineColor("[ERROR]Connection Failed", ConsoleColor.Red);
                Thread.Sleep(100);
                ShutDownClient();

            }

            if (tcpClient.Connected) //Ensure client was able to connect
            {
                Console.WriteLine("[INFO]Connected to Server on: " + serverIP + ":" + port);
                //Send Server playerName
                msgStream = tcpClient.GetStream();
                byte[] msgBuffer = Encoding.UTF8.GetBytes(string.Format(playerName));
                msgStream.Write(msgBuffer, 0, msgBuffer.Length);

                if (!IsDisconnected(tcpClient))
                {
                    //If still connected server accepted name
                    running = true;
                }
                else //Name checking is not implimented server side however this is a usefull check;
                {
                    //Server more than likely rejected name
                    CleanUpNetResources();
                    ConsoleExtentions.WriteLineColor("[ERROR]Server rejected player name: " + playerName, ConsoleColor.Red);
                    Thread.Sleep(2000);
                    ShutDownClient();

                }
            }
            else //Server is unable to connect
            {
                CleanUpNetResources();
                ConsoleExtentions.WriteLineColor("[ERROR]Unable to connect to Server on: " + serverIP + ":" + port, ConsoleColor.Red);
                Thread.Sleep(2000);
                ShutDownClient();

            }
        }
        private static bool IsDisconnected(TcpClient client) //Polls the client to see if they are still connected.
        {
            if (client == null)
            {
                return true;
            }
            try
            {
                Socket s = client.Client;
                return s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
            }
            catch (SocketException exeption)
            {
                //On a socket expection we assume the client is disconected
                return true;
            }
        }
        private static void CleanUpNetResources() //Used to clean up any network resources on disconect
        {
            if (msgStream != null)
            {
                msgStream.Close();
                msgStream = null;
                tcpClient.Close();
            }
        }

    }
}
