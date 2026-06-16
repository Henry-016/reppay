export const usuarioService = {
    cadastrar: async (dadosDoUsuario: any) => {
        const res = await fetch('http://localhost:5149/api/Usuario', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dadosDoUsuario)
        });

        if (!res.ok) {
            const erroData = await res.json().catch(() => ({}));
            throw new Error(erroData.mensagem || "Falha ao realizar o cadastro.")
        }

        return await res.json()
    },

    login: async (email: string, senha: string) => {
        const res = await fetch('http://localhost:5149/api/Usuario/Login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ Email: email, Senha: senha })
        })

        const dados = await res.json()

        if (!res.ok) {
            throw new Error(dados.mensagem || 'Erro ao realizar login. Verifique os seus dados.')
        }

        return dados

    },

    logOut: async (refreshToken: string, token: string) => {
        const res = await fetch('http://localhost:5149/api/Usuario/Logout', {
        method: 'POST', 
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify({ RefreshToken: refreshToken }) 
        
})
    
        if (!res.ok) {
            throw new Error("Falha ao Deslogar:")
        }

    }

}