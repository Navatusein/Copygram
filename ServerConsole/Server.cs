using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ModelsLibrary;

using TCP = ModelsLibrary;
using DB = ServerConsole.Models;
using Microsoft.Extensions.Configuration;

#pragma warning disable SYSLIB0011

namespace ServerConsole
{
    internal class Server
    {
        #region Properties

        private TcpListener listener = null!;
        private BinaryFormatter binaryFormatter = null!;
        private Semaphore semaphore = null!;

        private int usersMax = 10;

        private bool serverWork = true;

        private Dictionary<int, Client> clients = null!;

        #endregion

        public Server()
        {
            binaryFormatter = new();
            semaphore = new(usersMax, usersMax);
        }

        #region Server Logic
        /// <summary>
        /// Server start function.
        /// </summary>
        public void StartServer()
        {
            clients = new();

            var configuration = new ConfigurationBuilder().AddJsonFile("Config.json").Build();

            listener = new(IPAddress.Parse(configuration["ServerIp"]), int.Parse(configuration["ServerPort"]));
            listener.Start();

            serverWork = true;

            Thread thread = new Thread(UnavailableClientCollector);
            thread.IsBackground = true;
            thread.Start();

            WaitingForConnection();
        }

        /// <summary>
        /// A feature to collect and remove non-responding clients.
        /// </summary>
        private void UnavailableClientCollector()
        {
            ConsoleWriter.WriteMessage("Unavailable Client Collector: Started");

            while (serverWork)
            {
                Thread.Sleep(10000);

                List<int> unavailableClientsId = clients.Where(x => (DateTime.Now - x.Value.LastRequest).TotalSeconds >= 15)
                    .Select(x => x.Key)
                    .ToList();

                foreach(var value in unavailableClientsId)
                {
                    ConsoleWriter.WriteMessage($"Remove client {clients[value].User.Nickname}");
                    clients.Remove(value);
                }
            }
        }

