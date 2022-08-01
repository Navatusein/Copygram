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

        public void StartServer()
        {
            clients = new();

            listener = new(IPAddress.Parse("192.168.1.100"), 27015);
            listener.Start();

            serverWork = true;

            Task.Run(() => UnavailableCollector());

            WaitingForConnection();
        }

        private void UnavailableCollector()
        {
            while (serverWork)
            {
                Thread.Sleep(10000);

                List<int> unavailableClientsId = clients.Where(x => (DateTime.Now - x.Value.LastRequest).TotalSeconds >= 15)
                    .Select(x => x.Key)
                    .ToList();

                foreach(var value in unavailableClientsId)
                {
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Remove client {clients[value].User.Nickname}");
                    clients.Remove(value);
                }
            }
        }

        private void WaitingForConnection()
        {
            Console.WriteLine("Server started");

            while (serverWork)
            {
                semaphore.WaitOne();

                Task.Run(() => WaitingForRequest());
            }
        }

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
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {debugNickname}: Get request {command.Type}");


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

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"[{DateTime.Now.TimeOfDay}] Error: {ex.ToString}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        #endregion

        #region Response Processing

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
                client.Changes.Clear();

            SendResponse(netStream, ResponseType.Success, data, command);
        }

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
                if (clients.ContainsKey(user.UserId))
                {
                    clients[user.UserId].Changes.Append(message);
                }
            }
        }

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

            byte[] data = Serialization(DbConnector.SyncChats(command.User));
            SendResponse(netStream, ResponseType.Success, data, command);
        }

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

            byte[] data = Serialization(DbConnector.SyncChatMessages(syncChatMessages));
            SendResponse(netStream, ResponseType.Success, data, command);
        }

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

        private T Deserialization<T>(byte[] data)
        {
            T DeserializedObject;

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                DeserializedObject = (T)binaryFormatter.Deserialize(memoryStream);
            }

            return DeserializedObject;
        }

        private void SendError(NetworkStream netStream, KnownErrors knownErrors, Command command, string message = "")
        {
            TCP.Error error = new();

            error.Type = knownErrors;
            error.Text = message;

            byte[] data = Serialization(error);

            string debugNickname = command.User == null ? "unknown" : command.User.Nickname;

            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {debugNickname}: Send error {error.Type}, {error.Text}");

            SendResponse(netStream, ResponseType.Error, data, command);
        }

        private void SendResponse(NetworkStream netStream, ResponseType type, byte[] data, Command command)
        {
            if (type == ResponseType.Success)
            {
                string debugNickname = command.User == null ? "unknown" : command.User.Nickname;

                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {debugNickname}: Send response {command.Type}");
            }
 
            TCP.Response response = new();
            response.Type = type;
            response.Data = data;
            response.OnCommandResponse = command;

            byte[] serializedObject = Serialization(response);

            netStream.Write(serializedObject, 0, serializedObject.Length);
            netStream.Flush();
        }

        #endregion
    }
}
