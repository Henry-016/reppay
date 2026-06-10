import iconeCheck from './../../assets/check.svg'; 
import avatarPadrao from './../../assets/user_icon.svg';
import styles from './usuarioSelecao.module.scss'

interface ComponentProps {
    nome: string;
    estaSelecionado: boolean;
    onClick: () => void;

}

function usuarioSelecao({nome, estaSelecionado, onClick}: ComponentProps) {
    return (
        <>
            <section className={styles.usuario}>
                <button className={`${styles.padrao} ${estaSelecionado ? styles.selecionado : ''}`}>
                    <img src={avatarPadrao} className={styles.avatar}/>
                    <h2>{nome}</h2>
                    {estaSelecionado && (
                        <img src={iconeCheck} className={styles.check}/>

                    )}
                </button>
            </section>
        </>

    )


}

export default usuarioSelecao;