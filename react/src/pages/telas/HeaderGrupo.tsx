import styles from './HeaderGrupo.module.scss'
import iconeUsuario from '../../assets/user_icon.svg'
import { useState, useEffect, useRef } from 'react'
import ModalPerfil from '../modais/ModalPerfil'

interface ModalProps {
    nome: string;
    imagem?: string;
    tipo: string;
    nome_grupo: string;

}

function HeaderGrupo( {nome, tipo, nome_grupo}: ModalProps ) {

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
            <section className={styles.tela_header_grupo}>
                <h2 className={styles.titulo}>{nome_grupo}</h2>
                <div className={styles.usuario}>
                    <div className={styles.textoPerfil}>
                        <p className={styles.nome}>{nome}</p>
                        <p className={styles.tipo}>{tipo}</p>
                    </div>
                    <div className={styles.containerIcone}>
                        <img className={styles.user_icon} src={iconeUsuario} onClick={() => setModalAberto(!modalAberto)}/>
                        {modalAberto && (
                        <ModalPerfil />
                        )}
                    </div>
                </div>
            </section>
        </>

    )

}

export default HeaderGrupo;