        /// <summary>
        /// A function that creates threads to wait for a connection.
        /// </summary>
        private void WaitingForConnection()
        {
            ConsoleWriter.WriteMessage("Server: Started");

            while (serverWork)
            {
                semaphore.WaitOne();

                Thread thread = new Thread(WaitingForRequest);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        /// <summary>
        /// A function to wait for users to connect.
        /// </summary>
        private void WaitingForRequest()
        {
            NetworkStream netStream = null!;
            Command command = null!;
            try
            {
                TcpClient tcpClient = listener.AcceptTcpClient();

                netStream = tcpClient.GetStream();

                StreamReader reader = new(netStream, Encoding.UTF8);

                command = (Command)binaryFormatter.Deserialize(reader.BaseStream);

                if (command == null)
                {
                    SendError(netStream, KnownErrors.UnknownCommand, command, "Command is null");
                    semaphore.Release();
                    return;
                }

                string debugNickname = command.User == null ? "unknown" : command.User.Nickname;
                ConsoleWriter.WriteMessage($"{debugNickname}: Get request {command.Type}");

                switch (command.Type) 
                {
                    case CommandType.Login:
                        ResponseLogin(netStream, command);
                        break;

                    case CommandType.Register:
                        ResponseRegister(netStream, command);
                        break;

                    case CommandType.Sync:
                        ResponseSync(netStream, command);
                        break;

                    case CommandType.SyncChatMessage:
                        ResponseSyncChatMessage(netStream, command);
                        break;

                    case CommandType.RequestChanges:
                        ResponseRequestChanges(netStream, command);
                        break;

                    case CommandType.CheckForUser:
                        ResponseCheckForUser(netStream, command);
                        break;

                    case CommandType.NewChatMessage:
                        ResponseNewChatMessage(netStream, command);
                        break;

                    case CommandType.NewSystemChatMessage:
                        ResponseNewSystemChatMessage(netStream, command);
                        break;

                    case CommandType.Disconnect:
                        ResponseDisconnect(netStream, command);
                        break;

                    default:
                        SendError(netStream, KnownErrors.UnknownCommand, command);
                        break;
                }

                semaphore.Release();
            }
            catch (Exception ex)
            {
                SendError(netStream, KnownErrors.ProcessingError, command);
                ConsoleWriter.WriteError("WaitingForRequest", ex.ToString());
            }
        }

        #endregion

        #region Response Processing
        /// <summary>
        /// Function for handling Request Changes.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseRequestChanges(NetworkStream netStream, Command command)
        {
            if (!clients.ContainsKey(command.User!.UserId))
            {
                SendError(netStream, KnownErrors.OutOfSync, command);
                return;
            }

            Client client = clients[command.User.UserId];
            client.LastRequest = DateTime.Now;

            byte[] data = Serialization(client.Changes);

            if (client.Changes.Count > 0)
            {
                Console.WriteLine("Work");
                client.Changes.Clear();
            }

            SendResponse(netStream, ResponseType.Success, data, command);
        }

        /// <summary>
        /// Function for handling New Chat Message.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseNewChatMessage(NetworkStream netStream, Command command)
        {
            if (!clients.ContainsKey(command.User!.UserId))
            {
                SendError(netStream, KnownErrors.OutOfSync, command);
                return;
            }

            TCP.IMessage message = Deserialization<TCP.IMessage>(command.Data);

            TCP.ChatMessage? tcpChatMessage = message as TCP.ChatMessage;

            if (tcpChatMessage == null)
            {
                semaphore.Release();
                return;
            }

            Client client = clients[command.User.UserId];
            client.LastRequest = DateTime.Now;

            List<TCP.User> users = DbConnector.GetUsersFromChat(tcpChatMessage.ChatId);

            foreach (TCP.User user in users)
            {
                if (clients.ContainsKey(user.UserId) && user.UserId != command.User.UserId)
                {
                    clients[user.UserId].Changes.Add(message);
                }
            }

            DbConnector.RegisterNewMessage(ref tcpChatMessage);

            byte[] data = Serialization(tcpChatMessage);
            SendResponse(netStream, ResponseType.Success, data, command);
        }

        /// <summary>
        /// Function for handling Request Changes.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseNewSystemChatMessage(NetworkStream netStream, Command command)
        {
            if (!clients.ContainsKey(command.User!.UserId))
            {
                SendError(netStream, KnownErrors.OutOfSync, command);
                return;
            }

            TCP.IMessage message = Deserialization<TCP.IMessage>(command.Data);

            TCP.SystemChatMessage? systemChatMessage = message as TCP.SystemChatMessage;

            if (systemChatMessage == null)
            {
                SendError(netStream, KnownErrors.UnknownCommandArguments, command, "Unknown data");
                return;
            }

            TCP.Chat tcpChat = systemChatMessage.Chat;

            if (systemChatMessage.SystemMessageType == SystemChatMessageType.NewChat)
            {
                DbConnector.RegisterNewChat(ref tcpChat);

                byte[] data = Serialization(tcpChat);
                SendResponse(netStream, ResponseType.Success, data, command);
            }
            else if (systemChatMessage.SystemMessageType == SystemChatMessageType.UpdateChat)
            {
                DbConnector.UpdateChat(tcpChat);
            }
            else
            {
                SendError(netStream, KnownErrors.UnknownCommandArguments, command, "Unknown SystemMessageType");
                return;
            }

            Client client = clients[command.User.UserId];
            client.LastRequest = DateTime.Now;

            List<TCP.User> users = DbConnector.GetUsersFromChat(tcpChat.ChatId);

            foreach (TCP.User user in users)
            {
                if (clients.ContainsKey(user.UserId) && user.UserId != command.User.UserId)
                {
                    clients[user.UserId].Changes.Add(message);
                }
            }
        }

        /// <summary>
        /// Function for handling Sync.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseSync(NetworkStream netStream, Command command)
        {
            if (clients.ContainsKey(command.User!.UserId))
            {
                SendError(netStream, KnownErrors.SecondClient, command);
                return;
            }

            Client client = new(command.User);
            client.LastRequest = DateTime.Now;

            clients.Add(client.User.UserId, client);

            var syncData = DbConnector.SyncChats(command.User);

            byte[] data = Serialization(syncData);
            SendResponse(netStream, ResponseType.Success, data, command);
        }

        /// <summary>
        /// Function for handling Sync Chat Message.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseSyncChatMessage(NetworkStream netStream, Command command)
        {
            if (!clients.ContainsKey(command.User!.UserId))
            {
                SendError(netStream, KnownErrors.OutOfSync, command);
                return;
            }

            Client client = clients[command.User.UserId];
            client.LastRequest = DateTime.Now;

            TCP.SyncChatMessages syncChatMessages = Deserialization<TCP.SyncChatMessages>(command.Data);

            var syncData = DbConnector.SyncChatMessages(syncChatMessages);
            byte[] data = Serialization(syncData);
            SendResponse(netStream, ResponseType.Success, data, command);
        }

        /// <summary>
        /// Function for handling Check For User.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseCheckForUser(NetworkStream netStream, Command command)
        {
            if (!clients.ContainsKey(command.User!.UserId))
            {
                SendError(netStream, KnownErrors.OutOfSync, command);
                return;
            }

            string nicknameForCheck = Deserialization<string>(command.Data);

            TCP.User? user = DbConnector.CheckForUser(nicknameForCheck);

            Client client = clients[command.User.UserId];
            client.LastRequest = DateTime.Now;

            if (user == null)
            {
                SendError(netStream, KnownErrors.UnknownUser, command);
                return;
            }

            byte[] data = Serialization(user);
            SendResponse(netStream, ResponseType.Success, data, command);
        }

        /// <summary>
        /// Function for handling Login.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseLogin(NetworkStream netStream, Command command)
        {
            TCP.LoginData loginData = Deserialization<TCP.LoginData>(command.Data);

            TCP.User? tcpUser = DbConnector.TryToLogin(loginData);

            if (tcpUser == null)
            {
                SendError(netStream, KnownErrors.BadPasswordOrLogin, command);
                return;
            }
            else
            {
                byte[] data = Serialization(tcpUser);
                SendResponse(netStream, ResponseType.Success, data, command);
            }
        }

        /// <summary>
        /// Function for handling Register.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseRegister(NetworkStream netStream, Command command)
        {
            TCP.LoginData loginData = Deserialization<TCP.LoginData>(command.Data);

            TCP.User tcpUser = loginData.User;

            if (!DbConnector.IsLoginAllowed(loginData.Login))
            {
                SendError(netStream, KnownErrors.LoginBusy, command);
                return;
            }

            if (!DbConnector.IsNicknameAllowed(tcpUser.Nickname))
            {
                SendError(netStream, KnownErrors.NicknameBusy, command);
                return;
            }

            DbConnector.RegisterNewUser(ref tcpUser, loginData.Login, loginData.Password);

            byte[] data = Serialization(tcpUser);

            SendResponse(netStream, ResponseType.Success, data, command);
        }

        /// <summary>
        /// Function for handling Disconnect.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="command">The command to be processed</param>
        private void ResponseDisconnect(NetworkStream netStream, Command command)
        {
            if (command.User == null)
                return;

            if (!clients.ContainsKey(command.User.UserId))
                return;

            clients.Remove(command.User.UserId);

            SendResponse(netStream, ResponseType.Success, null!, command);
        }

        #endregion

        #region Utility
        /// <summary>
        /// Function to sterilize an object.
        /// </summary>
        /// <typeparam name="T">Type of object to be sterilized</typeparam>
        /// <param name="obj">Object to be sterilized</param>
        /// <returns>Byte array of the sterilized object</returns>
        private byte[] Serialization<T>(T obj)
        {
            if (obj == null)
                return null!;

            byte[] serializedObject;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                serializedObject = memoryStream.ToArray();
            }

            return serializedObject;
        }

        /// <summary>
        /// A function to deserialize objects.
        /// </summary>
        /// <typeparam name="T">The type of the returned object</typeparam>
        /// <param name="data">Byte array of the sterilized object</param>
        /// <returns>Deserialize objects</returns>
        private T Deserialization<T>(byte[] data)
        {
            T DeserializedObject;

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                DeserializedObject = (T)binaryFormatter.Deserialize(memoryStream);
            }

            return DeserializedObject;
        }

