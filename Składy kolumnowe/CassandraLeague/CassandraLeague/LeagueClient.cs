using Cassandra;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace CassandraLeague
{
    class LeagueClient
    {
        static Cluster cluster;
        static ISession session;

        static void Init()
        {
            cluster = Cluster.Builder()
                     .AddContactPoints("127.0.0.1")
                     .WithPort(9042)
                     .WithQueryTimeout(900000000)
                     .WithAuthProvider(new PlainTextAuthProvider("cassandra", "cassandra"))
                     .Build();

            session = cluster.Connect("system_distributed");
            session.Execute(new SimpleStatement("CREATE KEYSPACE IF NOT EXISTS myKeyspace WITH replication = { 'class': 'SimpleStrategy', 'replication_factor': '1' }"));
            session.Execute("USE myKeyspace");
            if (cluster == null || session == null)
            {
                Console.WriteLine("Błąd łączenia z serwerem.");
                return;
            }
            else
                Console.WriteLine("Połączono z serwerem: " + session.Cluster.Metadata.ClusterName);

            try
            {
                session.Execute("CREATE TABLE Players(id int PRIMARY KEY, name text, surname text, teamname text); ");
                Console.WriteLine("Utworzono bazę danych.");
            }
            catch
            {
                Console.WriteLine("Wykryto bazę danych.");
            }
        }

        static void Main(string[] args)
        {
            Init();
            Console.WriteLine("Klient wykorzystujący Apache Cassandra");
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
                                cluster.Shutdown();
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
                catch(FormatException e)
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
                        if(PlayerExists(newPlayer.Id))
                            Console.WriteLine("Podane id jest zajęte!"); 
                        else
                        {
                            session.Execute("INSERT INTO myKeyspace.Players (id,name,surname,teamname)VALUES(" + newPlayer.Id + ",'" + newPlayer.Name + "','" + newPlayer.Surname + "','" + newPlayer.TeamName + "')");
                            return;
                        }
                    }
                    catch(Exception)
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
                        session.Execute("DELETE FROM Players WHERE id = " + id);
                        Console.WriteLine("Zawodnik usunięty!");
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
            while (true)
            {
                try
                {
                    Console.Write("Podaj id: (0 by anulować) ");
                    id = int.Parse(Console.ReadLine());

                    if (id == 0)
                        return;

                    if (!PlayerExists(id))
                    {
                        Console.WriteLine("Nie odnaleziono zawodnika o podanym id");
                        return;
                    }
                    else
                    {
                        var res = session.Execute("SELECT * FROM Players WHERE id = " + id);
                        Console.Write("Edytowany zawodnik: ");
                        PrintData(res);

                        string temp;
                        Console.WriteLine("Co chcesz zamienić?\n1 - Imię\n2 - Nazwisko\n3 - Nazwa klubu\n4 - Anuluj\n");
                        try
                        {
                            var d = int.Parse(Console.ReadLine());
                            switch (d)
                            {
                                case 1:
                                    {
                                        Console.Write("Podaj imię: ");
                                        temp = Console.ReadLine();
                                        session.Execute("UPDATE Players SET name = '" + temp + "' WHERE id = " + id);
                                        Console.WriteLine("Zmodyfikowano zawodnika !");
                                        break;
                                    }
                                case 2:
                                    {
                                        Console.Write("Podaj nazwisko: ");
                                        temp = Console.ReadLine();
                                        session.Execute("UPDATE Players SET surname = '" + temp + "' WHERE id = " + id);
                                        Console.WriteLine("Zmodyfikowano zawodnika !");
                                        break;
                                    }
                                case 3:
                                    {
                                        Console.Write("Podaj nazwę klubu: ");
                                        temp = Console.ReadLine();
                                        session.Execute("UPDATE Players SET teamname = '" + temp + "' WHERE id = " + id);
                                        Console.WriteLine("Zmodyfikowano zawodnika !");
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
        }

        static void SelectPlayer()
        {
            int d;
            Console.WriteLine("Podaj kryterium wyszukiwania:\n1 - ID\n2 - Imię\n3 - Nazwisko\n4 - Nazwa klubu\n5 - Wyświetl wszystkich zawodników\n6 - Anuluj\n");
            Console.Write("Wybór: ");
            try
            {
                d = int.Parse(Console.ReadLine());

                switch (d)
                {
                    case 1:
                        {
                            try
                            {
                                Console.Write("Podaj ID: ");
                                var temp = int.Parse(Console.ReadLine());
                                var result = session.Execute("SELECT * FROM Players WHERE id = " + temp + " ALLOW FILTERING");
                                Console.WriteLine("Wyniki wyszukiwania: ");
                                PrintData(result);
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
                            Console.Write("Podaj imię: ");
                            var temp = Console.ReadLine(); 
                            var result = session.Execute("SELECT * FROM Players WHERE name = '" + temp + "' ALLOW FILTERING");
                            Console.WriteLine("Wyniki wyszukiwania: ");
                            PrintData(result);
                            break;
                        }
                    case 3: 
                        {
                            Console.Write("Podaj nazwisko: ");
                            var temp = Console.ReadLine();
                            var result = session.Execute("SELECT * FROM Players WHERE surname = '" + temp + "' ALLOW FILTERING");
                            Console.WriteLine("Wyniki wyszukiwania: ");
                            PrintData(result);
                            break;
                        }
                    case 4:
                        {
                            Console.Write("Podaj nazwę klubu: ");
                            var temp = Console.ReadLine();
                            var result = session.Execute("SELECT * FROM Players WHERE teamname = '" + temp + "' ALLOW FILTERING");
                            Console.WriteLine("Wyniki wyszukiwania: ");
                            PrintData(result);
                            break;
                        }
                    case 5:
                        {
                            var result = session.Execute("SELECT * FROM Players ALLOW FILTERING");
                            Console.WriteLine("Wyniki wyszukiwania: ");
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
            catch(FormatException e)
            {
                Console.WriteLine("Podano niepoprawną wartość");
            }
        }

        static void FillDatabase()
        {
            try
            {
                session.Execute("INSERT INTO myKeyspace.Players (id,name,surname,teamname)VALUES(1,'Paweł', 'Sokół', 'Korona Kielce') ");
                session.Execute("INSERT INTO myKeyspace.Players (id,name,surname,teamname)VALUES(2,'Marcin', 'Cebula', 'Korona Kielce') ");
                session.Execute("INSERT INTO myKeyspace.Players (id,name,surname,teamname)VALUES(3,'Karol', 'Krupa', 'KSZO Ostrowiec') ");
                session.Execute("INSERT INTO myKeyspace.Players (id,name,surname,teamname)VALUES(4,'Mateusz ', 'Podstolak', 'KSZO Ostrowiec') ");
                session.Execute("INSERT INTO myKeyspace.Players (id,name,surname,teamname)VALUES(5,'Igor', 'Lewczuk', 'KSZO Ostrowiec') ");

                Console.WriteLine("Dodano przykładowe dane.");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
         
        static void ClearDatabase()
        {
            try
            {
                session.Execute("TRUNCATE myKeyspace.Players");
                Console.WriteLine("Usunięto wszystkie dane.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void PrintData(RowSet rowset)
        {
            foreach(Row row in rowset)
                Console.WriteLine(row.GetValue<int>("id") + ": " + row.GetValue<string>("name") + " " + row.GetValue<string>("surname") + ", zawodnik drużyny " + row.GetValue<string>("teamname")); 
        }

        static bool PlayerExists(int id)
        {
            var result = session.Execute("SELECT * FROM Players WHERE id = " + id + " ALLOW FILTERING");
            if (result.GetRows().Count() > 0)
                return true;
            else
                return false;
        }
    }
}
