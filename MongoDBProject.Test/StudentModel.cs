using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Collections.Generic;

namespace MongoDBProject.Test
{
    public class StudentModel : IMongoModel
    {
        [BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
        public ObjectId id { get; set; }

        [BsonElement("firstname")]
        public string firstname { get; set; }

        [BsonElement("lastname")]
        public string lastname { get; set; }

        [BsonElement("subjects")]
        public List<string> subjects { get; set; }

        [BsonElement("class")]
        public string klass { get; set; }

        [BsonElement("age")]
        public int age { get; set; }

        public StudentModel()
        {
            this.subjects = new List<string>();
        }

        public BsonDocument ToBsonDocument()
        {
            return new BsonDocument
            {
                { "firstname", new BsonString(this.firstname) },
                { "lastname", new BsonString(this.lastname) },
                { "subjects", new BsonArray(this.subjects.ToArray()) },
                { "class", new BsonString(this.klass) },
                { "age", new BsonInt32(this.age) }
            };
        }
    }
}
