import styles from './ModalConfirmacao.module.scss'
import escudo from './../../assets/escudo.png'

interface ModalProps {
    isOpen: boolean;
    onClose: () => void
    onClick: () => void
    texto: string


}

function ModalConfirmacao( {isOpen, onClose, onClick, texto}: ModalProps ) {
    if (!isOpen) return null;

    return (
        <>
            <section className={styles.tela_modal_confirmacao}>
                <div className={styles.modal}>
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