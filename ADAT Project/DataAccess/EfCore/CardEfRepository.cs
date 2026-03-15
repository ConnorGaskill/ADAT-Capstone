using ADAT_Project.DataAccess.EfCore.Context;
using ADAT_Project.DataAccess.EfCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ADAT_Project.DataAccess.EfCore
{
    public class CardEfRepository : IDataAccess<Card>
    {
        private readonly ProjectDbContext _context;

        //Cached lookup tables
        //This is okay since these tables rarely or never change and I don't want the application side to push up bad data
        //The cache is automatically updated upon reinitialization of the repository
        private readonly List<Color> Colors; //never changes
        private readonly List<Cardtype> CardTypes; //changes every 1-2 years
        private readonly List<Set> Sets; // changes every 6 months


        public CardEfRepository(ProjectDbContext context)
        {
            _context = context;

            Colors = _context.Colors.ToList();
            CardTypes = _context.Cardtypes.ToList();
            Sets = _context.Sets.ToList();
        }

        public IEnumerable<Card> GetAll()
        {
            return _context.Cards
                .Include(c => c.CardPrintings)
                    .ThenInclude(cp => cp.Set)
                .Include(c => c.Colors)
                .Include(c => c.Cardtypes)
                .AsNoTracking()
                .ToList();
        }

        public Card? GetById(int id)
        {
            return _context.Cards
                .Include(c => c.CardPrintings)
                    .ThenInclude(cp => cp.Set)
                .Include(c => c.Colors)
                .Include(c => c.Cardtypes)
                .AsNoTracking()
            .FirstOrDefault(c => c.CardId == id);
        }

        public int Add(Set setEntity)
        {
            _context.Add(setEntity);

            _context.SaveChanges();

            return setEntity.SetId;
        }

        public int Add(Card entity)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // Load existing card with related data
                var card = _context.Cards
                    .Include(c => c.Colors)
                    .Include(c => c.Cardtypes)
                    .Include(c => c.CardPrintings)
                        .ThenInclude(p => p.Set)
                    .FirstOrDefault(c => c.Name == entity.Name);

                // Create card if it doesn't exist

                if (card == null)
                {
                    card = new Card
                    {
                        Name = entity.Name,
                        ManaCost = entity.ManaCost,
                        OracleText = entity.OracleText,
                        Power = entity.Power,
                        Toughness = entity.Toughness,
                        Rarity = entity.Rarity,
                        IsLegendary = entity.IsLegendary
                    };

                    _context.Cards.Add(card);
                    _context.SaveChanges(); // ensures CardId exists
                }

                // Map Colors
                var cachedColors = _context.Colors.ToList();

                foreach (var incomingColor in entity.Colors)
                {
                    var match = cachedColors.FirstOrDefault(c => c.Name == incomingColor.Name);
                    if (match == null)
                        throw new Exception($"Color '{incomingColor.Name}' not found in lookup table.");

                    if (!card.Colors.Any(c => c.ColorId == match.ColorId))
                        card.Colors.Add(match);
                }

                // Map CardTypes
                var cachedCardTypes = _context.Cardtypes.ToList();

                foreach (var incomingType in entity.Cardtypes)
                {
                    var match = cachedCardTypes.FirstOrDefault(ct => ct.Name == incomingType.Name);
                    if (match == null)
                        throw new Exception($"Cardtype '{incomingType.Name}' not found in lookup table.");

                    if (!card.Cardtypes.Any(ct => ct.CardtypeId == match.CardtypeId))
                        card.Cardtypes.Add(match);
                }

                // Map Printings
                var cachedSets = _context.Sets.ToList();

                foreach (var incomingPrinting in entity.CardPrintings)
                {
                    // Make sure Set exists
                    var set = cachedSets.FirstOrDefault(s => s.Code == incomingPrinting.Set.Code);
                    if (set == null)
                        throw new Exception($"Set '{incomingPrinting.Set.Code}' not found in lookup table.");

                    // Only add if this printing does not exist on the card
                    bool printingExists = card.CardPrintings.Any(p =>
                        p.SetId == set.SetId &&
                        p.CollectorNumber == incomingPrinting.CollectorNumber);

                    if (!printingExists)
                    {
                        var printing = new CardPrinting
                        {
                            CardId = card.CardId,
                            SetId = set.SetId,
                            CollectorNumber = incomingPrinting.CollectorNumber
                        };

                        card.CardPrintings.Add(printing);
                    }
                }
                _context.SaveChanges();
                transaction.Commit();

                return card.CardId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int SimpleAdd(Card entity)
        {
            _context.Add(entity);

            _context.SaveChanges();

            return entity.CardId;
        }
        public bool Update(Card entity)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var card = _context.Cards
                    .Include(c => c.Cardtypes)
                    .Include(c => c.Colors)
                    .Include(c => c.CardPrintings)
                        .ThenInclude(cp => cp.Set)
                    .FirstOrDefault(c => c.CardId == entity.CardId);

                if (card == null)
                    return false; // card not found

                // Map onto card
                card.Name = entity.Name;
                card.ManaCost = entity.ManaCost;
                card.OracleText = entity.OracleText;
                card.Power = entity.Power;
                card.Toughness = entity.Toughness;
                card.Rarity = entity.Rarity;
                card.IsLegendary = entity.IsLegendary;

                // Update Colors
                var resolvedColors = (entity.Colors ?? Enumerable.Empty<Color>())
                    .Select(c => _context.Colors.FirstOrDefault(db => db.Name == c.Name) ?? new Color { Name = c.Name })
                    .ToList();

                card.Colors = resolvedColors;

                // Update Cardtypes
                var resolvedTypes = (entity.Cardtypes ?? Enumerable.Empty<Cardtype>())
                    .Select(t => _context.Cardtypes.FirstOrDefault(db => db.Name == t.Name) ?? new Cardtype { Name = t.Name })
                    .ToList();

                card.Cardtypes = resolvedTypes;

                // Update CardPrintings

                // remove old printings
                var toRemove = card.CardPrintings
                    .Where(cp => !entity.CardPrintings.Any(e => e.PrintingId == cp.PrintingId))
                    .ToList();

                foreach (var cp in toRemove)
                    _context.CardPrintings.Remove(cp);

                // Add/update printings
                foreach (var incomingPrinting in entity.CardPrintings ?? Enumerable.Empty<CardPrinting>())
                {
                    var set = _context.Sets
                        .FirstOrDefault(s => s.Code == incomingPrinting.Set.Code)
                        ?? new Set { Code = incomingPrinting.Set.Code, Name = incomingPrinting.Set.Name };

                    if (set.SetId == 0)
                        _context.Sets.Add(set);

                    var existingPrinting = card.CardPrintings
                        .FirstOrDefault(cp => cp.PrintingId == incomingPrinting.PrintingId);

                    if (existingPrinting != null)
                    {
                        existingPrinting.CollectorNumber = incomingPrinting.CollectorNumber;
                        existingPrinting.Set = set;
                    }
                    else
                    {
                        _context.CardPrintings.Add(new CardPrinting
                        {
                            Card = card,
                            Set = set,
                            CollectorNumber = incomingPrinting.CollectorNumber
                        });
                    }
                }

                // Save changes
                _context.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch (DbUpdateException)
            {
                transaction.Rollback();
                return false;
            }
        }
        public bool Delete(int id)
        {
            var entity = _context.Cards.Find(id);
            if (entity == null)
                return false;

            _context.Cards.Remove(entity);
            return _context.SaveChanges() > 0;
        }

        public void ResetCardTable()
        {
            try
            {
                _context.Database.ExecuteSqlRaw("EXEC ResetCardTable");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to reset card table", ex);
            }
        }

        public Card CreateTestCard()
        {
            Card card = new Card {
                Name = "Lightning Bolt",
                ManaCost = "R",
                OracleText = "Lightning Bolt deals 3 damage to any target.",
                Power = null,
                Toughness = null,
                Rarity = "Common",
                IsLegendary = false,
            };

            card.Colors.Add(new Color { Name = "Red" });

            card.Cardtypes.Add(new Cardtype { Name = "Instant" });

            card.CardPrintings.Add(new CardPrinting
            {
                CollectorNumber = "150",
                Set = new Set {Code = "M10" }

            });

            return card;

        }


    }
}