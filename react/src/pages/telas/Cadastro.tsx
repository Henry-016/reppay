import { useState} from 'react'
import styles from './Cadastro.module.scss'
import Modal_FeedBack_ContaCriada from './../modais/Modal_FeedBack_ContaCriada'
import { useNavigate } from 'react-router-dom'
import { usuarioService } from '../../services/usuarioService'

function Cadastro() {
    const [nome, setNome] = useState('')
    const [email, setEmail] = useState('')
    const [senha, setSenha] = useState('')
    const [confirmarsenha, setConfirmarSenha] = useState('')
    const [modal, setModal] = useState(false)
    const [erro, setErro] = useState('')
    const navigate = useNavigate()

    const cadastrar = async (e: React.SubmitEvent) => {
        e.preventDefault()

        if (!nome) {
            setErro('Por favor, preencha o nome.')
            return

        }

        if (!email.includes('@') || !email.includes('.')) {
            setErro('Por favor, insira um e-mail válido.')
            return
        }

        if (!senha) {
            setErro('Por favor, preencha a senha.')
            return
        }

        if (senha !== confirmarsenha) {
            setErro('As senhas não coincidem!')
            return
        }

        const dadosDoUsuario = {
            nome: nome,
            email: email,
            senha: senha
        }

        try {
            await usuarioService.cadastrar(dadosDoUsuario);
            setModal(true)
            
        } catch (error: any) {
            console.error(error)
            setErro(error)

        }
    }  


  return (
    <>
        <section className={styles.tela_cadastro}>
            <div className={styles.inputs}>
                <h1 className={styles.reppay}>RepPay</h1>
                {erro && <div className={styles.caixaErro}>{erro}</div>}
                <form onSubmit={cadastrar} className={styles.formulario}>
                    <div className={styles.caixaInputsFundo}>
                        <div className={styles.caixaInputs}>
                            <h2>Crie sua conta</h2>
                            
                            <div className={styles.inputContainer}>
                                <p className={styles.textoInput}>Nome Completo</p>
                                <input type="text"
                                    value={nome}
                                    onChange={(e) => setNome(e.target.value)}
                                    placeholder='Ex: Maria Silva'
                                    className={styles.input} 
                                    onFocus={() => setErro('')}/>
                            </div>
                            
                            <div className={styles.inputContainer}>
                                <p className={styles.textoInput}>Email</p>
                                <input type="text"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder='voce@exemplo.com'
                                    className={styles.input} 
                                    onFocus={() => setErro('')}/>
                            </div>
                            
                            <div className={styles.inputContainer}>
                                <p className={styles.textoInput}>Criar Senha</p>
                                <input type="password"
                                    value={senha}
                                    onChange={(e) => setSenha(e.target.value)}
                                    placeholder='••••••••'
                                    className={styles.input} 
                                    onFocus={() => setErro('')}/>
                            </div>
                            
                            <div className={styles.inputContainer}>
                                <p className={styles.textoInput}>Confirmar Senha</p>
                                <input type="password"
                                    value={confirmarsenha}
                                    onChange={(e) => setConfirmarSenha(e.target.value)}
                                    placeholder='••••••••'
                                    className={styles.input} 
                                    onFocus={() => setErro('')}/>
                            </div>
                            <button type="submit" className={styles.cadastrar}>Cadastrar</button>
                            
                            <p className={styles.paragrafoEntrar}>
                                Já tem uma conta?{' '}
                                <button type="button" onClick={() => navigate('/login')} className={styles.entrar}>
                                    Entrar
                                </button>
                            </p>
                        </div>
                    </div>
                </form>
            </div>
        
            <div className={styles.imagem}>
                <div className={styles.textoImagemContainer}>
                    <h2>Divisão Automática e Justa</h2>
                    <p>
                        Esqueça as planilhas e a calculadora. O sistema faz o rateio exato das contas entre os moradores instantaneamente a cada nova despesa.
                    </p>
                </div>
            </div>
                
            <Modal_FeedBack_ContaCriada isOpen={modal} onClose={() => navigate('/login')} />
        </section>
    </>
  )
}

export default Cadastro
