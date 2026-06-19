import styles from './HeaderGeral.module.scss'
import iconeUsuario from '../../assets/user_icon.svg'
import { useState, useEffect, useRef } from 'react'
import ModalPerfil from '../modais/ModalPerfil'

interface ModalProps {
    nome: string
    icone?: string

}

function HeaderGeral( {nome, icone}: ModalProps ) {

    const [modalAberto, setModalAberto] = useState(false)

    const menuRef = useRef<HTMLDivElement>(null)

    useEffect(() => {
        const lidarComCliqueFora = (event: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
                setModalAberto(false)
            }
        }

        document.addEventListener('mousedown', lidarComCliqueFora)
        
        return () => {
            document.removeEventListener('mousedown', lidarComCliqueFora)
        }
    }, [])

    return (
        <>
            <section className={styles.tela_header_geral}>
                <h2 className={styles.titulo}>RepPay</h2>
                <div className={styles.usuario} ref={menuRef}>
                    <p className={styles.nome}>{nome}</p>
                    <div className={styles.containerIcone}>
                        <img className={styles.user_icon} src={icone || iconeUsuario} onClick={() => setModalAberto(!modalAberto)} />
                        {modalAberto && (
                        <ModalPerfil />
                        )}
                    </div>
                </div>
            </section>
        </>

    )

}

export default HeaderGeral