using Microsoft.AspNetCore.Mvc;
using SmartDocs.Api.IServices;
using SmartDocs.Api.Services;

namespace SmartDocs.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AIService _ai;
        private readonly IVectorStore _vectors;

        public ChatController(AIService ai, IVectorStore vectors)
        {
            _ai = ai;
            _vectors = vectors;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.DocumentId) || string.IsNullOrWhiteSpace(req.Question))
                return BadRequest("documentId e question são obrigatórios.");

            // 1) embedding da pergunta
            var qEmb = await _ai.GenerateEmbeddingAsync(req.Question);

            // 2) pegar todos os chunks do documento
            var allChunks = await _vectors.GetChunksByDocumentIdAsync(req.DocumentId);
            if (allChunks == null || allChunks.Count == 0)
                return NotFound("Documento não encontrado ou sem conteúdo.");

            // 3) calcular TopK dinamicamente
            int totalChunks = allChunks.Count;
            int topK;

            if (totalChunks <= 3)
                topK = totalChunks;                    // documentos pequenos
            else if (totalChunks <= 10)
                topK = Math.Min(5, totalChunks);       // documentos médios
            else
                topK = Math.Min((int)Math.Ceiling(totalChunks * 0.3), 10); // documentos grandes

            // 4) buscar topK trechos mais relevantes
            var nearest = await _vectors.QueryAsync(qEmb, topK);

            // 5) montar contexto
            var context = string.Join("\n\n---\n\n", nearest.Select(n => n.Content));

            // 6) montar prompt
            var prompt = $@"Você é um assistente que responde com base no contexto fornecido abaixo. Seja conciso e cite as seções do documento quando possível.
            Contexto:{context}
            Pergunta:{req.Question}";

            // 7) gerar resposta
            var modelToUse = "llama3.2:1b"; // rápido; mude para 3b se desejar
            var answer = await _ai.GenerateFromPromptAsync(modelToUse, prompt);

            return Ok(new
            {
                answer,
                contextCount = nearest.Count,
                sources = nearest.Select(n => new
                {
                    chunkIndex = n.Metadata?["chunkIndex"],
                    preview = n.Content.Length > 200 ? n.Content.Substring(0, 200) : n.Content
                })
            });
        }

    }
}
