using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HazelcastZoo
{
    [Serializable]
    class ZooKeeper
    {
        private const int ClassID = 100;

        String name;
        String lastName;

        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                    name = value;
            }
        }
        public String LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                if (lastName != value)
                    lastName = value;
            }
        }
        public String Signature
        {
            get { return "" + Name + " " + LastName; }
        }
        public ZooKeeper(String name, String lastName)
        {
            this.name = name;
            this.lastName = lastName;
        }
    }
}
