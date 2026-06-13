import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import './index.scss'
import Cadastro from './pages/telas/Cadastro.js'
import Login from './pages/telas/Login.js'
import HomeGeral from './pages/telas/HomeGeral'
import Admin from './pages/telas/Admin.js'
import Morador from './pages/telas/Morador'

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
      <Routes>
        <Route path="/cadastro" element={<Cadastro />} />
        <Route path="/login" element={<Login />} />
        <Route path='/home' element={<HomeGeral />} />
        <Route path="/home/admin/:idGrupo" element={<Admin />} />
        <Route path="/home/morador/:idGrupo" element={<Morador />} />
        <Route path="/" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
)
