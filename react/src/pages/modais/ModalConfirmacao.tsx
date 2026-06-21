import styles from './ModalConfirmacao.module.scss'
import escudo from './../../assets/escudo.png'
import { useEffect, useRef } from 'react';

interface ModalProps {
    isOpen: boolean;
    onClose: () => void
    onClick: () => void
    texto: string


}

function ModalConfirmacao( {isOpen, onClose, onClick, texto}: ModalProps ) {

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

    if (!isOpen) return null;

    return (
        <>
            <section className={styles.tela_modal_confirmacao}>
                <div className={styles.modal} ref={modalRef}>
                    <div className={styles.confirmar}>
                        <img className={styles.escudo} src={escudo}/>
                        <h2 className={styles.confirmarAcao}>Confirmar Ação</h2>
                    </div>
                    <p className={styles.texto}>{texto}</p>
                    <div className={styles.decisao}>
                        <button className={styles.cancelar} onClick={onClose}>Cancelar</button>
                        <button className={styles.continuar} onClick={onClick}>Sim, Continuar</button>
                    </div>
                </div>

            </section>
        
        </>

    )

}

export default ModalConfirmacao;