import styles from './HeaderGeral.module.scss'
import iconeUsuario from '../../assets/user_icon.svg';

interface ModalProps {
    nome: string;
    imagem?: string;

}

function HeaderGeral( {nome, imagem}: ModalProps ) {

    return (
        <>
            <section className={styles.tela_header_geral}>
                <h2 className={styles.titulo}>RepPay</h2>
                <div className={styles.usuario}>
                    <p className={styles.nome}>{nome}</p>
                    <img className={styles.user_icon} src={iconeUsuario}/>
                </div>
            </section>
        </>

    )

}

export default HeaderGeral;