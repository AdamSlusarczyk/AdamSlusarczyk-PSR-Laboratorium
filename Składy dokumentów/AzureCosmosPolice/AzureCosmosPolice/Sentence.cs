using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCosmosPolice
{
    class Sentence
    {
        public int value;
        public bool isJailSentence;

        public Sentence()
        {

        }
        public Sentence(bool isJailSentence, int value)
        {
            this.isJailSentence = isJailSentence;
            this.value = value;
        }

        public string GetDescription()
        {
            if (isJailSentence == true)
            {
                return "" + value + " lat więzienia.";
            }
            else
            {
                return "mandat wysokości " + value + "zł.";
            }

        }
    }
}
