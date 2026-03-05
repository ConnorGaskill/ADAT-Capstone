using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ADAT_Project.DataAccess.EfCore.Context;
using ADAT_Project.DataAccess.EfCore.Entities;

namespace ADAT_Project.DataAccess.EfCore
{
    public class CardEfRepository : IDataAccess<Card>
    {

        private readonly ProjectDbContext _context;

        public CardEfRepository(ProjectDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Card> GetAll()
        {
            return _context.Cards
                .AsNoTracking()
                .ToList();
        }

        public Card? GetById(int id)
        {
            return _context.Cards
            .AsNoTracking()
            .FirstOrDefault(c => c.CardId == id);
        }

        public int Add(Card entity)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // Resolve / Insert card (by Name)
                var card = _context.Cards
                    .FirstOrDefault(c => c.Name == entity.Name);

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
                    _context.SaveChanges(); // This makes sure I can return the cardId
                }

                // Resolve colors
                var resolvedColors = new List<Color>();

                foreach (var incomingColor in entity.Colors ?? Enumerable.Empty<Color>())
                {
                    var color = _context.Colors
                        .FirstOrDefault(c => c.Name == incomingColor.Name);

                    if (color == null)
                    {
                        color = new Color { Name = incomingColor.Name };
                        _context.Colors.Add(color);
                    }

                    resolvedColors.Add(color);
                }

                card.Colors = resolvedColors;

                // Resolve cardtypes

                var resolvedTypes = new List<Cardtype>();

                foreach (var incomingType in entity.Cardtypes ?? Enumerable.Empty<Cardtype>())
                {
                    var type = _context.Cardtypes
                        .FirstOrDefault(t => t.Name == incomingType.Name);

                    if (type == null)
                    {
                        type = new Cardtype { Name = incomingType.Name };
                        _context.Cardtypes.Add(type);
                    }

                    resolvedTypes.Add(type);
                }

                card.Cardtypes = resolvedTypes;

                // Resolve printings

                foreach (var incomingPrinting in entity.CardPrintings ?? Enumerable.Empty<CardPrinting>())
                {
                    var set = _context.Sets
                        .FirstOrDefault(s => s.Code == incomingPrinting.Set.Code)
                        ?? new Set
                        {
                            Code = incomingPrinting.Set.Code,
                            Name = incomingPrinting.Set.Name
                        };

                    if (set.SetId == 0)
                        _context.Sets.Add(set);

                    var printing = new CardPrinting
                    {
                        Card = card,
                        Set = set,
                        CollectorNumber = incomingPrinting.CollectorNumber
                    };

                    _context.CardPrintings.Add(printing);
                }

                // Save changes
                _context.SaveChanges();

                transaction.Commit();
                return card.CardId;
            }
            catch (DbUpdateException)
            {
                transaction.Rollback();
                return -1;
            }
        }

        public bool Update(Card entity)
        {
            _context.Cards.Update(entity);
            return _context.SaveChanges() > 0;
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
    }
}