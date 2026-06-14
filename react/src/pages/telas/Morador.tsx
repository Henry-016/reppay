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

    const nome = localStorage.getItem('nomeUsuario');

    const { idGrupo } = useParams<{ idGrupo: string }>()

    const { token } = useAuth()

    useEffect(() => {
        
        const buscarDadosDoGrupo = async () => {
            
            try {
                const dadosGrupo = await grupoService.buscarGrupo( idGrupo, token)

            if (dadosGrupo.isAdmin) {
                navigate(`/admin/${idGrupo}`)
                return

            }
            
            setGrupo(dadosGrupo)
                
                const [dadosMinhasDividas, dadosProximaConta, dadosAnalises, dadosHistorico] = await Promise.all([
                despesaService.buscarMinhasDividas(idGrupo, token),
                grupoService.buscarProximaConta(idGrupo, token),
                despesaService.buscarAnalises(idGrupo, token),
                despesaService.buscarHistorico(idGrupo, token)
            ])

                setMinhaDivida(dadosMinhasDividas.totalDevido)
                setProximaConta(dadosProximaConta)
                setPendentes(dadosMinhasDividas.listaDividas)
                setEmAnalise(dadosAnalises.listaAnalises)
                setHistoricoPago(dadosHistorico.listaHistorico)

            }

            if (idGrupo) {
                buscarDadosDoGrupo();

            }

    }

    }, [idGrupo, atualizarDados])

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
                        <div className={styles.containerDevedor}>
                            <p>SEU SALDO DEVEDOR</p>
                            <h2>{minhaDivida}</h2>
                        </div>
                        <div className={styles.containerVencimento}>
                            <div className={styles.proximoVencimento}>
                                <p>Próximo Vencimento</p>
                                <img src={calendario} className={styles.calendario} />
                            </div>
                            
                        </div>
                    </div>
                </div>

            </section>        
        </>

    )

}

export default Morador