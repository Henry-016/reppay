import styles from './DespesaPendente.module.scss'
import { utilitarios } from '../services/utilitariosService'

interface ComponentProps {
    icone: string
    nomeDespesa: string
    valor: number
    dataVencimento: string
    onEditar: () => void
    onApagar: () => void

}

function DespesaPendente( {icone, nomeDespesa, valor, dataVencimento, onEditar, onApagar}: ComponentProps ) {

    return (
        <>
            <section className={styles.despesa_pendente}>
                <div className={styles.containerDireita}>
                    <img src={icone} className={styles.icone}/>
                    <div className={styles.containerNome}>
                        <h2>{nomeDespesa}</h2>
                        <p>Data de Vencimento: {dataVencimento}</p>
                    </div>
                </div>
                <div className={styles.containerEsquerda}>
                    <h2 className={styles.valorDespesa}>{utilitarios.formatarValor(valor)}</h2>
                    <div className={styles.despesaPendenteBotoes}>
                        <button onClick={onApagar} className={styles.apagar}>
                            Apagar
                        </button>
                        <button onClick={onEditar} className={styles.editar}>
                            Editar
                        </button>
                    </div>
                </div>   
            </section>
        
        </>

    )

}

export default DespesaPendente;