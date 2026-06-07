import styles from './Modal_FeedBack_ContaCriada.module.scss'

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;

}

function Modal_FeedBack_ContaCriada( {isOpen, onClose}: ModalProps ) {
    if (!isOpen) return null;

    return (
        <>
        <section className={styles.tela_mfcc}>
            <div className={styles.modal}>
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
