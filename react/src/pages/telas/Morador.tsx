import styles from './Morador.module.scss'
import dashboard_ativado from './../../assets/dashboard_ativado.svg'
import sair from './../../assets/sair.svg'
import HeaderGrupo from './HeaderGrupo'
import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useParams } from 'react-router-dom'
import calendario from './../../assets/calendario.svg'
import { useAuth } from './../../context/AuthContext'
import { grupoService } from '../../services/grupoService'
import { despesaService } from '../../services/despesaService'
import ParcelaPagoIndividual from '../../components/ParcelaPagoIndividual'
import ParcelaPendenteIndividual from '../../components/ParcelaPendenteIndividual'
import ParcelaAnaliseIndividual from './../../components/ParcelaAnaliseIndividual'
import ModalConfirmacao from '../modais/ModalConfirmacao'
import ModalSucesso from '../modais/ModalSucesso'
import desfazer from './../../assets/desfazer.svg'
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

interface MinhasDividas {
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

function Morador() {

    const navigate = useNavigate()

    const [atualizarDados, setAtualizarDados] = useState(0)
    const [grupo, setGrupo] = useState<DadosGrupo | null>(null)
    const [minhaDivida, setMinhaDivida] = useState<number>(0)
    const [proximaConta, setProximaConta] = useState<ProximaConta>()
    const [emAnalise, setEmAnalise] = useState<Analise[]>([])
    const [pendentes, setPendentes] = useState<MinhasDividas[]>([])
    const [historicoPago, setHistoricoPago] = useState<Pago[]>([])
    const [abaAtiva, setAbaAtiva] = useState<number>(1)
    const [parcelaParaConfirmar, setParcelaParaConfirmar] = useState<number | null>(null)
    const [parcelaParaDesfazer, setParcelaParaDesfazer] = useState<number | null>(null)
    const [modalAviso, setModalAviso] = useState<boolean>(false)

    const { usuario, loading } = useAuth()
    const nome = usuario?.nome

    const { idGrupo } = useParams<{ idGrupo: string }>()

    const { token } = useAuth()

    useEffect(() => {

        if (loading) return
    
        if (!token) {
            navigate('/login')
            return;
        }

        if (!idGrupo) {
            navigate('/home')
            return
        }
        
        const buscarDadosDoGrupo = async () => {
            
            try {
                const dadosGrupo = await grupoService.buscarGrupo( idGrupo, token)

                if (dadosGrupo.isAdmin) {
                    navigate(`/home/admin/${idGrupo}`)
                    return

                }
                
                setGrupo(dadosGrupo)
                    
                    const [dadosMinhasDividas, dadosProximaConta, dadosAnalises, dadosHistorico] = await Promise.all([
                    despesaService.buscarMinhasDividas(idGrupo, token),
                    grupoService.buscarProximaConta(idGrupo, token),
                    despesaService.buscarAnalisesIndividuais(idGrupo, token),
                    despesaService.buscarHistoricoIndividuais(idGrupo, token)
                ])

                    setMinhaDivida(dadosMinhasDividas.totalDevido)
                    setProximaConta(dadosProximaConta)
                    setPendentes(dadosMinhasDividas.listaDividas)
                    setEmAnalise(dadosAnalises.listaAnalises)
                    setHistoricoPago(dadosHistorico)

            }

            catch (error) {
                console.error("Falha ao buscar os dados do grupo:", error)

            }

        }
        if (idGrupo) buscarDadosDoGrupo()

    }, [idGrupo, atualizarDados, token, loading])

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

    const desfazerPagamento = async (id: number) => {
        try {
            const mensagem = await despesaService.desfazerPagamento(id, token!);
            
            setAtualizarDados(prev => prev + 1);
            alert(mensagem);
            
            setParcelaParaDesfazer(null);
        } catch (error: any) {
            console.error(error);
            
        }

    }

    return (
        <>
            <section className={styles.tela_morador}>
                <div className={styles.sideBar}>
                    <div className={styles.sideBarUp}>
                        <h2>RepPay</h2>
                        <button className={styles.ativado}>
                            <img src={dashboard_ativado} />
                            Dashboard
                        </button>
                    </div>
                    <div className={styles.sideBarBottom}>
                        <button onClick={() => navigate('/home')}>
                            <img src={sair}/>
                            Sair
                        </button>
                    </div>

                </div>
                <div className={styles.principal}>
                    <HeaderGrupo nome={nome || 'Usuário'} tipo={grupo?.isAdmin ? 'ADMINISTRADOR' : 'MORADOR'} nome_grupo={grupo?.nome || 'Republica'} />
                    <div className={styles.conteudo}>
                        <div className={styles.containerInformacaoPrincipal}>
                            <div className={styles.containerDevedor}>
                                <p>SEU SALDO DEVEDOR</p>
                                <h2>{utilitarios.formatarValor(minhaDivida)}</h2>
                            </div>
                            <div className={styles.containerVencimento}>
                                <div className={styles.proximoVencimento}>
                                    <p>Próximo Vencimento</p>
                                    <img src={calendario} className={styles.calendario} />
                                </div>
                                <h2 className={proximaConta ? styles.existe : styles.naoExiste}>{proximaConta?.vencimento || "Não há nenhuma conta próxima do vencimento!"}</h2>
                                <p>{utilitarios.formatarValor(proximaConta?.valor || 0)} {proximaConta?.nomeDespesa || ''}</p>
                            </div>
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
                                {pendentes.length > 0 ? (
                                <div className={styles.contas}>
                                    {pendentes.map((parcela) => {
                                    
                                        return (

                                            <ParcelaPendenteIndividual 
                                                key={parcela.idParcela} 
                                                icone={parcela.icone} 
                                                vencimento={parcela.vencimento} 
                                                nomeDespesa={parcela.nomeDespesa}
                                                valor={parcela.valor} onClick={() => setParcelaParaConfirmar(parcela.idParcela)}
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

                                            <ParcelaAnaliseIndividual 
                                                key={parcela.idParcela} 
                                                icone={parcela.icone} 
                                                nomeDespesa={parcela.nomeDespesa}
                                                valor={parcela.valor} onClick={() => setModalAviso(true)}
                                                onCancel={() => setParcelaParaDesfazer(parcela.idParcela)}
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

                                            <ParcelaPagoIndividual 
                                                key={parcela.idParcela} 
                                                icone={parcela.icone} 
                                                nomeDespesa={parcela.nomeDespesa}
                                                valor={parcela.valorPago} 
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
                                texto={'Você tem certeza que deseja desfazer a validação deste pagamento? Após confirmar, não será mais possível reverter!'}
                                isOpen={parcelaParaDesfazer !== null} 
                                onClose={() => setParcelaParaDesfazer(null)} 
                                onClick={() => {
                                    if (parcelaParaDesfazer !== null) {
                                        desfazerPagamento(parcelaParaDesfazer)
                                    }
                                }}
                            />

                            <ModalSucesso isOpen={modalAviso} onClose={() => setModalAviso(false)} titulo={'Aguardando Validação!'} texto={'Essa despesa está aguardando a validaçãodo administrador responsável! Basta aguardar.'} imagem={desfazer} />

                    </div>

                </div>

            </section>        
        </>

    )

}

export default Morador