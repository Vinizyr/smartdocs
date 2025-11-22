using Microsoft.AspNetCore.Mvc;
using SmartDocs.Api.Payloads;
using SmartDocs.Api.Services;

namespace SmartDocs.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly AIService _ai;

        public AIController(AIService ai)
        {
            _ai = ai;
        }

        [HttpPost("summary")]
        public async Task<IActionResult> Summary([FromBody] SummaryRequest request)
        {
            var result = await _ai.GenerateSummaryAsync(request.Text);
            return Ok(new { summary = result });
        }

        [HttpPost("insights")]
        public async Task<IActionResult> Insights([FromBody] string text)
        {
            var result = await _ai.GenerateInsightsAsync(text);
            return Ok(new { insights = result });
        }
    }
}
