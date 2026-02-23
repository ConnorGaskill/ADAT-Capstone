using ADONet_Tools.ADONet.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace ADONet_Tools.ADONet.Repositories
{
    public class AuditRepository
    {
        private readonly string _connectionString;

        public AuditRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertAuditRecord(int entityId, string entityType, string action, SqlConnection conn, SqlTransaction? tx = default)
        {
            bool internalTx = tx is null;

            tx ??= conn.BeginTransaction();

            try
            {
                using var cmd = new SqlCommand(@"
            INSERT INTO dbo.audit_log (entity_id, entity_type, action, created_at)
            VALUES (@Id, @EntityType, @Action, GETUTCDATE());", conn, tx);

                cmd.Parameters.AddWithValue("@Id", entityId);
                cmd.Parameters.AddWithValue("@EntityType", entityType);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.ExecuteNonQuery();

                if (internalTx)
                    tx.Commit();
            }
            catch (Exception ex)
            {
                if (internalTx)
                {
                    tx.Rollback();
                }
                throw;
            }
        }

        public IEnumerable<AuditLog> GetAll()
        {
            var results = new List<AuditLog>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT audit_id, entity_id, entity_type, action, created_at FROM dbo.audit_log;", conn);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new AuditLog
                {
                    AuditId = reader.GetInt32(0),
                    EntityId = reader.GetInt32(1),
                    EntityType = reader.GetString(2),
                    Action = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }

            return results;
        }

        public void TruncateAuditTable()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("TRUNCATE TABLE dbo.audit_log;", conn);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<AuditLog> GetAuditLogsForCard(int cardId)
        {
            var results = new List<AuditLog>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT audit_id, entity_id, entity_type, action, created_at
                FROM dbo.audit_log
                WHERE entity_id = @CardId AND entity_type = 'Card';", conn);
            cmd.Parameters.AddWithValue("@CardId", cardId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new AuditLog
                {
                    AuditId = reader.GetInt32(0),
                    EntityId = reader.GetInt32(1),
                    EntityType = reader.GetString(2),
                    Action = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }

            return results;
        }

        public void TruncateAuditLog()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("TRUNCATE TABLE dbo.audit_log;", conn);
            cmd.ExecuteNonQuery();
        }
    }
}
