import { useState } from 'react'
import styles from './Cadastro.module.scss'
import Modal_FeedBack_ContaCriada from './Modal_FeedBack_ContaCriada'
import { useNavigate } from 'react-router-dom'

function Cadastro() {
  const [nome, setNome] = useState('')
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [confirmarsenha, setConfirmarSenha] = useState('')
  const [modal, setModal] = useState(false)
  const navigate = useNavigate()

  const cadastrar = (e: React.SubmitEvent) => {
    e.preventDefault()
    setModal(true);
  };

  return (
    <>
      <section id="tela_cadastro">
            <div id="inputs">
                <h1 id="reppay">RepPay</h1>
                <form onSubmit={cadastrar} id='formulario'>
                    <div id='caixaInputsFundo'>
                        <div id='caixaInputs'>
                            <h2>Crie sua conta</h2>
                            <div className='inputContainer'>
                                <p className='textoInput'>Nome Completo</p>
                                <input type="text"
                                    value={nome}
                                    onChange={(e) => setNome(e.target.value)}
                                    placeholder='Ex: Maria Silva'
                                    className='input'/>
                            </div>
                            <div className='inputContainer'>
                                <p className='textoInput'>Email</p>
                                <input type="text"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder='voce@exemplo.com'
                                    className='input'/>
                            </div>
                            <div className='inputContainer'>
                                <p className='textoInput'>Criar Senha</p>
                                <input type="text"
                                    value={senha}
                                    onChange={(e) => setSenha(e.target.value)}
                                    placeholder='••••••••'
                                    className='input'/>
                            </div>
                            <div className='inputContainer'>
                                <p className='textoInput'>Confirmar Senha</p>
                                <input type="text"
                                    value={confirmarsenha}
                                    onChange={(e) => setConfirmarSenha(e.target.value)}
                                    placeholder='••••••••'
                                    className='input'/>
                            </div>
                            <button id='cadastrar'>Cadastrar</button>
                            <p id="paragrafoEntrar">Já tem uma conta?{' '}<button onClick={() => navigate('/login')} id='entrar'>Entrar</button></p>
                        </div>
                    </div>
                </form>
            </div>
            <div id="imagem">
                <div id="textoImagemContainer">
                    <h2>
                        Divisão Automática e Justa
                    </h2>
                    <p>
                        Esqueça as planilhas e a calculadora. O sistema faz o rateio exato das contas entre os moradores instantaneamente a cada nova despesa.
                    </p>
                </div>
            </div>
            <Modal_FeedBack_ContaCriada isOpen={modal} onClose={() => navigate('/login')}/>
      </section>
    </>
  )
}

export default Cadastro
