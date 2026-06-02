/* ============================================================
   LÓGICA DA TELA DO PACIENTE (MEUS AGENDAMENTOS)
   ============================================================ */

const pacienteLogado = "Arnaldo Candido";

const elementosPaciente = {
    listaConsultas: document.getElementById("listaConsultas"),
    statusVazio: document.getElementById("statusConsultaVazio"),
    modalCancel: document.getElementById("modalAvisoCancel"),
    confirmarCancelBtn: document.getElementById("confirmarCancelamento"),
    modalSintomas: document.getElementById("modalVerSintomas"),
    textoSintomas: document.getElementById("textoSintomasPaciente")
};

let idParaCancelar = null;

const renderizarConsultasPaciente = () => {
    if (!elementosPaciente.listaConsultas) return;

    // 1. Carrega os dados do LocalStorage
    const consultas = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
    
    // 2. Filtra apenas as consultas deste paciente específico
    const minhasConsultas = consultas.filter(c => c.nome === pacienteLogado);

    // 3. Controle de estado vazio
    if (minhasConsultas.length === 0) {
        elementosPaciente.statusVazio.style.display = "block";
        // Remove cards antigos se houver
        const cardsAntigos = document.querySelectorAll('.card-consulta-item');
        cardsAntigos.forEach(card => card.remove());
        return;
    }

    // Oculta o aviso de vazio e limpa a lista para re-renderizar
    elementosPaciente.statusVazio.style.display = "none";
    // Remove apenas os cards de consulta, mantendo o esqueleto
    document.querySelectorAll('.card-consulta-item').forEach(c => c.remove());

    // 4. Criação dos Cards
    minhasConsultas.forEach(consulta => {
        const card = document.createElement("div");
        card.className = "card-moderno card-consulta-item";
        
        // CORREÇÃO: Passando o ID de forma totalmente explícita e segura entre aspas simples tratadas
        card.innerHTML = `
            <div class="card-body-info">
                <span class="badge">Sessão Confirmada</span>
                <h3 class="card-titulo-sessao" style="margin: 15px 0 10px 0;">${consulta.especialista}</h3>
                <div class="info-linha">
                    <p class="card-detalhe"><strong>📅 Data:</strong> ${consulta.data}</p>
                    <p class="card-detalhe"><strong>🕒 Horário:</strong> ${consulta.hora}</p>
                </div>
            </div>
            <div class="botoes-stack" style="margin-top: 20px; display: flex; flex-direction: column; gap: 10px;">
                <button class="btn-agendar-atalho" style="width: 100%; justify-content: center;" 
                        onclick="verSintomas('${consulta.id}')">
                    📋 Ver Meus Sintomas
                </button>
                <button class="btn-cancelar-sessao" onclick="prepararCancelamento('${consulta.id}')">
                    Desmarcar Sessão
                </button>
            </div>
        `;
        elementosPaciente.listaConsultas.appendChild(card);
    });
};

// --- FUNÇÕES DE INTERAÇÃO ---

// CORREÇÃO: Função mapeada globalmente com validação de existência dos modais no DOM
window.verSintomas = (idConsulta) => {
    const consultas = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
    const consultaEncontrada = consultas.find(c => String(c.id) === String(idConsulta));
    
    // Pega o sintoma salvo ou define uma mensagem padrão amigável caso esteja vazio
    const textoSintoma = consultaEncontrada && consultaEncontrada.sintomas 
        ? consultaEncontrada.sintomas 
        : "Nenhum sintoma foi relatado para esta consulta.";

    // Garante que os elementos existem na tela atual antes de executar o método showModal
    if (elementosPaciente.textoSintomas && elementosPaciente.modalSintomas) {
        elementosPaciente.textoSintomas.innerText = textoSintoma;
        elementosPaciente.modalSintomas.showModal();
    } else {
        console.error("Erro: Certifique-se de que os elementos 'textoSintomasPaciente' e 'modalVerSintomas' existem no seu arquivo HTML.");
    }
};

window.prepararCancelamento = (id) => {
    idParaCancelar = id;
    if (elementosPaciente.modalCancel) {
        elementosPaciente.modalCancel.showModal();
    }
};

elementosPaciente.confirmarCancelBtn.onclick = () => {
    if (idParaCancelar) {
        let consultas = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
        
        // Localiza a consulta para liberar o horário na agenda antes de deletar
        const consultaRemovida = consultas.find(c => String(c.id) === String(idParaCancelar));
        
        if (consultaRemovida) {
            // 1. Remove da lista de consultas
            const novasConsultas = consultas.filter(c => String(c.id) !== String(idParaCancelar));
            localStorage.setItem("consultas_fisio", JSON.stringify(novasConsultas));

            // 2. Libera o horário na agenda global para que outros possam marcar
            const agendaGlobal = JSON.parse(localStorage.getItem('agendaFisioData')) || {};
            if (agendaGlobal[consultaRemovida.dataISO]) {
                delete agendaGlobal[consultaRemovida.dataISO][consultaRemovida.hora];
                localStorage.setItem('agendaFisioData', JSON.stringify(agendaGlobal));
            }
        }

        if (elementosPaciente.modalCancel) {
            elementosPaciente.modalCancel.close();
        }
        renderizarConsultasPaciente(); // Atualiza a tela instantaneamente
    }
};

// Inicializa ao carregar a página
document.addEventListener("DOMContentLoaded", renderizarConsultasPaciente);

// Escuta mudanças feitas em outras abas e atualiza a interface atual
window.addEventListener('storage', (e) => {
    if (e.key === 'consultas_fisio' || e.key === 'agendaFisioData') {
        if (typeof renderizarConsultasPaciente === 'function') renderizarConsultasPaciente();
        if (typeof renderizarDashboard === 'function') renderizarDashboard();
        if (typeof carregarTela === 'function') carregarTela();
    }
});
