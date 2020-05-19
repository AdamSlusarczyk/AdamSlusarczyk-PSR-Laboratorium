using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongodbPolice
{
    class Criminal
    {
        [BsonElement("id")]
        public int id;
        [BsonElement("name")]
        public string name;
        [BsonElement("secondName")]
        public string secondName;
        [BsonElement("age")]
        public int age;
        [BsonElement("crime")]
        public CrimeEnum crime;
        [BsonElement("sentence")]
        public ISentence sentence;

        public Criminal()
        {

        }

        public Criminal(int id, string name,string secondName,int age, CrimeEnum crime, ISentence sentence)
        {
            this.id = id;
            this.name = name;
            this.secondName = secondName;
            this.age = age;
            this.crime = crime;
            this.sentence = sentence;
        }

        public String GetDescription()
        {
            return id +". " + name + " " + secondName + ". Wiek: " + age + ". Zatrzymany za: " + crime + ". Wyrok: " + sentence.GetDescription();
        }
    }

    public enum CrimeEnum
    {
        Napad_na_bank = 1, Kradzież = 2, Morderstwo = 3
    }
}
