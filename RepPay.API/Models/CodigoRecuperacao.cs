namespace RepPay.API.Models
{
    public class CodigoRecuperacao
    {
        public int IdCodigo { get; set; }
        public string Codigo { get; set; }
        public DateTime DataExpiracao { get; set; }
        public bool CodigoUsado { get; set; }
        public int Tentativas { get; set; }
        public int IdUsuario { get; set; }

        public virtual Usuario IdUsuarioNavigation { get; set; }
    }
}