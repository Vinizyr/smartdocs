using Microsoft.AspNetCore.Mvc;
using SmartDocs.Api.IServices;
using SmartDocs.Api.Models;
using SmartDocs.Api.Services;

namespace SmartDocs.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly PdfTextExtractorService _pdfService;
        private readonly AIService _ai;
        private readonly TextChunkerService _chunker;
        private readonly IVectorStore _vectors;

        public DocumentController(PdfTextExtractorService pdfService, AIService ai, TextChunkerService chunker, IVectorStore vectors)
        {
            _pdfService = pdfService;
            _ai = ai;
            _chunker = chunker;
            _vectors = vectors;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            using var stream = file.OpenReadStream();
            var text = _pdfService.ExtractText(stream);

            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("Não foi possível extrair texto do PDF.");

            // resumo + insights (pode ser pesado - responsável por demorar)
            var summaryTask = _ai.GenerateSummaryAsync(text);
            var insightsTask = _ai.GenerateInsightsAsync(text);

            // chunk + embeddings
            var chunks = _chunker.ChunkText(text, maxChunkSize: 1000);

            var docId = Guid.NewGuid().ToString();

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var emb = await _ai.GenerateEmbeddingAsync(chunk);
                var doc = new VectorDocument
                {
                    Id = $"{docId}:{i}",
                    Content = chunk,
                    Embedding = emb,
                    Metadata = new Dictionary<string, string>
                    {
                        { "documentId", docId },
                        { "chunkIndex", i.ToString() }
                    }
                };
                await _vectors.UpsertAsync(doc);
            }

            var summary = await summaryTask;
            var insights = await insightsTask;

            return Ok(new
            {
                success = true,
                documentId = docId,
                text,
                summary,
                insights,
                chunks = chunks.Count
            });
        }
    }
}
