using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Db4oLeague
{
    class Player
    {
        int id;
        int teamId;
        string name;
        string surname;
        string teamName;

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                    id = value;
            }
        }

        public string Name
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
        public string Surname
        {
            get
            {
                return surname;
            }
            set
            {
                if (surname != value)
                    surname = value;
            }
        }
        public string TeamName
        {
            get
            {
                return teamName;
            }
            set
            {
                if (teamName != value)
                    teamName = value;
            }
        }

        public Player() { }
        public Player(int id, string name, string surname, string teamName)
        {
            Id = id;
            Name = name;
            Surname = surname;
            TeamName = teamName;
        }

        public string GetDescription()
        {
            return id + ". " + Name + " " + Surname + " - zawodnik drużyny " + TeamName;
        }
    }
}
