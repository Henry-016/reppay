import styles from './HomeGeral.module.scss'
import HeaderGeral from './HeaderGeral'
import plus from './../../assets/plus.svg'
import { useState, useEffect } from 'react'
import Modal_EscolhaCriarEntrar from './../modais/Modal_EscolhaCriarEntrar'
import CardGrupo from './../../components/CardGrupo'
import { useNavigate } from 'react-router-dom';
import { useAuth } from './../../context/AuthContext'
import { grupoService } from '../../services/grupoService'

interface GrupoUsuario {
    idGrupo: number;
    nome: string;
    codigoAcesso: string;
    imagemBanner: string | null;
    isAdmin: boolean;
}

function HomeGeral() {

    const [modal, setModal] = useState(false)
    const [grupos, setGrupos] = useState<GrupoUsuario[]>([])

    const { token, loading, usuario } = useAuth()

    const nome = usuario?.nome

    const navigate = useNavigate()

    useEffect(() => {
        
        if (loading) return

        if (!token) {
            navigate('/login')
            return

        }

        const carregarGrupos = async () => {
            try {
                const dados = await grupoService.buscarGrupos(token)
                setGrupos(dados)

            } catch (error) {
                console.error("Erro ao carregar grupos:", error)

            }
        }
    
        carregarGrupos()

        if (!token) {
            navigate('/login')
            return

        }

    }, [token, grupos, modal, loading])

    return (
        <>
            <section className={styles.tela_home_geral}>
                <HeaderGeral nome={nome || 'Usuário'}/>
                <div className={styles.conteudo}>
                    <div className={styles.titulos}>
                        <h2>Bem-vindo de volta, {nome}!</h2>
                        <p>Selecione seu painel ativo para gerenciar suas finanças compartilhadas.</p>
                    
                    </div>
                    <div className={styles.republicas}>
                        <div onClick={() => setModal(true)}className={styles.adicionarRepublicas}>
                            <img src={plus} className={styles.plus}/>
                            <h2>Nova República</h2>
                            <p>Crie um novo ambiente ou junte-se a uma república existente usando o código.</p>
                        </div>
                        {grupos.map((grupo) => (
                            <CardGrupo
                                key={grupo.idGrupo}
                                imagem={grupo.imagemBanner || 'Usuário'}
                                tipo={grupo.isAdmin ? 'ADMINISTRADOR' : 'MORADOR'}
                                titulo={grupo.nome}
                                texto={'Acesso total ao painel financeiro, gestão de moradores e relatórios detalhados de despesas mensais.'}
                                onClick={() => {
                                    if (grupo.isAdmin) {
                                        navigate(`/home/admin/${grupo.idGrupo}`);
                                    } else {
                                        navigate(`/home/morador/${grupo.idGrupo}`);
                                    }
                                }}
                            />
                        ))}
                    </div>
                </div>

                <Modal_EscolhaCriarEntrar isOpen={modal} onClose={() => setModal(false)}/>

            </section>
        </>

    )

}

export default HomeGeral;
