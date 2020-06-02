using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Db4objects;
using Db4objects.Db4o;

namespace Db4oLeague
{
    class LeagueClient
    {
        static IEmbeddedObjectContainer db;
        private static void Init()
        {
            db = Db4oEmbedded.OpenFile(Db4oEmbedded.NewConfiguration(), "Players"); 
        }
        static void Main(string[] args)
        {
            Init();
            Console.WriteLine("Klient wykorzystujący Db4o");
            int decision;
            while (true)
            {
                Console.WriteLine("Wybierz opcję:\n1 - Dodaj zawodnika\n2 - Usuń zawodnika\n3 - Modyfikuj zawodnika\n4 - Wyszukaj zawodnika\n5 - Dodaj przykładowe dane\n6 - Wyczyść dane\n7 - Wyjście");
                Console.Write("Wybór: ");
                try
                {
                    decision = int.Parse(Console.ReadLine());
                    switch (decision)
                    {
                        case 1:
                            {
                                AddPlayer();
                                break;
                            }
                        case 2:
                            {
                                DeletePlayer();
                                break;
                            }
                        case 3:
                            {
                                ModifyPlayer();
                                break;
                            }
                        case 4:
                            {
                                SelectPlayer();
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
                        case 7:
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
                catch
                {
                    Console.WriteLine("Podano niepoprawną wartość");
                }
            }
        }

        static void AddPlayer()
        {
            Player newPlayer = new Player();
            Console.Write("Podaj imię: ");
            newPlayer.Name = Console.ReadLine();
            Console.Write("Podaj nazwisko: ");
            newPlayer.Surname = Console.ReadLine();
            Console.Write("Podaj nazwę klubu: ");
            newPlayer.TeamName = Console.ReadLine();
            while (true)
            {
                while (true)
                {
                    Console.Write("Podaj id: (0 by anulować) ");
                    newPlayer.Id = int.Parse(Console.ReadLine());
                    if (newPlayer.Id == 0)
                        return;
                    try
                    {
                        if (PlayerExists(newPlayer.Id))
                            Console.WriteLine("Podane id jest zajęte!");
                        else
                        {
                            db.Store(newPlayer);
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Podano niepoprawne dane! (id musi być typu int)");
                    }
                }
            }
        }

        static void DeletePlayer()
        {
            int id;

            while (true)
            {
                try
                {
                    Console.Write("Podaj id: (0 by anulować) ");
                    id = int.Parse(Console.ReadLine());

                    if (id == 0)
                        return;

                    try
                    {
                        Player prototype = new Player() { Id = id };
                        var result =  db.QueryByExample(prototype);
                        if (result != null)
                        {
                            db.Delete(result);
                            Console.WriteLine("Zawodnik usunięty!");
                        }
                        else
                            Console.WriteLine("Nie odnaleziono zawodnika o podanym id...");
                        
                        return;
                    }

                    catch
                    {
                        Console.WriteLine("Błąd usuwania...");
                    }
                }
                catch
                {
                    Console.WriteLine("Podano niepoprawne dane! (id musi byc typu int)");
                }
            }
        }

        static void ModifyPlayer()
        {
            int id;

            try
            {
                Console.Write("Podaj id: (0 by anulować) ");
                id = int.Parse(Console.ReadLine());

                if (id == 0)
                    return;


                var editedPlayer = db.QueryByExample(new Player() { Id = id });

                if (editedPlayer.Count == 0)
                {
                    Console.WriteLine("Nie odnaleziono zawodnika o podanym id");
                    return;
                }
                else
                {
                    Console.Write("Edytowany zawodnik: ");
                    PrintData(editedPlayer);

                    var player = editedPlayer[0];

                    Console.WriteLine("Co chcesz zamienić?\n1 - Imię\n2 - Nazwisko\n3 - Nazwa klubu\n4 - Anuluj\n");
                    try
                    {
                        var d = int.Parse(Console.ReadLine());
                        switch (d)
                        {
                            case 1:
                                {
                                    Console.Write("Podaj imię: ");
                                    player.Name = Console.ReadLine();
                                    db.Store(player);
                                    break;
                                }
                            case 2:
                                {
                                    Console.Write("Podaj nazwisko: ");
                                    player.Surname = Console.ReadLine();
                                    db.Store(player);
                                    break;
                                }
                            case 3:
                                {
                                    Console.Write("Podaj nazwę klubu: ");
                                    player.TeamName = Console.ReadLine();
                                    db.Store(player);
                                    break;
                                }
                            case 4:
                                {
                                    return;
                                }
                            default:
                                {
                                    Console.WriteLine("Podano niepoprawną wartość");
                                    break;
                                }
                        }

                        Console.WriteLine("Zmodyfikowano zawodnika !");
                    }
                    catch
                    {
                        Console.WriteLine("Podano niepoprawną wartość");
                    }
                }
            }
            catch
            {
                Console.WriteLine("Podano niepoprawne dane! (id musi byc typu int)");
            }

        }

        static void SelectPlayer()
        {
            int d;
            Console.WriteLine("Podaj kryterium wyszukiwania:\n1 - ID\n2 - Imię\n3 - Nazwisko\n4 - Nazwa klubu\n5 - Wyświetl wszystkich zawodników\n6 - Anuluj\n");
            Console.Write("Wybór: ");
            try
            {
                d = int.Parse(Console.ReadLine());
                var p = new Player();

                switch (d)
                {
                    case 1:
                        {
                            Console.Write("Podaj ID: ");
                            p.Id = int.Parse(Console.ReadLine());
                            var result = db.QueryByExample(p);
                            Console.WriteLine("Wyniki wyszukiwania: ");
                            PrintData(result);
                            break;
                        }
                    case 2:
                        {
                            Console.Write("Podaj imię: ");
                            p.Name = Console.ReadLine();
                            var result = db.QueryByExample(p);
                            Console.WriteLine("Wyniki wyszukiwania: ");
                            PrintData(result);
                            break;
                        }
                    case 3:
                        {
                            Console.Write("Podaj nazwisko: ");
                            p.Surname = Console.ReadLine();
                            var result = db.QueryByExample(p);
                            Console.WriteLine("Wyniki wyszukiwania: ");
                            PrintData(result);
                            break;
                        }
                    case 4:
                        {
                            Console.Write("Podaj nazwę klubu: ");
                            p.TeamName = Console.ReadLine();
                            var result = db.QueryByExample(p);
                            Console.WriteLine("Wyniki wyszukiwania: ");
                            PrintData(result);
                            break;
                        }
                    case 5:
                        {
                            var result = db.QueryByExample(new Player());
                            PrintData(result);
                            break;
                        }
                    case 6:
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

        static void FillDatabase()
        {
            try
            {
                db.Store(new Player(1, "Paweł", "Sokół", "Korona Kielce"));
                db.Store(new Player(2, "Marcin", "Cebula", "Korona Kielce"));
                db.Store(new Player(3, "Karol", "Krupa", "KSZO Ostrowiec"));
                db.Store(new Player(4, "Mateusz", "Podstolak", "KSZO Ostrowiec"));
                db.Store(new Player(5, "Igor", "Lewczuk", "Legia Warszawa"));

                Console.WriteLine("Dodano przykładowe dane.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void ClearDatabase()
        {
            try
            {
                var d = db.QueryByExample(new Player());
                var e = d.GetEnumerator();
                while (e.MoveNext())
                    db.Delete(e.Current);


                Console.WriteLine("Usunięto wszystkie dane.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void PrintData(IObjectSet<Player> objectSet)
        {
            if (objectSet.Count == 0)
            {
                Console.WriteLine("Brak danych");
            }
            else
            {
                var e = objectSet.GetEnumerator();

                while (e.MoveNext())
                    Console.WriteLine(e.Current.GetDescription());
            }
        }

        static bool PlayerExists(int id)
        {
            var result = db.QueryByExample(new Player() { Id = id});

            if(result.Count > 0)
                return true;
            else
                return false;
        }
    }
}
