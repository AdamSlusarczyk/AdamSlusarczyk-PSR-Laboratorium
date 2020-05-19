using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using DnsClient;
using Microsoft.SqlServer.Server;
using MongoDB;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongodbPolice
{
    class Program
    {
        static MongoClient client;
        static IMongoDatabase database;
        static IMongoCollection<Criminal> criminalCollection;

        static void Main(string[] args)
        {
            Console.WriteLine("Klient wykorzystujący MongoDB");
            BsonClassMap.RegisterClassMap<Fine>();
            BsonClassMap.RegisterClassMap<JailSentence>();

            client = new MongoClient();
            if (client == null)
            {
                Console.WriteLine("Błąd łączenia z serwerem...");
                Console.Read();
                return;
            }
            Console.WriteLine("Połączono z serwerem - " + client.Settings.Server.Host + ":" + client.Settings.Server.Port);
            database = client.GetDatabase("PoliceDB");

            criminalCollection = database.GetCollection<Criminal>("CriminalCollection");
            int optioni;
            string options;



            while (true)
            {
                Console.WriteLine("=====================================================================================");
                Console.WriteLine("Klient wykorzystujący MongoDB");
                Console.WriteLine("1 - Dodaj przestępcę\n2 - Usuń przestępcę\n3 - Modyfikuj przestępcę\n4 - Wypisz wszystkich przestępców\n5 - Wyszukaj przestępców\n6 - Dodaj do bazy przykładowe dane\n7 - Wyczyść bazę danych\n0 - Wyjdź");
                Console.Write("Wybór: ");
                try
                {
                    optioni = int.Parse(Console.ReadLine());
                    Console.WriteLine("-------------------------------------------------------------------------------------");
                    switch (optioni)
                    {
                        case 1:
                            {
                                AddCriminal();
                                break;
                            }
                        case 2:
                            {
                                DeleteCriminal();
                                break;
                            }
                        case 3:
                            {
                                ModifyCriminal();
                                break;
                            }
                        case 4:
                            {
                                ListCriminals();
                                ;
                                break;
                            }
                        case 5:
                            {
                                FindCriminals();
                                break;
                            }
                        case 6:
                            {
                                FillDatabase();
                                break;
                            }
                        case 7:
                            {
                                Console.WriteLine("Na pewno? (t\\n)");
                                options = Console.ReadLine();
                                if (options == "t")
                                    ClearDatabase();
                                else
                                    break;
                                break;
                            }
                        case 0:
                            {
                                Console.WriteLine("Zakończono działanie programu.");
                                Console.ReadLine();
                                return;
                            }
                        default:
                            Console.WriteLine("Podano niepoprawną wartość");
                            break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("-------------------------------------------------------------------------------------");
                    Console.WriteLine("Podano niepoprawną wartość");
                }
            }
        }

        private static void AddCriminal()
        {
            int temp;
            Criminal newCriminal = new Criminal();

            Console.Write("Podaj imię: ");
            newCriminal.name = Console.ReadLine();
            Console.Write("Podaj nazwisko: ");
            newCriminal.secondName = Console.ReadLine();
            Console.Write("Podaj wiek: ");
            newCriminal.age = int.Parse(Console.ReadLine());
            Console.WriteLine("Podaj przestępstwo:\n1 - Napad na bank\n2 - Kradzież\n3 - Morderstwo");
            temp = int.Parse(Console.ReadLine());
            if (0 < temp || temp < 5)
                newCriminal.crime = (CrimeEnum)temp;
            else
                newCriminal.crime = (CrimeEnum)1;
            Console.WriteLine("Podaj wyrok:\n1 - Mandat\n2 - Więzienie");
            temp = int.Parse(Console.ReadLine());

            switch (temp)
            {
                case 1:
                    {
                        var fine = new Fine();
                        Console.Write("Podaj wysokość mandatu: ");
                        try { fine.ticketValue = int.Parse(Console.ReadLine()); }
                        catch (FormatException) { fine.ticketValue = 100; }
                        newCriminal.sentence = fine;
                        break;
                    }
                case 2:
                    {
                        var jail = new JailSentence();
                        Console.Write("Podaj długość wyroku: ");
                        try { jail.length = int.Parse(Console.ReadLine()); }
                        catch (FormatException) { jail.length = 2; }
                        newCriminal.sentence = jail;
                        break;
                    }
                default:
                    {
                        Console.Write("Podano niepoprawną wartość. Przypiswywanie domyślnych danych.");
                        var jail = new JailSentence();
                        jail.length = 2; 
                        newCriminal.sentence = jail;
                        break;
                    }
            }

            newCriminal.id = GetId();

            criminalCollection.InsertOne(newCriminal);
            Console.WriteLine("Dodano nowy dokument do bazy danych.");
            
        }

        private static void DeleteCriminal()
        {
            Console.Write("Podaj id przestępcy który ma zostać usunięty: ");
            int id;
            try
            {
                id = int.Parse(Console.ReadLine());
                FieldDefinition<Criminal, int> field = "id";
                var builder = Builders<Criminal>.Filter;
                var filter = builder.Eq(field, id);
                criminalCollection.DeleteOne(filter);
                Console.WriteLine("Usunięto dokument.");
            }
            catch (FormatException) 
            {
                Console.WriteLine("Podano niepoprawną wartość");
            }
        }

        private static void ModifyCriminal()
        {
            Console.Write("Podaj id przestępcy którego dane mają zostać zmodyfikowany: ");
            int id;
            try
            {
                id = int.Parse(Console.ReadLine());
                var query = from e in criminalCollection.AsQueryable<Criminal>()
                            where e.id == id
                            select e;

                if(query == null)
                {
                    Console.Write("Nie odnaleziono przestępcy o podanym id.");
                    return;
                }

                var criminal = query.First();
                Console.Write("Co chcesz zmodyfikować?\n1 - Imię\n2 - Nazwisko\n3 - wiek\n4 - Zarzut");
                int option = int.Parse(Console.ReadLine());

                switch (option)
                {
                    case 1:
                        {
                            Console.Write("Podaj imię: ");
                            criminal.name = Console.ReadLine();
                           

                            break;
                        }
                    case 2:
                        {
                            Console.Write("Podaj nowe nazwisko: ");
                            criminal.secondName = Console.ReadLine();


                            break;
                        }
                       
                    case 3:
                        {
                            Console.Write("Podaj nowy wiek: ");
                            criminal.age = int.Parse(Console.ReadLine());


                            break;
                        }
                        
                    case 4:
                        {
                            Console.WriteLine("Podaj przestępstwo:\n1 - Napad na bank\n2 - Kradzież\n3 - Morderstwo");
                            criminal.crime = (CrimeEnum)int.Parse(Console.ReadLine());


                            break;
                        }
                         
                    default:
                        {
                            Console.WriteLine("Podano niepoprawną wartość");
                            return;
                        }
                }

                FieldDefinition<Criminal, int> field = "id";
                var builder = Builders<Criminal>.Filter;
                var filter = builder.Eq(field, id);

                criminalCollection.FindOneAndReplace(filter, criminal);

                Console.WriteLine("Zmodyfikowano dokument");
            }
            catch (FormatException)
            {
                Console.WriteLine("Podano niepoprawną wartość");
            }
        }

        private static void ListCriminals()
        {
            var query = from e in criminalCollection.AsQueryable<Criminal>()
                        where e.name != ""
                        select e;

            Console.WriteLine("Całkowita liczba przestępców: " + query.Count());
            Task t = new Task(() =>
            {
                foreach (Criminal criminal in query)
                    Console.WriteLine(criminal.GetDescription());
            });
            t.Start();
            t.Wait();
        }

        private static void FindCriminals()
        {
            Console.WriteLine("Podaj kryterium wyszukiwania:\n1 - ID\n2 - Największa wartość mandatu\n3 - Rodzaj przestępstwa\n");
            try
            {
                int option = int.Parse(Console.ReadLine());

                switch (option)
                {
                    case 1:
                        {
                            Console.Write("Podaj id przestępcy: ");
                            int id;
                            try
                            {
                                id = int.Parse(Console.ReadLine());
                                var query = from e in criminalCollection.AsQueryable<Criminal>()
                                            where e.id == id
                                            select e;

                                Console.WriteLine("Całkowita liczba przestępców: " + query.Count());
                                Task t = new Task(() =>
                                {
                                    if (query == null)
                                        Console.WriteLine("Brak wyników");
                                    else
                                    {
                                        foreach (Criminal criminal in query)
                                            Console.WriteLine(criminal.GetDescription());
                                    }
                                });
                                t.Start();
                                t.Wait();
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("Podano niepoprawną wartość");
                                break;
                            }
                        }
                    case 2:
                        {
                            try
                            {
                                var query = from e in criminalCollection.AsQueryable<Criminal>()
                                            where e.name != ""
                                            select e;

                                Task t = new Task(() =>
                                {

                                    if (query == null)
                                        Console.WriteLine("Brak wyników");
                                    else
                                    { 
                                        Fine max = new Fine(-1);
                                        string desc = "Brak wyników";

                                        foreach (Criminal criminal in query)
                                        {
                                            if(criminal.sentence.GetType() == max.GetType())
                                                if (((Fine)criminal.sentence).ticketValue > max.ticketValue)
                                                {
                                                    max.ticketValue = ((Fine)criminal.sentence).ticketValue;
                                                    desc = criminal.GetDescription();
                                                }
                                                    
                                        }

                                        Console.WriteLine(desc);
                                    }
                                });
                                t.Start();
                                t.Wait();
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("Podano niepoprawną wartość");
                                break;
                            }
                        }
                    case 3:
                        {
                            Console.WriteLine("Podaj przestępstwo:\n1 - Napad na bank\n2 - Kradzież\n3 - Morderstwo");
                            try
                            {
                                int crime = int.Parse(Console.ReadLine());

                                var query = from e in criminalCollection.AsQueryable<Criminal>()
                                            where e.name != ""
                                            select e;

                                Console.WriteLine("Wyniki wyszukiwania: \n");
                                Task t = new Task(() =>
                                {
                                    foreach (Criminal criminal in query)
                                    {
                                        if(criminal.crime == (CrimeEnum)crime)
                                            Console.WriteLine(criminal.GetDescription());
                                    }
                                        
                                });
                                t.Start();
                                t.Wait();
                            }
                            catch
                            {
                                Console.WriteLine("Podano niepoprawną wartość");
                            }
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

        private static void FillDatabase()
        {
            List<Criminal> list = new List<Criminal>();
            int id = GetId();
            list.Add(new Criminal(id, "Jan", "Kowalski", 42, CrimeEnum.Kradzież, new Fine(2000)));
            list.Add(new Criminal(id+1, "Anna", "Nowak", 52, CrimeEnum.Morderstwo, new Fine(3500)));
            list.Add(new Criminal(id+2, "Bartosz", "Ktokolwiek", 19, CrimeEnum.Napad_na_bank, new JailSentence(5)));
            list.Add(new Criminal(id+3, "Jakub", "Bak", 26, CrimeEnum.Kradzież, new Fine(1800)));
            criminalCollection.InsertMany(list);
            Console.WriteLine("Dodano nowe dokumenty do bazy danych.");
        }

        private static void ClearDatabase()
        {
            FieldDefinition<Criminal,int> field = "id";
            var builder = Builders<Criminal>.Filter;
            var filter = builder.Gt(field, -1);
            criminalCollection.DeleteMany(filter);

            if(criminalCollection.CountDocuments(filter) == 0)
                Console.WriteLine("Wyczyszczono bazę danych.");
            else
                Console.WriteLine("Błąd podczas czyszczenia bazy danych. Liczba pozostałych dokumentów: " + criminalCollection.CountDocuments(filter));
        }

        private static int GetId()
        {
            var result = criminalCollection.Find(x => true).SortByDescending(d => d.id).Limit(1).FirstOrDefaultAsync();
            if (result.Result != null)
                return (result.Result.id + 1);
            else
                return 1;
        }
    }
}
