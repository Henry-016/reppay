import { AuthProvider } from './context/AuthContext';
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import './index.scss'
import Cadastro from './pages/telas/Cadastro.js'
import Login from './pages/telas/Login.js'
import HomeGeral from './pages/telas/HomeGeral'
import Admin from './pages/telas/Admin.js'
import Morador from './pages/telas/Morador'
import ValidarEmail from './pages/telas/ValidarEmail';
import ValidarCodigo from './pages/telas/ValidarCodigo';
import NovaSenha from './pages/telas/NovaSenha';

createRoot(document.getElementById('root')!).render(
  <AuthProvider>
    <BrowserRouter>
        <Routes>
          <Route path="/cadastro" element={<Cadastro />} />
          <Route path="/login" element={<Login />} />
          <Route path="/validarEmail" element={<ValidarEmail />} />
          <Route path="/validarCodigo" element={<ValidarCodigo />} />
          <Route path="/novaSenha" element={<NovaSenha />} />
          <Route path='/home' element={<HomeGeral />} />
          <Route path="/home/admin/:idGrupo" element={<Admin />} />
          <Route path="/home/morador/:idGrupo" element={<Morador />} />
          <Route path="/" element={<Navigate to="/login" replace />} />
          
        </Routes>
      </BrowserRouter>
  </AuthProvider>
  
)
