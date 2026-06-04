import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.scss'
import './pages/global.scss'
import App from './pages/App.js'
import Cadastro from './pages/Cadastro.js'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Cadastro />

  </StrictMode>,
)
