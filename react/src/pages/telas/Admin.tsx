import styles from './Admin.module.scss'
import { useState, useEffect } from 'react'
import HeaderGrupo from './HeaderGrupo'
import { useParams } from 'react-router-dom'
import { useNavigate } from 'react-router-dom'
import dashboard_ativado from './../../assets/dashboard_ativado.svg'
import moradores_desativado from './../../assets/moradores_desativado.svg'
import dashboard_desativado from './../../assets/dashboard_desativado.svg'
import moradores_ativado from './../../assets/moradores_ativado.svg'
import back from './../../assets/arrow_back.svg'
import sair from './../../assets/sair.svg'
import add from './../../assets/add.svg'
import ModalCriarDespesa from './../modais/ModalCriarDespesa'
import ParcelaPendente from './../../components/ParcelaPendente'
import ParcelaAnalise from './../../components/ParcelaAnalise'
import ParcelaPago from './../../components/ParcelaPago'
import ModalConfirmacao from '../modais/ModalConfirmacao'
import MoradorComponente from '../../components/MoradorComponente'
import key from './../../assets/key.svg'
import { useAuth } from './../../context/AuthContext'
import { grupoService } from './../../services/grupoService'
import { despesaService } from './../../services/despesaService'
import { utilitarios } from '../../services/utilitariosService'

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

interface Moradores {
    idUsuario: number
    nome: string
    isAdmin: boolean
    email: string 
    totalDevido: number

}

interface Inadimplentes {
    idParcela: number
    nomeDespesa: string
    icone: string
    valor: number
    vencimento: string
    status: string
    nomeMorador: string

}

interface Analise {
    idParcela: number
    nomeMorador: string
    nomeDespesa: string
    icone: string
    valor: number
    dataSinalizacao: string

}

interface Pago {
    idParcela: number
    nomeMorador: string
    nomeDespesa: string
    icone: string
    valorPago: number
    dataPagamento: string
    vencimento: string

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
    const [abaAtiva, setAbaAtiva] = useState<number>(1);
    const [inadimplentes, setInadimplentes] = useState<Inadimplentes[]>([])
    const [emAnalise, setEmAnalise] = useState<Analise[]>([])
    const [historicoPago, setHistoricoPago] = useState<Pago[]>([])
    const [parcelaParaConfirmar, setParcelaParaConfirmar] = useState<number | null>(null)
    const [parcelaParaAceitar, setParcelaParaAceitar] = useState<number | null>(null)
    const [parcelaParaRejeitar, setParcelaParaRejeitar] = useState<number | null>(null)
    const [pagina, setPagina] = useState<number>(1)
    const [moradores, setMoradores] = useState<Moradores[]>([])
    const [modalSair, setModalSair] = useState<boolean>(false)
    
    const { usuario, loading } = useAuth()
    const nome = usuario?.nome
    const { token } = useAuth()

    useEffect(() => {

        if (loading) return
        
        if (!token) {
            navigate('/login')
            return
        }

        if (!idGrupo) {
            navigate('/home')
            return
        }

        const buscarDadosDoGrupo = async () => {
            const dadosGrupo = await grupoService.buscarGrupo( idGrupo, token)

            if (!dadosGrupo.isAdmin) {
                navigate(`/home/morador/${idGrupo}`)
                return
                
            }
            
            setGrupo(dadosGrupo)

            const [dadosInadimplentes, dadosMinhasDividas, dadosProximaConta, dadosAnalises, dadosHistorico, dadosMoradores] = await Promise.all([
                despesaService.buscarInadimplentes(idGrupo, token),
                despesaService.buscarMinhasDividas(idGrupo, token),
                grupoService.buscarProximaConta(idGrupo, token),
                despesaService.buscarAnalises(idGrupo, token),
                despesaService.buscarHistorico(idGrupo, token),
                grupoService.buscarMoradores(idGrupo, token)
            ])

            setInadimplentes(dadosInadimplentes.listaInadimplentes)
            setTotalReceber(dadosInadimplentes.totalAReceber)
            setMoradores(dadosMoradores)
            setMinhaDivida(dadosMinhasDividas.totalDevido)
            setProximaConta(dadosProximaConta)
            setEmAnalise(dadosAnalises.listaAnalises)
            setHistoricoPago(dadosHistorico.listaHistorico)

        }

        if (idGrupo) buscarDadosDoGrupo()
    }, [idGrupo, modal, atualizarDados, loading, token])

    const sinalizarPagamento = async (id: number) => {
        try {
            await despesaService.sinalizarPagamento(id, token || '')

            setAtualizarDados(prev => prev + 1)
            alert("Pagamento sinalizado com sucesso!")
            setParcelaParaConfirmar(null)
        } catch (error) {
            console.error("Falha ao sinalizar:", error)

        }

    }

    const validarPagamento = async (id: number, decisao: boolean) => {
        try {
            const mensagem = await despesaService.validarPagamento(id, decisao, token!);
            
            setAtualizarDados(prev => prev + 1);
            alert(mensagem);
            
            setParcelaParaAceitar(null);
            setParcelaParaRejeitar(null);
        } catch (error: any) {
            console.error(error);
            
        }

    }

