import styles from './MoradorComponente.module.scss'
import icone from './../assets/user_icon.svg'
import { utilitarios } from '../services/utilitariosService'
import expulsar from './../assets/cancel.svg'

interface ComponentProps {
    nome: string
    email: string
    tipo: string
    valor: number
    onClick: () => void
    clickExpulsar: () => void
    isAdmin: boolean | undefined
    iconeUsuario: string

}

function MoradorComponente( {nome, email, tipo, valor, onClick, clickExpulsar, isAdmin, iconeUsuario}: ComponentProps ) {

    return (
        <>
            <section className={styles.componente_morador}>
                <div className={styles.informacoesMorador}>
                    <img src={iconeUsuario || icone} className={styles.icone} />
                    <div className={styles.informacoes}>
                        <h2>{nome}</h2>
                        <p>{email}</p>
                    </div>
                    {(tipo === 'Morador' && isAdmin)  && 
                        <img src={expulsar} onClick={clickExpulsar} className={styles.expulsar}/>
                    
                    }
                </div>
                <div className={styles.informacoesCargo}>
                    <p className={`${tipo === 'Admin' ? styles.admin : styles.morador} ${isAdmin && tipo !== "Admin"? styles.userAdmin : ""}`} onClick={onClick}>{tipo}</p>
                </div>
                <div className={styles.informacoesDivida}>
                    <h2>{utilitarios.formatarValor(valor)}</h2>
                </div>

            </section>
        
        </>

    )

}

export default MoradorComponente;
