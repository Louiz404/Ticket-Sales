namespace TicketSales.Models.ViewModels
{
    public class DashboardViewModel
    {

        public decimal FaturamentoTotal { get; set; }
        public int TotalIngressosVendidos { get; set; }
        public int TotalEventosAtivos { get; set; }
        public int TotalClientes { get; set; }

        public List<string> LabelsGrafico { get; set; } = new List<string>();
        public List<decimal> DadosGrafico { get; set; } = new List<decimal>();

    }
}
