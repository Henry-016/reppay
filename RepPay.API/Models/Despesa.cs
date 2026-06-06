using System;
using System.Collections.Generic;

namespace RepPay.API.Models;

public partial class Despesa
{
    public int IdDespesa { get; set; }

    public DateOnly DataCadastro { get; set; }

    public DateOnly Vencimento { get; set; }

    public string Nome { get; set; } = null!;

    public decimal Valor { get; set; }

    public string? Icone { get; set; }

    public int IdGrupo { get; set; }

    public bool Ativo { get; set; } = true;

    public StatusDespesa Status { get; set; }

    public virtual Grupo IdGrupoNavigation { get; set; } = null!;

    public virtual ICollection<Parcela> Parcelas { get; set; } = new List<Parcela>();
}
