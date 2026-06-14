import { useState } from 'react'
import styles from './Login.module.scss'
import { useNavigate } from 'react-router-dom';
import { useAuth } from './../../context/AuthContext'

function Login() {
    const [email, setEmail] = useState('')
    const [senha, setSenha] = useState('')
    const [erro, setErro] = useState('')
    const navigate = useNavigate()
    const { setAuth } = useAuth()

    const fazerLogin = async (e: React.SubmitEvent) => {
        e.preventDefault()
        if (!email || !senha) {
            setErro('Por favor, preencha o e-mail e a senha.');
            return;
        }

        if (!email.includes('@') || !email.includes('.')) {
            setErro('Por favor, insira um e-mail válido.');
            return;
        }

        setErro('');

        try {
            const resposta = await fetch('http://localhost:5149/api/Usuario/Login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    Email: email,
                    Senha: senha
                })
            });

            const dados = await resposta.json();

            if (resposta.ok) {
                setAuth(dados.token, { id: dados.idUsuario, nome: dados.nome }, dados.refreshToken)
                navigate('/home') 
            } else {
                setErro(dados.mensagem || 'Erro ao realizar login. Verifique os seus dados.');
            }

        } catch (error) {
            console.error('Erro na requisição:', error);
            setErro('Não foi possível ligar ao servidor. Tente novamente mais tarde.');
        }
    }

  return (
    <>
        <section className={styles.tela_login}>
            <div className={styles.inputs}>
                <h1 className={styles.reppay}>RepPay</h1>
                {erro && <div className={styles.caixaErro}>{erro}</div>}
                <form onSubmit={fazerLogin}>
                    <div className={styles.caixaInputsFundo}>
                        <div className={styles.caixaInputs}>
                            <h2>Boas-vindas</h2>
                            <p className={styles.facaLogin}>Faça login para acessar sua conta.</p>
                            <div className={styles.inputContainer}>
                                <p className={styles.textoInput}>Email</p>
                                <input type="text"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder='voce@exemplo.com'
                                    className={`${styles.input} ${styles.emailInput}
                                    `} onFocus={() => setErro('')}/>
                            </div>
                            <div className={styles.inputContainer}>
                                <div className={styles.senhaContainer}>
                                    <p className={styles.textoInput}> Senha</p>
                                    <button className={styles.esqueceu} type='button'>esqueceu?</button>
                                </div>
                                <input type="password"
                                    value={senha}
                                    onChange={(e) => setSenha(e.target.value)}
                                    placeholder='••••••••'
                                    className={`${styles.input} ${styles.senhaInput}`} onFocus={() => setErro('')}/>
                            </div>
                            <button className={styles.entrar} type='submit' >Entrar</button>
                            <p className={styles.paragrafoCadastrar}>Não tem uma conta?{' '}<button className={styles.cadastro} type='button' onClick={() => navigate('/cadastro')}>Cadastre-se</button></p>
                        </div>
                    </div>
                </form>
            </div>
            <div className={styles.imagem}>
                <div className={styles.textoImagemContainer}>
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
