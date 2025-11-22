using SmartDocs.Api.Models;

namespace SmartDocs.Api.IServices
{
    public interface IVectorStore
    {
        Task UpsertAsync(VectorDocument doc);
        Task<List<VectorDocument>> QueryAsync(float[] queryEmbedding, int topK = 3);
        Task<List<VectorChunk>> GetChunksByDocumentIdAsync(string documentId);
    }
}
