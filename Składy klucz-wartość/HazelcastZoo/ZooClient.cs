using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HazelcastZoo
{
    class ZooClient
    {
        static void Main(string[] args)
        {
            int action;
            var clientConfig = new ClientConfig();

            clientConfig.GetNetworkConfig().AddAddress("192.168.56.1");

            IHazelcastInstance client = HazelcastClient.NewHazelcastClient(clientConfig);
            Console.WriteLine("Klient wykorzystujący Hazelcast.");
            if (client == null)
                Console.WriteLine("Błąd połączenia z serwerem.");
            else
            {
                Console.WriteLine("Pomyślnie połączono z serwerem. ");
                while (true)
                {
                    Console.WriteLine("Wpisz:\n1 - Obsługa zbioru zwierząt\n2 - Obsługa zbioru opiekunów\n3 - Wypełnij przykładowymi danymi\n4 - Usuń wszystkie dane\n5 - Wyjście");
                    try 
                    {
                        action = int.Parse(Console.ReadLine());
                        switch (action)
                        {
                            case 1:
                                HandleAnimals(client);
                                break;
                            case 2:
                                HandleKeepers(client);
                                break;
                            case 3:
                                FillWithData(client);
                                break;
                            case 4:
                                {
                                    Console.WriteLine("Na pewno? (t/n)");
                                    var x = Console.ReadLine();
                                    if (String.Equals("t", x))
                                    {
                                        var animalMap = client.GetMap<int, Animal>("animalMap");
                                        animalMap.EvictAll();
                                        var keeperMap = client.GetMap<int, ZooKeeper>("keeperMap");
                                        keeperMap.EvictAll();
                                    }
                                }
                                break;
                            case 5:
                                client.Shutdown();
                                return;
                            default:
                                Console.WriteLine("Podano niepoprawną wartość.");
                                break;
                        }
                    }
                    catch(Exception e) { Console.WriteLine(e.Message); }
                }
            }
        }

        static void HandleAnimals(IHazelcastInstance client)
        {
            IMap<int, Animal> animalMap;
            int action;

            Console.WriteLine("Wpisz:\n1 - Dodaj/Aktualizuj zwierzę\n2 - Usuń zwierzę\n3 - Pobierz listę zwierząt\n4 - Znajdź zwierzę\n5 - Wstecz");
            action = int.Parse(Console.ReadLine());

            switch (action)
            {
                case 1:
                    {
                        String name, species;
                        int annexNo, key, keeperId;
                        string x;

                        Console.WriteLine("Podaj imię zwierzęcia: ");
                        name = Console.ReadLine();
                        Console.WriteLine("Podaj gatunek zwierzęcia: ");
                        species = Console.ReadLine();
                        Console.WriteLine("Podaj numer pawilonu w którym żyje zwierzę: ");
                        annexNo = int.Parse(Console.ReadLine());
                        Console.WriteLine("Podaj Id (klucz) opiekuna: ");
                        keeperId = int.Parse(Console.ReadLine());

                        while (true)
                        {
                            var newAnimal = new Animal(name, species, annexNo, keeperId);
                            animalMap = client.GetMap<int, Animal>("animalMap");

                            Console.WriteLine("Podaj id (klucz) nowego zwierzęcia, lub zwierzęcia które ma zostać zmodyfikowane: ");
                            key = int.Parse(Console.ReadLine());

                            if (animalMap.ContainsKey(key))
                            {
                                var animal = animalMap.Get(key);
                                Console.WriteLine("Modyfikacja: " + animal.Signature + " ==> " + newAnimal.Signature);
                                Console.WriteLine("Kontynuować ? (t - tak, n - nie, a - anuluj: ");
                                x = Console.ReadLine();

                                switch (x)
                                {
                                    case "t":
                                        animalMap.Put(key, newAnimal);
                                        Console.WriteLine("animalMap MODIFY => " +  newAnimal.Signature+ " on key " + key);
                                        return;
                                    case "n":
                                        Console.WriteLine("Anulowano.");
                                        break;
                                    case "a":
                                        Console.WriteLine("Anulowano.");
                                        return;
                                    default:
                                        Console.WriteLine("Anulowano.");
                                        break;
                                }
                            }
                            else
                            {
                                animalMap.Put(key, newAnimal);
                                Console.WriteLine("animalMap PUT => " + newAnimal.Signature + " on key " + key);
                                return;
                            } 
                        }
                    }
                case 2:
                    {
                        animalMap = client.GetMap<int, Animal>("animalMap");
                        Console.WriteLine("Podaj id (klucz) zwierzęcia które ma zostać usunięte: ");
                        int key = int.Parse(Console.ReadLine());
                        if (animalMap.ContainsKey(key))
                        {
                            var animal = animalMap.Get(key);
                            Console.WriteLine("Usuwanie: " + animal.Signature);
                            Console.WriteLine("Kontynuować ? (t/n) ");
                            var x = Console.ReadLine();

                            switch (x)
                            {
                                case "t":
                                    animalMap.Remove(key);
                                    Console.WriteLine("animalMap REMOVE => " + animal.Signature + " on key " + key);
                                    break;
                                case "n":
                                    Console.WriteLine("Anulowano.");
                                    break;
                                default:
                                    Console.WriteLine("Anulowano.");
                                    break;
                            }
                        }
                        else
                            Console.WriteLine("Brak wyników wyszukiwania.");
                    }
                    break;
                case 3:
                    {
                        var animals = client.GetMap<int, Animal>("animalMap").EntrySet();
                        var enumerator = animals.GetEnumerator();
                        Console.WriteLine("Spis zwierząt: ");
                        while (enumerator.MoveNext())
                            Console.WriteLine(enumerator.Current.Key + " " + enumerator.Current.Value.Signature);
                    }
                    break;
                case 4:
                    {
                        Console.WriteLine("Kryterium wyszukiwania:\n1 - Klucz\n2 - Imie\n3 - Klucz opiekuna\n4 - Wstecz");
                        action = int.Parse(Console.ReadLine());

                        switch (action)
                        {
                            case 1:
                                {
                                    Console.WriteLine("Podaj klucz: ");
                                    int key = int.Parse(Console.ReadLine());
                                    animalMap = client.GetMap<int, Animal>("animalMap");
                                    
                                    if (animalMap.ContainsKey(key))
                                        Console.WriteLine("key " + key + " = " + animalMap.Get(key).Signature);
                                        
                                    else
                                        Console.WriteLine("Brak wyników wyszukiwania.");
                                }
                                break;
                            case 2:
                                {
                                    Console.WriteLine("Podaj klucz: ");
                                    String name = Console.ReadLine();
                                    animalMap = client.GetMap<int, Animal>("animalMap");

                                    var animals = animalMap.EntrySet();
                                    var enumerator = animals.GetEnumerator();

                                    while (enumerator.MoveNext())
                                        if (!String.Equals(enumerator.Current.Key,name))
                                            animals.Remove(enumerator.Current);


                                    if (animals.Count > 0)
                                    {
                                        Console.WriteLine("Zwierzęta o imieniu " + name + ":");

                                        enumerator = animals.GetEnumerator();
                                        while (enumerator.MoveNext())
                                            Console.WriteLine("Klucz: " + enumerator.Current.Key + ": " + enumerator.Current.Value.Signature);
                                    }
                                    else
                                        Console.WriteLine("Brak zwierząt o podanym imieniu");
                                }
                                break;
                            case 3:
                                {
                                    Console.WriteLine("Podaj klucz opiekuna: ");
                                    int key = int.Parse(Console.ReadLine());

                                    var animals = client.GetMap<int, Animal>("animalMap").EntrySet();
                                    List<Animal> result = new List<Animal>();

                                    var enumerator = animals.GetEnumerator();

                                    while (enumerator.MoveNext())
                                        if (enumerator.Current.Value.KeeperId == key)
                                            result.Add(enumerator.Current.Value);

                                    if (result.Count > 0)
                                    {
                                        Console.WriteLine("Zwierzęta którymi zajmuje się opiekun " + key);
                                        foreach (Animal animal in result)
                                            Console.WriteLine(animal.Signature);
                                    }
                                    else
                                        Console.WriteLine("Brak wyników wyszukiwania.");
                                }
                                break;
                            case 4:
                                return;
                        }
                    }
                    break;
                case 5:
                    return;
                default:
                    Console.WriteLine("Podano niepoprawną wartość.");
                    break;
            }
        }

        static void HandleKeepers(IHazelcastInstance client) 
        {
            int action, key;
            String name, lastName, x;
            IMap<int, ZooKeeper> keeperMap;

            Console.WriteLine("Wpisz:\n1 - Dodaj / Aktualizuj opiekuna\n2 - Usuń opiekuna\n3 - Pobierz listę opiekunów i ich podopiecznych\n4 - Wstecz");
            action = int.Parse(Console.ReadLine());

            switch (action)
            {
                case 1:
                    {
                        Console.WriteLine("Podaj imię opiekuna: ");
                        name = Console.ReadLine();
                        Console.WriteLine("Podaj nazwisko opiekuna: ");
                        lastName = Console.ReadLine();
                        while (true)
                        {
                            var newKeeper = new ZooKeeper(name, lastName);
                            keeperMap = client.GetMap<int, ZooKeeper>("keeperMap");

                            Console.WriteLine("Podaj Id (klucz) opiekuna: ");
                            key = int.Parse(Console.ReadLine());

                            if (keeperMap.ContainsKey(key))
                            {
                                var keeper = keeperMap.Get(key);
                                Console.WriteLine("Modyfikacja: " + keeper.Signature + " ==> " + newKeeper.Signature);
                                Console.WriteLine("Kontynuować ? (t - tak, n - nie, a - anuluj: ");
                                x = Console.ReadLine();

                                switch (x)
                                {
                                    case "t":
                                        keeperMap.Put(key, newKeeper);
                                        Console.WriteLine("animalMap MODIFY => " + newKeeper.Signature + " on key " + key);
                                        return;
                                    case "n":
                                        Console.WriteLine("Anulowano.");
                                        break;
                                    case "a":
                                        Console.WriteLine("Anulowano.");
                                        return;
                                    default:
                                        Console.WriteLine("Anulowano.");
                                        break;
                                }
                            }
                            else
                            {
                                keeperMap.Put(key, newKeeper);
                                Console.WriteLine("animalMap PUT => " + newKeeper.Signature + " on key " + key);
                                return;
                            }
                            break;
                        }
                        break;
                    }
                case 2:
                    {
                        keeperMap = client.GetMap<int, ZooKeeper>("keeperMap");
                        Console.WriteLine("Podaj id (klucz) opiekuna który ma zostać usunięty: ");
                        key = int.Parse(Console.ReadLine());
                        if (keeperMap.ContainsKey(key))
                        {
                            var keeper = keeperMap.Get(key);
                            Console.WriteLine("Usuwanie: " + keeper.ToString());
                            Console.WriteLine("Kontynuować ? (t/n) ");
                            x = Console.ReadLine();

                            switch (x)
                            {
                                case "t":
                                    keeperMap.Remove(key);
                                    Console.WriteLine("keeperMap REMOVE => " + keeper.Signature + " on key " + key);
                                    break;
                                case "n":
                                    Console.WriteLine("Anulowano.");
                                    break;
                                default:
                                    Console.WriteLine("Anulowano.");
                                    break;
                            }
                        }
                        else
                            Console.WriteLine("Brak wyników wyszukiwania.");
                    }
                    break;
                case 3:
                    {
                        var keepers = client.GetMap<int, ZooKeeper>("keeperMap").EntrySet();
                        var animals = client.GetMap<int, Animal>("animalMap").EntrySet();
                        var keeperEnum = keepers.GetEnumerator();
                        Console.WriteLine("Spis opiekunów: ");
                        while (keeperEnum.MoveNext())
                        {
                            Console.WriteLine(keeperEnum.Current.Key + " " + keeperEnum.Current.Value.Signature + ":");
                            var animalEnum = animals.GetEnumerator();
                            while (animalEnum.MoveNext())
                                if (animalEnum.Current.Value.KeeperId == keeperEnum.Current.Key)
                                    Console.WriteLine("     " + animalEnum.Current.Value.Signature);
                        }
                    }
                    break;
                case 4:
                    return;
                default:
                    Console.WriteLine("Podano niepoprawną wartość.");
                    break;
            }
        } 
    
        static void FillWithData(IHazelcastInstance client)
        {
            var animalMap = client.GetMap<int, Animal>("animalMap");
            animalMap.Put(1, new Animal("Harambe","Goryl",1,1));
            animalMap.Put(2, new Animal("Simba","Lew",2,1));
            animalMap.Put(3, new Animal("Pumba","Guziec",2,2));
            animalMap.Put(4, new Animal("Timon","Surykatka",2,2));

            var keeperMap = client.GetMap<int, ZooKeeper>("keeperMap");
            var newKeeper = new ZooKeeper("Jan", "Kowalski");
            keeperMap.Put(1,newKeeper);
            newKeeper = new ZooKeeper("Anna", "Nowak");
            keeperMap.Put(2, newKeeper);

            Console.WriteLine("Dodano przykładowe  dane");
        }
    }
}
