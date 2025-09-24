namespace Banco.Application.Dtos
{
    public class ReportResultDTO
    {
        public IEnumerable<ReportDTO> Json { get; set; }
        public string PdfBase64 { get; set; }
    }
}
