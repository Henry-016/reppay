import styles from './ParcelaAnalise.module.scss'
import confirm from './../assets/confirm.svg'
import cancel from './../assets/cancel.svg'

interface ComponentProps {
    icone: string
    nomeDespesa: string
    nomeMorador: string
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
                    <h2>R$ {valor}</h2>
                    <div className={styles.analiseBotoes}>
                        <button className={styles.emAnalise}>
                            Em Análise
                        </button>
                        <button className={styles.desfazer}>
                            Desfazer
                        </button>
                    </div>
                </div>   
            </section>
        
        </>

    )

}

export default ComponenteEscolha;