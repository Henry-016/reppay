import styles from './Admin.module.scss'
import { useState, useEffect } from 'react'
import HeaderGrupo from './HeaderGrupo'
import { useParams } from 'react-router-dom'
import { useNavigate } from 'react-router-dom'
import dashboard_ativado from './../../assets/dashboard_ativado.svg'
import moradores_desativado from './../../assets/moradores_desativado.svg'
import dashboard_desativado from './../../assets/dashboard_desativado.svg'
import moradores_ativado from './../../assets/moradores_ativado.svg'
import sair from './../../assets/sair.svg'
import add from './../../assets/add.svg'
import ModalCriarDespesa from './../modais/ModalCriarDespesa'
import ParcelaPendente from './../../components/ParcelaPendente'
import ParcelaAnalise from './../../components/ParcelaAnalise'
import ParcelaPago from './../../components/ParcelaPago'
import ModalConfirmacao from '../modais/ModalConfirmacao'
import MoradorComponente from '../../components/MoradorComponente'
import key from './../../assets/key.svg'

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
    
    const nome = localStorage.getItem('nomeUsuario');

    const token = localStorage.getItem('token');

    const buscarGrupo = async () => {
        try {
            const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}`, { headers: { 'Authorization': `Bearer ${token}` } });
            if (!res.ok) { navigate('/home'); return; }
            const dados = await res.json();
            if (!dados.isAdmin) { navigate(`/morador/${idGrupo}`); return; }
            setGrupo(dados);
        } catch (error) { console.error(error); }
    };

    const buscarInadimplentes = async () => {
        try {
            const res = await fetch(`http://localhost:5149/api/Despesa/Inadimplentes/${idGrupo}`, { headers: { 'Authorization': `Bearer ${token}` } });
            if (res.ok) {
                const dados = await res.json();
                setTotalReceber(dados.totalAReceber || 0);
                setInadimplentes(dados.listaInadimplentes || []);
            }
        } catch (error) { console.error(error); }
    };

    const buscarMinhasDividas = async () => {
        try {
            const res = await fetch(`http://localhost:5149/api/Despesa/MinhasDividas`, { headers: { 'Authorization': `Bearer ${token}` } });
            if (res.ok) {
                const dados = await res.json();
                setMinhaDivida(dados.totalDevido || 0);
            }
        } catch (error) { console.error(error); }
    };

    const buscarProximaConta = async () => {
        try {
            const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}/proximaConta`, { headers: { 'Authorization': `Bearer ${token}` } });
            if (res.ok && (await res.text()).trim() !== "") { setProximaConta(await res.json());
            }
            else {
                setProximaConta(undefined)
            }
        } catch (error) { console.error(error); }
    }

    const buscarAnalises = async () => {
        try {
            const res = await fetch(`http://localhost:5149/api/Despesa/AnalisesPendentes/${idGrupo}`, { headers: { 'Authorization': `Bearer ${token}` } });
            if (res.ok) {
                const dados = await res.json();
                setEmAnalise(dados.listaAnalises || []);
            }
        } catch (error) { console.error(error); }
    };

    const buscarHistorico = async () => {
        try {
            const res = await fetch(`http://localhost:5149/api/Despesa/HistoricoGrupo/${idGrupo}`, { headers: { 'Authorization': `Bearer ${token}` } });
            if (res.ok) {
                const dados = await res.json();
                setHistoricoPago(dados.listaHistorico || []);
            }
        } catch (error) { console.error(error); }
    };

    const buscarMoradores = async () => {
        try {
            const res = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}/Membros`, { headers: { 'Authorization': `Bearer ${token}` } });
            if (res.ok) setMoradores(await res.json());
        } catch (error) { console.error(error); }
    };

    useEffect(() => {
        const buscarDadosDoGrupo = async () => {
            await buscarGrupo();
            await Promise.all([
                buscarInadimplentes(),
                buscarMinhasDividas(),
                buscarProximaConta(),
                buscarAnalises(),
                buscarHistorico(),
                buscarMoradores()
            ]);
        };

        if (idGrupo) buscarDadosDoGrupo();
    }, [idGrupo, modal, atualizarDados]);

    const sinalizarPagamento = async (id: number) => {
        try {
            const res = await fetch(`http://localhost:5149/api/Despesa/SinalizarPagamento/${id}`, {
                method: 'PUT',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            const resData = await res.json();
            if (res.ok) {
                setAtualizarDados(prev => prev + 1);
                alert(resData.mensagem);
                setParcelaParaConfirmar(null);
            } else { alert(resData.mensagem); }
        } catch (error) { console.error(error); }
    };

    const validarPagamento = async (id: number, decisao: boolean) => {
        try {
            const res = await fetch(`http://localhost:5149/api/Despesa/ValidarPagamento/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
                body: JSON.stringify({ aprovado: decisao })
            });
            const resData = await res.json();
            if (res.ok) {
                setAtualizarDados(prev => prev + 1);
                alert(resData.mensagem);
                setParcelaParaAceitar(null);
                setParcelaParaRejeitar(null);
            } else { alert(resData.mensagem); }
        } catch (error) { console.error(error); }
    };

    const copiarParaAreaDeTransferencia = async () => {
        if (grupo) await navigator.clipboard.writeText(grupo.codigoAcesso);
    };

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
                            <img src={sair}/>
                            Sair
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
                                    <h2>R$ {totalReceber}</h2>
                                </div>
                                <div className={styles.despesasRepublicaBottom}>
                                    <div className={styles.dividaIndividual}>
                                        <p>Sua parte individual</p>
                                        <h2>R$ {minhaDivida}</h2>
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
                                    <p>Membros</p>
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
                                    <button onClick={() => copiarParaAreaDeTransferencia()} className={styles.copiar}>Copiar</button>
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