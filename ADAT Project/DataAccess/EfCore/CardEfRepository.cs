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
            _context.Cards.Add(entity);
            _context.SaveChanges();
            return entity.CardId;
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
    }
}