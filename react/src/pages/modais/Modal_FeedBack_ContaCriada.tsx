import styles from './Modal_FeedBack_ContaCriada.module.scss'
import { useEffect, useRef } from 'react'

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;

}

function Modal_FeedBack_ContaCriada( {isOpen, onClose}: ModalProps ) {

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

    if (!isOpen) return null

    return (
        <>
        <section className={styles.tela_mfcc}>
            <div className={styles.modal} ref={modalRef}>
                <div className={styles.imagem}></div>
                <h2>Conta Criada com Sucesso!</h2>
                <p className={styles.suaJornada}>Sua jornada rumo a uma república mais organizada começa agora.</p>
                <div className={styles.divAmbienteSeguro}>
                    <div className={styles.escudo}></div>
                    <div className={styles.ambienteSeguroTexto}>
                        <h3>Ambiente Seguro</h3>
                        <p>Criptografia de ponta a ponta ativa</p>
                    </div>
                </div>
                <button className={styles.continuar} onClick={onClose}>CONTINUAR</button>
            </div>

        </section>
        
        </>

    )

}

export default Modal_FeedBack_ContaCriada;