    const sairDoGrupo = async () => {
        try {
            await grupoService.sairDoGrupo(idGrupo, token)
            setAtualizarDados(prev => prev + 1)
            navigate('/home')
            
        } catch(error: any) {
            alert(error.message)

        }

    }

    return (
        <>
            <section className={styles.tela_admin}>
                <div className={styles.sideBar}>
                    <div className={styles.sideBarUp}>
                        <h2>RepPay</h2>
                        <button onClick={() => setPagina(1)}className={`${pagina === 1 ? styles.ativado : styles.desativado}`}>
                            <img src={pagina === 1 ? dashboard_ativado : dashboard_desativado}/>
                            Dashboard
                        </button>
                        <button onClick={() => setPagina(2)}className={`${pagina === 2 ? styles.ativado : styles.desativado}`}>
                            <img src={pagina === 2 ? moradores_ativado : moradores_desativado}/>
                            Moradores
                        </button>
                    </div>
                    <div className={styles.sideBarBottom}>
                        <button onClick={() => navigate('/home')}>
                            <img src={back}/>
                            Voltar para Home
                        </button>
                        <button onClick={() => setModalSair(true)}>
                            <img src={sair}/>
                            Sair do Grupo
                        </button>
                    </div>

                </div>
                <div className={styles.principal}>
                    <HeaderGrupo nome={nome || 'Usuário'} tipo={grupo?.isAdmin ? 'ADMINISTRADOR' : 'MORADOR'} nome_grupo={grupo?.nome || 'Republica'} />
                    {pagina === 1 && 
                        <div className={styles.conteudo}>
                        <div className={styles.despesasRepublica}>
                            <div className={styles.informacaoPrincipal}>
                                <div className={styles.dividaTotal}>
                                    <p>DÍVIDA TOTAL DA REPÚBLICA</p>
                                    <h2>{utilitarios.formatarValor(totalReceber)}</h2>
                                </div>
                                <div className={styles.despesasRepublicaBottom}>
                                    <div className={styles.dividaIndividual}>
                                        <p>Sua parte individual</p>
                                        <h2>{utilitarios.formatarValor(minhaDivida)}</h2>
                                    </div>
                                    <div className={styles.vencimento}>
                                        <p>Próximo Vencimento</p>
                                        <h2>{proximaConta?.vencimento || "Não há nenhuma conta próxima do vencimento!"}</h2>
                                    </div>
                                </div>
                            </div>
                            <button onClick={() => setModal(true)} className={styles.add}>
                                <img src={add} />
                                <h2>Lançar Nova Despesa</h2>
                            </button>

                        </div>
                        <div className={styles.opcoes}>
                            <button onClick={() => setAbaAtiva(1)}className={`${styles.opcao} ${abaAtiva === 1 ? styles.ativo : ''}`}>
                                Pendentes
                            </button>
                            <button onClick={() => setAbaAtiva(2)} className={`${styles.opcao} ${abaAtiva === 2 ? styles.ativo : ''}`}>
                                Em Análise
                            </button>
                            <button onClick={() => setAbaAtiva(3)} className={`${styles.opcao} ${abaAtiva === 3 ? styles.ativo : ''}`}>
                                Histórico Pago
                            </button>
                        </div>
                        {abaAtiva === 1 && 
                            <div className={styles.containerDespesas}>
                                <h2 className={styles.detalhesConta}>DETALHES DA FATURA</h2>
                                {inadimplentes.length > 0 ? (
                                <div className={styles.contas}>
                                    {inadimplentes.map((parcela) => {
                                        const eminhadivida = parcela.nomeMorador === nome
                                    
                                        return (

                                            <ParcelaPendente 
                                                key={parcela.idParcela} 
                                                icone={parcela.icone} 
                                                vencimento={parcela.vencimento} 
                                                nomeDespesa={parcela.nomeDespesa} 
                                                nomeMorador={parcela.nomeMorador}
                                                valor={parcela.valor} onClick={() => setParcelaParaConfirmar(parcela.idParcela)}
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
                        {abaAtiva === 2 && 
                            <div className={styles.containerDespesas}>
                                <h2 className={styles.detalhesConta}>DÍVIDAS AGUARDANDO CONFIRMAÇÃO</h2>
                                {emAnalise.length > 0 ? (
                                <div className={styles.contas}>
                                    {emAnalise.map((parcela) => {
                                    
                                        return (

                                            <ParcelaAnalise 
                                                key={parcela.idParcela} 
                                                icone={parcela.icone} 
                                                nomeDespesa={parcela.nomeDespesa}
                                                valor={parcela.valor} onClick={() => setParcelaParaAceitar(parcela.idParcela)}
                                                onCancel={() => setParcelaParaRejeitar(parcela.idParcela)}
                                                nomeMorador={parcela.nomeMorador}
                                                dataSinalizacao={parcela.dataSinalizacao}
                                            />
                                        )})}
                                </div>
                                ) : (
                                    <div className={styles.vazio}>
                                        <p>Não foi encontrada nenhuma dívida no momento.</p>
                                    </div>
                                )}
 
                        </div>}
                        {abaAtiva === 3 && 
                            <div className={styles.containerDespesas}>
                                <h2 className={styles.detalhesConta}>DÍVIDAS PAGAS</h2>
                                {historicoPago.length > 0 ? (
                                <div className={styles.contas}>
                                    {historicoPago.map((parcela) => {
                                    
                                        return (

                                            <ParcelaPago 
                                                key={parcela.idParcela} 
                                                icone={parcela.icone} 
                                                nomeDespesa={parcela.nomeDespesa}
                                                valor={parcela.valorPago} 
                                                nomeMorador={parcela.nomeMorador}
                                                dataPago={parcela.dataPagamento}
                                            />
                                        )})}
                                </div>
                                ) : (
                                    <div className={styles.vazio}>
                                        <p>Não foi encontrada nenhuma dívida no momento.</p>
                                    </div>
                                )}

                        </div>}

                            <ModalConfirmacao 
                                texto={'Você tem certeza que deseja prosseguir com a validação deste pagamento? Após confirmar, a despesa vai para a aba “Em Análise” para o administrador validar!'}
                                isOpen={parcelaParaConfirmar !== null} 
                                onClose={() => setParcelaParaConfirmar(null)} 
                                onClick={() => {
                                    if (parcelaParaConfirmar !== null) {
                                        sinalizarPagamento(parcelaParaConfirmar);
                                    }
                                }}
                            />

                            <ModalConfirmacao 
                                texto={'Você tem certeza que deseja prosseguir com a validação deste pagamento? Após confirmar, não será mais possível reverter!'}
                                isOpen={parcelaParaAceitar !== null} 
                                onClose={() => setParcelaParaAceitar(null)} 
                                onClick={() => {
                                    if (parcelaParaAceitar !== null) {
                                        validarPagamento(parcelaParaAceitar, true);
                                    }
                                }}
                            />

                            <ModalConfirmacao 
                                texto={'Você tem certeza que deseja prosseguir com a rejeição deste pagamento? Após confirmar, não será mais possível reverter!'}
                                isOpen={parcelaParaRejeitar !== null} 
                                onClose={() => setParcelaParaRejeitar(null)} 
                                onClick={() => {
                                    if (parcelaParaRejeitar !== null) {
                                        validarPagamento(parcelaParaRejeitar, false);
                                    }
                                }}
                            />
                            <ModalConfirmacao 
                                texto={'Você tem certeza que deseja sair deste grupo, se fizer isso o efeito será irreversivel!'}
                                isOpen={modalSair} 
                                onClose={() => setModalSair(false)} 
                                onClick={sairDoGrupo}
                            />

                        </div>}
                    
                    {pagina === 2 &&
                        <div className={styles.moradores}>
                            <div className={styles.moradoresTexto}>
                                <h2 className={styles.gerenciamento}>Gerenciamento de Moradores</h2>
                                <p className={styles.gerenciamentoP}>Controle de acesso e membros do grupo da república.</p>
                                
                            </div>
                            <div className={styles.containerMoradores}>
                                <div className={styles.containerMembrosDoGrupo}>
                                    <h2>Membros do Grupo</h2>
                                    <p>{moradores.length} Membros</p>
                                </div>
                                <div className={styles.colunasMoradores}>
                                    <p className={`${styles.coluna} ${styles.colunaMorador}`}>MORADOR</p>
                                    <p className={`${styles.coluna} ${styles.colunaCargo}`}>CARGO</p>
                                    <p className={`${styles.coluna} ${styles.colunaDivida}`}>DÍVIDA ATUAL</p>
                                </div>
                                <div className={styles.moradorComponente}>
                                    {moradores.map((morador) => (
                                    <MoradorComponente
                                        key={morador.idUsuario}
                                        nome={morador.nome}
                                        tipo={morador.isAdmin ? 'Admin' : 'Morador'}
                                        valor={morador.totalDevido}
                                        email={morador.email}                                   
                                    />
                                ))}
                                </div>
                            </div>
                            <div className={styles.containerCodigo}>
                                <div className={styles.containerChave}>
                                        <h2>CÓDIGO DE ACESSO</h2>
                                        <img src={key} className={styles.key} />
                                </div>
                                <div className={styles.codigoCopiar}>
                                    <h2>{grupo?.codigoAcesso || ''}</h2>
                                    <button onClick={() => utilitarios.copiarParaAreaDeTransferencia(grupo)} className={styles.copiar}>Copiar</button>
                                </div>
                                <p>Compartilhe este código para convidar novos moradores ao seu grupo.</p>
                            </div>
                        </div>}
                </div>
                <ModalCriarDespesa isOpen={modal} onClose={() => setModal(false)} />

            </section>
        </>

    )

}

export default Admin;