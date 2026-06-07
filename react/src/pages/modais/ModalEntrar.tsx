import styles from './ModalEntrar.module.scss'
import x from './../../assets/x.svg'
import { useState, SubmitEvent } from 'react'
import ModalCriarEntrarSucesso from './ModalCriarEntrarSucesso'
import imagem from './../../assets/users_codigo.svg'

interface ModalProps {
    isOpen: boolean
    onClose: () => void

}

function ModalEntrar( {isOpen, onClose}: ModalProps ) {

    const [codigo, setCodigo] = useState('')
    const [modal, setModal] = useState(false)

    const [erro, setErro] = useState('')
    const [carregando, setCarregando] = useState(false)

    const entrarGrupo = async (e: SubmitEvent) => {
        e.preventDefault();

        if (!codigo) {
            setErro('Por favor, insira o código do grupo.');
            return;
        }

        setErro('');
        setCarregando(true);

        const token = localStorage.getItem('token');

        try {
            const resposta = await fetch('http://localhost:5149/api/Grupo/Entrar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}` 
                },
                body: JSON.stringify({
                    CodigoAcesso: codigo
                })
            });

            const dados = await resposta.json();

            if (resposta.ok) {
                setModal(true); 
            } else {
                setErro(dados.mensagem || 'Erro ao tentar entrar no grupo.')
            }

        } catch (error) {
            console.error('Erro na requisição:', error)
            setErro('Não foi possível conectar ao servidor.')
        } finally {
            setCarregando(false)
        }
    };

    const fecharELimpar = () => {
        setCodigo('');
        setErro('');
        setModal(false);
        onClose();
    };

    if (!isOpen) return null;

    return (
        <>
            <section className={styles.tela_modal_entrar}>
                <div className={styles.modal}>
                    <div className={styles.imagemContainer}>
                        <img onClick={fecharELimpar} src={x} className={styles.x}/>
                    </div>
                    <img src={imagem} className={styles.imagem}/>
                    {erro && <div className={styles.mensagemErro}>{erro}</div>}
                    <h2>Entrar em uma República</h2>
                    <p>Insira o código exclusivo de 8 dígitos</p>
                    <form onSubmit={entrarGrupo}>
                        <div className={styles.inputContainer}>
                            <input type="text" value={codigo} onChange={(e) => setCodigo(e.target.value)} placeholder='XXXXXXXX'/>
                        </div>
                        <button>ENTRAR NO GRUPO</button>
                    </form>
                    <div className={styles.aviso}>
                        <p>Não tem um convite? Peça a um administrador</p>
                    </div>
                </div>

                <ModalCriarEntrarSucesso isOpen={modal} onClose={onClose} titulo='Grupo Encontrado!' texto='Parabéns! Você agora faz parte do Grupo XXXXXX' />

            </section>
        </>

    )

}

export default ModalEntrar;