        /// <summary>
        /// A function to send an error to the user.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="knownErrors">Error type</param>
        /// <param name="command">Command to be answered</param>
        /// <param name="message">Message text</param>
        private void SendError(NetworkStream netStream, KnownErrors knownErrors, Command? command, string message = "")
        {
            TCP.Error error = new();

            error.Type = knownErrors;
            error.Text = message;

            byte[] data = Serialization(error);

            if (command != null)
            {
                string debugNickname = command.User == null ? "unknown" : command.User.Nickname;
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {debugNickname}: Send error {error.Type}, {error.Text}");
            }
            
            SendResponse(netStream, ResponseType.Error, data, command);
        }

        /// <summary>
        /// A function to send a response to the user.
        /// </summary>
        /// <param name="netStream">User network stream</param>
        /// <param name="type">Result success or error</param>
        /// <param name="data">Byte data array</param>
        /// <param name="command">Command to be answered</param>
        private void SendResponse(NetworkStream netStream, ResponseType type, byte[] data, Command? command)
        {
            try
            {
                if (type == ResponseType.Success)
                {
                    string debugNickname = command!.User == null ? "unknown" : command.User.Nickname;
                    ConsoleWriter.WriteMessage($"{debugNickname}: Send response {command.Type}");
                }

                TCP.Response response = new();
                response.Type = type;
                response.Data = data;
                response.OnCommandResponse = command!;

                byte[] serializedObject = Serialization(response);

                netStream.Write(serializedObject, 0, serializedObject.Length);
                netStream.Flush();
            }
            catch (Exception ex)
            {
                ConsoleWriter.WriteError("SendResponse", ex.ToString());
            } 
        }
        #endregion
    }
}
