using MsgPack.Serialization;

namespace ICM.Common.Kafka
{
    public class MessageSerializer
    {
        public byte[] Serialize<T>(T model)
        {
            var ctx = new SerializationContext() { SerializationMethod = SerializationMethod.Map };
            var serializer = MessagePackSerializer.Get<T>(ctx);
            return serializer.PackSingleObject(model);
        }

        public T Deserialize<T>(byte[] message)
        {
            var ctx = new SerializationContext() { SerializationMethod = SerializationMethod.Map };
            var serializer = MessagePackSerializer.Get<T>(ctx);
            return serializer.UnpackSingleObject(message);
        }
    }
}
