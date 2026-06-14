export const grupoService = {
    buscarGrupo: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}`, { 
            headers: { 'Authorization': `Bearer ${token}` } 

        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar grupo: ${res.status}`)

        }

        return await res.json();
    },

    buscarMoradores: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}/Membros`, { 
            headers: { 'Authorization': `Bearer ${token}` } 
        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar moradores: ${res.status}`)

        }

        return await res.json()
    },

    buscarProximaConta: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}/proximaConta`, { 
            headers: { 'Authorization': `Bearer ${token}` } 
        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar próxima conta: ${res.status}`)

        }

        const text = await res.text()

        if (text.trim() === "") {
            return undefined
        }

        return JSON.parse(text)

    },

    buscarGrupos: async (token: string) => {
        const res = await fetch('http://localhost:5149/api/Grupo/MeusGrupos', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            }

        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar os grupos: ${res.status}`)

        }

        return await res.json()

    }

}

