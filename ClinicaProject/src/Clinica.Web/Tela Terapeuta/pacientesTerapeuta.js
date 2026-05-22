// ============================================================
// 1. ESTADO GLOBAL E UTILITÁRIOS
// ============================================================
const state = {
    // Busca dados e já realiza a FAXINA AUTOMÁTICA de itens "undefined"
    get consultas() {
        const dadosBrutos = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
        // Filtra apenas consultas válidas para evitar erros visuais
        const validas = dadosBrutos.filter(c => c && c.nome && c.data && c.id);

        // Se encontrou lixo, limpa o LocalStorage silenciosamente
        if (dadosBrutos.length !== validas.length) {
            localStorage.setItem("consultas_fisio", JSON.stringify(validas));
        }
        return validas;
    },
    get hoje() {
        return new Date().toLocaleDateString('pt-br');
    },
    get mesAtual() {
        // Retorna o mês atual formatado (ex: "04")
        return (new Date().getMonth() + 1).toString().padStart(2, '0');
    }
};

const el = {
    grid: document.getElementById("gridConsultas"), // Onde aparecem os cards
    dateText: document.getElementById("currentDate"),
    statsCards: document.querySelectorAll(".stat-value"), // Os círculos/cards de números
    modalProntuario: document.getElementById("modalProntuario"),
    nomePacienteProntuario: document.getElementById("nomePacienteProntuario"),
    textoProntuario: document.getElementById("textoProntuario"),
    btnSalvarProntuario: document.getElementById("btnSalvarProntuario")
};

// ============================================================
// 2. RENDERIZAÇÃO DO DASHBOARD
// ============================================================
function renderizarDashboard() {
    if (!el.grid) return;
    el.grid.innerHTML = "";

    const listaGeral = state.consultas;
    const hojeStr = state.hoje;
    const mesAtualStr = state.mesAtual;

    const agora = new Date();
    const horaAtualMinutos = (agora.getHours() * 60) + agora.getMinutes();

    let totalHoje = 0;
    let totalMes = 0;

    // Ordenar por horário para exibição organizada
    const listaOrdenada = listaGeral.sort((a, b) => a.hora.localeCompare(b.hora));

    listaOrdenada.forEach(c => {
        const dataConsulta = c.data;
        const mesConsulta = dataConsulta.split('/')[1];

        // Lógica de Contagem para os Widgets Superiores
        if (dataConsulta === hojeStr) totalHoje++;
        if (mesConsulta === mesAtualStr) totalMes++;

        // Renderiza no grid apenas se for a consulta de HOJE
        if (dataConsulta === hojeStr) {
            const [horaC, minC] = c.hora.split(':').map(Number);
            const consultaMinutos = (horaC * 60) + minC;
            const tempoEstimadoSessao = 50;

            let statusClass = "status-azul";
            let statusTexto = "Agendado";

            // Lógica de Status em tempo real
            if (horaAtualMinutos >= consultaMinutos && horaAtualMinutos <= (consultaMinutos + tempoEstimadoSessao)) {
                statusClass = "status-amarelo";
                statusTexto = "Em Atendimento";
            } else if (horaAtualMinutos > (consultaMinutos + tempoEstimadoSessao)) {
                statusClass = "status-verde";
                statusTexto = "Concluído";
            }

            const card = document.createElement("div");
            card.className = "card-consulta";
            card.innerHTML = `
                <div class="card-horario">🕒 ${c.hora}</div>
                <div class="card-info">
                    <h3 class="card-paciente-nome">${c.nome}</h3>
                    <p class="card-especialista">${c.especialista || "Fisioterapia"}</p>
                </div>
                <div class="card-status ${statusClass}">● ${statusTexto}</div>
                <div class="card-acoes">
                    <button class="btn-card-inline" onclick="abrirProntuario('${c.id}')">
                        Ver Prontuário
                    </button>
                    <button class="btn-cancelar-discreto" onclick="cancelarConsulta('${c.id}')" title="Desmarcar">
                        ✕
                    </button>
                </div>
            `;
            el.grid.appendChild(card);
        }
    });

    // Atualiza os widgets superiores (Consultas Hoje, Mês, Total)
    if (el.statsCards && el.statsCards.length >= 3) {
        el.statsCards[0].innerText = totalHoje;
        el.statsCards[1].innerText = totalMes;
        el.statsCards[2].innerText = listaGeral.length; // Atendidos/Total
    }

    if (totalHoje === 0) {
        el.grid.innerHTML = `<div class="card-vazio-msg">☕ Sem consultas para hoje.</div>`;
    }
}

// ============================================================
// 3. AÇÕES (Global window para funcionar no HTML dinâmico)
// ============================================================

window.cancelarConsulta = (id) => {
    if (confirm("Deseja desmarcar este paciente? O horário será liberado em todas as telas.")) {
        const consultas = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
        const consultaParaRemover = consultas.find(c => String(c.id) === String(id));

        if (consultaParaRemover) {
            // Remove da lista principal
            const novaLista = consultas.filter(c => String(c.id) !== String(id));
            localStorage.setItem("consultas_fisio", JSON.stringify(novaLista));

            // Libera o cadeado na agenda global
            const agendaGlobal = JSON.parse(localStorage.getItem('agendaFisioData')) || {};
            if (agendaGlobal[consultaParaRemover.dataISO]) {
                delete agendaGlobal[consultaParaRemover.dataISO][consultaParaRemover.hora];
                localStorage.setItem('agendaFisioData', JSON.stringify(agendaGlobal));
            }

            renderizarDashboard();
            window.dispatchEvent(new Event('storage'));
        }
    }
};

window.abrirProntuario = (id) => {
    const consultas = state.consultas;
    const consulta = consultas.find(c => String(c.id) === String(id));

    if (consulta) {
        el.nomePacienteProntuario.innerText = consulta.nome;
        // Recupera evolução anterior se existir, senão mostra a queixa
        el.textoProntuario.value = consulta.evolucao || `--- QUEIXA DO PACIENTE ---\n${consulta.sintomas || "Nenhum relato."}\n\n--- EVOLUÇÃO MÉDICA ---\n`;

        el.modalProntuario.showModal();

        // Configura o botão de salvar para este paciente específico
        el.btnSalvarProntuario.onclick = () => {
            const todas = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
            const index = todas.findIndex(p => String(p.id) === String(id));

            if (index !== -1) {
                todas[index].evolucao = el.textoProntuario.value;
                localStorage.setItem("consultas_fisio", JSON.stringify(todas));

                el.btnSalvarProntuario.innerText = "⌛ Salvando...";
                setTimeout(() => {
                    alert(`✅ Evolução de ${consulta.nome} salva com sucesso!`);
                    el.btnSalvarProntuario.innerText = "Salvar Evolução";
                    el.modalProntuario.close();
                }, 500);
            }
        };
    }
};

// ============================================================
// 4. SINCRONIZAÇÃO E INICIALIZAÇÃO
// ============================================================

window.addEventListener('storage', (e) => {
    if (e.key === 'consultas_fisio' || e.key === 'agendaFisioData') {
        renderizarDashboard();
    }
});

document.addEventListener("DOMContentLoaded", () => {
    // Data no topo da página
    if (el.dateText) {
        el.dateText.innerText = new Date().toLocaleDateString('pt-br', {
            weekday: 'long', day: 'numeric', month: 'long'
        });
    }

    renderizarDashboard();
});