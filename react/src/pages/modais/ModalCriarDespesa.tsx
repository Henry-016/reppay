import styles from './ModalCriarDespesa.module.scss'
import x from './../../assets/x.svg'
import { useState} from 'react'

interface ModalProps {
    isOpen: boolean
    onClose: () => void

}

function ModalCriarDespesa( {isOpen, onClose}: ModalProps ) {

    const [nome, setNome] = useState('')
    const [valor, setValor] = useState('')
    const [data, setData] = useState('')
    const [link, setLink] = useState('')

    const [erro, setErro] = useState('')
    const [carregando, setCarregando] = useState<boolean>(false)

    const [codigoGerado, setCodigoGerado] = useState<string | null>(null)

    const idUsuarioLogado = localStorage.getItem('idUsuario')
    const [selecionados, setSelecionados] = useState<number[]>([]);

    const criarGrupo = async (e: React.SubmitEvent) => {
        e.preventDefault()
        if (!nome) {
            setErro('O nome da república é obrigatório.');
            return
        }

        setErro('')
        setCarregando(true)

        const token = localStorage.getItem('token');

        try {
            const resposta = await fetch('http://localhost:5149/api/Grupo', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}` 
                },
                body: JSON.stringify({
                    Nome: nome,
                    ImagemBanner: link,

                })
            });

            const dados = await resposta.json()

            if (resposta.ok) {
                setCodigoGerado(dados.codigoAcesso)
            } else {
                setErro(dados.mensagem || 'Erro ao criar a república.')
            }

        } catch (error) {
            console.error('Erro na requisição:', error)
            setErro('Não foi possível conectar ao servidor.')
        } finally {
            setCarregando(false)
        }
    }

    const fecharELimpar = () => {
        setNome('')
        setValor('')
        setData('')
        setLink('')
        setErro('')
        setCodigoGerado(null)
        onClose()

    }

    if (!isOpen) return null

    return (
        <>
            <section className={styles.tela_modal_criar_despesa}>
                <div className={styles.modal}>
                    <div className={styles.imagemContainer}>
                        <img onClick={fecharELimpar} src={x} className={styles.x}/>
                    </div>
                    {erro && <div className={styles.mensagemErro}>{erro}</div>}
                    <h2>Lançar Nova Despesa</h2>
                    <form onSubmit={criarGrupo}>
                        <div className={styles.inputContainer}>
                            <p>LINK DO ICONE</p>
                            <input type="text" value={link} onChange={(e) => setLink(e.target.value)} placeholder='Ex: https://site.com/sua-foto.jpg' onFocus={() => setErro('')}/>
                        </div>
                        <div className={styles.inputContainerNome}>
                            <p>NOME DA DESPESA</p>
                            <input type="text" value={nome} onChange={(e) => setNome(e.target.value)} placeholder='Ex: Aluguel Maio' onFocus={() => setErro('')}/>
                        </div>
                        <div className={styles.inputContainerValorData}>
                            <div className={styles.containerValor}>
                                <p>VALOR</p>
                                <input type="text" value={valor} onChange={(e) => setValor(e.target.value)} placeholder='0,00' onFocus={() => setErro('')}/>
                            </div>
                            <div className={styles.containerData}>
                                <p>DATA DE VENCIMENTO</p>
                                <input type="date"  value={data} onChange={(e) => setData(e.target.value)} placeholder='mm/dd/yyyy' onFocus={() => setErro('')}/>
                            </div>
                        </div>
                        <div className={styles.pessoas}>
                            <p>MORADORES ENVOLVIDOS</p>
                            <div>

                            </div>
                        </div>
                        <button>LANÇAR DESPESA</button>
                    </form>
                </div>
            </section>
        </>

    )

}

export default ModalCriarDespesa;