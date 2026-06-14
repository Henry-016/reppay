export const despesaService = {
    buscarInadimplentes: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Despesa/Inadimplentes/${idGrupo}`, { 
            headers: { 'Authorization': `Bearer ${token}` }

        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar inadimplentes: ${res.status}`)

        }

        const dados = await res.json();
        return {
            totalAReceber: dados.totalAReceber || 0,
            listaInadimplentes: dados.listaInadimplentes || []
        }

    },

    buscarMinhasDividas: async (token: string) => {
        

    }

}

const buscarMinhasDividas = async () => {
    try {
        const res = await fetch(`http://localhost:5149/api/Despesa/MinhasDividas`, { headers: { 'Authorization': `Bearer ${token}` } });
        if (res.ok) {
            const dados = await res.json();
            setMinhaDivida(dados.totalDevido || 0);
        }
    } catch (error) { console.error(error); }
};