import { createContext, useState, useContext, useEffect, type ReactNode } from 'react';

interface Usuario {
    id: string;
    nome: string;
}

const AuthContext = createContext({
    token: null as string | null,
    usuario: null as Usuario | null,
    setAuth: (token: string | null, usuario: Usuario | null, refreshToken?: string) => {},
    logout: () => {}, 
    loading: true
    
})

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [token, setToken] = useState<string | null>(null);
    const [usuario, setUsuario] = useState<Usuario | null>(null)
    const [loading, setLoading] = useState(true)

    const logout = () => {
        setToken(null)
        setUsuario(null)
        localStorage.removeItem('token')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('usuario')
    }

    const setAuth = (novoToken: string | null, novoUsuario: Usuario | null, novoRefreshToken?: string) => {
        setToken(novoToken);
        setUsuario(novoUsuario);
        if (novoRefreshToken) localStorage.setItem('refreshToken', novoRefreshToken)
        if (novoUsuario) localStorage.setItem('usuario', JSON.stringify(novoUsuario))
    }

    useEffect(() => {
        const restaurarSessao = async () => {
            const savedRefreshToken = localStorage.getItem('refreshToken')
            const savedUsuario = localStorage.getItem('usuario')
            if (!savedRefreshToken) {
                setLoading(false)
                return
            }

            try {
                const res = await fetch('http://localhost:5149/api/Usuario/RefreshToken', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ refreshToken: savedRefreshToken })
                });

                if (res.ok) {
                    const dados = await res.json();
                    setAuth(dados.token, savedUsuario ? JSON.parse(savedUsuario) : null, dados.refreshToken)
                } else {
                    localStorage.removeItem('refreshToken')
                }
            } catch (err) {
                console.error(err)
            } finally {
                setLoading(false)
            }

        }
        restaurarSessao()
    }, [])

    return (
        <AuthContext.Provider value={{ token, loading, usuario, logout, setAuth }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);