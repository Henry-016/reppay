import styles from './Admin.module.scss'
import { useState, useEffect } from 'react'
import HeaderGrupo from './HeaderGrupo'
import { useParams } from 'react-router-dom';

interface DadosGrupo {
    idGrupo: number;
    nome: string;
    codigoAcesso: string;
    imagemBanner: string | null;
    isAdmin: boolean;
}

function Admin() {

    const { idGrupo } = useParams<{ idGrupo: string }>()
    
    const [grupo, setGrupo] = useState<DadosGrupo | null>(null)

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
                    setGrupo(dados)
                } else {
                    console.error("Erro ao buscar dados do grupo")
                }
            } catch (error) {
                console.error("Erro na requisição:", error)
            }
        }

        if (idGrupo) {
            buscarDetalhesDoGrupo();
        }
    }, [idGrupo]);

    return (
        <>
            <section className={styles.tela_admin}>
                <div className={styles.sideBar}>
                    <div className={styles.sideBarUp}>
                        <h2>RepPay</h2>
                        <button>Dashboard</button>
                        <button>Moradores</button>
                    </div>
                    <div className={styles.sideBarBottom}>
                        <button>Sair</button>
                    </div>

                </div>
                <div className={styles.principal}>
                    <HeaderGrupo nome={nome || 'Usuário'} tipo={grupo?.isAdmin ? 'ADMINISTRADOR' : 'MORADOR'} nome_grupo={grupo?.nome || 'Republica'} />
                    <div className={styles.conteudo}></div>
                </div>

            </section>
        </>

    )

}

export default Admin;