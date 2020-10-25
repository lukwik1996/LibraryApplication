using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommonData.Models
{
    [Serializable]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Login { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
