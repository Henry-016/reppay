using System;
using System.Collections.Generic;

namespace RepPay.API.Models;

public partial class Grupo
{
    public int IdGrupo { get; set; }

    public string CodigoAcesso { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public string? ImagemBanner { get; set; }

    public int IdAdmin { get; set; }

    public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();

    public virtual Usuario IdAdminNavigation { get; set; } = null!;

    public virtual ICollection<Usuario> IdUsuarios { get; set; } = new List<Usuario>();
}
