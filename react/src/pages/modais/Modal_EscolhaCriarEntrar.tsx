import styles from './Modal_EscolhaCriarEntrar.module.scss'
import ModalCriar from './ModalCriar'
import ModalEntrar from './ModalEntrar'
import x from './../../assets/x.svg'
import overlay from './../../assets/Overlay.svg'
import ComponenteEscolha from './../../components/ComponenteEscolha'
import casa from './../../assets/casa_criar.svg'
import users from './../../assets/users_entrar.svg'
import { useState, useEffect, useRef } from 'react'

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;

}

function Modal_EscolhaCriarEntrar( {isOpen, onClose}: ModalProps ) {

    const [criar, setCriar] = useState(false)
    const [entrar, setEntrar] = useState(false)

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

    if (!isOpen) return null;

    return (
        <>
        <section className={styles.tela_mece}>
            <div className={styles.modal} ref={modalRef}>
                <div className={styles.imagemContainer}>
                    <img onClick={onClose} src={x} className={styles.x}/>
                </div>
                <img src={overlay} className={styles.overlay}/>
                <h2>Inicie sua Jornada</h2>
                <p>Escolha como deseja gerenciar sua república e facilite a divisão de despesas financeiras.</p>
                <div className={styles.componentes}>
                    <ComponenteEscolha imagem={casa} titulo='Criar República' texto='Crie uma nova casa, adicione membros e comece a organizar as finanças do zero.' button='COMEÇAR' onClick={() => setCriar(true)}/>

                    <ComponenteEscolha imagem={users} titulo='Entrar com Código' texto='Recebeu um convite? Insira o código enviado pelo seu administrador para acessar a casa.' button='Inserir Código' onClick={() => setEntrar(true)}/>
                </div>
            </div>
            <ModalCriar isOpen={criar} onClose={() => setCriar(false)} onFinish={onClose}/>
            <ModalEntrar isOpen={entrar} onClose={() => setEntrar(false)} onFinish={onClose} />
        </section>
        
        </>

    )

}

export default Modal_EscolhaCriarEntrar;
