using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Model
{
    public class EmailTemplate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Key { get; set; }

        public string Description { get; set; }
        
        public string Language { get; set; }
        
        public string Subject { get; set; }
        
        public BsonDocument BodyJson { get; set; }

        public string BodyHtml { get; set; }

        public BsonDocument BodyParameters { get; set; }

        #region MetaData

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        #endregion
    }
}
