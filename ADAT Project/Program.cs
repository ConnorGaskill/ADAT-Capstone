using ADAT_Project;
using ADAT_Project.Models;

string connectionString =
                "Server=(localDB)\\MSSQLLocalDB;Database=mtg_database;" +
                "Trusted_Connection=True;TrustServerCertificate=True;";

//TestGetAll(connectionString);
//TestGetById(connectionString);

//TestGetAllFull(connectionString);

TestAdd(connectionString);

static void TestAdd(string conn)
{
    CardRepository repo = new CardRepository(conn);

    Card? card = new Card
    {
        Name = "Lightning Bolt",
        ManaCost = "R",
        OracleText = "Lightning Bolt deals 3 damage to any target.",
        Power = null,
        Toughness = null,
        Rarity = "Common",
        IsLegendary = false,
        Colors = new List<Color>
    {
        new Color { Name = "Red" }
    },
        Types = new List<string> { "Instant" },
        Printings = new List<Printing>
    {
        new Printing
        {
            CollectorNumber = "150",
            Set = new Set
            {
                Code = "M10",
                Name = "Magic 2010"
            }
        }
    }
    };


    int cardId = repo.Add(card);

    Console.WriteLine("Adding Valid card...");

    Console.WriteLine("Success: Card" +  repo.GetById(cardId).ToFullDetailString());

    cardId = repo.Add(card);

    Console.WriteLine("Testing adding duplicate card... ");

    if (cardId == -1)
    {
        Console.WriteLine("Success: duplicate card not added -- transaction rolled back");
    }
    else
    {
        Console.WriteLine($"Error: Duplicate card added. CardId: {cardId}");
    }
        repo.ResetCardTable();


}
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

static void TestGetAllFull(string conn) {

    CardRepository repo = new CardRepository(conn);

    IEnumerable<Card> cards = repo.GetAllFull();

    foreach (Card card in cards)
    {
        Console.WriteLine(card.ToFullDetailString());
    }

}