import styles from './ModalPerfil.module.scss'
import sair from './../../assets/sair.svg'
import { usuarioService } from '../../services/usuarioService'
import { useAuth } from './../../context/AuthContext'
import { useNavigate } from 'react-router-dom';

function ModalPerfil() {

     const { token, logout } = useAuth()

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
            <button onClick={Deslogar}>
                <img src={sair} />
                    Sair
            </button>
        </section>

        
        </>

    )

}

export default ModalPerfil