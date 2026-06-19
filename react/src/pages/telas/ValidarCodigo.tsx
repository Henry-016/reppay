import { useState} from 'react'
import styles from './ValidarCodigo.module.scss'
import { useLocation, useNavigate } from 'react-router-dom'
import { usuarioService } from '../../services/usuarioService'
import back from './../../assets/arrow_back.svg'

function ValidarCodigo() {
    const [codigo, setCodigo] = useState('')
    const [erro, setErro] = useState('')
    const navigate = useNavigate()
    const location = useLocation()

    const email = location.state?.email

    if (!email) {
        navigate('/verificarEmail')
        return

    }

    const validar = async (e: React.SubmitEvent) => {
        e.preventDefault()

        if (!codigo) {
            setErro('Por favor, preencha o email.')
            return

        }

        try {
            await usuarioService.validarCodigo(email, codigo)
            
        } catch (erro: any) {
            console.error(erro);
            alert(erro.message || 'Erro ao solicitar codigo.')

        }

    }  


  return (
    <>
        <section className={styles.tela_validar_email}>
            <div className={styles.inputs}>
                <h1 className={styles.reppay}>RepPay</h1>
                {erro && <div className={styles.caixaErro}>{erro}</div>}
                <div className={styles.caixaInputsFundo}>
                    <div className={styles.caixaInputs}>
                        <h2>Recuperar Senha</h2>
                        <p>Insira o código enviado para seu email.</p>
                        
                        <form onSubmit={validar}>
                            <div className={styles.inputContainer}>
                                <p className={styles.textoInput}>Código</p>
                                <input type="text"
                                    value={codigo}
                                    onChange={(e) => setCodigo(e.target.value)}
                                    placeholder='XXXXXX'
                                    className={styles.input}
                                    onFocus={() => setErro('')}/>
                            </div>
                            
                            <button type="submit" className={styles.enviar}>Verificar Código</button>
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

export default ValidarCodigo
