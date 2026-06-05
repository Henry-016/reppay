import { useState } from 'react'
import './Login.scss'
import { useNavigate } from 'react-router-dom';

function Login() {
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const navigate = useNavigate()

  const cadastrar = (e: React.SubmitEvent) => {
    e.preventDefault();
  };

  return (
    <>
      <section id="tela_login">
            <div id="inputs">
                <h1 id="reppay">RepPay</h1>
                <form onSubmit={cadastrar} id='formulario'>
                    <div id='caixaInputsFundo'>
                        <div id='caixaInputs'>
                            <h2>Boas-vindas</h2>
                            <p id='facaLogin'>Faça login para acessar sua conta.</p>
                            <div className='inputContainer'>
                                <p className='textoInput'>Email</p>
                                <input type="text"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder='voce@exemplo.com'
                                    className='input' id='emailInput'/>
                            </div>
                            <div className='inputContainer'>
                                <div id='senhaContainer'>
                                    <p className='textoInput'> Senha</p>
                                    <button id='esqueceu' type='button'>esqueceu?</button>
                                </div>
                                <input type="text"
                                    value={senha}
                                    onChange={(e) => setSenha(e.target.value)}
                                    placeholder='••••••••'
                                    className='input' id='senhaInput'/>
                            </div>
                            <button id='entrar' type='submit'>Entrar</button>
                            <p id="paragrafoCadastrar">Não tem uma conta?{' '}<button id='cadastro' type='button' onClick={() => navigate('/cadastro')}>Cadastre-se</button></p>
                        </div>
                    </div>
                </form>
            </div>
            <div id="imagem">
                <div id="textoImagemContainer">
                    <h2>
                        Fim das Cobranças Chatas
                    </h2>
                    <p>
                        Diga adeus aos atritos no grupo do WhatsApp. Acompanhe o saldo devedor de cada membro em tempo real e deixe a gestão financeira muito mais leve.
                    </p>
                </div>
            </div>
      </section>
    </>
  )
}

export default Login
