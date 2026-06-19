import { createContext, useState, useContext, useEffect, type ReactNode } from 'react';

const AuthContext = createContext({
    token: null as string | null,
    setAuth: (_token: string | null, _refreshToken?: string) => {},
    logout: () => {}, 
    loading: true
});

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [token, setToken] = useState<string | null>(null)
    const [loading, setLoading] = useState(true)

    const logout = () => {
        setToken(null)
        localStorage.removeItem('refreshToken')
    }

    const setAuth = (novoToken: string | null, novoRefreshToken?: string) => {
        setToken(novoToken)

        if (novoRefreshToken) {
            localStorage.setItem('refreshToken', novoRefreshToken)

        }

    }

    useEffect(() => {
        const restaurarSessao = async () => {
            const savedRefreshToken = localStorage.getItem('refreshToken')
            
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
                    const dados = await res.json()
                    setAuth(dados.token, dados.refreshToken)

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
        <AuthContext.Provider value={{ token, loading, logout, setAuth }}>
            {children}
        </AuthContext.Provider>
    )
}

export const useAuth = () => useContext(AuthContext)