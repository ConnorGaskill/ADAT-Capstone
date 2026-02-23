using ADONet_Tools.ADONet.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ADONet_Tools.ADONet.Repositories
{
    public class CardRepository : IDataAccess<Card>
    {
        private readonly string _connectionString;

        private Card ReadCard(SqlDataReader reader)
        {
            return new Card
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

        private T ExecuteInTransaction<T>(Func<SqlConnection, SqlTransaction, T> work)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                T result = work(conn, tx);
                tx.Commit();
                return result;
            }
            catch (SqlException sqlEx)
            {
                tx.Rollback();
                throw new Exception("Database operation failed.", sqlEx);
            }
            catch (Exception)
            {
                tx.Rollback();
                throw;
            }
        }

        private int InefficientAddCardLogic(Card card, SqlConnection conn, SqlTransaction tx)
        {
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

                // 3. Insert Colors
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

                    using var getColorId = new SqlCommand(
                        "SELECT color_id FROM dbo.colors WHERE name = @Name;", conn, tx);
                    getColorId.Parameters.AddWithValue("@Name", color.Name);
                    int colorId = (int)getColorId.ExecuteScalar();

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

                // 4. Insert Types
                foreach (var type in card.Types)
                {
                    using var insertType = new SqlCommand(@"
            INSERT INTO dbo.cardtypes (name)
            SELECT @Name
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.cardtypes WHERE name = @Name
            );", conn, tx);
                    insertType.Parameters.AddWithValue("@Name", type);
                    insertType.ExecuteNonQuery();

                    using var getTypeId = new SqlCommand(
                        "SELECT cardtype_id FROM dbo.cardtypes WHERE name = @Name;", conn, tx);
                    getTypeId.Parameters.AddWithValue("@Name", type);
                    int typeId = (int)getTypeId.ExecuteScalar();

                    using var link = new SqlCommand(@"
            INSERT INTO dbo.card_cardtypes (card_id, cardtype_id)
            SELECT @CardId, @TypeId
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.card_cardtypes
                WHERE card_id = @CardId AND cardtype_id = @TypeId
            );", conn, tx);
                    link.Parameters.AddWithValue("@CardId", cardId);
                    link.Parameters.AddWithValue("@TypeId", typeId);
                    link.ExecuteNonQuery();
                }

                // 5. Insert Printings + Sets
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

                    using var getSetId = new SqlCommand(
                        "SELECT set_id FROM dbo.sets WHERE code = @Code;", conn, tx);
                    getSetId.Parameters.AddWithValue("@Code", printing.Set.Code);
                    int setId = (int)getSetId.ExecuteScalar();

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

                return cardId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add card '{card.Name}'", ex);
            }
        }

        private int AddCardLogic(Card card, SqlConnection conn, SqlTransaction tx)
        {
            try
            {
                int cardId;

                // 1. Get or insert card
                using (var cmd = new SqlCommand("GetCardIdOrInsert", conn, tx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Name", card.Name);
                    cmd.Parameters.AddWithValue("@ManaCost", (object?)card.ManaCost ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OracleText", (object?)card.OracleText ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Power", (object?)card.Power ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Toughness", (object?)card.Toughness ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rarity", (object?)card.Rarity ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsLegendary", card.IsLegendary);

                    var outputId = new SqlParameter("@CardId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputId);

                    cmd.ExecuteNonQuery();
                    cardId = (int)outputId.Value;
                }

                // 2. Link colors
                if (card.Colors.Count > 0)
                {
                    string colorNames = string.Join(",", card.Colors.Select(c => c.Name));

                    using var cmd = new SqlCommand("LinkCardColors", conn, tx);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CardId", cardId);
                    cmd.Parameters.AddWithValue("@ColorNames", colorNames);

                    cmd.ExecuteNonQuery();
                }

                // 3`[. Insert printings + sets
                foreach (var printing in card.Printings)
                {
                    int setId;

                    // Get or insert set
                    using (var cmd = new SqlCommand("GetSetIdOrInsert", conn, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Code", printing.Set.Code);
                        cmd.Parameters.AddWithValue("@Name", printing.Set.Name);

                        var outputSetId = new SqlParameter("@SetId", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputSetId);

                        cmd.ExecuteNonQuery();
                        setId = (int)outputSetId.Value;
                    }

                    // Link printing
                    using (var cmd = new SqlCommand("LinkCardPrinting", conn, tx))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CardId", cardId);
                        cmd.Parameters.AddWithValue("@SetId", setId);
                        cmd.Parameters.AddWithValue("@CollectorNumber", printing.CollectorNumber);

                        cmd.ExecuteNonQuery();
                    }
                }

                return cardId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add card '{card.Name}' via stored procedures", ex);
            }
        }

        public CardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Add(Card card)
        {
            try
            {
                return ExecuteInTransaction((conn, tx) => AddCardLogic(card, conn, tx));
            }
            catch (Exception ex)
            {
                throw new Exception($"Add operation failed for card '{card.Name}'", ex);
            }
        }

        public int InefficientAdd(Card card)
        {
            try
            {
                return ExecuteInTransaction((conn, tx) => InefficientAddCardLogic(card, conn, tx));
            }
            catch (Exception ex)
            {
                throw new Exception($"InefficientAdd operation failed for card '{card.Name}'", ex);
            }
        }

        public int AddWithAudit(Card card)
        {
            try
            {
                return ExecuteInTransaction((conn, tx) =>
                {
                    int cardId = AddCardLogic(card, conn, tx);

                    var auditRepo = new AuditRepository(_connectionString);
                    auditRepo.InsertAuditRecord(cardId, "Card", "Insert", conn, tx);

                    return cardId;
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"AddWithAudit failed for card '{card.Name}'", ex);
            }
        }

        public bool Delete(int id)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand(
                    @"DELETE FROM dbo.Cards
            WHERE card_id = @CardId;", conn);

                cmd.Parameters.Add("@CardId", SqlDbType.Int).Value = id;

                conn.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete card with ID {id}", ex);
            }
        }

        public IEnumerable<Card> GetAll()
        {
            try
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
                                Card c = ReadCard(reader);
                                results.Add(c);
                            }
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve all cards", ex);
            }
        }

        public IEnumerable<Card> GetAllFull()
        {
            try
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
                    var card = ReadCard(reader);

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
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve full card data", ex);
            }
        }

        public Card? GetById(int id)
        {
            try
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
                                result = ReadCard(reader);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve card with ID {id}", ex);
            }
        }

        public bool Update(Card card)
        {
            try
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
                cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = card.Name;
                cmd.Parameters.Add("@ManaCost", SqlDbType.VarChar).Value =
                    (object?)card.ManaCost ?? DBNull.Value;
                cmd.Parameters.Add("@Power", SqlDbType.VarChar).Value =
                    (object?)card.Power ?? DBNull.Value;
                cmd.Parameters.Add("@Toughness", SqlDbType.VarChar).Value =
                    (object?)card.Toughness ?? DBNull.Value;
                cmd.Parameters.Add("@Rarity", SqlDbType.VarChar).Value =
                    (object?)card.Rarity ?? DBNull.Value;
                cmd.Parameters.Add("@IsLegendary", SqlDbType.Bit).Value = card.IsLegendary;

                conn.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update card '{card.Name}'", ex);
            }
        }

        public void ResetCardTable()
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand("ResetCardTable", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to reset card table", ex);
            }
        }
    }
}
