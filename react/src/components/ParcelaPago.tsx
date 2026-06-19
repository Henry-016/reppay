import styles from './ParcelaPendente.module.scss'
import { utilitarios } from '../services/utilitariosService'

interface ComponentProps {
    icone: string
    nomeDespesa: string
    nomeMorador: string
    valor: number
    dataPago: string

}

function ParcelaPago( {icone, dataPago, nomeDespesa,nomeMorador, valor}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_pendente}>
                <div className={styles.containerDireita}>
                    <img src={icone} className={styles.icone}/>
                    <div className={styles.containerNome}>
                        <h2>{nomeMorador}</h2>
                        <p>{nomeDespesa}</p>
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

export default ParcelaPago;