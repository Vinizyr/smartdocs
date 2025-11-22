namespace SmartDocs.Api.Models
{
    public class VectorDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
