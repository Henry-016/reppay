import styles from './Morador.module.scss'
import dashboard_ativado from './../../assets/dashboard_ativado.svg'
import sair from './../../assets/sair.svg'
import HeaderGrupo from './HeaderGrupo'
import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useParams } from 'react-router-dom'
import calendario from './../../assets/calendario.svg'
import { useAuth } from './../../context/AuthContext'

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

function Morador() {

    const navigate = useNavigate()

    const [atualizarDados, setAtualizarDados] = useState(0)
    const [grupo, setGrupo] = useState<DadosGrupo | null>(null)
    const [minhaDivida, setMinhaDivida] = useState<number>(0)
    const [proximaConta, setProximaConta] = useState<ProximaConta>()

    const nome = localStorage.getItem('nomeUsuario');

    const { idGrupo } = useParams<{ idGrupo: string }>()

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
                    
                    if (dados.isAdmin) {
                        navigate(`/admin/${idGrupo}`);
                        return; 
                    }

                    setGrupo(dados)
                    setAtualizarDados(prev => prev + 1)
                }
                
                const respostaDividas = await fetch(`http://localhost:5149/api/Despesa/MinhasDividas`, {
                    method: 'GET',
                    headers: { 'Authorization': `Bearer ${token}` }
                })
                
                if (respostaDividas.ok) {
                    const dadosDividas = await respostaDividas.json()
                    setMinhaDivida(dadosDividas.totalDevido || 0)

                }

                else {
                    navigate('/home');
                }

            } catch (error) {
                console.error("Erro na requisição:", error)
            }
        }

        if (idGrupo) {
            buscarDadosDoGrupo();
        }

    }, [idGrupo, atualizarDados]);

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