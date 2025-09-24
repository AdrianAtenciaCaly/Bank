using Banco.Application.Dtos;

namespace Banco.Application.Interfaces
{
    public interface IReportService
    {
        Task<ReportResultDTO> GetReportAsync(Guid customerId, DateTime start, DateTime end);

    }
}
