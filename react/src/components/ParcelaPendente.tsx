import styles from './ParcelaPendente.module.scss'
import { utilitarios } from '../services/utilitariosService'

interface ComponentProps {
    icone: string
    nomeDespesa: string
    nomeMorador: string
    valor: number
    vencimento: string
    onClick: () => void
    mostrarBotao: boolean

}

function ComponenteEscolha( {icone, vencimento, nomeDespesa,nomeMorador, valor, mostrarBotao, onClick}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_pendente}>
                <div className={styles.containerDireita}>
                    <img src={icone} className={styles.icone}/>
                    <div className={styles.containerNome}>
                        <h2>{nomeMorador}</h2>
                        <p>{nomeDespesa}</p>
                        <p>Data de Vencimento: {vencimento}</p>
                    </div>
                </div>
                <div className={styles.containerEsquerda}>
                    <h2>{utilitarios.formatarValor(valor)}</h2>
                    {mostrarBotao && (
                    <button onClick={onClick}>Pagar</button>
                    )}
                </div>   
            </section>
        
        </>

    )

}

export default ComponenteEscolha;