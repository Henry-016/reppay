export const despesaService = {

    lancarDespesa: async (dadosDespesa: any, token: string) => {
        const res = await fetch('http://localhost:5149/api/Despesa/LancarDespesa', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            
            body: JSON.stringify(dadosDespesa)

        });

        if (!res.ok) {
            const erroData = await res.json().catch(() => ({}));
            throw new Error(erroData.mensagem || 'Falha ao lançar despesa. Verifique as informações.')

        }

        return await res.json().catch(() => ({}))

    },

    buscarInadimplentes: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/Inadimplentes/${idGrupo}`, { 
            headers: { 'Authorization': `Bearer ${token}` }

        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar inadimplentes: ${res.status}`)

        }

        const dados = await res.json()
        return {
            totalAReceber: dados.totalAReceber || 0,
            listaInadimplentes: dados.listaInadimplentes || []
        }

    },

    buscarMinhasDividas: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/MinhasDividas/${idGrupo}`, {
            headers: { 'Authorization': `Bearer ${token}` }

        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar minhas dividas: ${res.status}`)

        }

        const dados = await res.json()

        return {
            totalDevido: dados.totalDevido || 0,
            listaDividas: dados.listaDividas || []

        }

    },

    buscarAnalises: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/AnalisesPendentes/${idGrupo}`, { 
            headers: { 
                'Authorization': `Bearer ${token}` 

            } 
    
        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar minhas dividas: ${res.status}`)

        }

        const dados = await res.json()

        return {
            listaAnalises: dados.listaAnalises || []

        }

    },

    buscarHistorico: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/HistoricoGrupo/${idGrupo}`, { 
            headers: { 
                'Authorization': `Bearer ${token}` 

            } 
    
        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar minhas dividas: ${res.status}`)

        }

        const dados = await res.json()

        return {
            listaHistorico: dados.listaHistorico || [],

        }

    },

    sinalizarPagamento: async (id: number, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/SinalizarPagamento/${id}`, {
            method: 'PUT',
            headers: { 
                'Authorization': `Bearer ${token}` 
            }

        })

        if (!res.ok) {
            throw new Error(`Erro ao Sinalizar o Pagamento: ${res.status}`)

        }

    },

    validarPagamento: async (id: number, decisao: boolean, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/ValidarPagamento/${id}`, {
            method: 'PUT',
            headers: { 
                'Content-Type': 'application/json', 
                'Authorization': `Bearer ${token}` 
            },
            body: JSON.stringify({ aprovado: decisao })
        });

        const resData = await res.json();

        if (!res.ok) {
            throw new Error(resData.mensagem || "Erro ao validar pagamento");
        }

        return resData.mensagem;

    },

    buscarAnalisesIndividuais: async (idGrupo: string,token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/MinhasAnalises/${idGrupo}`, { 
            headers: { 
                'Authorization': `Bearer ${token}` 

            } 
    
        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar minhas analises: ${res.status}`)

        }

        const dados = await res.json()

        return {
            listaAnalises: dados.listaAnalises || []

        }

    },

    buscarHistoricoIndividuais: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/HistoricoPago/${idGrupo}`, { 
            headers: { 
                'Authorization': `Bearer ${token}` 

            } 
    
        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar minhas dividas pagas: ${res.status}`)

        }

        const dados = await res.json()

        return dados

    },

    desfazerPagamento: async (idParcela: number, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/DesfazerSinalizacaoPagamento/${idParcela}`, {
            method: 'PUT',
            headers: { 
                'Authorization': `Bearer ${token}` 
            }
        });
    
        const resData = await res.json().catch(() => ({}));
    
        if (!res.ok) {
            
            throw new Error(resData.mensagem || "Erro ao desfazer a sinalização de pagamento.")
        }
    
        
        return resData.mensagem
    },

    quitarParcela: async (idParcela: number, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/QuitarDividaAdministrativamente/${idParcela}`, {
            method: 'PUT',
            headers: { 
                'Authorization': `Bearer ${token}` 
            }

        })

        if (!res.ok) {
            throw new Error(`Erro ao Sinalizar o Pagamento: ${res.status}`)

        }

    }

}
