import styles from './ModalCriar.module.scss'
import x from './../../assets/x.svg'
import { useState} from 'react'
import ModalSucesso from './ModalSucesso'
import { grupoService } from '../../services/grupoService'
import { useAuth } from '../../context/AuthContext'

interface ModalProps {
    isOpen: boolean
    onClose: () => void
    onFinish: () => void

}



function ModalCriar( {isOpen, onClose, onFinish}: ModalProps ) {

    const [nome, setNome] = useState('')
    const [link, setLink] = useState('')
    const [modal, setModal] = useState(false)

    const [erro, setErro] = useState('')

    const { token } = useAuth()

    const criarGrupo = async (e: React.SubmitEvent) => {
        e.preventDefault()
        if (!nome) {
            setErro('O nome da república é obrigatório.');
            return
        }

        setErro('')

        try {
            await grupoService.criarGrupo(nome, link, token!);
            setModal(true);

        } catch (error: any) {
            console.error('Erro na requisição:', error)
            setErro(error.message)

        }
    }

    const fecharELimpar = () => {
        setNome('')
        setLink('')
        setErro('')
        onClose()

    }

    if (!isOpen) return null

    return (
        <>
            <section className={styles.tela_modal_criar}>
                <div className={styles.modal}>
                    <div className={styles.imagemContainer}>
                        <img onClick={fecharELimpar} src={x} className={styles.x}/>
                    </div>
                    {erro && <div className={styles.mensagemErro}>{erro}</div>}
                    <h2>Criar Nova República</h2>
                    <p>Organize as contas da sua casa em segundos</p>
                    <form onSubmit={criarGrupo}>
                        <div className={styles.inputContainer}>
                            <p>Link do Banner</p>
                            <input type="text" value={link} onChange={(e) => setLink(e.target.value)} placeholder='Ex: https://site.com/sua-foto.jpg' onFocus={() => setErro('')}/>
                        </div>
                        <div className={styles.inputContainer}>
                            <p>Nome da República</p>
                            <input type="text" value={nome} onChange={(e) => setNome(e.target.value)} placeholder='Ex: República Central' onFocus={() => setErro('')}/>
                        </div>
                        <button>CRIAR GRUPO</button>
                    </form>
                    <div className={styles.aviso}>
                        <p>Ao criar, você automaticamente se tornará o administrador do grupo.</p>
                    </div>
                </div>

                <ModalSucesso isOpen={modal} onClose={onFinish} titulo='Grupo Criado!' texto='O grupo foi criado com sucesso e você se tornou o administrador dele.' />

            </section>
        </>

    )

}

export default ModalCriar;