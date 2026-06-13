namespace RepPay.API.DTOs
{
	public class AnaliseMoradorDTO
	{
		public int IdParcela { get; set; }
		public string NomeDespesa { get; set; } = string.Empty;
		public string? Icone { get; set; }
		public decimal Valor { get; set; }
		public DateOnly Vencimento { get; set; }
		public DateOnly? DataSinalizacao { get; set; }
	}

	public class AnaliseAdminDTO
	{
		public int IdParcela { get; set; }
        public string? Icone { get; set; }
        public string NomeMorador { get; set; } = string.Empty;
		public string NomeDespesa { get; set; } = string.Empty;
		public decimal Valor { get; set; }
		public DateOnly? DataSinalizacao { get; set; }
	}

	public class HistoricoGrupoDTO
	{
		public int IdParcela { get; set; }
        public string? Icone { get; set; }
        public string NomeMorador { get; set; } = string.Empty;
		public string NomeDespesa { get; set; } = string.Empty;
		public decimal ValorPago { get; set; }
		public DateOnly? DataPagamento { get; set; }
		public DateOnly Vencimento { get; set; }
	}
}