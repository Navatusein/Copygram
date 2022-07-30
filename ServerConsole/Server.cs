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
        private TcpListener listener = null!;
        private BinaryFormatter binaryFormatter = null!;
        private Semaphore semaphore = null!;

        private int usersMax = 10;

        private bool serverWork = true;

        private Dictionary<int, Client> clients = null!;

        public Server()
        {
            binaryFormatter = new();
            semaphore = new(usersMax, usersMax);
        }

        public void StartServer()
        {
            clients = new();

            listener = new(IPAddress.Parse("192.168.1.100"), 27015);
            listener.Start();

            serverWork = true;

            //DbConnector.Test();

            //Task.Run(() => UnavailableCollector());

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
            while (serverWork)
            {
                semaphore.WaitOne();

                Task.Run(() => WaitingForRequest());
            }
        }

        private void WaitingForRequest()
        {
            try
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                NetworkStream stream = tcpClient.GetStream();
                StreamReader reader = new(stream, Encoding.UTF8);

                Command command = (Command)binaryFormatter.Deserialize(reader.BaseStream);

                if (command == null)
                {
                    tcpClient.Close();
                    semaphore.Release();
                    return;
                }
                string debugNickname = command.User == null ? "unknown" : command.User.Nickname;
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {debugNickname}: Get command {command.Type}");

                if (command.Type == CommandType.RequestChanges)
                {
                    if (!clients.ContainsKey(command.User!.UserId))
                    {
                        SendError(stream, KnownErrors.OutOfSync, command);
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

                    SendResponse(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.NewChatMessage)
                {
                    if (!clients.ContainsKey(command.User!.UserId))
                    {
                        SendError(stream, KnownErrors.OutOfSync, command);
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

                    SendResponse(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.NewSystemChatMessage)
                {
                    if (!clients.ContainsKey(command.User!.UserId))
                    {
                        SendError(stream, KnownErrors.OutOfSync, command);
                        return;
                    }

                    TCP.IMessage message = Deserialization<TCP.IMessage>(command.Data);

                    TCP.SystemChatMessage? systemChatMessage = message as TCP.SystemChatMessage;

                    if (systemChatMessage == null)
                    {
                        semaphore.Release();
                        return;
                    }

                    TCP.Chat tcpChat = systemChatMessage.Chat;

                    if (systemChatMessage.SystemMessageType == SystemChatMessageType.NewChat)
                    {
                        DbConnector.RegisterNewChat(ref tcpChat);

                        byte[] data = Serialization(tcpChat);

                        SendResponse(stream, ResponseType.Success, data, command);
                    }
                    else if (systemChatMessage.SystemMessageType == SystemChatMessageType.UpdateChat)
                    {
                        DbConnector.UpdateChat(tcpChat);
                    }
                    else
                    {
                        SendError(stream, KnownErrors.UnknownCommandArguments, command, "Unknown SystemMessageType");
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
                else if (command.Type == CommandType.Sync)
                {
                    if (clients.ContainsKey(command.User!.UserId))
                    {
                        SendError(stream, KnownErrors.SecondClient, command);
                        return;
                    }

                    Client client = new(command.User);
                    client.LastRequest = DateTime.Now;

                    clients.Add(client.User.UserId, client);

                    var test = DbConnector.SyncChats(command.User);

                    byte[] data = Serialization(test);

                    SendResponse(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.SyncChatMessage)
                {
                    if (!clients.ContainsKey(command.User!.UserId))
                    {
                        SendError(stream, KnownErrors.OutOfSync, command);
                        return;
                    }

                    Client client = clients[command.User.UserId];
                    client.LastRequest = DateTime.Now;

                    TCP.SyncChatMessages syncChatMessages = Deserialization<TCP.SyncChatMessages>(command.Data);

                    var a = DbConnector.SyncChatMessages(syncChatMessages);

                    byte[] data = Serialization(a);

                    SendResponse(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.CheckForUser)
                {
                    if (!clients.ContainsKey(command.User!.UserId))
                    {
                        SendError(stream, KnownErrors.OutOfSync, command);
                        return;
                    }

                    string nicknameForCheck = Encoding.UTF8.GetString(command.Data);

                    TCP.User? user = DbConnector.CheckForUser(nicknameForCheck);

                    Client client = clients[command.User.UserId];
                    client.LastRequest = DateTime.Now;

                    if (user == null)
                    {
                        SendError(stream, KnownErrors.UnknownUser, command);
                        return;
                    }

                    byte[] data = Serialization(user);

                    SendResponse(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.Login)
                {
                    TCP.LoginData loginData = Deserialization<TCP.LoginData>(command.Data);

                    TCP.User? tcpUser = DbConnector.TryToLogin(loginData);

                    if (tcpUser == null)
                    {
                        SendError(stream, KnownErrors.BadPasswordOrLogin, command);
                        return;
                    }
                    else
                    {
                        byte[] data = Serialization(tcpUser);

                        SendResponse(stream, ResponseType.Success, data, command);
                    }
                }
                else if (command.Type == CommandType.Register)
                {
                    TCP.LoginData loginData = Deserialization<TCP.LoginData>(command.Data);
                    TCP.User tcpUser = loginData.User;

                    if (!DbConnector.IsLoginAllowed(loginData.Login))
                    {
                        SendError(stream, KnownErrors.LoginBusy, command);
                        return;
                    }

                    if (!DbConnector.IsNicknameAllowed(tcpUser.Nickname))
                    {
                        SendError(stream, KnownErrors.NicknameBusy, command);
                        return;
                    }

                    DbConnector.RegisterNewUser(ref tcpUser, loginData.Login, loginData.Password);

                    byte[]  data = Serialization(tcpUser);

                    SendResponse(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.Disconnect)
                {
                    if (command.User == null)
                    {
                        SendError(stream, KnownErrors.OutOfSync, command);
                        return;
                    }

                    if (!clients.ContainsKey(command.User.UserId))
                    {
                        SendError(stream, KnownErrors.OutOfSync, command);
                        return;
                    }

                    clients.Remove(command.User.UserId);

                    SendResponse(stream, ResponseType.Success, null!, command);
                }
                else
                {
                    SendError(stream, KnownErrors.UnknownCommand, command);
                    return;
                }

                semaphore.Release();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

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

        private void SendError(NetworkStream stream, KnownErrors knownErrors, Command command, string message = "")
        {
            TCP.Error error = new();

            error.Type = knownErrors;
            error.Text = message;

            byte[] data = Serialization(error);

            SendResponse(stream, ResponseType.Error, data, command);

            semaphore.Release();
        }

        private void SendResponse(NetworkStream stream, ResponseType type, byte[] data, Command command)
        {
            string debugNickname = command.User == null ? "unknown" : command.User.Nickname;
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {debugNickname}: Send response on command {command.Type}, {type}");

            TCP.Response response = new();
            response.Type = type;
            response.Data = data;
            response.OnCommandResponse = command;

            byte[] serializedObject = Serialization(response);

            stream.Write(serializedObject, 0, serializedObject.Length);
            stream.Flush();
        }
    }
}
