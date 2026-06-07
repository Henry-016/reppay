import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import './index.scss'
import './pages/telas/global.scss'
import Cadastro from './pages/telas/Cadastro.js'
import Login from './pages/telas/Login.js'
import HomeGeral from './pages/telas/HomeGeral'

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
      <Routes>
        <Route path="/cadastro" element={<Cadastro />} />
        <Route path="/login" element={<Login />} />
        <Route path='/home' element={<HomeGeral />} />
        <Route path="/" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
)
