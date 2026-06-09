import styles from './HomeGeral.module.scss'
import HeaderGeral from './HeaderGeral'
import plus from './../../assets/plus.svg'
import { useState, useEffect } from 'react'
import Modal_EscolhaCriarEntrar from './../modais/Modal_EscolhaCriarEntrar'
import CardGrupo from './../../components/CardGrupo'
import { useNavigate } from 'react-router-dom';

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

    const nome = localStorage.getItem('nomeUsuario')

    const navigate = useNavigate()

    useEffect(() => {
        const buscarGrupos = async () => {
            const token = localStorage.getItem('token')
            try {
                const resposta = await fetch('http://localhost:5149/api/Grupo/Meus', {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                });

                if (resposta.ok) {
                    const dados = await resposta.json();
                    setGrupos(dados)
                }
            } catch (error) {
                console.error(error)
            }
        };

        buscarGrupos();
    }, []);

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
                                        navigate(`/admin/${grupo.idGrupo}`);
                                    } else {
                                        navigate(`/morador/${grupo.idGrupo}`);
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
