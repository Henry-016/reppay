using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepPay.API.Models
{
	[Table("codigo_recuperacao")]
	public class CodigoRecuperacao
	{
		[Key]
		[Column("id_codigo")]
		public int IdCodigo { get; set; }

		[Column("codigo")]
		public string Codigo { get; set; }

		[Column("data_expiracao")]
		public DateTime DataExpiracao { get; set; }

		[Column("codigo_usado")]
		public bool CodigoUsado { get; set; }

		[Column("tentativas")]
		public int Tentativas { get; set; }

		[Column("id_usuario")]
		public int IdUsuario { get; set; }
	}
}