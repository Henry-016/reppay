import styles from './ComponenteEscolha.module.scss'

interface ComponentProps {
    imagem: string;
    titulo: string;
    texto: string;
    button: string;
    onClick: () => void;

}

function ComponenteEscolha( {imagem, titulo, texto, button, onClick}: ComponentProps ) {

    return (
        <>
            <section className={styles.tela_componente_escolha}>
                <img src={imagem} className={styles.imagem}/>
                <h2>{titulo}</h2>
                <p>{texto}</p>
                <button onClick={onClick}>{button}</button>                

            </section>
        
        </>

    )

}

export default ComponenteEscolha;
