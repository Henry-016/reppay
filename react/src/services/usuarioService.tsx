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
            const erroBackend = await res.json().catch(() => ({}));
        
            throw new Error(erroBackend.mensagem || "Falha ao realizar o login. Verifique seus dados.")

        }

        return dados

    },

    validarEmail: async (email: string) => {
        const res = await fetch('http://localhost:5149/api/Usuario/EsqueciSenha', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },

            body: JSON.stringify({ Email: email }) 
        });

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({}));
            throw new Error(erroBackend.mensagem || "Erro ao solicitar a recuperação.");
        }

        return true

    },

    validarCodigo: async (email: string, codigo: string) => {
        const res = await fetch('http://localhost:5149/api/Usuario/ValidarCodigo', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ Email: email, Codigo: codigo })

        })

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({}))
            throw new Error(erroBackend.mensagem || "Erro ao validar o código.");
        }

        return true
    },

    resetarSenha: async (dadosReset: any) => {
        const res = await fetch('http://localhost:5149/api/Usuario/ResetarSenha', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dadosReset)

        })

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({}));
            throw new Error(erroBackend.mensagem || "Erro ao redefinir a senha.")
            
        }

        return true

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

    },

    meuPerfil: async (token: string) => {
        const res = await fetch('http://localhost:5149/api/Usuario/MeuPerfil', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            }

        })

        if (!res.ok) {
            throw new Error(`Erro ao buscar os dados do usuário: ${res.status}`)

        }

        return await res.json()

    },

    atualizar: async (token: string, dados: any) => {
        const res = await fetch(`http://localhost:5149/api/Usuario/Atualizar`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(dados)

        })

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({})); 
            throw new Error(erroBackend.mensagem || "Erro ao atualizar os dados do perfil.")

        }

        return true

    },

    excluirConta: async (token: string) => {
        const res = await fetch('http://localhost:5149/api/Usuario/Deletar', {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }

        })

        if (!res.ok) {
            const erroBackend = await res.json().catch(() => ({})); 
            throw new Error(erroBackend.mensagem || "Erro ao deletar Usuário.")

        }

        return await res.json()

    }

}