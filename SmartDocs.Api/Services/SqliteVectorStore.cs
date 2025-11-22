namespace SmartDocs.Api.Services
{
    using global::SmartDocs.Api.IServices;
    using global::SmartDocs.Api.Models;
    using Microsoft.Data.Sqlite;
    using System.Text.Json;

    namespace SmartDocs.Api.Services
    {
        public class SqliteVectorStore : IVectorStore
        {
            private readonly string _dbFile;

            public SqliteVectorStore(IConfiguration config)
            {
                _dbFile = config.GetValue<string>("VectorStore:SqliteFile") ?? "smartdocs_vectors.db";
                Initialize();
            }

            private void Initialize()
            {
                using var conn = new SqliteConnection($"Data Source={_dbFile}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS vectors (
                    id TEXT PRIMARY KEY,
                    content TEXT,
                    embedding TEXT,
                    metadata TEXT
                );";
                cmd.ExecuteNonQuery();
            }

            public async Task UpsertAsync(VectorDocument doc)
            {
                using var conn = new SqliteConnection($"Data Source={_dbFile}");
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                INSERT OR REPLACE INTO vectors (id, content, embedding, metadata)
                VALUES ($id, $content, $embedding, $metadata);";
                cmd.Parameters.AddWithValue("$id", doc.Id);
                cmd.Parameters.AddWithValue("$content", doc.Content);
                cmd.Parameters.AddWithValue("$embedding", JsonSerializer.Serialize(doc.Embedding));
                cmd.Parameters.AddWithValue("$metadata", doc.Metadata is null ? "" : JsonSerializer.Serialize(doc.Metadata));
                await cmd.ExecuteNonQueryAsync();
            }

            public async Task<List<VectorDocument>> QueryAsync(float[] queryEmbedding, int topK = 3)
            {
                var list = new List<(VectorDocument doc, double score)>();

                using var conn = new SqliteConnection($"Data Source={_dbFile}");
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id, content, embedding, metadata FROM vectors;";
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var id = reader.GetString(0);
                    var content = reader.GetString(1);
                    var embJson = reader.GetString(2);
                    var metaJson = reader.GetString(3);

                    var emb = JsonSerializer.Deserialize<float[]>(embJson) ?? Array.Empty<float>();
                    var meta = string.IsNullOrEmpty(metaJson) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(metaJson);

                    var score = CosineSimilarity(queryEmbedding, emb);
                    var doc = new VectorDocument { Id = id, Content = content, Embedding = emb, Metadata = meta };
                    list.Add((doc, score));
                }

                var ordered = list.OrderByDescending(x => x.score)
                                  .Take(topK)
                                  .Select(x => x.doc)
                                  .ToList();
                return ordered;
            }

            private static double CosineSimilarity(float[] a, float[] b)
            {
                if (a == null || b == null || a.Length == 0 || b.Length == 0) return 0;
                int len = Math.Min(a.Length, b.Length);
                double dot = 0, na = 0, nb = 0;
                for (int i = 0; i < len; i++)
                {
                    dot += a[i] * b[i];
                    na += a[i] * a[i];
                    nb += b[i] * b[i];
                }
                if (na == 0 || nb == 0) return 0;
                return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
            }

            public async Task<List<VectorChunk>> GetChunksByDocumentIdAsync(string documentId)
            {
                var chunks = new List<VectorChunk>();

                using var conn = new SqliteConnection($"Data Source={_dbFile}");
                await conn.OpenAsync();

                string sql = "SELECT content, metadata FROM vectors WHERE id LIKE @DocumentId || '%';";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DocumentId", documentId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var content = reader.GetString(0);
                    var metadataJson = reader.IsDBNull(1) ? null : reader.GetString(1);

                    var metadata = string.IsNullOrEmpty(metadataJson)
                        ? null
                        : JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);

                    chunks.Add(new VectorChunk
                    {
                        Content = content,
                        Metadata = metadata
                    });
                }

                return chunks;
            }
        }
    }

}
