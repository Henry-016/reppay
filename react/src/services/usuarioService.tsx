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
            throw new Error(erroData.mensagem || "Falha ao realizar o cadastro.");
        }

        return await res.json();
    }
};