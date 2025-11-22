namespace SmartDocs.Api.Models
{
    public class VectorChunk
    {
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
