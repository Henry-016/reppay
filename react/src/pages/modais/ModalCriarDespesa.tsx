import styles from './ModalCriarDespesa.module.scss'
import x from './../../assets/x.svg'
import { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom';
import UsuarioSelecao from '../../components/UsuarioSelecao'
import ModalSucesso from './ModalSucesso'
import { useAuth } from './../../context/AuthContext'
import { grupoService } from './../../services/grupoService'
import { despesaService } from '../../services/despesaService';

interface ModalProps {
    isOpen: boolean
    onClose: () => void

}

interface Morador {
    idUsuario: number
    nome: string
    isAdmin: boolean
}

function ModalCriarDespesa( {isOpen, onClose}: ModalProps ) {

    const [nome, setNome] = useState('')
    const [valor, setValor] = useState('')
    const [data, setData] = useState('')
    const [link, setLink] = useState('')
    const [modalValidado, setModalValidado] = useState<boolean>(false)

    const [moradores, setMoradores] = useState<Morador[]>([])

    const [erro, setErro] = useState('')

    const [selecionados, setSelecionados] = useState<number[]>([]);

    const { idGrupo } = useParams<{ idGrupo: string }>()

    const { token } = useAuth()

    const { loading } = useAuth()

    useEffect(() => {

        if (loading) return
        
        const buscarMoradores = async () => {
            try {
                const dados = await grupoService.buscarMoradores(idGrupo || "", token || "")
                setMoradores(dados || [])
            } catch (err) {
                console.error("Erro ao buscar moradores:", err)
            }
        }
        buscarMoradores()
        

    }, [idGrupo, token, loading, isOpen])

    const alternarSelecao = (id: number) => {
        setSelecionados(prev => 
            prev.includes(id) 
                ? prev.filter(item => item !== id) 
                : [...prev, id]
        );
        setErro('')
    }

    const lancarDespesa = async (e: React.SubmitEvent) => {
        e.preventDefault()
        
        if (!nome) {
            setErro('Preencha a caixa do nome')
            return

        }

        const valorFormatado = valor.replace(',', '.')
    
        if (isNaN(Number(valorFormatado))) {
            setErro("Valor invalido!")
            return

        }

        if (!data) {
            setErro('Preencha a caixa da data')
            return

        }

        if (selecionados.length === 0) {
            setErro('Escolha ao menos uma pessoa para pagar a conta')
            return

        }

        const dadosDespesa = {
            Nome: nome,
            Valor: valorFormatado,
            Vencimento: data,
            Icone: link || null,
            IdGrupo: Number(idGrupo),
            MoradoresIds: selecionados
        }

        try {
            
            await despesaService.lancarDespesa(dadosDespesa, token!)
    
            setModalValidado(true)

        } catch (error: any) {
            setErro(error.message)
        }


    }

    const fecharELimpar = () => {
        setNome('')
        setValor('')
        setData('')
        setLink('')
        setErro('')
        onClose()
        setModalValidado(false)

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
                    <form onSubmit={lancarDespesa}> 
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
                                <input type="text" value={valor} onChange={(e) => { const valorDigitado = e.target.value; 
                                    if (valorDigitado === '' || /^[0-9.,]+$/.test(valorDigitado)) {
                                        setValor(valorDigitado);
                                    }
                                    }} 
                                    placeholder='0,00' onFocus={() => setErro('')}/>
                            </div>
                            <div className={styles.containerData}>
                                <p>DATA DE VENCIMENTO</p>
                                <input type="date"  value={data} onChange={(e) => setData(e.target.value)} placeholder='mm/dd/yyyy' onFocus={() => setErro('')}/>
                            </div>
                        </div>
                        <div className={styles.pessoas}>
                            <p>MORADORES ENVOLVIDOS</p>
                            <div className={styles.pessoasContainer}>
                                {moradores.map((morador) => (
                                    <UsuarioSelecao 
                                        key={morador.idUsuario}
                                        nome={morador.nome}
                                        estaSelecionado={selecionados.includes(morador.idUsuario)}
                                        onClick={() => alternarSelecao(morador.idUsuario)}
                                    
                                    />

                                ))}
                            </div>
                        </div>
                        <button className={styles.lancar}>LANÇAR DESPESA</button>
                    </form>
                </div>
                <ModalSucesso isOpen={modalValidado} onClose={fecharELimpar} titulo='Dívida Registrada!' texto='A dívida foi registrada com sucesso. Todos
os moradores selecionados serão notificados.' /> 
            </section>
        </>

    )

}

export default ModalCriarDespesa;