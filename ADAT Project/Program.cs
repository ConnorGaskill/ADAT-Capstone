using ADAT_Project;

string connectionString =
                "Server=(localDB)\\MSSQLLocalDB;Database=mtg_database;" +
                "Trusted_Connection=True;TrustServerCertificate=True;";

CardRepository repo = new CardRepository(connectionString);

TestGetAll(connectionString);
TestGetById(connectionString);


static void TestGetAll(string conn)
{
    CardRepository repo = new CardRepository(conn);

    Console.WriteLine("[Testing GetAll()]\n");

    foreach (Card card in repo.GetAll())
        Console.WriteLine(card);

    Console.WriteLine();
}

static void TestGetById(string conn)
{
    CardRepository repo = new CardRepository(conn);

    Console.WriteLine("[Testing GetById with valid id (1)]\n");

    Console.WriteLine(repo.GetById(1));

    Console.WriteLine("\n[Testing GetById with invalid id (-1)]\n");

    Card? card = repo.GetById(-1);

    if (card is null) {
        Console.WriteLine("Card with Id -1 does not exist");
    }
    else
    {
        Console.WriteLine($"Error, card found: {card}");
    }


}