import styles from './CardGrupo.module.scss'

interface ComponentProps {
    imagem: string;
    tipo: string;
    titulo: string;
    texto: string;
    onClick: () => void;

}

function CardGrupo( {imagem, tipo, titulo, texto, onClick}: ComponentProps ) {

    return (
        <>
            <section className={styles.tela_componente_card}>
                <div className = {styles.imagem} style={{backgroundImage: `url(${imagem})`}}>
                    <div className={`${tipo === 'ADMINISTRADOR' ? styles.admin : styles.morador}`}>
                        <p>{tipo}</p>
                    </div>
                </div>
                <div className={styles.conteudo}>
                    <div className={styles.texto}>
                        <h2>{titulo}</h2>
                        <p>{texto}</p>
                    </div>
                    <button onClick={onClick}>Acessar Painel</button>
                </div>                

            </section>
        </>

    )

}

export default CardGrupo;
