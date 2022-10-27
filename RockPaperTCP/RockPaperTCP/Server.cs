//RockPaperTCP
//Tilly Dewing Fall 2019 Networking Project

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Net;

namespace RockPaperTCP
{
    class Server
    {
        private static TcpListener server;
        private static gameClient connectedPlayer;
        private static bool playerjoined;
        private static readonly int bufferSize = 2048; //2KB buffer size for messages

        private static List<string> chatLog = new List<string>();
        private static List<string> serverLog = new List<string>();
        private static int chatlogLimit = 8; //number of messages to display in chat log
        private static int selectedOption = 0;
        private static byte clientsMove = 0;
        private static byte serversMove = 0;
        private static string chatbox = "";

        public static int port = 25555;
        public static string localPlayerName = "LOCPlayer";
        public static bool running = false;
        public static bool isServerTurn = false;
        public static int roundLength = 15;
        public static byte serverScore = 0;
        public static byte clientScore = 0;
        public static byte scorelimit = 3;

        public static void Shutdown()
        {
            if (running)
            {
                Console.WriteLine("[Info]Shutting down server...");
                CleanUpNetResources();
                running = false;
                Console.Clear();

            }
        }

        public static void StartServer(string localPlayerName)
        {
            Server.localPlayerName = localPlayerName;
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            running = true;
            Console.WriteLine("Server started awaiting client connection...");        
        }

        public static void AcceptClientConnection()
        {
            //Handle intial client connection
            connectedPlayer = new gameClient();
            connectedPlayer.tcpClient = server.AcceptTcpClient();
            NetworkStream netstream = connectedPlayer.tcpClient.GetStream();

            //Set expected buffer sizes for new client
            connectedPlayer.tcpClient.SendBufferSize = bufferSize;
            connectedPlayer.tcpClient.ReceiveBufferSize = bufferSize;

            //Output debug info about incoming connection
            EndPoint endPoint = connectedPlayer.tcpClient.Client.RemoteEndPoint;
            Console.WriteLine("New player connecting from: ", endPoint);

            //client sends player name data
            byte[] msgBuffer = new byte[bufferSize];
            int bytesRead = netstream.Read(msgBuffer, 0, msgBuffer.Length);

            if (bytesRead > 0)
            {
                connectedPlayer.playerName = Encoding.UTF8.GetString(msgBuffer, 0, bytesRead); //encodes recevied byte array as a string and sets player name.
                playerjoined = true;
                Console.WriteLine("Player: {0} joined the game", connectedPlayer.playerName);
                StartServerTurn();
            }
            else
            {
                connectedPlayer.tcpClient.Close(); //Close connection if we don't recevie anything
            }
        }

