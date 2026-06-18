import styles from './ModalMeusDados.module.scss'
import icon from './../../assets/user_icon.svg'
import { useState, useEffect } from 'react'
import { useAuth } from '../../context/AuthContext'
import { usuarioService } from '../../services/usuarioService'
import { useNavigate } from 'react-router-dom'

interface Usuario {
    idUsuario: number
    nome: string
    email: string

}

function ModalMeusDados() {

    const { token, loading } = useAuth()
    const navigate = useNavigate()

    const [nome, setNome] = useState('')
    const [link, setLink] = useState('')
    const [email, setEmail] = useState('')
    const [senha, setSenha] = useState('')
    const [usuario, setUsuario] = useState<Usuario>()

    useEffect(() => {
    
            if (loading) return
            
            if (!token) {
                navigate('/login')
                return
            }

            const buscarDadosUsuario = async () => {
                try {
                    const dados = await usuarioService.meuPerfil(token || "")

                    setUsuario(dados)

                } catch (error) {
                    console.log(error)

                }

            }

            buscarDadosUsuario()
    
    }, [token, loading])



    return (
        <>
            <section>
                <div className={styles.header}>Meus Dados</div>
                <div className={styles.conteudo}>
                    <img src={icon} className={styles.icon}/>
                    <form>
                        <div className={styles.inputContainer}>
                            <input type="text" value={link} onChange={(e) => setLink(e.target.value)} placeholder='Ex: https://site.com/sua-foto.jpg'/>
                        </div>
                        <div className={styles.inputContainer}>
                            <input type="text" value={nome} onChange={(e) => setNome(e.target.value)} placeholder={usuario?.nome || "usuario"}/>
                        </div>
                        <div className={styles.inputContainer}>
                            <input type="text" value={email} onChange={(e) => setEmail(e.target.value)} placeholder={usuario?.email || "email"}/>
                        </div>
                        <div className={styles.inputContainer}>
                            <input type="password" value={senha} onChange={(e) => setSenha(e.target.value)} placeholder='••••••••' />
                        </div>
                        <button>Salvar Alterações</button>
                    </form>

                </div>
            </section>
        
        </>

    )

}

export default ModalMeusDados