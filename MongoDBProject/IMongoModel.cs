using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoDBProject
{
    /// <summary>
    /// Interfaz que contiene los metodos que todos los modelos deben implementar
    /// </summary>
    public interface IMongoModel
    {
        /// <summary>
        /// Propiedad que debe tener todo modelo
        /// </summary>
        [BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
        ObjectId id { get; set; }

        /// <summary>
        /// Convierte un objeto T a su formato BsonDocument
        /// </summary>
        /// <returns></returns>
        BsonDocument ToBsonDocument();
    }
}
