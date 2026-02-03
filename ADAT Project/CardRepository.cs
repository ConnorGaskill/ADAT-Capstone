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
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Insert card if not exists
                using (var cmd = new SqlCommand(@"
            INSERT INTO dbo.cards
            (name, mana_cost, oracle_text, power, toughness, rarity, is_legendary)
            SELECT @Name, @ManaCost, @OracleText, @Power, @Toughness, @Rarity, @IsLegendary
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.cards
                WHERE name = @Name
                  AND ISNULL(oracle_text, '') = ISNULL(@OracleText, '')
            );", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Name", card.Name);
                    cmd.Parameters.AddWithValue("@ManaCost", (object?)card.ManaCost ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OracleText", (object?)card.OracleText ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Power", (object?)card.Power ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Toughness", (object?)card.Toughness ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rarity", (object?)card.Rarity ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsLegendary", card.IsLegendary);
                    cmd.ExecuteNonQuery();
                }

                // 2. Get CardId
                int cardId;
                using (var cmd = new SqlCommand(@"
            SELECT card_id
            FROM dbo.cards
            WHERE name = @Name
              AND ISNULL(oracle_text, '') = ISNULL(@OracleText, '');", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@Name", card.Name);
                    cmd.Parameters.AddWithValue("@OracleText", (object?)card.OracleText ?? DBNull.Value);
                    cardId = (int)cmd.ExecuteScalar();
                }

                // 3. Colors
                foreach (var color in card.Colors)
                {
                    using var insertColor = new SqlCommand(@"
                INSERT INTO dbo.colors (name)
                SELECT @Name
                WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.colors WHERE name = @Name
                );", conn, tx);

                    insertColor.Parameters.AddWithValue("@Name", color.Name);
                    insertColor.ExecuteNonQuery();

                    int colorId;
                    using var getColorId = new SqlCommand(
                        "SELECT color_id FROM dbo.colors WHERE name = @Name;", conn, tx);
                    getColorId.Parameters.AddWithValue("@Name", color.Name);
                    colorId = (int)getColorId.ExecuteScalar();

                    using var link = new SqlCommand(@"
                INSERT INTO dbo.card_colors (card_id, color_id)
                SELECT @CardId, @ColorId
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.card_colors
                    WHERE card_id = @CardId AND color_id = @ColorId
                );", conn, tx);

                    link.Parameters.AddWithValue("@CardId", cardId);
                    link.Parameters.AddWithValue("@ColorId", colorId);
                    link.ExecuteNonQuery();
                }

                // 4. Types
                foreach (var type in card.Types)
                {
                    using var insertType = new SqlCommand(@"
                INSERT INTO dbo.types (name)
                SELECT @Name
                WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.types WHERE name = @Name
                );", conn, tx);

                    insertType.Parameters.AddWithValue("@Name", type);
                    insertType.ExecuteNonQuery();

                    int typeId;
                    using var getTypeId = new SqlCommand(
                        "SELECT type_id FROM dbo.types WHERE name = @Name;", conn, tx);
                    getTypeId.Parameters.AddWithValue("@Name", type);
                    typeId = (int)getTypeId.ExecuteScalar();

                    using var link = new SqlCommand(@"
                INSERT INTO dbo.card_types (card_id, type_id)
                SELECT @CardId, @TypeId
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.card_types
                    WHERE card_id = @CardId AND type_id = @TypeId
                );", conn, tx);

                    link.Parameters.AddWithValue("@CardId", cardId);
                    link.Parameters.AddWithValue("@TypeId", typeId);
                    link.ExecuteNonQuery();
                }

                // 5. Printings + Sets
                foreach (var printing in card.Printings)
                {
                    using var insertSet = new SqlCommand(@"
                INSERT INTO dbo.sets (code, name)
                SELECT @Code, @Name
                WHERE NOT EXISTS (
                    SELECT 1 FROM dbo.sets WHERE code = @Code
                );", conn, tx);

                    insertSet.Parameters.AddWithValue("@Code", printing.Set.Code);
                    insertSet.Parameters.AddWithValue("@Name", printing.Set.Name);
                    insertSet.ExecuteNonQuery();

                    int setId;
                    using var getSetId = new SqlCommand(
                        "SELECT set_id FROM dbo.sets WHERE code = @Code;", conn, tx);
                    getSetId.Parameters.AddWithValue("@Code", printing.Set.Code);
                    setId = (int)getSetId.ExecuteScalar();

                    using var insertPrinting = new SqlCommand(@"
                INSERT INTO dbo.card_printings (card_id, set_id, collector_number)
                SELECT @CardId, @SetId, @CollectorNumber
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.card_printings
                    WHERE card_id = @CardId
                      AND set_id = @SetId
                      AND collector_number = @CollectorNumber
                );", conn, tx);

                    insertPrinting.Parameters.AddWithValue("@CardId", cardId);
                    insertPrinting.Parameters.AddWithValue("@SetId", setId);
                    insertPrinting.Parameters.AddWithValue("@CollectorNumber", printing.CollectorNumber);
                    insertPrinting.ExecuteNonQuery();
                }

                tx.Commit();
                return cardId;
            }
            catch
            {
                tx.Rollback();
                return -1;
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
