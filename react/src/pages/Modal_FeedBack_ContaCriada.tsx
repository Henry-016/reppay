import './Modal_FeedBack_ContaCriada.scss'

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;

}

function Modal_FeedBack_ContaCriada( {isOpen, onClose}: ModalProps ) {
    if (!isOpen) return null;

    return (
        <>
        <section id='tela_mfcc'>
            <div id='modal'>
                <div id='imagem'></div>
                <h2>Conta Criada com Sucesso!</h2>
                <p id='suaJornada'>Sua jornada rumo a uma república mais organizada começa agora.</p>
                <div id='divAmbienteSeguro'>
                    <div id='escudo'></div>
                    <div id='ambienteSeguroTexto'>
                        <h3>Ambiente Seguro</h3>
                        <p>Criptografia de ponta a ponta ativa</p>
                    </div>
                </div>
                <button id='continuar' onClick={onClose}>CONTINUAR</button>
            </div>

        </section>
        
        </>

    )

}

export default Modal_FeedBack_ContaCriada;