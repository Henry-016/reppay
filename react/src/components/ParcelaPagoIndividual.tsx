import styles from './ParcelaPendente.module.scss'
import { utilitarios } from '../services/utilitariosService'

interface ComponentProps {
    icone: string
    nomeDespesa: string
    valor: number
    dataPago: string

}

function ParcelaPagoIndividual( {icone, dataPago, nomeDespesa, valor}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_pendente}>
                <div className={styles.containerDireita}>
                    <img src={icone} className={styles.icone}/>
                    <div className={styles.containerNome}>
                        <h2>{nomeDespesa}</h2>
                        <p>Data de Pagamento: {dataPago}</p>
                    </div>
                </div>
                <div className={styles.containerEsquerda}>
                    <h2>{utilitarios.formatarValor(valor)}</h2>
                </div>   
            </section>
        
        </>

    )

}

export default ParcelaPagoIndividual;