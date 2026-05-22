document.addEventListener("DOMContentLoaded", () => {
    const el = {
        dataInput: document.getElementById("dataGestao"),
        grid: document.getElementById("gridGestao"),
        listaGeral: document.getElementById("listaGeralPacientes"),
        countHoje: document.getElementById("countHoje"),
        countSemana: document.getElementById("countSemana"),
        countMes: document.getElementById("countMes")
    };

    const gradeBase = ["10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00"];

    // Define data inicial como hoje
    const hojeISO = new Date().toISOString().split('T')[0];
    if (el.dataInput) el.dataInput.value = hojeISO;

    // ============================================================
    // ATUALIZAÇÃO DO DASHBOARD COM FILTRO ANTI-UNDEFINED
    // ============================================================
    const atualizarDashboardELista = () => {
        // 1. Pega os dados e remove itens incompletos ou corrompidos (Faxina Automática)
        const dadosBrutos = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
        const consultasFisio = dadosBrutos.filter(c => c && c.nome && c.id && c.data);

        // 2. Se houveram itens removidos na faxina, atualiza o localStorage para limpar o "lixo"
        if (dadosBrutos.length !== consultasFisio.length) {
            localStorage.setItem("consultas_fisio", JSON.stringify(consultasFisio));
        }

        const hojeStr = new Date().toLocaleDateString('pt-br');
        let totalHoje = 0, totalSemana = 0, totalMes = 0;

        el.listaGeral.innerHTML = "";

        if (consultasFisio.length === 0) {
            el.listaGeral.innerHTML = "<p class='vazio'>Aguardando novos agendamentos...</p>";
            el.countHoje.innerText = 0;
            el.countSemana.innerText = 0;
            el.countMes.innerText = 0;
            return;
        }

        // Ordenar consultas por data e hora
        consultasFisio.sort((a, b) => {
            const dataA = a.data.split('/').reverse().join('');
            const dataB = b.data.split('/').reverse().join('');
            return dataA.localeCompare(dataB) || a.hora.localeCompare(b.hora);
        });

        consultasFisio.forEach(info => {
            const [dia, mes, ano] = info.data.split('/');
            const dataConsultaObj = new Date(ano, mes - 1, dia);
            const hojeObj = new Date();
            hojeObj.setHours(0, 0, 0, 0);

            const diffDias = Math.floor((dataConsultaObj - hojeObj) / (1000 * 60 * 60 * 24));

            if (info.data === hojeStr) totalHoje++;
            if (diffDias >= 0 && diffDias <= 7) totalSemana++;
            if (diffDias >= 0 && diffDias <= 30) totalMes++;

            // Renderiza na lista lateral
            el.listaGeral.innerHTML += `
                <div class="paciente-item-geral">
                    <div class="info">
                        <strong>${info.nome}</strong>
                        <span>📅 ${info.data} — ${info.hora}</span>
                    </div>
                    <button class="btn-prontuario-lista" onclick="abrirProntuarioTerapeuta('${info.nome}')">
                        📋 Prontuário
                    </button>
                </div>`;
        });

        if (el.countHoje) el.countHoje.innerText = totalHoje;
        if (el.countSemana) el.countSemana.innerText = totalSemana;
        if (el.countMes) el.countMes.innerText = totalMes;
    };

    // ============================================================
    // GESTÃO DA GRADE DE HORÁRIOS
    // ============================================================
    window.carregarTela = () => {
        const agendaGlobal = JSON.parse(localStorage.getItem('agendaFisioData')) || {};
        const consultasFisio = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
        const dataSelISO = el.dataInput.value;
        const dataSelBR = dataSelISO.split('-').reverse().join('/');

        el.grid.innerHTML = "";

        gradeBase.forEach(hora => {
            const consultaExistente = consultasFisio.find(c => c.data === dataSelBR && c.hora === hora);
            const statusAgenda = agendaGlobal[dataSelISO] ? agendaGlobal[dataSelISO][hora] : null;

            let status = "disponivel";
            let textoBotao = `🔓 ${hora}`;
            let classe = "liberado";

            if (consultaExistente) {
                status = "ocupado";
                textoBotao = `👤 ${hora}`;
                classe = "ocupado";
            } else if (statusAgenda === "bloqueado") {
                status = "bloqueado";
                textoBotao = `🔒 ${hora}`;
                classe = "bloqueado";
            }

            const btn = document.createElement("button");
            btn.className = `btn-gestao ${classe}`;
            btn.innerHTML = textoBotao;

            btn.onclick = () => {
                if (status === "ocupado") {
                    if (confirm(`O paciente ${consultaExistente.nome} está agendado aqui. Desmarcar e bloquear horário?`)) {
                        removerConsultaTotal(consultaExistente.id, dataSelISO, hora);
                    }
                    return;
                }

                const agendaAtu = JSON.parse(localStorage.getItem('agendaFisioData')) || {};
                if (!agendaAtu[dataSelISO]) agendaAtu[dataSelISO] = {};

                agendaAtu[dataSelISO][hora] = (status === "bloqueado") ? null : "bloqueado";

                localStorage.setItem('agendaFisioData', JSON.stringify(agendaAtu));
                carregarTela();
            };
            el.grid.appendChild(btn);
        });

        atualizarDashboardELista();
    };

    function removerConsultaTotal(id, dataISO, hora) {
        let consultas = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
        consultas = consultas.filter(c => String(c.id) !== String(id));
        localStorage.setItem("consultas_fisio", JSON.stringify(consultas));

        let agenda = JSON.parse(localStorage.getItem("agendaFisioData")) || {};
        if (!agenda[dataISO]) agenda[dataISO] = {};
        agenda[dataISO][hora] = "bloqueado";
        localStorage.setItem("agendaFisioData", JSON.stringify(agenda));

        carregarTela();
    }

    if (el.dataInput) el.dataInput.onchange = carregarTela;
    carregarTela();
});

// ============================================================
// FUNÇÕES GLOBAIS E SINCRONIZAÇÃO
// ============================================================
window.abrirProntuarioTerapeuta = (nomePaciente) => {
    const todasConsultas = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
    const dadosPaciente = todasConsultas.find(c => c.nome === nomePaciente);

    const modal = document.getElementById("modalProntuario");
    const campoTexto = document.getElementById("textoProntuario");
    const tituloNome = document.getElementById("nomePacienteProntuario");

    if (dadosPaciente && modal) {
        tituloNome.innerText = dadosPaciente.nome;
        campoTexto.value = `--- QUEIXA DO PACIENTE ---\n${dadosPaciente.sintomas || "Não informado."}\n\n--- EVOLUÇÃO MÉDICA ---\n`;
        modal.showModal();
    }
};

window.addEventListener('storage', (e) => {
    if (e.key === 'consultas_fisio' || e.key === 'agendaFisioData') {
        if (typeof carregarTela === 'function') carregarTela();
    }
});