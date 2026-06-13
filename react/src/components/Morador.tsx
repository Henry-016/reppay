import styles from './Morador.module.scss'
import icone from './../assets/user_icon.svg'

interface ComponentProps {
    nome: string
    email: string
    tipo: string
    valor: number

}

function Morador( {nome, email, tipo, valor}: ComponentProps ) {

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
                    <p className={`${tipo === 'Admin' ? styles.admin : styles.morador}`}>{tipo}</p>
                </div>
                <div className={styles.informacoesDivida}>
                    <h2>R$ {valor}</h2>
                </div>

            </section>
        
        </>

    )

}

export default Morador;
