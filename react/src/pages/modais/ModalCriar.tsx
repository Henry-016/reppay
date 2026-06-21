import styles from './ModalCriar.module.scss'
import x from './../../assets/x.svg'
import { useState, useEffect, useRef} from 'react'
import ModalSucesso from './ModalSucesso'
import { grupoService } from '../../services/grupoService'
import { useAuth } from '../../context/AuthContext'

interface ModalProps {
    isOpen: boolean
    onClose: () => void
    onFinish: () => void

}



function ModalCriar( {isOpen, onClose, onFinish}: ModalProps ) {

    const [nome, setNome] = useState('')
    const [link, setLink] = useState('')
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
            if (e.key === 'Escape') {
                onClose();
            }
        }

        document.addEventListener('keydown', handleKeyDown)

        return () => {
            document.removeEventListener('keydown', handleKeyDown);
            document.body.style.overflow = 'unset'
            
        } 


    }, [isOpen])

    const criarGrupo = async (e: React.SubmitEvent) => {
        e.preventDefault()
        if (!nome) {
            setErro('O nome da república é obrigatório.');
            return
        }

        setErro('')

        try {
            await grupoService.criarGrupo(nome, link, token!);
            setModal(true);

        } catch (error: any) {
            console.error('Erro na requisição:', error)
            setErro(error.message)

        }
    }

    const fecharELimpar_x = () => {
        setNome('')
        setLink('')
        setErro('')
        onClose()

    }

    const fecharELimpar = () => {
        setNome('')
        setLink('')
        setErro('')
        onClose()
        onFinish()

    }

    if (!isOpen) return null

    return (
        <>
            <section className={styles.tela_modal_criar}>
                <div className={styles.modal} ref={modalRef}>
                    <div className={styles.imagemContainer}>
                        <img onClick={fecharELimpar_x} src={x} className={styles.x}/>
                    </div>
                    {erro && <div className={styles.mensagemErro}>{erro}</div>}
                    <h2>Criar Nova República</h2>
                    <p>Organize as contas da sua casa em segundos</p>
                    <form onSubmit={criarGrupo}>
                        <div className={styles.inputContainer}>
                            <p>Link do Banner</p>
                            <input type="text" value={link} onChange={(e) => setLink(e.target.value)} placeholder='Ex: https://site.com/sua-foto.jpg' onFocus={() => setErro('')}/>
                        </div>
                        <div className={styles.inputContainer}>
                            <p>Nome da República</p>
                            <input type="text" value={nome} onChange={(e) => setNome(e.target.value)} placeholder='Ex: República Central' onFocus={() => setErro('')}/>
                        </div>
                        <button>CRIAR GRUPO</button>
                    </form>
                    <div className={styles.aviso}>
                        <p>Ao criar, você automaticamente se tornará o administrador do grupo.</p>
                    </div>
                </div>

                <ModalSucesso isOpen={modal} onClose={fecharELimpar} titulo='Grupo Criado!' texto='O grupo foi criado com sucesso e você se tornou o administrador dele.' />

            </section>
        </>

    )

}

export default ModalCriar;