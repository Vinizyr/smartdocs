using System.Text;

namespace SmartDocs.Api.Services
{
    public class TextChunkerService
    {
        /// <summary>
        /// Divide o texto em chunks aproximados por tamanho (caracteres).
        /// </summary>
        public List<string> ChunkText(string text, int maxChunkSize = 1000)
        {
            var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => p.Trim())
                                 .Where(p => !string.IsNullOrEmpty(p))
                                 .ToList();

            var chunks = new List<string>();
            var cur = new StringBuilder();

            foreach (var p in paragraphs)
            {
                if (cur.Length + p.Length > maxChunkSize && cur.Length > 0)
                {
                    chunks.Add(cur.ToString().Trim());
                    cur.Clear();
                }

                if (p.Length > maxChunkSize)
                {
                    // quebra parágrafos muito longos
                    for (int i = 0; i < p.Length; i += maxChunkSize)
                    {
                        var sub = p.Substring(i, Math.Min(maxChunkSize, p.Length - i));
                        chunks.Add(sub.Trim());
                    }
                }
                else
                {
                    cur.AppendLine(p);
                }
            }

            if (cur.Length > 0) chunks.Add(cur.ToString().Trim());
            return chunks;
        }
    }
}

