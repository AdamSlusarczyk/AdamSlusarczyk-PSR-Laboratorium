using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongodbPolice
{
    [BsonDiscriminator("Fine")]
    class Fine : ISentence
    {
        [BsonElement("ticketValue")]
        public int ticketValue;

        public Fine()
        {

        }
        public Fine(int ticketValue)
        {
            this.ticketValue = ticketValue;
        }
        public string GetDescription()
        {
            return "mandat wysokości " + ticketValue + "zł.";
        }

    }
}
