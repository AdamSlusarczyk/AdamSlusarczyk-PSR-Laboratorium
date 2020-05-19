using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongodbPolice
{
    [BsonDiscriminator("JailSentence")]
    class JailSentence : ISentence
    {
        [BsonElement("length")]
        public int length;

        public JailSentence()
        {

        }

        public JailSentence(int length)
        {
            this.length = length;
        }


        public string GetDescription()
        {
            return "" + length + " lat więzienia.";
        }
    }
}
