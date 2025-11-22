namespace SmartDocs.Api.Controllers
{
    public class ChatRequest
    {
        public string DocumentId { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
    }
}
