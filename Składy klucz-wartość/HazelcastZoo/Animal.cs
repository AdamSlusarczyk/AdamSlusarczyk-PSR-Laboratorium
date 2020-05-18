using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HazelcastZoo
{
    [Serializable]
    class Animal 
    {
        String name;
        String species;
        int annexNo;
        int keeperId;

        public String Name
        {
            set
            {
                if (name != value)
                    name = value;
            }
            get
            {
                return name;
            }

        }
        public String Species
        {
            set
            {
                if (species != value)
                    species = value;
            }
            get
            {
                return species;
            }
        }
        public int KeeperId
        {
            get
            {
                return keeperId;
            }
            set
            {
                if (keeperId != value)
                    keeperId = value;
            }
        }
        public String Signature
        {
            get
            {
                return "" + species + " " + name + " z pawilonu numer " + annexNo + ". ID opiekuna: " + keeperId;
            }
           
        }
        public int AnnexNo
        {
            set
            {
                if (annexNo != value)
                    annexNo = value;
            }
            get
            {
                return annexNo;
            }
        }
        public Animal(String name, String species, int annexNo, int keeperId)
        {
            this.name = name;
            this.species = species;
            this.annexNo = annexNo;
            this.keeperId = keeperId;
        }          
    }
}
