import { createContext, useState, useContext, ReactNode } from 'react';

interface Usuario {
    id: string;
    nome: string;
}

const AuthContext = createContext({
    token: null as string | null,
    usuario: null as Usuario | null,
    setAuth: (token: string | null, usuario: Usuario | null) => {},
});

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [token, setToken] = useState<string | null>(null);
    const [usuario, setUsuario] = useState<Usuario | null>(null);

    const setAuth = (token: string | null, usuario: Usuario | null) => {
        setToken(token);
        setUsuario(usuario);
    };

    return (
        <AuthContext.Provider value={{ token, usuario, setAuth }}>
            {children}
        </AuthContext.Provider>
    )
}

export const useAuth = () => useContext(AuthContext);