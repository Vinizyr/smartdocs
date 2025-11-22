using System.Text;
using UglyToad.PdfPig;

namespace SmartDocs.Api.Services
{
    public class PdfTextExtractorService
    {
        public string ExtractText(Stream pdfStream)
        {
            if (pdfStream == null || pdfStream.Length == 0)
                return string.Empty;

            pdfStream.Position = 0;

            var sb = new StringBuilder();

            using (var pdf = PdfDocument.Open(pdfStream))
            {
                foreach (var page in pdf.GetPages())
                {
                    var text = page.Text;

                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }
            }

            return sb.ToString();
        }
    }
}

