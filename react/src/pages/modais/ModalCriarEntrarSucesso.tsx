import styles from './ModalCriarEntrarSucesso.module.scss'

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
    titulo: string;
    texto: string;


}

function ModalCriarEntrarSucesso( {isOpen, onClose, titulo, texto}: ModalProps ) {
    if (!isOpen) return null;

    return (
        <>
            <section className={styles.tela_modal_criar_sucesso}>
                <div className={styles.modal}>
                    <div className={styles.imagem}></div>
                    <h2>{titulo}</h2>
                    <p className={styles.suaJornada}>{texto}</p>
                    <button className={styles.continuar} onClick={onClose}>VOLTAR AO INICIO</button>
                </div>

            </section>
        
        </>

    )

}

export default ModalCriarEntrarSucesso;
