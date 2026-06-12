import styles from './Admin.module.scss'
import { useState, useEffect } from 'react'
import HeaderGrupo from './HeaderGrupo'
import { useParams } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';
import dashboard_ativado from './../../assets/dashboard_ativado.svg'
import moradores_desativado from './../../assets/moradores_desativado.svg'
import sair from './../../assets/sair.svg'
import add from './../../assets/add.svg'
import ModalCriarDespesa from './../modais/ModalCriarDespesa'
import ParcelaPendente from './../../components/ParcelaPendente'

interface DadosGrupo {
    idGrupo: number
    nome: string
    codigoAcesso: string
    imagemBanner: string | null
    isAdmin: boolean
}

interface ProximaConta {
    nomeDespesa: string
    nomeGrupo: string | null
    vencimento: string
    valor: number

}

interface MinhasDividas {
    idParcela: number
    nomeDespesa: string
    icone: string
    valor: number
    vencimento: string
    status: string
    nomeMorador: string

}



function Admin() {
    
    const navigate = useNavigate()

    const { idGrupo } = useParams<{ idGrupo: string }>()
    
    const [atualizarDados, setAtualizarDados] = useState(0);
    const [grupo, setGrupo] = useState<DadosGrupo | null>(null)
    const [totalReceber, setTotalReceber] = useState<number>(0)
    const [minhaDivida, setMinhaDivida] = useState<number>(0)
    const [modal, setModal] = useState(false)
    const [proximaConta, setProximaConta] = useState<ProximaConta>()
    const [pendente, setPendente] = useState<boolean>(false)
    const [analise, setAnalise] = useState<boolean>(false)
    const [pago, setPago] = useState<boolean>(false)
    const [minhasDividas, setMinhasDividas] = useState<MinhasDividas[]>([])
    
    const nome = localStorage.getItem('nomeUsuario');

    useEffect(() => {
        
        const buscarDadosDoGrupo = async () => {
            const token = localStorage.getItem('token')
            
            
            try {
                const resposta = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}`, {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                })

                if (resposta.ok) {
                    const dados = await resposta.json()
                    
                    if (!dados.isAdmin) {
                        navigate(`/morador/${idGrupo}`);
                        return; 
                    }

                    setGrupo(dados)
                } else {
                    navigate('/home');
                }

                const respostaInadimplentes = await fetch(`http://localhost:5149/api/Despesa/Inadimplentes/${idGrupo}`, {
                    method: 'GET',
                    headers: { 'Authorization': `Bearer ${token}` }
                })
                
                if (respostaInadimplentes.ok) {
                    const dadosInadimplentes = await respostaInadimplentes.json()
                    setTotalReceber(dadosInadimplentes.totalAReceber || 0)
                    setMinhasDividas(dadosInadimplentes.listaInadimplentes || [])
                }

                const respostaDividas = await fetch(`http://localhost:5149/api/Despesa/MinhasDividas`, {
                    method: 'GET',
                    headers: { 'Authorization': `Bearer ${token}` }
                })
                
                if (respostaDividas.ok) {
                    const dadosDividas = await respostaDividas.json()
                    setMinhaDivida(dadosDividas.totalDevido || 0)

                }

                const respostaVencimento = await fetch(`http://localhost:5149/api/Usuario/${idGrupo}/proximaConta`, 
                {method: 'GET', headers: { 'Authorization': `Bearer ${token}`}})

                if (respostaVencimento.ok) {
                    const dadosVencimento = await respostaVencimento.json()
                    setProximaConta(dadosVencimento)

                }            

            } catch (error) {
                console.error("Erro na requisição:", error)
            }
        }

        if (idGrupo) {
            buscarDadosDoGrupo();
        }
    }, [idGrupo, modal, atualizarDados]);

    const mudarOpcao = (id: number) => {

        if (id === 1) {
            setPendente(true)
            setAnalise(false)
            setPago(false)

        }

        else if (id === 2) {
            setPendente(false)
            setAnalise(true)
            setPago(false)

        }

        else if (id === 3) {
            setPendente(false)
            setAnalise(false)
            setPago(true)

        }

    }

    const sinalizarPagamento = async (id: number) => {
        const token = localStorage.getItem('token')

        try {
            const resposta = await fetch(`http://localhost:5149/api/Despesa/SinalizarPagamento/${id}`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}` 
            }
        })

        const resultado = await resposta.json()

        if (resposta.ok) {
            setAtualizarDados(prev => prev + 1)
            alert(resultado.mensagem)
        } else {
            alert(resultado.mensagem || "Erro ao sinalizar pagamento.")
        }

        } catch (error) {
        console.error("Erro ao conectar com a API:", error)
        }   

    }

    return (
        <>
            <section className={styles.tela_admin}>
                <div className={styles.sideBar}>
                    <div className={styles.sideBarUp}>
                        <h2>RepPay</h2>
                        <button className={styles.ativado}>
                            <img src={dashboard_ativado}/>
                            Dashboard
                        </button>
                        <button className={styles.desativado}>
                            <img src={moradores_desativado}/>
                            Moradores
                        </button>
                    </div>
                    <div className={styles.sideBarBottom}>
                        <button>
                            <img src={sair}/>
                            Sair
                        </button>
                    </div>

                </div>
                <div className={styles.principal}>
                    <HeaderGrupo nome={nome || 'Usuário'} tipo={grupo?.isAdmin ? 'ADMINISTRADOR' : 'MORADOR'} nome_grupo={grupo?.nome || 'Republica'} />
                    <div className={styles.conteudo}>
                        <div className={styles.despesasRepublica}>
                            <div className={styles.informacaoPrincipal}>
                                <div className={styles.dividaTotal}>
                                    <p>DÍVIDA TOTAL DA REPÚBLICA</p>
                                    <h2>R$ {totalReceber}</h2>
                                </div>
                                <div className={styles.despesasRepublicaBottom}>
                                    <div className={styles.dividaIndividual}>
                                        <p>Sua parte individual</p>
                                        <h2>R$ {minhaDivida}</h2>
                                    </div>
                                    <div className={styles.vencimento}>
                                        <p>Próximo Vencimento</p>
                                        <h2>{proximaConta?.vencimento}</h2>
                                    </div>
                                </div>
                            </div>
                            <button onClick={() => setModal(true)} className={styles.add}>
                                <img src={add} />
                                <h2>Lançar Nova Despesa</h2>
                            </button>

                        </div>
                        <div className={styles.opcoes}>
                            <button onClick={() => mudarOpcao(1)}className={`${styles.opcao} ${pendente ? styles.ativo : ''}`}>
                                Pendentes
                            </button>
                            <button onClick={() => mudarOpcao(2)} className={`${styles.opcao} ${analise ? styles.ativo : ''}`}>
                                Em Análise
                            </button>
                            <button onClick={() => mudarOpcao(3)} className={`${styles.opcao} ${pago ? styles.ativo : ''}`}>
                                Histórico Pago
                            </button>
                        </div>
                        {pendente && 
                            <div className={styles.containerDespesas}>
                                <h2 className={styles.detalhesFatura}>DETALHES DA FATURA</h2>
                                {minhasDividas.length > 0 ? (
                                <div className={styles.pendentes}>
                                    {minhasDividas.map((parcela) => {
                                        const eminhadivida = parcela.nomeMorador === nome
                                    
                                        return (

                                            <ParcelaPendente 
                                                key={parcela.idParcela} 
                                                icone={parcela.icone} 
                                                vencimento={parcela.vencimento} 
                                                nomeDespesa={parcela.nomeDespesa} 
                                                nomeMorador={parcela.nomeMorador}
                                                valor={parcela.valor} onClick={() => sinalizarPagamento(parcela.idParcela)}
                                                mostrarBotao={eminhadivida}
                                            />
                                        )})}
                                </div>
                                ) : (
                                    <div className={styles.vazio}>
                                        <p>Não foi encontrada nenhuma dívida no momento.</p>
                                    </div>
                                )}
                                
                            </div>}

                    </div>
                </div>
                <ModalCriarDespesa isOpen={modal} onClose={() => setModal(false)} />

            </section>
        </>

    )

}

export default Admin;