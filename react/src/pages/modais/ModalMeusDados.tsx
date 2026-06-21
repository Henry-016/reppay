import styles from './ModalMeusDados.module.scss'
import icon from './../../assets/user_icon.svg'
import { useState, useEffect, useRef } from 'react'
import { useAuth } from '../../context/AuthContext'
import { usuarioService } from '../../services/usuarioService'
import { useNavigate } from 'react-router-dom'
import x from './../../assets/x.svg'
import apagar from './../../assets/apagar.svg'
import ModalConfirmacao from './ModalConfirmacao'
import ModalSucesso from './ModalSucesso'

interface Usuario {
    idUsuario: number
    nome: string
    email: string
    fotoDePerfil: string

}

interface ModalProps {
    isOpen: boolean
    onClose: () => void

}

function ModalMeusDados({isOpen, onClose}: ModalProps) {

    const { token, loading, logout } = useAuth()
    const navigate = useNavigate()
    const modalRef = useRef<HTMLDivElement>(null)

    const [, setUsuario] = useState<Usuario>()
    const [nome, setNome] = useState("")
    const [link, setLink] = useState("")
    const [email, setEmail] = useState("")
    const [senha, setSenha] = useState("")
    const [confirmarSenha, setConfirmarSenha] = useState("")
    const [modalAlteracao, setModalAlteracao] = useState<boolean>(false)
    const [modalExcluir, setModalExcluir] = useState<boolean>(false)
    const [erro, setErro] = useState("")

    useEffect(() => {
    
            if (loading) return
            
            if (!token) {
                navigate('/login')
                return
            }

            const buscarDadosUsuario = async () => {
                try {
                    const dados = await usuarioService.meuPerfil(token || "")

                    setUsuario(dados)

                    setNome(dados?.nome)
                    setLink(dados?.fotoDePerfil || "")
                    setEmail(dados?.email)

                } catch (error) {
                    console.log(error)

                }

            }

            buscarDadosUsuario()

            if (!isOpen) {
                document.body.style.overflow = 'unset'
                return
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

            }
    
            document.addEventListener('keydown', handleKeyDown)
    
            return () => {
                document.removeEventListener('keydown', handleKeyDown);
                document.body.style.overflow = 'unset'
                
            } 
    
    }, [token, loading, isOpen])

    if (!isOpen) return null

    const alterar = async (e: React.SubmitEvent) => {
        e.preventDefault()

        if (senha !== confirmarSenha) {
            setErro('As senhas não coincidem!')
            return

        }

        try {
            const dados = {
                nome: nome,
                email: email,
                fotoDePerfil: link,
                senha: senha || null

            }

            await usuarioService.atualizar(token || "", dados)
            setModalAlteracao(true)

        } catch (error) {
            console.error("Erro ao salvar:", error)
            alert(error instanceof Error ? error.message : 'Erro desconhecido')

        }

    }

    const Deslogar = async () => {
        try {
            const refreshToken = localStorage.getItem('refreshToken')

            if (token && refreshToken) {
                await usuarioService.logOut(refreshToken, token)

            }

            logout()
    
            navigate('/login')

        } catch (error) {
            console.error("Falha ao Deslogar:", error)

        }

    }   

    const fecharELimpar = () => {
        setNome('')
        setEmail('')
        setLink('')
        setSenha('')
        setConfirmarSenha('')
        setModalAlteracao(false)
        onClose()

    }

    const excluirConta = async () => {
        try {
            await usuarioService.excluirConta(token || "")
            fecharELimpar()
            Deslogar()

        } catch(error) {
            alert(error)

        }

    }

    return (
        <>
            <section className={styles.tela_modal_MeusDados}>
                <div className={styles.modal} ref={modalRef}>
                    <div className={styles.header}>
                        <h2>Meus Dados</h2>
                        <img src={x} className={styles.x} onClick={fecharELimpar}/>
                    </div>
                    <div className={styles.conteudo}>
                    {erro && <div className={styles.mensagemErro}>{erro}</div>}
                        <img src={link || icon} className={styles.icon}/>
                        <form onSubmit={alterar}>
                            <div className={styles.inputContainer}>
                                <p>LINK DA FOTO DE PERFIL</p>
                                <input type="text" value={link} onChange={(e) => setLink(e.target.value)} placeholder='Ex: https://site.com/sua-foto.jpg'onFocus={() => setErro('')}/>
                            </div>                            
                            <div className={styles.inputContainer}>
                                <p>NOME COMPLETO</p>
                                <input type="text" value={nome} onChange={(e) => setNome(e.target.value)} placeholder={"Ex: Maria Silva"} onFocus={() => setErro('')}/>
                            </div>
                            <div className={styles.inputContainer}>
                                <p>EMAIL</p>
                                <input type="text" value={email} onChange={(e) => setEmail(e.target.value)} placeholder={"voce@exemplo.com"} onFocus={() => setErro('')}/>
                            </div>
                            <div className={styles.inputContainer}>
                                <p>NOVA SENHA</p>
                                <input type="password" value={senha} onChange={(e) => setSenha(e.target.value)} placeholder='••••••••' onFocus={() => setErro('')}/>
                            </div>
                            <div className={styles.inputContainer}>
                                <p>CONFIRMAR SENHA</p>
                                <input type="password" value={confirmarSenha} onChange={(e) => setConfirmarSenha(e.target.value)} placeholder='••••••••' onFocus={() => setErro('')}/>
                            </div>
                            <button>Salvar Alterações</button>
                        </form>
                        <div className={styles.apagar} onClick={() => setModalExcluir(true)}>
                            <img src={apagar} />
                            <p>Excluir Minha Conta</p>
                        </div>
                    </div>
                    <ModalSucesso titulo={"Dados Alterados!"} texto={""} isOpen={modalAlteracao} onClose={fecharELimpar} />
                    <ModalConfirmacao isOpen={modalExcluir} onClose={() => setModalExcluir(false)} texto={"Você tem certeza que quer apagar a sua conta?"} onClick={excluirConta} />
                    

                </div>
            </section>
        
        </>

    )

}

export default ModalMeusDados