using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Generic;

namespace MongoDBProject
{
    /// <summary>
    /// Clase que extiende la funcionalidad de Mongo
    /// </summary>
    public static class MongoExtensions
    {
        /// <summary>
        /// Convierte una lista en formato BsonDocument a una lista de objetos tipados
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bsonList"></param>
        /// <returns></returns>
        public static List<T> GetObjectList<T>(List<BsonDocument> bsonList) where T : new()
        {
            List<T> objectList = new List<T>();
            bsonList.ForEach(d =>
            {
                objectList.Add(BsonSerializer.Deserialize<T>(d));
            });

            return objectList;
        }

        /// <summary>
        /// Convierte una lista de objetos a una lista en formato BsonDocument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectList"></param>
        /// <returns></returns>
        public static List<BsonDocument> GetBsonDocumentList<T>(List<T> objectList)
        {
            List<BsonDocument> bsonList = new List<BsonDocument>();
            objectList.ForEach(o =>
            {
                IMongoModel entity = (IMongoModel)o;
                bsonList.Add(entity.ToBsonDocument());
            });

            return bsonList;
        }

        /// <summary>
        /// Obtiene la lista de ids una lista de objetos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectList"></param>
        /// <returns></returns>
        public static List<ObjectId> GetObjectIdList<T>(List<T> objectList)
        {
            List<ObjectId> bsonIdList = new List<ObjectId>();
            objectList.ForEach(o =>
            {
                IMongoModel entity = (IMongoModel)o;
                bsonIdList.Add(entity.id);
            });

            return bsonIdList;
        }

        /// <summary>
        /// Renderiza un filtro a su formato BsonDocument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static BsonDocument RenderToBsonDocument<T>(this FilterDefinition<T> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            return filter.Render(documentSerializer, serializerRegistry);
        }
    }
}
