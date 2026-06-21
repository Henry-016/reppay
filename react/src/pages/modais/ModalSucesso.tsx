import styles from './ModalSucesso.module.scss'
import imagemPadrao from './../../assets/Success_Icon_Container.png'
import { useEffect, useRef } from 'react'

interface ModalProps {
    isOpen: boolean
    onClose: () => void
    titulo: string
    texto: string
    imagem?: string


}

function ModalSucesso( {isOpen, imagem, onClose, titulo, texto}: ModalProps ) {

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
            <section className={styles.tela_modal_criar_sucesso}>
                <div className={styles.modal} ref={modalRef}>
                    <img src={imagem || imagemPadrao }className={styles.imagem} />
                    <h2>{titulo}</h2>
                    <p className={styles.suaJornada}>{texto}</p>
                    <button className={styles.continuar} onClick={onClose}>VOLTAR AO INICIO</button>
                </div>

            </section>
        
        </>

    )

}

export default ModalSucesso;
