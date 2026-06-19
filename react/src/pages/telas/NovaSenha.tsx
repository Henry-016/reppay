import { useState} from 'react'
import styles from './NovaSenha.module.scss'
import { useLocation, useNavigate } from 'react-router-dom'
import { usuarioService } from '../../services/usuarioService'
import back from './../../assets/arrow_back.svg'

function NovaSenha() {
    const [senha, setSenha] = useState('')
    const [confirmarSenha, setConfirmarSenha] = useState('')
    const [erro, setErro] = useState('')
    
    const navigate = useNavigate()
    const location = useLocation()
    
    const email = location.state?.email;
    const codigo = location.state?.codigo

    if (!email) {
        navigate('/verificarEmail')
        return

    }

    const salvarNovaSenha = async (e: React.SubmitEvent) => {
        e.preventDefault()

        if (senha.length < 8) {
            setErro('A senha deve ter no mínimo 8 caracteres.')
            return
        }

        if (senha !== confirmarSenha) {
            setErro('As senhas não coincidem!')
            return
        }

        try {

            const dadosReset = {
                Email: email,
                Codigo: codigo,
                NovaSenha: senha
            }

            await usuarioService.resetarSenha(dadosReset)
            navigate('/login')
            
        } catch (erro: any) {
            console.error(erro);
            alert(erro.message || 'Erro ao solicitar codigo.')

        }

    }  


  return (
    <>
        <section className={styles.tela_nova_senha}>
            <div className={styles.inputs}>
                <h1 className={styles.reppay}>RepPay</h1>
                {erro && <div className={styles.caixaErro}>{erro}</div>}
                <div className={styles.caixaInputsFundo}>
                    <div className={styles.caixaInputs}>
                        <h2>Recuperar Senha</h2>
                        <p>Insira sua nova senha</p>
                        
                        <form onSubmit={salvarNovaSenha}>
                            <div className={styles.inputContainer}>
                                <p className={styles.textoInput}>Confirmar Senha</p>
                                <input type="password"
                                    value={senha}
                                    onChange={(e) => setSenha(e.target.value)}
                                    placeholder='********'
                                    className={styles.input}
                                    onFocus={() => setErro('')}/>
                            </div>
                            <div className={styles.inputContainer}>
                                <p className={styles.textoInput}>Confirmar Senha</p>
                                <input type="password"
                                    value={confirmarSenha}
                                    onChange={(e) => setConfirmarSenha(e.target.value)}
                                    placeholder='********'
                                    className={styles.input}
                                    onFocus={() => setErro('')}/>
                            </div>
                            
                            <button type="submit" className={styles.enviar}>Salvar Senha</button>
                        </form>
                            <button type="button" onClick={() => navigate('/login')} className={styles.voltar}>
                                <img src={back}/>
                                Voltar para o login
                            </button>
                    </div>
                </div>
            </div>
        
            <div className={styles.imagem}>
                <div className={styles.textoImagemContainer}>
                    <h2>Paz na República</h2>
                    <p>
                        Centralize vencimentos, contas e o histórico de pagamentos em um único painel. Mais organização para a casa, menos dor de cabeça durante a sua semana de provas.
                    </p>
                </div>
            </div>
        </section>
    </>
  )
}

export default NovaSenha
