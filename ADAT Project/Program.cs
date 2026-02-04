using ADAT_Project;
using ADAT_Project.Models;

string connectionString =
                "Server=(localDB)\\MSSQLLocalDB;Database=mtg_database;" +
                "Trusted_Connection=True;TrustServerCertificate=True;";

//TestGetAll(connectionString);
//TestGetById(connectionString);

//TestGetAllFull(connectionString);

//TestAdd(connectionString);
//TestDelete(connectionString);
TestUpdate(connectionString);
//TestAddWithAudit(connectionString);

static void TestAdd(string conn)
{
    CardRepository repo = new CardRepository(conn);

    Card? card = CreateTestCard();

    int cardId = repo.Add(card);

    Console.WriteLine("Adding Valid card...");

    Console.WriteLine($" ---Success---\nAdded Card:\n {repo.GetById(cardId).ToFullDetailString()}");

    cardId = repo.Add(card);

    Console.WriteLine("Testing adding duplicate card... ");

    IEnumerable<Card> cards = repo.GetAllFull();

    card = repo.GetById(cardId);

    int cardCount = 0;

    foreach (Card c in cards)
    {
        if (c.CardId == card.CardId)
            cardCount++;
    }

    if (cardCount == 1)
    {
        Console.WriteLine("Success: Card was not duplicated on add");
    }
    else
    {
        Console.WriteLine("Failure: card was duplicated" + cardCount);
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

static void TestDelete(string conn)
{
    CardRepository repo = new CardRepository(conn);

    Card card = repo.GetById(1);

    Console.WriteLine("Testing Deleting an existing card...");

    bool isDeleted = repo.Delete(card.CardId);

    if (isDeleted)
    {
        Console.WriteLine($"--- Success---\nCard Deleted:\n{card.ToFullDetailString()}");
    }
    else
    {
        Console.WriteLine("Error: Card not deleted");
    }
    repo.ResetCardTable();
}
static void TestGetAllFull(string conn)
{

    CardRepository repo = new CardRepository(conn);

    IEnumerable<Card> cards = repo.GetAllFull();

    foreach (Card card in cards)
    {
        Console.WriteLine(card.ToFullDetailString());
    }

}

static void TestUpdate(string conn)
{
    CardRepository repo = new CardRepository(conn);

    Card card = repo.GetById(1);

    Console.WriteLine("Testing Update...");

    Console.WriteLine($"Card before: {card.ToFullDetailString()}");

    card.Name = "Testing";
    card.ManaCost = "Testing";

    repo.Update(card);

    card = repo.GetById(card.CardId);

    Console.WriteLine($"Card after: {card.ToFullDetailString()}");

    if (String.Equals(card.Name, "Testing")) {
        Console.WriteLine("-- Success--");
    }
    else
    {
        Console.WriteLine("-- Failure --");
    }

    repo.ResetCardTable();
}

static void TestAddWithAudit(string conn)
{
    var cardRepo = new CardRepository(conn);
    var auditRepo = new AuditRepository(conn);

    Card card = CreateTestCard();

    try
    {

        Console.WriteLine("Testing with valid Card...");
        int cardId = cardRepo.AddWithAudit(card);

        // Verify card exists
        var insertedCard = cardRepo.GetById(cardId);
        Console.WriteLine(insertedCard != null
            ? $"Card inserted: {insertedCard.Name}"
            : "Card not found!");

        var auditLogs = auditRepo.GetAll().Where(a => a.EntityId == cardId && a.EntityType == "Card");
        Console.WriteLine(auditLogs.Any()
            ? "Audit log created successfully."
            : "Audit log not created.");

        Console.WriteLine("\nTesting with invalid Card");

        // Force failure by creating invalid card (Name = null)
        Card invalidCard = new Card
        {
            Name = null,
            ManaCost = "G",
            OracleText = "Invalid card",
            IsLegendary = false
        };

        try
        {
            cardRepo.AddWithAudit(invalidCard);
            Console.WriteLine("FAILED: Invalid card was added (should not happen).");
        }
        catch
        {
            Console.WriteLine("AddWithAudit failed as expected for invalid card.");
        }

        // Ensure no extra audit was created
        int auditCount = auditRepo.GetAll().Count(a => a.EntityType == "Card");
        Console.WriteLine(auditCount == 1
            ? "No audit log created for failed insert (as expected)."
            : "Unexpected audit log created for failed insert.");
    }
    finally
    {
        cardRepo.ResetCardTable();
        auditRepo.TruncateAuditTable();
        Console.WriteLine("\nCleanup complete.");
    }
}

static Card CreateTestCard()
{
    return new Card
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
}