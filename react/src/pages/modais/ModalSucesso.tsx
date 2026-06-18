import styles from './ModalSucesso.module.scss'
import imagemPadrao from './../../assets/Success_Icon_Container.png'

interface ModalProps {
    isOpen: boolean
    onClose: () => void
    titulo: string
    texto: string
    imagem?: string


}

function ModalSucesso( {isOpen, imagem, onClose, titulo, texto}: ModalProps ) {
    if (!isOpen) return null;

    return (
        <>
            <section className={styles.tela_modal_criar_sucesso}>
                <div className={styles.modal}>
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
