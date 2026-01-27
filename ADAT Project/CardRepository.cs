using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADAT_Project
{
    public class CardRepository : IDataAccess<Card>
    {
        private readonly string _connectionString;

        public CardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void Add(Card entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Card> GetAll()
        {
            List<Card> results = new List<Card>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT
                card_id,
                name,
                mana_cost,
                oracle_text,
                power,
                toughness,
                rarity,
                is_legendary
              FROM dbo.cards;", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Card c = new Card
                            {
                                CardId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ManaCost = reader.IsDBNull(2) ? null : reader.GetString(2),
                                OracleText = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Power = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Toughness = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Rarity = reader.IsDBNull(6) ? null : reader.GetString(6),
                                IsLegendary = reader.GetBoolean(7)
                            };

                            results.Add(c);
                        }
                    }
                }
            }

            return results;
        }

        public Card? GetById(int id)
        {
            Card? result = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT card_id,
                     name,
                     mana_cost,
                     oracle_text,
                     power,
                     toughness,
                     rarity,
                     is_legendary
              FROM dbo.cards
              WHERE card_id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new Card
                            {
                                CardId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ManaCost = reader.IsDBNull(2) ? null : reader.GetString(2),
                                OracleText = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Power = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Toughness = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Rarity = reader.IsDBNull(6) ? null : reader.GetString(6),
                                IsLegendary = reader.GetBoolean(7)
                            };
                        }
                    }
                }
            }

            return result;
        }


        public bool UpdateOrder(Card card)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                  @"UPDATE dbo.Cars
                  SET CustomerId = @CustomerId,
                  OrderDate = @OrderDate,
                  OrderStatus = @OrderStatus
              WHERE OrderId = @OrderId;", conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerId", order.CustomerId);
                    cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                    cmd.Parameters.AddWithValue("@OrderStatus", order.OrderStatus);
                    cmd.Parameters.AddWithValue("@OrderId", order.OrderId);

                    int rows = cmd.ExecuteNonQuery();
                    return rows == 1;
                }
            }
        }
    }
}
