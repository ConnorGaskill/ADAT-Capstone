using ADAT_Project.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

        public int Add(Card card)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            using SqlTransaction tx = conn.BeginTransaction();

            try
            {
                // 1. Insert card
                int cardId;
                using (SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO dbo.Cards
              (name, mana_cost, oracle_text, power, toughness, rarity, is_legendary)
              VALUES
              (@Name, @ManaCost, @OracleText, @Power, @Toughness, @Rarity, @IsLegendary);
              SELECT CAST(SCOPE_IDENTITY() AS int);",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Name", card.Name);
                    cmd.Parameters.AddWithValue("@ManaCost", (object?)card.ManaCost ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OracleText", (object?)card.OracleText ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Power", (object?)card.Power ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Toughness", (object?)card.Toughness ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rarity", (object?)card.Rarity ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsLegendary", card.IsLegendary);

                    cardId = (int)cmd.ExecuteScalar();
                }

                // 2. Insert colors
                foreach (var color in card.Colors)
                {
                    using SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO dbo.Card_Colors (card_id, color_id)
                  SELECT @CardId, color_id FROM dbo.Colors WHERE name = @ColorName;",
                        conn, tx);

                    cmd.Parameters.AddWithValue("@CardId", cardId);
                    cmd.Parameters.AddWithValue("@ColorName", color.Name);

                    cmd.ExecuteNonQuery();
                }

                // 3. Insert types
                foreach (var type in card.Types)
                {
                    using SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO dbo.Card_Types (card_id, type_id)
                  SELECT @CardId, type_id FROM dbo.Types WHERE name = @TypeName;",
                        conn, tx);

                    cmd.Parameters.AddWithValue("@CardId", cardId);
                    cmd.Parameters.AddWithValue("@TypeName", type);

                    cmd.ExecuteNonQuery();
                }

                // 4. Insert printings
                foreach (var printing in card.Printings)
                {
                    using SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO dbo.Card_Printings (card_id, set_id, collector_number)
                  SELECT @CardId, set_id, @CollectorNumber
                  FROM dbo.Sets WHERE code = @SetCode;",
                        conn, tx);

                    cmd.Parameters.AddWithValue("@CardId", cardId);
                    cmd.Parameters.AddWithValue("@SetCode", printing.Set.Code);
                    cmd.Parameters.AddWithValue("@CollectorNumber", printing.CollectorNumber);

                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                return cardId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }


        public bool Delete(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(
                @"DELETE FROM dbo.Cards
            WHERE card_id = @CardId;", conn);

            cmd.Parameters.Add("@CardId", SqlDbType.Int).Value = id;

            conn.Open();
            return cmd.ExecuteNonQuery() == 1;
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

        public IEnumerable<Card> GetAllFull()
        {
            var cards = new List<Card>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("GetAllCardsFull", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var card = new Card
                {
                    CardId = reader.GetInt32(reader.GetOrdinal("card_id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    ManaCost = reader["mana_cost"] as string,
                    OracleText = reader["oracle_text"] as string,
                    Power = reader["power"] as string,
                    Toughness = reader["toughness"] as string,
                    Rarity = reader["rarity"] as string,
                    IsLegendary = reader.GetBoolean(reader.GetOrdinal("is_legendary"))
                };

                card.Colors = (reader["colors"] as string)?
                    .Split(", ").Select(c => new Color { Name = c }).ToList()
                    ?? new();

                card.Types = (reader["types"] as string)?
                    .Split(", ").ToList()
                    ?? new();

                cards.Add(card);
            }

            return cards;
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

        public bool Update(Card card)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(
            @"UPDATE dbo.cards
         SET
          name = @Name,
          mana_cost = @ManaCost,
          power = @Power,
          toughness = @Toughness,
          rarity = @Rarity,
          is_legendary = @IsLegendary
         WHERE card_id = @CardId;", conn);

            cmd.Parameters.Add("@CardId", SqlDbType.Int).Value = card.CardId;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar, 255).Value = card.Name;
            cmd.Parameters.Add("@ManaCost", SqlDbType.VarChar, 50).Value =
                (object?)card.ManaCost ?? DBNull.Value;
            cmd.Parameters.Add("@Power", SqlDbType.VarChar, 10).Value =
                (object?)card.Power ?? DBNull.Value;
            cmd.Parameters.Add("@Toughness", SqlDbType.VarChar, 10).Value =
                (object?)card.Toughness ?? DBNull.Value;
            cmd.Parameters.Add("@Rarity", SqlDbType.VarChar, 20).Value =
                (object?)card.Rarity ?? DBNull.Value;
            cmd.Parameters.Add("@IsLegendary", SqlDbType.Bit).Value = card.IsLegendary;

            conn.Open();
            return cmd.ExecuteNonQuery() == 1;
        }


        public void ResetCardTable()
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand("ResetCardTable", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
