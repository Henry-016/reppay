using System;
using System.Collections.Generic;

namespace RepPay.API.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nome { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool Ativo { get; set; }

    public virtual ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();

    public virtual ICollection<Parcela> Parcelas { get; set; } = new List<Parcela>();
}
