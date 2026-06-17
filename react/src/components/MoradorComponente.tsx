import styles from './MoradorComponente.module.scss'
import icone from './../assets/user_icon.svg'
import { utilitarios } from '../services/utilitariosService'

interface ComponentProps {
    nome: string
    email: string
    tipo: string
    valor: number
    onClick: () => void

}

function MoradorComponente( {nome, email, tipo, valor, onClick}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_morador}>
                <div className={styles.informacoesMorador}>
                    <img src={icone} className={styles.icone} />
                    <div className={styles.informacoes}>
                        <h2>{nome}</h2>
                        <p>{email}</p>
                    </div>  
                </div>
                <div className={styles.informacoesCargo}>
                    <p className={`${tipo === 'Admin' ? styles.admin : styles.morador}`} onClick={onClick}>{tipo}</p>
                </div>
                <div className={styles.informacoesDivida}>
                    <h2>{utilitarios.formatarValor(valor)}</h2>
                </div>

            </section>
        
        </>

    )

}

export default MoradorComponente;
