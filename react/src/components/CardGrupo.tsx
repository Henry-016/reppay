import styles from './CardGrupo.module.scss'
import apagar from './../assets/cancel.svg'

interface ComponentProps {
    imagem: string
    tipo: string
    titulo: string
    texto: string
    onClick: () => void
    clickApagar: () => void

}

function CardGrupo( {imagem, tipo, titulo, texto, onClick, clickApagar}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_card}>
                <div className={`${styles.imagem} ${tipo === 'ADMINISTRADOR' ? styles.imagemAdmin : ""}`} style={{backgroundImage: `url(${imagem})`}}>
                    { tipo === 'ADMINISTRADOR' && 
                            <img src={apagar} onClick={clickApagar} className={styles.apagar}/>
                    }
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
