using Microsoft.SqlServer.Server;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jLibrary
{
    class LibraryClient
    {
        static GraphClient client;
        static bool Init()
        {
            client =  new GraphClient(new Uri("http://localhost:7474/db/data"),"user1", "password1");

            if(client != null)
            {
                client.Connect();
                
                Console.WriteLine("Połączono z bazą danych");
                return true;
            }
            else
            {
                Console.WriteLine("Błąd połączenia z bazą danych");
                Console.ReadLine();
                return false;
            }
        }

        static void Main(string[] args)
        {
            if (!Init())
                return;

            Console.WriteLine("Klient wykorzystujący Neo4j");
            int decision;
            while (true)
            {
                Console.WriteLine("Wybierz opcję:\n1 - Dodaj książkę\n2 - Usuń książkę\n3 - Modyfikuj książkę\n4 - Wyszukaj książkę\n5 - Dodaj przykładowe dane\n6 - Wyczyść dane\n0 - Wyjście");
                Console.Write("Wybór: ");
                try
                {
                    decision = int.Parse(Console.ReadLine());
                    switch (decision)
                    {
                        case 1:
                            {
                                AddBook();
                                break;
                            }
                        case 2:
                            {
                                DeleteBook();
                                break;
                            }
                        case 3:
                            {
                                ModifyBook();
                                break;
                            }
                        case 4:
                            {
                                SelectBook();
                                break;
                            }
                        case 5:
                            {
                                FillDatabase();
                                break;
                            }
                        case 6:
                            {
                                ClearDatabase();
                                break;
                            }
                        case 0:
                            {
                                return;
                            }
                        default:
                            {
                                Console.WriteLine("Podano niepoprawną wartość");
                                break;
                            }
                    }
                    Console.WriteLine("==============================================================================");
                }
                catch(FormatException)
                {
                    Console.WriteLine("Podano niepoprawną wartość");
                }
            }
        }

        private static void ClearDatabase()
        {
            client.Cypher.Match("(book:Book)").Delete("book").ExecuteWithoutResults();
            Console.WriteLine("Wyczyszczono bazę danych.");
        }

        private static void FillDatabase()
        {
            Book[] books = new Book[5];
            books[0] = new Book(1, "Bajki Robotów", "Stanisław", "Lem");
            books[1] = new Book(2, "Solaris", "Stanisław", "Lem");
            books[2] = new Book(3, "Eragon", "Cristopher", "Paolini");
            books[3] = new Book(4, "Najstarszy", "Cristopher", "Paolini");
            books[4] = new Book(5, "Brisingr", "Cristopher", "Paolini");

            foreach (Book newBook in books)
            {
                client.Cypher.Create("(b:Book {newBook})").WithParam("newBook", newBook).ExecuteWithoutResults();
            }
            Console.WriteLine("Dodano przykładowe dane.");
        }

        private static void SelectBook()
        {
            int i;
            string s;
            Console.WriteLine("Podaj kryterium wyszukiwania:\n1 - ID\n2 -Tytuł\n3 - Imię Autora\n4 - Nazwisko Autora\n5 - Wszystkie\n0 - Anuluj");
            Console.Write("Wybór: ");
            try
            {
                i = int.Parse(Console.ReadLine());

                switch (i)
                {
                    case 1:
                        {
                            Console.Write("Podaj ID: ");
                            try 
                            {
                                i = int.Parse(Console.ReadLine());
                                var result = client.Cypher.Match("(book:Book)").Where((Book book) => book.Id == i).Return(book => book.As<Book>()).Results;
                                PrintData(result);
                            }
                            catch(FormatException){ Console.WriteLine("Podano niepoprawną wartość"); }
                            break;
                        }
                    case 2:
                        {
                            Console.Write("Podaj tytuł: ");
                            s = Console.ReadLine();
                            var result = client.Cypher.Match("(book:Book)").Where((Book book) => book.Title == s).Return(book => book.As<Book>()).Results;
                            PrintData(result);
                            break;
                        }
                    case 3:
                        {
                            Console.Write("Podaj imię autora: ");
                            s = Console.ReadLine();
                            var result = client.Cypher.Match("(book:Book)").Where((Book book) => book.AuthorName == s).Return(book => book.As<Book>()).Results;
                            PrintData(result);
                            break;
                        }
                    case 4:
                        {
                            Console.Write("Podaj nazwisko autora: ");
                            s = Console.ReadLine();
                            var result = client.Cypher.Match("(book:Book)").Where((Book book) => book.AuthorSurname == s).Return(book => book.As<Book>()).Results;
                            PrintData(result);
                            break;
                        }
                    case 5:
                        {
                            var data = client.Cypher.Match("(book:Book)").Return(book => book.As<Book>()).Results;
                            PrintData(data);
                            break;
                        }
                    case 0:
                        {
                            Console.WriteLine("Anulowano");
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Podano niepoprawną wartość");
                            break;
                        }
                }
            }
            catch
            {
                Console.WriteLine("Podano niepoprawną wartość");
            }
        }

        private static void ModifyBook()
        {
            Console.Write("Podaj id modyfikowanej książki: ");

            try
            {
                int selectedId = int.Parse(Console.ReadLine());
                var results = client.Cypher.Match("(book:Book)").Where((Book book) => book.Id == selectedId).Return(book => book.As<Book>()).Results;
                if (results.Count() == 0)
                {
                    Console.WriteLine("Książka o podanym id nie istnieje!");
                    return;
                }
                var result = results.ToArray<Book>()[0];
                Console.WriteLine("Modyfikowana książka: " + result.GetInfo());
                Console.Write("Co chcesz modyfikować?\n1 - Tytuł\n2 - Imię Autora\n3 - Nazwisko Autora\n0 - Anuluj\nWybór: ");
                int decision = int.Parse(Console.ReadLine());
                while (true)
                {
                    switch (decision)
                    {
                        case 1:
                            {
                                Console.Write("Podaj tytuł: ");
                                result.Title = Console.ReadLine();
                                client.Cypher.Match("(book:Book)").Where((Book book) => book.Id == selectedId).Set("book = {result}").WithParam("result", result).ExecuteWithoutResults();
                                Console.WriteLine("Operacja przebiegła pomyślnie.");
                                return;
                            }
                        case 2:
                            {
                                Console.Write("Podaj imię autora: ");
                                result.AuthorName = Console.ReadLine();
                                client.Cypher.Match("(book:Book)").Where((Book book) => book.Id == selectedId).Set("book = {result}").WithParam("result", result).ExecuteWithoutResults();
                                Console.WriteLine("Operacja przebiegła pomyślnie.");
                                return;
                            }
                        case 3:
                            {
                                Console.Write("Podaj nazwisko autora: ");
                                result.AuthorSurname = Console.ReadLine();
                                client.Cypher.Match("(book:Book)").Where((Book book) => book.Id == selectedId).Set("book = {result}").WithParam("result", result).ExecuteWithoutResults();
                                Console.WriteLine("Operacja przebiegła pomyślnie.");
                                return;
                            }
                        case 0:
                            {
                                Console.WriteLine("Anulowano");
                                return;
                            }
                        default:
                            {
                                Console.WriteLine("Podano niepoprawną wartość");
                                break;
                            }
                    }
                }
            }
            catch(FormatException)
            {
                Console.WriteLine("Podano niepoprawną wartość");
            }
            
        }

        private static void DeleteBook()
        {
            int selectedId;
            Console.Write("Podaj id książki którą chcesz usunąć: ");
            try
            {
                selectedId = int.Parse(Console.ReadLine());
                try
                { 
                    client.Cypher.Match("(b:Book)").Where((Book b) => b.Id == selectedId).Delete("b").ExecuteWithoutResults();
                    Console.WriteLine("Operacja przebiegła pomyślnie.");
                }
                catch(NullReferenceException)
                {
                    Console.WriteLine("Błąd podczas usuwania.");
                }
            }
            catch(FormatException)
            {
                Console.WriteLine("Podano niepoprawną wartość");
            }
        }

        private static void AddBook()
        {
            Book newBook = new Book();
            Console.Write("Podaj tytuł: ");
            newBook.Title = Console.ReadLine();
            Console.Write("Podaj imię autora: ");
            newBook.AuthorName = Console.ReadLine();
            Console.Write("Podaj nazwisko autora: ");
            newBook.AuthorSurname = Console.ReadLine();
            while (true)
            {
                Console.Write("Podaj id (0 by anulować): ");
                try
                {
                    newBook.Id = int.Parse(Console.ReadLine());
                    if (newBook.Id == 0)
                        return;
                    if (newBook.Id < 0)
                        Console.WriteLine("Podano niepoprawną wartość");
                    else
                    {
                         if(client.Cypher.Match("(book:Book)").Where((Book book) => book.Id == newBook.Id).Return(book => book.As<Book>()).Results.Count() != 0)
                            Console.WriteLine("Podane id jest zajęte!");
                        else
                        {
                            client.Cypher.Create("(b:Book {newBook})").WithParam("newBook",newBook).ExecuteWithoutResults();
                            Console.WriteLine("Operacja przebiegła pomyślnie.");
                            return;
                        }
                    }    
                }
                catch(FormatException)
                {
                    Console.WriteLine("Podano niepoprawną wartość");
                }
            }

        }

        private static void PrintData(IEnumerable<Book> data)
        {
            Console.WriteLine("Wyniki wyszukiwania: ");
            if (data.Count() == 0)
                Console.WriteLine("Brak Danych.");
            else
            {
                var e = data.GetEnumerator();
                while (e.MoveNext())
                    Console.WriteLine(e.Current.GetInfo());
            }

        }
    }
}
