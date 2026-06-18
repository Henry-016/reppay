export const utilitarios = {
    formatarValor: (valor: number) => {
        const valorBRL = new Intl.NumberFormat('pt-BR', {
            style: 'currency',
            currency: 'BRL'
        }).format(valor)
        
        return valorBRL

    },

    copiarParaAreaDeTransferencia: async (grupo: any) => {
        if (grupo) await navigator.clipboard.writeText(grupo.codigoAcesso)

    }

}

