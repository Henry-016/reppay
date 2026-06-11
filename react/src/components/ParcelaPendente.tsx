import styles from './ParcelaPendente.module.scss'

interface ComponentProps {
    icone: string;
    nomeDespesa: string;
    valor: number;
    vencimento: string;
    onClick: () => void;

}

function ComponenteEscolha( {icone, vencimento, nomeDespesa, valor, onClick}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_pendente}>
                <div className={styles.containerDireita}>
                    <img src={icone} className={styles.icone}/>
                    <div className={styles.containerNome}>
                        <h2>{nomeDespesa}</h2>
                        <p>Data de Vencimento: {vencimento}</p>
                    </div>
                </div>
                <div className={styles.containerEsquerda}>
                    <h2>R$ {valor}</h2>
                    <button onClick={onClick}>Pagar</button>
                </div>   
            </section>
        
        </>

    )

}

export default ComponenteEscolha;