using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jLibrary
{
    class Book
    {
        public int Id
        {
            get;set;
        }

        public string Title
        {
            get;set;
        }

        public string AuthorName
        {
            get; set;
        }

        public string AuthorSurname
        {
            get; set;
        }

        public Book()
        {

        }

        public Book(int id, string title, string authorName, string authorSurname)
        {
            Id = id;
            Title = title;
            AuthorName = authorName;
            AuthorSurname = authorSurname;
        }

        public string GetInfo()
        {
            return Id + ". " + Title + " - " + AuthorName + " " + AuthorSurname;
        }
    }
}
