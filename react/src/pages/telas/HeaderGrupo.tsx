import styles from './HeaderGrupo.module.scss'
import iconeUsuario from '../../assets/user_icon.svg';

interface ModalProps {
    nome: string;
    imagem?: string;
    tipo: string;
    nome_grupo: string;

}

function HeaderGrupo( {nome, imagem, tipo, nome_grupo}: ModalProps ) {

    return (
        <>
            <section className={styles.tela_header_grupo}>
                <h2 className={styles.titulo}>{nome_grupo}</h2>
                <div className={styles.usuario}>
                    <div className={styles.textoPerfil}>
                        <p className={styles.nome}>{nome}</p>
                        <p className={styles.tipo}>{tipo}</p>
                    </div>
                    <img className={styles.user_icon} src={iconeUsuario}/>
                </div>
            </section>
        </>

    )

}

export default HeaderGrupo;
