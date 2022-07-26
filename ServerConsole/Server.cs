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

            WaitingForConnection();
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

                if (command.Type == CommandType.RequestChanges)
                {
                    byte[] data = null!;

                    if (!clients.ContainsKey(command.User.UserId))
                    {
                        TCP.Error error = new();
                        error.Type = KnownErrors.OutOfSync;

                        data = Serialization(error);

                        Response(stream, ResponseType.Error, data, command);

                        semaphore.Release();
                        return;
                    }

                    Client client = clients[command.User.UserId];

                    data = Serialization(client.Changes);

                    Response(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.NewMessage)
                {
                    byte[] data = null!;

                    if (!clients.ContainsKey(command.User.UserId))
                    {
                        TCP.Error error = new();
                        error.Type = KnownErrors.OutOfSync;

                        data = Serialization(error);

                        Response(stream, ResponseType.Error, data, command);

                        semaphore.Release();
                        return;
                    }
                }
                else if (command.Type == CommandType.Sync)
                {
                    byte[] data = null!;

                    if (clients.ContainsKey(command.User.UserId))
                    {
                        TCP.Error error = new();
                        error.Type = KnownErrors.SecondClient;

                        data = Serialization(error);

                        Response(stream, ResponseType.Error, data, command);

                        semaphore.Release();
                        return;
                    }

                    data = Serialization(DbConnector.SyncChats(command.User));

                    Response(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.SyncChatMessage)
                {
                    byte[] data = null!;

                    if (!clients.ContainsKey(command.User.UserId))
                    {
                        TCP.Error error = new();
                        error.Type = KnownErrors.OutOfSync;

                        data = Serialization(error);

                        Response(stream, ResponseType.Error, data, command);

                        semaphore.Release();
                        return;
                    }

                    TCP.SyncChatMessages syncChatMessages = Deserialization<TCP.SyncChatMessages>(command.Data);

                    data = Serialization(DbConnector.SyncChatMessages(syncChatMessages));

                    Response(stream, ResponseType.Success, data, command);
                }
                else if (command.Type == CommandType.Login)
                {
                    TCP.LoginData loginData = Deserialization<TCP.LoginData>(command.Data);

                    TCP.User? tcpUser = DbConnector.TryToLogin(loginData);

                    if (tcpUser == null)
                    {
                        TCP.Error error = new();

                        error.Type = KnownErrors.BadPasswordOrLogin;

                        byte[] data = Serialization(error);

                        Response(stream, ResponseType.Error, data, command);;
                    }
                    else
                    {
                        byte[] data = Serialization(tcpUser);

                        Response(stream, ResponseType.Success, data, command);
                    }
                }
                else if (command.Type == CommandType.Register)
                {
                    TCP.LoginData loginData = Deserialization<TCP.LoginData>(command.Data);
                    TCP.User tcpUser = loginData.User;

                    byte[] data = null;

                    if (!DbConnector.IsLoginAllowed(loginData.Login))
                    {
                        TCP.Error error = new();

                        error.Type = KnownErrors.LoginBusy;

                        data = Serialization(error);

                        Response(stream, ResponseType.Error, data, command);

                        semaphore.Release();
                        return;
                    }

                    if (!DbConnector.IsNicknameAllowed(tcpUser.Nickname))
                    {
                        TCP.Error error = new();

                        error.Type = KnownErrors.NicknameBusy;

                        data = Serialization(error);

                        Response(stream, ResponseType.Error, data, command);

                        semaphore.Release();
                        return;
                    }

                    DbConnector.RegisterUser(ref tcpUser, loginData.Login, loginData.Password);

                    data = Serialization(tcpUser);

                    Response(stream, ResponseType.Success, data, command);
                }
                else
                {
                    TCP.Error error = new();
                    error.Type = KnownErrors.SecondClient;
                    error.Text = "Unknown command";

                    byte[] data = Serialization(error);

                    Response(stream, ResponseType.Error, data, command);
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

        private void Response(NetworkStream stream, ResponseType type, byte[] data, Command command)
        {
            TCP.Response response = new();
            response.Type = type;
            response.Data = data;
            response.OnCommandResponse = command;

            byte[] serializedObject = Serialization(response);

            stream.Write(serializedObject, 0, serializedObject.Length);
        }
    }
}
