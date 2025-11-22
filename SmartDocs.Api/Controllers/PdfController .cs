using Microsoft.AspNetCore.Mvc;
using SmartDocs.Api.Services;

namespace SmartDocs.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfTextExtractorService _pdfService;

        public PdfController(PdfTextExtractorService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpPost("upload")]
        public IActionResult UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            using var stream = file.OpenReadStream();
            var text = _pdfService.ExtractText(stream);

            return Ok(new
            {
                success = true,
                extractedText = text
            });
        }
    }
}
