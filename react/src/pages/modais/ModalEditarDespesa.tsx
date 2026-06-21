import styles from './ModalEditarDespesa.module.scss'
import x from './../../assets/x.svg'
import { useState, useEffect, useRef } from 'react'
import ModalSucesso from './ModalSucesso'
import { useAuth } from './../../context/AuthContext'
import { despesaService } from '../../services/despesaService';

interface ModalProps {
    isOpen: boolean
    onClose: () => void
    idDespesa: number | undefined
    nomeAtual: string | undefined
    valorAtual: number | undefined
    iconeAtual: string | undefined
    vencimentoAtual: string | undefined

}

function ModalEditarDespesa( {isOpen, onClose, idDespesa, nomeAtual, valorAtual, iconeAtual, vencimentoAtual}: ModalProps ) {

    const [nome, setNome] = useState('')
    const [valor, setValor] = useState('')
    const [data, setData] = useState('')
    const [link, setLink] = useState('')
    const [modalValidado, setModalValidado] = useState<boolean>(false)

    const [erro, setErro] = useState('')

    const { token } = useAuth()

    const { loading } = useAuth()

    const modalRef = useRef<HTMLDivElement>(null)

    useEffect(() => {

        if (loading) return

        const atualizarDados = () => {
            setNome(nomeAtual || "")
            setValor((valorAtual || "").toString())
            setData(vencimentoAtual || "")
            setLink(iconeAtual || "")

        }
        
        atualizarDados()

        if (!isOpen) {
            document.body.style.overflow = 'unset'
            return;
        }

        document.body.style.overflow = 'hidden'

        const focusableElements = modalRef.current?.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        ) as NodeListOf<HTMLElement>

        if (!focusableElements || focusableElements.length === 0) return

        const firstElement = focusableElements[0]
        const lastElement = focusableElements[focusableElements.length - 1]

        firstElement.focus()

        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Tab') {
                if (e.shiftKey && document.activeElement === firstElement) {
                    e.preventDefault()
                    lastElement.focus()
                } else if (!e.shiftKey && document.activeElement === lastElement) {
                    e.preventDefault()
                    firstElement.focus()
                }
            }
            if (e.key === 'Escape') {
                onClose();
            }
        }

        document.addEventListener('keydown', handleKeyDown)

        return () => {
            document.removeEventListener('keydown', handleKeyDown);
            document.body.style.overflow = 'unset'
        } 

    }, [token, loading, isOpen, modalValidado])

    const editarDespesa = async (e: React.SubmitEvent) => {
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

        if (!idDespesa) {
            setErro('Erro: Nenhuma despesa selecionada.');
            return;
        }

        const dadosDespesa = {
            Nome: nome,
            Valor: Number(valorFormatado), 
            Vencimento: data,
            Icone: link
        }

        try {
            
            await despesaService.editarDespesa(idDespesa,dadosDespesa, token!)
    
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
            <section className={styles.tela_modal_editar_despesa}>
                <div className={styles.modal} ref={modalRef}>
                    <div className={styles.imagemContainer}>
                        <img onClick={fecharELimpar} src={x} className={styles.x}/>
                    </div>
                    {erro && <div className={styles.mensagemErro}>{erro}</div>}
                    <h2>Editar Despesa</h2>
                    <form onSubmit={editarDespesa}> 
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
                        <button className={styles.lancar}>SALVAR ALTERAÇÕES</button>
                    </form>
                </div>
                <ModalSucesso isOpen={modalValidado} onClose={fecharELimpar} titulo='Dívida Alterada!' texto='A dívida foi alterada com sucesso.' /> 
            </section>
        </>

    )

}

export default ModalEditarDespesa;