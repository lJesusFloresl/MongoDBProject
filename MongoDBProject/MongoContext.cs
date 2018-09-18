using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace MongoDBProject
{
    /// <summary>
    /// Clase que manipula el contexto de una base de datos de mongo
    /// </summary>
    public class MongoContext
    {
        #region Propiedades

        protected MongoClient Client { get; set; }
        protected IMongoDatabase Database { get; set; }

        #endregion

        #region Constructor

        public MongoContext(string connectionString, string database)
        {
            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase(database);
        }

        public MongoContext(string database, string username, string password, int port, string host)
        {
            var credential = MongoCredential.CreateCredential(database, username, password);
            var settings = new MongoClientSettings
            {
                Credential = credential,
                Server = new MongoServerAddress(host, Convert.ToInt32(port))
            };

            Client = new MongoClient(settings);
            Database = Client.GetDatabase(database);
        }

        #endregion

        #region Metodos Generales

        /// <summary>
        /// Prepara un documento para su uso
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public IMongoCollection<BsonDocument> UseTable(string tableName)
        {
            return Database.GetCollection<BsonDocument>(tableName);
        }

        /// <summary>
        /// Inserta o actualiza en una tabla un registro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        public MongoResponse Save<T>(IMongoCollection<BsonDocument> table, T entity)
        {
            IMongoModel mongoEntity = (IMongoModel)entity;
            if (mongoEntity.id != new ObjectId())
                return Update(table, entity);
            else
                return Insert(table, entity);
        }

        /// <summary>
        /// Inserta o actualiza en una tabla una lista de registros
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        public MongoResponse SaveMany<T>(IMongoCollection<BsonDocument> table, List<T> entity)
        {
            try
            {
                entity.ForEach(e =>
                    {
                        IMongoModel mongoEntity = (IMongoModel)e;
                        if (mongoEntity.id != new ObjectId())
                            Update(table, e);
                        else
                            Insert(table, e);
                    });

                return new MongoResponse(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new MongoResponse(false, ex.ToString());
            }
        }

        /// <summary>
        /// Inserta en una tabla un registro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        public MongoResponse Insert<T>(IMongoCollection<BsonDocument> table, T entity)
        {
            try
            {
                IMongoModel mongoEntity = (IMongoModel)entity;
                table.InsertOne(mongoEntity.ToBsonDocument());
                return new MongoResponse(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new MongoResponse(false, ex.ToString());
            }
        }

        /// <summary>
        /// Inserta en una tabla una lista de registros
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        public MongoResponse InsertMany<T>(IMongoCollection<BsonDocument> table, List<T> entity)
        {
            try
            {
                table.InsertMany(MongoExtensions.GetBsonDocumentList(entity));
                return new MongoResponse(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new MongoResponse(false, ex.ToString());
            }
        }

        /// <summary>
        /// Actualiza varios registros de la tabla asignada en base a su id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        public MongoResponse Update<T>(IMongoCollection<BsonDocument> table, T entity)
        {
            try
            {
                IMongoModel mongoEntity = (IMongoModel)entity;
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", mongoEntity.id);
                table.ReplaceOne(filter, mongoEntity.ToBsonDocument());
                return new MongoResponse(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new MongoResponse(false, ex.ToString());
            }
        }

        /// <summary>
        /// Actualiza un registro de la tabla asignada en base a su id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        public MongoResponse UpdateMany<T>(IMongoCollection<BsonDocument> table, List<T> entity)
        {
            try
            {
                entity.ForEach(e =>
                {
                    IMongoModel mongoEntity = (IMongoModel)e;
                    var builder = Builders<BsonDocument>.Filter;
                    var filter = builder.Eq("_id", mongoEntity.id);
                    table.ReplaceOne(filter, mongoEntity.ToBsonDocument());
                });
                return new MongoResponse(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new MongoResponse(false, ex.ToString());
            }
        }

        /// <summary>
        /// Elimina un registro de la tabla asignada en base a su id
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public MongoResponse Delete(IMongoCollection<BsonDocument> table, ObjectId id)
        {
            try
            {
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", id);
                table.DeleteOne(filter);
                return new MongoResponse(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new MongoResponse(false, ex.ToString());
            }
        }

        /// <summary>
        /// Elimina varios registros de la tabla asignada en base a su id
        /// </summary>
        /// <param name="table"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public MongoResponse DeleteMany(IMongoCollection<BsonDocument> table, List<ObjectId> ids)
        {
            try
            {
                ids.ForEach(id =>
                {
                    var builder = Builders<BsonDocument>.Filter;
                    var filter = builder.Eq("_id", id);
                    table.DeleteOne(filter);
                });

                return new MongoResponse(true, string.Empty);
            }
            catch (Exception ex)
            {
                return new MongoResponse(false, ex.ToString());
            }
        }

        /// <summary>
        /// Obtiene los registros de una tabla
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public List<T> GetAll<T>(IMongoCollection<BsonDocument> table) where T : new()
        {
            var recordCollecion = table.Find(_ => true).ToListAsync();
            recordCollecion.Wait();
            var objectList = MongoExtensions.GetObjectList<T>(recordCollecion.Result);
            return objectList;
        }

        /// <summary>
        /// Obtiene los registros de una tabla en base a los filtros enviados
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public List<T> GetAllWithFilter<T>(IMongoCollection<BsonDocument> table, FilterDefinition<T> filters) where T : new()
        {
            var query = filters.RenderToBsonDocument().ToJson();
            var recordCollecion = table.Find(query).ToListAsync();
            recordCollecion.Wait();
            var objectList = MongoExtensions.GetObjectList<T>(recordCollecion.Result);
            return objectList;
        }

        #endregion
    }
}
