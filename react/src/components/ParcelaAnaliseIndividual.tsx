import styles from './ParcelaAnaliseIndividual.module.scss'
import { utilitarios } from '../services/utilitariosService'

interface ComponentProps {
    icone: string
    nomeDespesa: string
    valor: number
    dataSinalizacao: string
    onClick: () => void
    onCancel: () => void

}

function ComponenteEscolha( {icone, nomeDespesa, valor, dataSinalizacao, onClick, onCancel}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_pendente}>
                <div className={styles.containerDireita}>
                    <img src={icone} className={styles.icone}/>
                    <div className={styles.containerNome}>
                        <h2>{nomeDespesa}</h2>
                        <p>Data da Sinalização: {dataSinalizacao}</p>
                    </div>
                </div>
                <div className={styles.containerEsquerda}>
                    <h2>{utilitarios.formatarValor(valor)}</h2>
                    <div className={styles.analiseBotoes}>
                        <button onClick={onClick} className={styles.emAnalise}>
                            Em Análise
                        </button>
                        <button onClick={onCancel} className={styles.desfazer}>
                            Desfazer
                        </button>
                    </div>
                </div>   
            </section>
        
        </>

    )

}

export default ComponenteEscolha;