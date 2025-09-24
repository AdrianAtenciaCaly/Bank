namespace Banco.Application.Dtos
{
    public class ReportDTO
    {
        public string Cliente { get; set; }
        public string NumeroCuenta { get; set; }
        public string Tipo { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalDebitos { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal SaldoDisponible { get; set; }
    }
}
