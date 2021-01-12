using ProtoBuf;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Margatroid.Alice
{
    static class MessageHelper
    {
        public static byte[] SerializeMessage(Message message)
        {
            using var streamBuffer = new MemoryStream();
            Serializer.Serialize(streamBuffer, message);
            return streamBuffer.ToArray();
        }

        public static Message DeserializeMessage(byte[] data)
        {
            using var streamBuffer = new MemoryStream(data);
            return Serializer.Deserialize<Message>(streamBuffer);
        }

        public static async Task<Message> ReceiveMessage(UdpClient udpClient)
        {
            var receivedResult = await udpClient.ReceiveAsync();
            return DeserializeMessage(receivedResult.Buffer);
        }

        public static async Task SendMessage(UdpClient udpClient, Message message, IPEndPoint remote)
        {
            var data = SerializeMessage(message);
            await udpClient.SendAsync(data, data.Length, remote);
        }
    }
}
