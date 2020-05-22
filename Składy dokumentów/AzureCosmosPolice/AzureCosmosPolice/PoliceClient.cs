using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AzureCosmosPolice
{
    class PoliceClient
    {
        static Container criminalCollection;
        static Microsoft.Azure.Cosmos.Database policeDB;
        static CosmosClient cosmosClient;


        static async Task<int> InitDB()
        {
            string endpoint = "https://localhost:8081";
            string primaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            cosmosClient = new CosmosClient(endpoint, primaryKey);

            await cosmosClient.CreateDatabaseIfNotExistsAsync("PoliceDB");
            policeDB = cosmosClient.GetDatabase("PoliceDB");
            await policeDB.CreateContainerIfNotExistsAsync("criminalCollection","/id");
            criminalCollection = policeDB.GetContainer("criminalCollection");
            return 1;
        }

        static void Main(string[] args)
        {
            Task<int> task;
            Console.WriteLine("Łączenie...");
            task = InitDB();
            task.Wait();
            Console.Clear();


            int optioni;
            string options;
            while (true)
            {
                Console.WriteLine("=====================================================================================");
                Console.WriteLine("Klient wykorzystujący Azure Cosmos DB");
                Console.WriteLine("1 - Dodaj przestępcę\n2 - Usuń przestępcę\n3 - Modyfikuj przestępcę\n4 - Wypisz wszystkich przestępców\n5 - Wyszukaj przestępców\n6 - Dodaj przykładowe dane\n7 - Wyczyść bazę danych\n0 - Wyjdź");
                Console.Write("Wybór: ");
                try
                {
                    optioni = int.Parse(Console.ReadLine());
                    Console.WriteLine("-------------------------------------------------------------------------------------");
                    switch (optioni)
                    {
                        case 1:
                            {
                                task =  AddCriminal();
                                task.Wait();
                                break;
                            }
                        case 2:
                            {
                                task =  DeleteCriminal();
                                task.Wait();
                                break;
                            }
                        case 3:
                            {
                                task = ModifyCriminal();
                                task.Wait();
                                break;
                            }
                        case 4:
                            {
                                task = ListCriminals();
                                task.Wait();
                                break;
                            }
                        case 5:
                            {
                                task = FindCriminals();
                                task.Wait();
                                break;
                            }
                        case 6:
                            {
                                task = FillDatabase();
                                task.Wait();
                                break;
                            }
                        case 7:
                            {
                                Console.WriteLine("Na pewno? (t\\n)");
                                options = Console.ReadLine();
                                if (options == "t")
                                {
                                    task = ClearDatabase();
                                    task.Wait();
                                }
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
        private static async Task<int> AddCriminal()
        {
            int temp;
            Criminal newCriminal = new Criminal();
            Console.Write("Podaj id: ");
            newCriminal.id = Console.ReadLine();
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
                        var fine = new Sentence();
                        Console.Write("Podaj wysokość mandatu: ");
                        try { fine.value = int.Parse(Console.ReadLine()); }
                        catch (FormatException) { fine.value = 100; }
                        newCriminal.sentence = fine;
                        break;
                    }

                case 2:
                    {
                        var jail = new Sentence();
                        Console.Write("Podaj długość wyroku: ");
                        try { jail.value = int.Parse(Console.ReadLine()); }
                        catch (FormatException) { jail.value = 2; }
                        newCriminal.sentence = jail;
                        break;
                    }

                default:
                    {
                        Console.Write("Podano niepoprawną wartość. Przypiswywanie domyślnych danych.");
                        var jail = new Sentence();
                        jail.value = 2;
                        newCriminal.sentence = jail;
                        return -1;
                    }
            }
            newCriminal.id = "1";
            await criminalCollection.CreateItemAsync(newCriminal);
            Console.WriteLine("Dodano nowy dokument do bazy danych.");
            return 1;
        }
        private static async Task<int> DeleteCriminal()
        {
            Console.Write("Podaj id przestępcy który ma zostać usunięty: ");
            string id;
            id = Console.ReadLine();

            try
            {
                var result = await criminalCollection.DeleteItemAsync<Criminal>(id, new Microsoft.Azure.Cosmos.PartitionKey("sss"));
              
                if (result.StatusCode == HttpStatusCode.OK)
                    Console.WriteLine("Usunięto dokument.");
                else
                    Console.WriteLine("Wystąplił błąd podczas usuwania dokumentu: " + result.StatusCode.ToString());

            }
            catch (FormatException)
            {
                Console.WriteLine("Podano niepoprawną wartość");
            }
            
            return 1;
        }
        private static async Task<int> ModifyCriminal()
        {
            Console.Write("Podaj id przestępcy którego dane mają zostać zmodyfikowany: ");
            try
            {
                var id = Console.ReadLine();

                QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id > '0'");
                FeedIterator<Criminal> queryResultSetIterator = criminalCollection.GetItemQueryIterator<Criminal>(queryDefinition);
                Microsoft.Azure.Cosmos.FeedResponse<Criminal> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                
                if(currentResultSet.Count == 0)
                {
                    Console.WriteLine("Brak wyników wyszukiwania.");
                    return 1;
                }
                
                var criminal = currentResultSet.First();
                Console.Write("Co chcesz zmodyfikować?\n1 - Imię\n2 - Nazwisko\n3 - wiek\n4 - Zarzut\nWybór: ");
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
                            return 1;
                        }
                }

                await criminalCollection.ReplaceItemAsync(criminal, id);
                Console.WriteLine("Zmodyfikowano dokument");
            }
            catch (FormatException)
            {
                Console.WriteLine("Podano niepoprawną wartość");
            }
            return 1;
        }
        private static async Task<int> ListCriminals()
        {
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id > '0'");
            FeedIterator<Criminal> queryResultSetIterator = criminalCollection.GetItemQueryIterator<Criminal>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                Microsoft.Azure.Cosmos.FeedResponse<Criminal> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Criminal cr in currentResultSet)
                {
                    Console.WriteLine(cr.GetDescription());
                }
            }
            return 1;
        }
        private static async Task<int> FindCriminals()
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
                            string id = Console.ReadLine();
                            try
                            {
                                QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE (c.id = @id)");
                                queryDefinition.WithParameter("@id", id);
                                FeedIterator<Criminal> queryResultSetIterator = criminalCollection.GetItemQueryIterator<Criminal>(queryDefinition);

                                Console.WriteLine("Lista przestępców: ");
                                while (queryResultSetIterator.HasMoreResults)
                                {

                                    Microsoft.Azure.Cosmos.FeedResponse<Criminal> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                                    Console.WriteLine(currentResultSet.Count);

                                    foreach (Criminal cr in currentResultSet)
                                        Console.WriteLine(cr.GetDescription()); 
                                 }

                                break;
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                        }
                    case 2:
                        {
                            var max = new Sentence(false, -1);
                            string desc = "Brak danych";

                            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id > '0'");
                            FeedIterator<Criminal> queryResultSetIterator = criminalCollection.GetItemQueryIterator<Criminal>(queryDefinition);
                            Console.Write("Największy mandat: ");

                            while (queryResultSetIterator.HasMoreResults)
                            {
                                Microsoft.Azure.Cosmos.FeedResponse<Criminal> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                                foreach (Criminal criminal in currentResultSet)
                                {
                                    if(criminal.sentence.isJailSentence == false && (criminal.sentence.value > max.value))
                                    {
                                        max.value = criminal.sentence.value;
                                        desc = criminal.GetDescription();
                                    }
                                }
                            }
                            Console.WriteLine(desc);
                            break;
                        }
                    case 3:
                        {
                            Console.WriteLine("Podaj przestępstwo:\n1 - Napad na bank\n2 - Kradzież\n3 - Morderstwo");
                            try
                            {
                                int crime = int.Parse(Console.ReadLine());
                                List<Criminal> list = new List<Criminal>();

                                QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id > '0'");
                                FeedIterator<Criminal> queryResultSetIterator = criminalCollection.GetItemQueryIterator<Criminal>(queryDefinition);
                                

                                while (queryResultSetIterator.HasMoreResults)
                                {
                                    Microsoft.Azure.Cosmos.FeedResponse<Criminal> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                                    foreach (Criminal criminal in currentResultSet)
                                    {
                                        if (criminal.crime == (CrimeEnum)crime)
                                            list.Add(criminal); 
                                    }
                                }
                                Console.WriteLine("Lista przestępców osądzonych za {0}: ", (CrimeEnum)crime);
                                foreach (Criminal criminal in list)
                                    Console.WriteLine(criminal.GetDescription());           
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
            return 1;
        }
        private static async Task<int> FillDatabase()
        {
            await criminalCollection.CreateItemAsync(new Criminal("1", "Jan", "Kowalski", 42, CrimeEnum.Kradzież, new Sentence(false, 2000)));
            await criminalCollection.CreateItemAsync(new Criminal("2", "Anna", "Nowak", 52, CrimeEnum.Morderstwo, new Sentence(false, 3500)));
            await criminalCollection.CreateItemAsync(new Criminal("3", "Bartosz", "Ktokolwiek", 19, CrimeEnum.Napad_na_bank, new Sentence(true, 5)));
            await criminalCollection.CreateItemAsync(new Criminal("4", "Jakub", "Bak", 26, CrimeEnum.Kradzież, new Sentence(false, 1800)));
            Console.WriteLine("Dodano nowe dokumenty do bazy danych.");
            return 1;
        }
        private static async Task<int> ClearDatabase()
        {
            /*     FieldDefinition<Criminal, int> field = "id";
                 var builder = Builders<Criminal>.Filter;
                 var filter = builder.Gt(field, -1);
                 criminalCollection.DeleteMany(filter);

                 if (criminalCollection.CountDocuments(filter) == 0)
                     Console.WriteLine("Wyczyszczono bazę danych.");
                 else
                     Console.WriteLine("Błąd podczas czyszczenia bazy danych. Liczba pozostałych dokumentów: " + criminalCollection.CountDocuments(filter));*/

            return 1;
        }
    }
}

