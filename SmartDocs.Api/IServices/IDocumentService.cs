using SmartDocs.Api.Models;

namespace SmartDocs.Api.IServices
{
    public interface IDocumentService
    {
        Task<List<VectorChunk>> GetChunksByDocumentId(string documentId);
    }
}