        public static void Update()
        {
            CheckMessages();
            if (!running) //server update won't run if server isn't started.
            {
                return;
            }

            if (server.Pending() && !playerjoined)
            {
                AcceptClientConnection(); //If server has a pending connection accept it. 
            }

            if (playerjoined) //Main game loop
            {
                
                Console.SetCursorPosition(0, 0);
                ConsoleExtentions.WriteLineColor("Chat Log:", ConsoleColor.Cyan);
                foreach (string msg in chatLog)
                {
                    Console.WriteLine("> " + msg);
                }
                Console.WriteLine("____________");
                ConsoleExtentions.WriteLineColor("Server Log:", ConsoleColor.White);
                foreach (string msg in serverLog)
                {
                    Console.WriteLine("> " + msg);
                }
                Console.WriteLine("____________");

                if (isServerTurn)
                {
                    if (!Timer.running)
                    {
                        SendMessage("INFO|Local Player Didn't chose in time. Server will select random move");
                        serverLog.Add("Local Player Didn't chose in time. Server will select random move");
                        Random ran = new Random();
                        serversMove = (byte)ran.Next(1, 3);
                        StartClientTurn();
                        return;
                    }

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
                        serversMove = (byte)(selectedOption + 1);
                        StartClientTurn();
                        return;
                    }
                    selectedOption = Math.Clamp(selectedOption, 0, 2);

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
                else 
                {
                    if (clientsMove != 0) //if we have recevied a move from client
                    {
                        string outcome = GetRoundOutCome();
                        serverLog.Add(outcome); //Logs outcome Serverside
                        SendMessage("INFO|" + outcome); //formats and sends info about round result to client
                        StartServerTurn();
                        return;
                    }
                    else if (!Timer.running) //If timer is run out and client hasn't moved.
                    {
                        SendMessage("INFO|Player Didn't chose in time. Server will select random move");
                        serverLog.Add("Player Didn't chose in time. Server will select random move");
                        Random ran = new Random();
                        clientsMove = (byte)ran.Next(1, 3);
                        string outcome = GetRoundOutCome();
                        serverLog.Add(outcome); //Logs outcome Serverside
                        SendMessage("INFO|" + outcome); //formats and sends info about round result to client
                        StartServerTurn();
                        return;
                    }


                    //chat goes here
                    foreach (ConsoleKey key in Input.keysDown)
                    {
                        if (key == ConsoleKey.Enter)
                        {
                            Console.Clear();
                            chatLog.Add(localPlayerName + "> " + chatbox);
                            SendMessage("CHAT|" + localPlayerName + "> " + chatbox);
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

            //check game messages here and send data bassed on state
        }

        private static string GetRoundOutCome()
        {
            string outcome = "";
            switch (serversMove) //1 - rock 2 - paper 3 - scissors
            {
                case 1:
                    switch (clientsMove)
                    {
                        case 1:
                            outcome = ("Round is a tie. Score is " + serverScore + " to " + clientScore);
                            break;
                        case 2:
                            clientScore++;
                            outcome = (connectedPlayer.playerName + " Wins!!! Score is " + serverScore + " to " + clientScore);
                            break;
                        case 3:
                            serverScore++;
                            outcome = (localPlayerName + " Wins!!! Score is " + serverScore + " to " + clientScore);
                            break;
                    }
                    break;
                case 2:
                    switch (clientsMove)
                    {
                        case 1:
                            serverScore++;
                            outcome = (localPlayerName + " Wins!!! Score is " + serverScore + " to " + clientScore);
                            break;
                        case 2:
                            outcome = ("Round is a tie. Score is " + serverScore + " to " + clientScore);
                            break;
                        case 3:
                            clientScore++;
                            outcome = (connectedPlayer.playerName + " Wins!!! Score is " + serverScore + " to " + clientScore);
                            break;
                    }
                    break;
                case 3:
                    switch (clientsMove)
                    {
                        case 1:
                            clientScore++;
                            outcome = (connectedPlayer.playerName + " Wins!!! Score is " + serverScore + " to " + clientScore);
                            break;
                        case 2:
                            serverScore++;
                            outcome = (localPlayerName + " Wins!!! Score is " + serverScore + " to " + clientScore);
                            break;
                        case 3:
                            outcome = ("Round is a tie. Score is " + serverScore + " to " + clientScore);
                            break;
                    }
                    break;
            } //Sets outcome of round and upadates score;
            return outcome;
        }
        private static void StartServerTurn()
        {
            clientsMove = 0;
            serversMove = 0;
            isServerTurn = true;
            Console.Clear();
            SendMessage("CMD|ENDTURN"); //tells client to end turn;
            Timer.StopTimer();
            Timer.StartTimer(roundLength); //Starts timer for 15 seconds
        }
        private static void StartClientTurn()
        {
            Console.Clear();
            isServerTurn = false;
            SendMessage("CMD|STARTTURN"); //tells client it their turn;
            Timer.StopTimer();
            Timer.StartTimer(roundLength); //Start timer for client to send move longer than round timer to allow for transmission/processing time (This is probably not nessesary but wanted to be safe)
        }

        private static void CleanUpNetResources() //Used to clean up any network resources on server shutdown
        {
            if (server != null)
            {
                if (connectedPlayer.tcpClient != null)
                {
                    connectedPlayer.tcpClient.Close();
                    connectedPlayer.tcpClient = null;
                    connectedPlayer.playerName = "";
                }
                server.Stop();
                server = null;
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

        private static void CheckMessages()
        {
            if (!IsDisconnected(connectedPlayer.tcpClient))
            {
                int msgLength = connectedPlayer.tcpClient.Available;

                if (msgLength > 0)
                {
                    byte[] msgBuffer = new byte[msgLength];
                    connectedPlayer.tcpClient.GetStream().Read(msgBuffer, 0, msgLength);
                    string msg = Encoding.UTF8.GetString(msgBuffer);
                    string[] splitMsg = msg.Split('|');

                    switch(splitMsg[0])
                    {
                        case "CHAT":
                            chatLog.Add(connectedPlayer.playerName + "> " + splitMsg[1]);
                            if (chatLog.Count >= chatlogLimit)
                            {
                                chatLog.RemoveAt(0);
                            }
                            break;
                        case "MOVE":
                            if(!isServerTurn)
                            {
                                Timer.StopTimer(); //stops timer and sets clients move
                                clientsMove = byte.Parse(splitMsg[1]);
                            }
                            break;

                    }
                    
                }
            }
            
        }

        private static void SendMessage(string msg)
        {
            try
            {
                byte[] msgBuffer = Encoding.UTF8.GetBytes(msg); //Encode string to byte array to be sent
                connectedPlayer.tcpClient.GetStream().Write(msgBuffer, 0, msgBuffer.Length); //sends byte array to client
            }
            catch (IOException exeption)
            {
                Shutdown();
            }
        }
    } 

    struct gameClient //Used to store TCPclient and name for other player.
    {
        public TcpClient tcpClient;
        public string playerName;
    }
}
