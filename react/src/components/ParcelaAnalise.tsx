import styles from './ParcelaAnalise.module.scss'
import confirm from './../assets/confirm.svg'
import cancel from './../assets/cancel.svg'
import { utilitarios } from '../services/utilitariosService'

interface ComponentProps {
    icone: string
    nomeDespesa: string
    nomeMorador: string
    valor: number
    dataSinalizacao: string
    onClick: () => void
    onCancel: () => void

}

function ComponenteEscolha( {icone, nomeDespesa,nomeMorador, valor, dataSinalizacao, onClick, onCancel}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_pendente}>
                <div className={styles.containerDireita}>
                    <img src={icone} className={styles.icone}/>
                    <div className={styles.containerNome}>
                        <h2>{nomeMorador}</h2>
                        <p>{nomeDespesa}</p>
                        <p>Data da Sinalização: {dataSinalizacao}</p>
                    </div>
                </div>
                <div className={styles.containerEsquerda}>
                    <h2>{utilitarios.formatarValor(valor)}</h2>
                    <div className={styles.analiseBotoes}>
                        <img className={styles.analiseBotao} src={cancel} onClick={onCancel} />
                        <img className={styles.analiseBotao} src={confirm} onClick={onClick} />
                    </div>
                </div>   
            </section>
        
        </>

    )

}

export default ComponenteEscolha;