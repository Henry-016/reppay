export const grupoService = {

    criarGrupo: async (nome: string, link: string, token: string) => {
        const res = await fetch('http://localhost:5149/api/Grupo', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` 
            },
            body: JSON.stringify({
                Nome: nome,
                ImagemBanner: link
            })
        });

        const dados = await res.json();

        if (!res.ok) {
            throw new Error(dados.mensagem || 'Erro ao criar a república.')

        }

        return dados
    },

    entrarGrupo: async (codigo: string, token: string) => {
        const res = await fetch('http://localhost:5149/api/Grupo/Entrar', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` 
            },
            body: JSON.stringify({
                CodigoAcesso: codigo
            })
        })

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({})); 
            throw new Error(erroBackend.mensagem || "Erro ao entrar em grupo.")

        }

        return await res.json()

    },

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

    },

    sairDoGrupo: async (idGrupo: string, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}/Sair`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }

        })

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({}))
            throw new Error(`Erro ao Sair do grupo: ${erroBackend.mensagem}`)

        }


    },

    transferirAdmin: async (idGrupo: string, idNovoAdmin: number, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}/TransferirAdmin/${idNovoAdmin}`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`

            }

        })

        if (!res.ok) {
            throw new Error(`Erro ao trocar o admin: ${res.status}`)

        }

        return await res.json()

    },

    expulsarMorador: async (idGrupo: string, idMorador: number, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}/Expulsar/${idMorador}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`

            }

        })

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({})); 
            
            const textoDoErro = erroBackend.mensagem || `Erro ao expulsar morador: ${res.status}`;
            
            throw new Error(textoDoErro);

        }

        return await res.json()

    },

    removerGrupo: async (idGrupo: number, token: string) => {
        const res = await fetch(`http://localhost:5149/api/Grupo/Deletar/${idGrupo}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`

            }

        })

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({})); 
            
            const textoDoErro = erroBackend.mensagem || `Erro ao deletar grupo: ${res.status}`;
            
            throw new Error(textoDoErro);

        }

        return await res.json()

    }

}

