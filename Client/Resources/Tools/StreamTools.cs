using ModelsLibrary;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media.Imaging;

#pragma warning disable SYSLIB0011

namespace Client.Resources.Tools
{
    public static class StreamTools
    {
        /// <summary>
        /// Serializator
        /// </summary>
        /// <param name="obj">Data to serialize</param>
        public static byte[]? Serialize(object obj)
        {
            try
            {
                BinaryFormatter binFormatter = new();

                byte[]? result = null;

                using (MemoryStream memStream = new MemoryStream())
                {
                    binFormatter.Serialize(memStream, obj);
                    result = memStream.ToArray();
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace, "Serialize Error",
                        MessageBoxButton.OK);
                return null;
            }
        }

        /// <summary>
        /// Deserializator
        /// </summary>
        /// <param name="byteArray">Data to deserialize</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data)
        {

            if (data == null)
                throw new Exception("Argument could not be deserialized");

            BinaryFormatter binFormatter = new();

            T DeserializedObject;

            using (MemoryStream memStream = new MemoryStream(data))
            {
                DeserializedObject = (T)binFormatter.Deserialize(memStream);
            }

            return DeserializedObject;
        }

        /// <summary>
        /// Special converter for images
        /// </summary>
        /// <param name="data">image byte array</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage ToBitmapImage(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();

                if (img.CanFreeze)
                    img.Freeze();

                return img;
            }
        }

        public static Response NetworkGet(Stream stream)
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            return (Response)binFormatter.Deserialize(stream);
        }

        public static void NetworkSend(NetworkStream netStream, Command cmd)
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            binFormatter.Serialize(netStream, cmd);
            netStream.Flush();
        }

    }
}
