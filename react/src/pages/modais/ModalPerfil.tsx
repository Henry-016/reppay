import styles from './ModalPerfil.module.scss'
import sair from './../../assets/sair.svg'
import icon from './../../assets/icon.svg'
import { usuarioService } from '../../services/usuarioService'
import { useAuth } from './../../context/AuthContext'
import { useNavigate } from 'react-router-dom'
import ModalMeusDados from './ModalMeusDados'
import { useState } from 'react'

function ModalPerfil() {

    const { token, logout } = useAuth()
    const [modal, setModal] = useState<boolean>(false)

    const Deslogar = async () => {
            try {
                const refreshToken = localStorage.getItem('refreshToken')

                if (token && refreshToken) {
                    await usuarioService.logOut(refreshToken, token)

                }

                logout()
        
                navigate('/login')

            } catch (error) {
                console.error("Falha ao Deslogar:", error)
    
            }
    
    }

    const navigate = useNavigate()

    return (
        <>
            <section className={styles.tela_ModalPerfil}>
                <button className={styles.botao} onClick={() => setModal(true)}>
                    <img className={styles.icon} src={icon} />
                        Meus Dados
                </button>
                <button className={styles.botao} onClick={Deslogar}>
                    <img className={styles.icon} src={sair} />
                        Sair
                </button>
            </section>
            <ModalMeusDados isOpen={modal} onClose={() => setModal(false)}/>

        </>

    )

}

export default ModalPerfil