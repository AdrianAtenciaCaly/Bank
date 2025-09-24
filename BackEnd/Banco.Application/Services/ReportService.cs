using Banco.Application.Dtos;
using Banco.Application.Interfaces;
using Banco.Domain.Entities;
using Banco.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Banco.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _uow;

        public ReportService(IUnitOfWork uow) => _uow = uow;

        public async Task<ReportResultDTO> GetReportAsync(Guid customerId, DateTime start, DateTime end) {
            QuestPDF.Settings.License = LicenseType.Community;

            var repoAcc = _uow.Repository<Account>();
            var repoMov = _uow.Repository<Transaction>();
            var repoCus = _uow.Repository<Customer>();
            var customerQuery = repoCus.GetQueryable()
                                       .Include(c => c.Person);
            var customer = await customerQuery.FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer is null)
                return new ReportResultDTO
                {
                    Json = Array.Empty<ReportDTO>(),
                    PdfBase64 = null
                };



            var cuentas = (await repoAcc.FindAsync(c => ((Account)c).CustomerId == customerId))
                .Cast<Account>().ToList();

            var movimientos = (await repoMov.FindAsync(m =>
                ((Transaction)m).Date >= start && ((Transaction)m).Date <= end))
                .Cast<Transaction>().ToList();

            var report = cuentas.Select(c =>
            {
                var movsCuenta = movimientos.Where(m => m.AccountId == c.Id).ToList();
                return new ReportDTO
                {
                    Cliente = $"{customer.Person?.FirstName ?? ""} {customer.Person?.LastName ?? ""}".Trim(),

                    NumeroCuenta = c.AccountNumber,
                    Tipo = c.AccountType,
                    SaldoInicial = c.InitialBalance,
                    TotalDebitos = movsCuenta.Where(m => m.Amount < 0).Sum(m => Math.Abs(m.Amount)),
                    TotalCreditos = movsCuenta.Where(m => m.Amount > 0).Sum(m => m.Amount),
                    SaldoDisponible = movsCuenta.Any() ? movsCuenta.Last().BalanceAfter : c.CurrentBalance
                };
            }).ToList();


            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Header().Text("Estado de Cuenta").FontSize(18).Bold().AlignCenter();
                    page.Content().Column(column =>
                    {
                        column.Item().Text($"Cliente: {customer.Person?.FirstName ?? ""} {customer.Person?.LastName ?? ""}").FontSize(14).Bold();

                        column.Item().Text($"Periodo: {start:yyyy-MM-dd} a {end:yyyy-MM-dd}").FontSize(12);

                        column.Item().PaddingBottom(10);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(120); // Cuenta
                                cols.ConstantColumn(80);  // Tipo
                                cols.ConstantColumn(80);  // Débitos
                                cols.ConstantColumn(80);  // Créditos
                                cols.ConstantColumn(80);  // Saldo final
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Cuenta").Bold();
                                header.Cell().Text("Tipo").Bold();
                                header.Cell().Text("Débitos").Bold().AlignRight();
                                header.Cell().Text("Créditos").Bold().AlignRight();
                                header.Cell().Text("Saldo Final").Bold().AlignRight();
                            });

                            foreach (var r in report)
                            {
                                table.Cell().Text(r.NumeroCuenta);
                                table.Cell().Text(r.Tipo);
                                table.Cell().AlignRight().Text(r.TotalDebitos.ToString("F2"));
                                table.Cell().AlignRight().Text(r.TotalCreditos.ToString("F2"));
                                table.Cell().AlignRight().Text(r.SaldoDisponible.ToString("F2"));
                            }

                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(2).Text("Totales").Bold().AlignLeft();
                                footer.Cell().AlignRight().Text(report.Sum(r => r.TotalDebitos).ToString("F2")).Bold();
                                footer.Cell().AlignRight().Text(report.Sum(r => r.TotalCreditos).ToString("F2")).Bold();
                                footer.Cell().AlignRight().Text(report.Sum(r => r.SaldoDisponible).ToString("F2")).Bold();
                            });
                        });
                    });
                });
            }).GeneratePdf();

            var base64 = Convert.ToBase64String(pdfBytes);
            return new ReportResultDTO
            {
                Json = report,
                PdfBase64 = base64
            };
        }
    }
}
