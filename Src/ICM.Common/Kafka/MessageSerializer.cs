using MsgPack.Serialization;

namespace ICM.Common.Kafka
{
    public class MessageSerializer
    {
        public byte[] Serialize<T>(T model)
        {
            var serializer = MessagePackSerializer.Get<T>();
            return serializer.PackSingleObject(model);
        }

        public T Deserialize<T>(byte[] message)
        {
            var serializer = MessagePackSerializer.Get<T>();
            return serializer.UnpackSingleObject(message);
        }
    }
}
