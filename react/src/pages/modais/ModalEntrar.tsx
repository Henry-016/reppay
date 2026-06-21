import styles from './ModalEntrar.module.scss'
import x from './../../assets/x.svg'
import { useState, useEffect, useRef } from 'react'
import ModalSucesso from './ModalSucesso'
import imagem from './../../assets/users_codigo.svg'
import { grupoService } from '../../services/grupoService'
import { useAuth } from '../../context/AuthContext'

interface ModalProps {
    isOpen: boolean
    onClose: () => void
    onFinish: () => void

}

function ModalEntrar( {isOpen, onClose, onFinish}: ModalProps ) {

    const [codigo, setCodigo] = useState('')
    const [modal, setModal] = useState(false)

    const [erro, setErro] = useState('')

    const { token } = useAuth()

    const modalRef = useRef<HTMLDivElement>(null)

    useEffect(() => {
        if (!isOpen) {
            document.body.style.overflow = 'unset'
            return
        }

        document.body.style.overflow = 'hidden'

        const focusableElements = modalRef.current?.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        ) as NodeListOf<HTMLElement>

        if (!focusableElements || focusableElements.length === 0) return

        const firstElement = focusableElements[0]
        const lastElement = focusableElements[focusableElements.length - 1]

        firstElement.focus()

        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Tab') {
                if (e.shiftKey && document.activeElement === firstElement) {
                    e.preventDefault()
                    lastElement.focus()
                } else if (!e.shiftKey && document.activeElement === lastElement) {
                    e.preventDefault()
                    firstElement.focus()
                }
                
            }

        }

        document.addEventListener('keydown', handleKeyDown)

        return () => {
            document.removeEventListener('keydown', handleKeyDown);
            document.body.style.overflow = 'unset'
            
        } 


    }, [isOpen])

    const entrarGrupo = async (e: React.SubmitEvent) => {
        e.preventDefault();

        if (!codigo) {
            setErro('Por favor, insira o código do grupo.')
            return

        }

        try {
            await grupoService.entrarGrupo(codigo, token!)
            setModal(true);
            
        } catch (error: any) {
            setErro(error.message || 'Erro ao tentar entrar no grupo.')
            
        }
    }

    const fecharELimpar_x = () => {
        setCodigo('')
        setErro('')
        setModal(false)
        onClose()
    }

    const fecharELimpar = () => {
        setCodigo('')
        setErro('')
        setModal(false)
        onClose()
        onFinish()
    }

    if (!isOpen) return null

    return (
        <>
            <section className={styles.tela_modal_entrar}>
                <div className={styles.modal} ref={modalRef}>
                    <div className={styles.imagemContainer}>
                        <img onClick={fecharELimpar_x} src={x} className={styles.x}/>
                    </div>
                    <img src={imagem} className={styles.imagem}/>
                    {erro && <div className={styles.mensagemErro}>{erro}</div>}
                    <h2>Entrar em uma República</h2>
                    <p>Insira o código exclusivo de 8 dígitos</p>
                    <form onSubmit={entrarGrupo}>
                        <div className={styles.inputContainer}>
                            <input type="text" value={codigo} onChange={(e) => setCodigo(e.target.value)} placeholder='XXXXXXXX'/>
                        </div>
                        <button>ENTRAR NO GRUPO</button>
                    </form>
                    <div className={styles.aviso}>
                        <p>Não tem um convite? Peça a um administrador</p>
                    </div>
                </div>

                <ModalSucesso isOpen={modal} onClose={fecharELimpar} titulo='Grupo Encontrado!' texto='Parabéns! Você agora faz parte do Grupo XXXXXX' />

            </section>
        </>

    )

}

export default ModalEntrar;