using ADAT_Project.DataAccess.EfCore;
using ADAT_Project.DataAccess.EfCore.Context;
using ADAT_Project.DataAccess.EfCore.Entities;
using ADAT_Project.Utilities;
using ADONet_Tools.ADONet.Models;
using ADONet_Tools.ADONet.Repositories;

using ADOCard = ADONet_Tools.ADONet.Models.Card;
using ADOColor = ADONet_Tools.ADONet.Models.Color;
using ADOSet = ADONet_Tools.ADONet.Models.Set;

using EFCard = ADAT_Project.DataAccess.EfCore.Entities.Card;
using EFColor = ADAT_Project.DataAccess.EfCore.Entities.Color;
using EFSet = ADAT_Project.DataAccess.EfCore.Entities.Set;
internal class Program
{
    private static void Main(string[] args)
    {
        string connectionString =
                "Server=(localDB)\\MSSQLLocalDB;Database=mtg_database;" +
                "Trusted_Connection=True;TrustServerCertificate=True;";
        using var context = new ProjectDbContext();

        //TestGetAll(connectionString);
        //TestGetById(connectionString);

        //TestGetAllFull(connectionString);
        //TestAdd(connectionString);
        //TestDelete(connectionString);
        //TestUpdate(connectionString);
        //TestAddWithAudit(connectionString);
        //TimeTestAdd(connectionString);

        //TestEfGetAll(context);
        //context.ChangeTracker.Clear();
        //TestEfGetById(context);
        //context.ChangeTracker.Clear();
        //TestEfAdd(context);
        //context.ChangeTracker.Clear();
        //TestEFDelete(context);
        //context.ChangeTracker.Clear();
        //TestEFUpdate(context);
        //context.ChangeTracker.Clear();
        //TestSimpleAdd(context);
        //context.ChangeTracker.Clear();
        TestEfOneToManyAdd(context);
        context.ChangeTracker.Clear();

        static void TestEfOneToManyAdd(ProjectDbContext context)
        {
            var repo = new CardEfRepository(context);

            repo.ResetCardTable();

            var card = repo.GetById(1);

            var printing = new CardPrinting
            {
                CollectorNumber = "024",
                Set = new EFSet
                {
                    Code = "STA",
                    Name = "Strixhaven Mystical Archives"
                }
            };

            Console.WriteLine($"Adding a new printing to the following card...\n\n{card!.ToFullDetailString()}");

            card.CardPrintings.Add(printing);
            repo.Add(card);

            Console.WriteLine($"New Printing added:\n\n{repo.GetById(card.CardId)!.ToFullDetailString()}");

            card = repo.GetById(1);

            bool found = card.CardPrintings.Any(cp => cp.Set.Code == "STA");

            if (found){
                Console.WriteLine("New Printing added");
            }
            else
            {
                Console.WriteLine("Could not find new printing");
            }
            repo.ResetCardTable();
        }

        static void TestSimpleAdd(ProjectDbContext context)
        {

            CardEfRepository repo = new CardEfRepository(context);
            EFCard card = repo.CreateTestCard();

            int id = repo.SimpleAdd(card);

            Console.WriteLine($"{repo.GetById(id).ToFullDetailString()}");

            repo.ResetCardTable();
        }

        static void TestEfGetAll(ProjectDbContext context)
        {

            var cardRepository = new CardEfRepository(context);

            // Get all cards
            var allCards = cardRepository.GetAll();

            Console.WriteLine("All Cards:");
            foreach (var card in allCards)
            {
                Console.WriteLine($"{card.ToString()}");
            }

            Console.WriteLine();
        }

        static void TestEfGetById(ProjectDbContext context)
        {
            var cardRepository = new CardEfRepository(context);


            var cardById = cardRepository.GetById(1);

            if (cardById != null)
            {
                Console.WriteLine("Card with ID 1:");
                Console.WriteLine($"{cardById.ToFullDetailString()}");
            }
            else
            {
                Console.WriteLine("Card with ID 1 not found.");
            }

        }

        static void TestEfAdd(ProjectDbContext context)
        {

            var repo = new CardEfRepository(context);

            repo.ResetCardTable();

            EFCard card = repo.CreateTestCard();

            Console.WriteLine($"Adding the following Card...\n\n{card.ToFullDetailString()}");

            int cardId = repo.Add(card);

            Console.WriteLine("Adding Valid card...");

            Console.WriteLine($" ---Success---\nAdded Card:\n {repo.GetById(cardId)!.ToFullDetailString()}");

            repo.ResetCardTable();
        }

        static void TestEFDelete (ProjectDbContext context) {

            var repo = new CardEfRepository(context);

            repo.ResetCardTable();

            int cardId = repo.Add(repo.CreateTestCard());

            Console.WriteLine("Getting added card...");

            Console.WriteLine(repo.GetById(cardId)!.ToFullDetailString());

            Console.WriteLine("\nDeleting card...");

            repo.Delete(cardId);

            if (repo.GetById(cardId) is null)
            {
                Console.WriteLine("Success: card successfully deleted");
                repo.ResetCardTable();
                return;
            }

            Console.WriteLine("Failure: delete failed");
            repo.ResetCardTable();
        }

        static void TestEFUpdate(ProjectDbContext context) { 

            var repo = new CardEfRepository(context);

            EFCard card = repo.GetById(1)!;

            Console.WriteLine($"Card before change: {card.ToFullDetailString()}");

            Console.WriteLine("Updating card...");

            string testName = "This is a test";

            card.Name = testName;

            repo.Update(card);

            card = repo.GetById(1)!;

            Console.WriteLine($"Updated card: {card.ToFullDetailString()}");

            if (card.Name == testName) {

                Console.WriteLine("Success: card was updated");

                repo.ResetCardTable();
                return;
            }

            Console.WriteLine("Failure: card was not updated");

            repo.ResetCardTable();

        }

        static void TimeTestAdd(string conn)
        {
            CardRepository repo = new CardRepository(conn);

            repo.ResetCardTable();

            ADOCard? card = repo.CreateTestCard();

            Console.WriteLine("Testing adding a card inefficiently...");

            TimeSpan inefficientAdd = TimeUtilities.RunWithStopwatch(() => repo.InefficientAdd(card));

            Console.WriteLine($"Card added inefficiently in {inefficientAdd} seconds\n");

            repo.ResetCardTable();

            Console.WriteLine("Testing adding a card efficiently...");

            TimeSpan efficientAdd = TimeUtilities.RunWithStopwatch(() => repo.Add(card));

            Console.WriteLine($"Card added efficiently in {efficientAdd} seconds\n");

            Console.WriteLine(TimeUtilities.GetFastest(
                new KeyValuePair<string, TimeSpan>("inefficient add", inefficientAdd),
                new KeyValuePair<string, TimeSpan>("efficient add", efficientAdd))
                );

            repo.ResetCardTable();
        }

        static void TestAdd(string conn)
        {
            CardRepository repo = new CardRepository(conn);

            ADOCard? card = repo.CreateTestCard(); ;

            int cardId = repo.Add(card);

            Console.WriteLine("Adding Valid card...");

            Console.WriteLine($" ---Success---\nAdded Card:\n {repo.GetById(cardId)!.ToFullDetailString()}");

            cardId = repo.Add(card);

            Console.WriteLine("Testing adding duplicate card... ");

            IEnumerable<ADOCard> cards = repo.GetAllFull();

            card = repo.GetById(cardId);

            int cardCount = 0;

            foreach (ADOCard c in cards)
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

            foreach (ADOCard card in repo.GetAll())
                Console.WriteLine(card);

            Console.WriteLine();
        }

        static void TestGetById(string conn)
        {
            CardRepository repo = new CardRepository(conn);

            Console.WriteLine("[Testing GetById with valid id (1)]\n");

            Console.WriteLine(repo.GetById(1));

            Console.WriteLine("\n[Testing GetById with invalid id (-1)]\n");

            ADOCard? card = repo.GetById(-1);

            if (card is null)
            {
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

            ADOCard card = repo.GetById(1)!;

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

            IEnumerable<ADOCard> cards = repo.GetAllFull();

            foreach (ADOCard card in cards)
            {
                Console.WriteLine(card.ToFullDetailString());
            }

        }

        static void TestUpdate(string conn)
        {
            CardRepository repo = new CardRepository(conn);

            ADOCard card = repo.GetById(1)!;

            Console.WriteLine("Testing Update...");

            Console.WriteLine($"Card before: {card.ToFullDetailString()}");

            card.Name = "Testing";
            card.ManaCost = "Testing";

            repo.Update(card);

            card = repo.GetById(card.CardId)!;

            Console.WriteLine($"Card after: {card.ToFullDetailString()}");

            if (string.Equals(card.Name, "Testing"))
            {
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

            ADOCard card = cardRepo.CreateTestCard();

            try
            {

                Console.WriteLine("Testing with valid Card...");
                int cardId = cardRepo.AddWithAudit(card);

                var insertedCard = cardRepo.GetById(cardId);
                Console.WriteLine(insertedCard != null
                    ? $"Card inserted: {insertedCard.Name}"
                    : "Card not found!");

                var auditLogs = auditRepo.GetAll().Where(a => a.EntityId == cardId && a.EntityType == "Card");
                Console.WriteLine(auditLogs.Any()
                    ? "Audit log created successfully."
                    : "Audit log not created.");

                Console.WriteLine("\nTesting with invalid Card");

                ADOCard invalidCard = new ADOCard
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
    }
}