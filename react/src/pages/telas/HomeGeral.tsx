import styles from './HomeGeral.module.scss'
import HeaderGeral from './HeaderGeral'
import plus from './../../assets/plus.svg'
import { useState } from 'react'
import Modal_EscolhaCriarEntrar from './../modais/Modal_EscolhaCriarEntrar'

function HomeGeral() {

    const [modal, setModal] = useState(false)

    const idDoUsuario = localStorage.getItem('idUsuario');
    const nome = localStorage.getItem('nomeUsuario');

    return (
        <>
            <section className={styles.tela_home_geral}>
                <HeaderGeral nome={nome}/>
                <div className={styles.conteudo}>
                    <div className={styles.titulos}>
                        <h2>Bem-vindo de volta, {nome}!</h2>
                        <p>Selecione seu painel ativo para gerenciar suas finanças compartilhadas.</p>
                    
                    </div>
                    <div className={styles.republicas}>
                        <div onClick={() => setModal(true)}className={styles.adicionarRepublicas}>
                            <img src={plus} className={styles.plus}/>
                            <h2>Nova República</h2>
                            <p>Crie um novo ambiente ou junte-se a uma república existente usando o código.</p>
                        </div>

                    </div>
                </div>

                <Modal_EscolhaCriarEntrar isOpen={modal} onClose={() => setModal(false)}/>

            </section>
        </>

    )

}

export default HomeGeral;