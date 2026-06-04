namespace RepPay.API.Models
{
    public class Pertence
    {
        public int IdUsuario { get; set; }
        public int IdGrupo { get; set; }

        public virtual Usuario IdUsuarioNavigation { get; set; }
        public virtual Grupo IdGrupoNavigation { get; set; }
    }
}