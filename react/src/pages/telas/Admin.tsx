import styles from './Admin.module.scss'
import { useState, useEffect } from 'react'
import HeaderGrupo from './HeaderGrupo'
import { useParams } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';
import dashboard_ativado from './../../assets/dashboard_ativado.svg'
import moradores_desativado from './../../assets/moradores_desativado.svg'
import sair from './../../assets/sair.svg'
import add from './../../assets/add.svg'

interface DadosGrupo {
    idGrupo: number;
    nome: string;
    codigoAcesso: string;
    imagemBanner: string | null;
    isAdmin: boolean;
}

function Admin() {
    
    const navigate = useNavigate()

    const { idGrupo } = useParams<{ idGrupo: string }>()
    
    const [grupo, setGrupo] = useState<DadosGrupo | null>(null)
    const [totalReceber, setTotalReceber] = useState<number>(0)
    const [minhaDivida, setMinhaDivida] = useState<number>(0)

    const nome = localStorage.getItem('nomeUsuario');

    useEffect(() => {
        
        const buscarDetalhesDoGrupo = async () => {
            const token = localStorage.getItem('token')
            
            
            try {
                const resposta = await fetch(`http://localhost:5149/api/Grupo/${idGrupo}`, {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });

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
                }

                const respostaDividas = await fetch(`http://localhost:5149/api/Despesa/MinhasDividas`, {
                    method: 'GET',
                    headers: { 'Authorization': `Bearer ${token}` }
                })
                
                if (respostaDividas.ok) {
                    const dadosDividas = await respostaDividas.json()
                    setMinhaDivida(dadosDividas.totalDevido || 0)
                }

            } catch (error) {
                console.error("Erro na requisição:", error)
            }
        }

        if (idGrupo) {
            buscarDetalhesDoGrupo();
        }
    }, [idGrupo, navigate]);

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
                                        <h2>15 Out, 2026</h2>
                                    </div>
                                </div>
                            </div>
                            <button className={styles.add}>
                                <img src={add} />
                                <h2>Lançar Nova Despesa</h2>
                            </button>

                        </div>

                    </div>
                </div>

            </section>
        </>

    )

}

export default Admin;