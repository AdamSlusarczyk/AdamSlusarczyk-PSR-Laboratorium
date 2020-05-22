using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCosmosPolice
{
    class Criminal
    {
        [JsonProperty(PropertyName = "id")]
        public string id;
        public string name;
        public string secondName;
        public int age;
        public CrimeEnum crime;
        public Sentence sentence;

        public Criminal()
        {

        }

        public Criminal(string id, string name, string secondName, int age, CrimeEnum crime, Sentence sentence)
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
            return id + ". " + name + " " + secondName + ". Wiek: " + age + ". Zatrzymany za: " + crime + ". Wyrok: " + sentence.GetDescription();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    
    public enum CrimeEnum
    {
        Napad_na_bank = 1, Kradzież = 2, Morderstwo = 3
    }
}       
