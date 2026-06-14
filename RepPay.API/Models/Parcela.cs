using System;
using System.Collections.Generic;

namespace RepPay.API.Models;

public partial class Parcela
{
    public int IdParcela { get; set; }

    public decimal Valor { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public int IdUsuario { get; set; }

    public int IdDespesa { get; set; }

    public StatusParcela Status {  get; set; }

    public virtual Despesa IdDespesaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
