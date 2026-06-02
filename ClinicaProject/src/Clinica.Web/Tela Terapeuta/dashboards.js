// ============================================================
// 1. ELEMENTOS
// ============================================================
const el = {
    grid: document.getElementById("gridConsultas"),
    dateText: document.getElementById("currentDate"),

    btnAbrir: document.getElementById("btnAbrir"),
    btnConfirmar: document.getElementById("confirmarAgendamento"),
    btnAbrirProntuario: document.getElementById("btnAbrirProntuario"),
    btnAberturaProntuarioPaciente: document.getElementById("btn-abrir-pronturario-paciente"),
    btnFecharProntuario: document.getElementById("btn-fechar-prontuario"),

    modalAgenda: document.getElementById("modalAgenda"),
    modalSucesso: document.getElementById("modalSucesso"),
    modalProntuario: document.getElementById("modalProntuario"),

    selectPrincipal: document.getElementById("selectTerapeutaPrincipal"),
    selectModal: document.getElementById("selectTerapeutaModal"),
    inputData: document.getElementById("dataAgendamento"),
    containerHorarios: document.getElementById("containerHorarios"),

    confirmMedico: document.getElementById("confirmMedico"),
    confirmDataHora: document.getElementById("confirmDataHora"),


    nomePacienteProntuario: document.getElementById("nomePacienteAgendado"),
    textoProntuario: document.getElementById("textoProntuario"),
    btnSalvarProntuario: document.getElementById("btnSalvarProntuario")
};

// ============================================================
// 3. INIT
// ============================================================
document.addEventListener("DOMContentLoaded", () => {
    init();
});

function init() {
    setDate();
    renderConsultas();
    bindEvents();
}

// ============================================================
// 4. UI - DATA
// ============================================================
function setDate() {
    if (!el.dateText) return;

    const hoje = new Date();
    el.dateText.innerText = hoje.toLocaleDateString('pt-br', {
        weekday: 'long',
        day: 'numeric',
        month: 'long'
    });
}

// ============================================================
// 5. RENDERIZAÇÃO
// ============================================================
function renderConsultas() {
    if (!el.grid) return;

    el.grid.innerHTML = "";

    state.consultasSemana.forEach(c => {
        const card = document.createElement("div");
        card.className = "card-consulta";

        card.innerHTML = `
            <div class="card-horario">🕒 ${c.hora}</div>
            <h3 class="card-paciente-nome">${c.nome}</h3>
            <div class="card-status">${c.tipo}</div>

            <button class="btn-card-inline" data-paciente="${c.nome}">
                Ver Prontuário
            </button>
        `;


        el.grid.appendChild(card);
    });
}

// ============================================================
// 6. HORÁRIOS
// ============================================================
function gerarHorarios() {
    if (!el.containerHorarios) return;

    el.containerHorarios.innerHTML = "";

    const horarios = ["08:00", "10:00", "14:00", "16:00"];

    horarios.forEach(h => {
        const wrapper = document.createElement("div");

        const btnHora = document.createElement("button");
        btnHora.className = "btn-hora";
        btnHora.innerText = h;

        btnHora.onclick = () => {
            if (btnHora.classList.contains("indisponivel")) return;

            document.querySelectorAll(".btn-hora")
                .forEach(b => b.classList.remove("selecionado"));

            btnHora.classList.add("selecionado");
        };

        const btnBloquear = document.createElement("button");
        btnBloquear.innerText = "Bloquear";

        btnBloquear.onclick = (e) => {
            e.stopPropagation();

            const bloqueado = btnHora.classList.toggle("indisponivel");

            btnBloquear.innerText = bloqueado ? "Desbloquear" : "Bloquear";
            btnBloquear.style.color = bloqueado ? "#22c55e" : "#ef4444";
        };

        wrapper.appendChild(btnHora);
        wrapper.appendChild(btnBloquear);
        el.containerHorarios.appendChild(wrapper);
    });
}

// ============================================================
// 7. AGENDAMENTO
// ============================================================
function abrirModalAgendamento() {
    if (!el.selectPrincipal.value) {
        alert("Selecione um profissional.");
        return;
    }

    el.selectModal.value = el.selectPrincipal.value;
    gerarHorarios();
    el.modalAgenda.showModal();
}

function finalizarAgendamento() {
    const hora = document.querySelector(".btn-hora.selecionado");

    if (!el.inputData.value || !hora) {
        alert("Escolha data e horário.");
        return;
    }

    const medico = el.selectModal.options[el.selectModal.selectedIndex].text;
    const data = el.inputData.value.split("-").reverse().join("/");

    state.consultas.push({
        nome: "Leonardo Ernandes",
        medico,
        hora: hora.innerText,
        data
    });

    if (el.confirmMedico) el.confirmMedico.innerText = medico;
    if (el.confirmDataHora) el.confirmDataHora.innerText = `${data} às ${hora.innerText}`;

    el.modalAgenda.close();
    el.modalSucesso.showModal();
}

// ============================================================
// 8. PRONTUÁRIO
// ============================================================


// 2. Escutador de evento no botão de abertura
el.btnAbrirProntuario.addEventListener('click', () => {
    // Pega o texto do paciente do seu HTML dinamicamente
    const nomePaciente = document.getElementById('nomePacienteAgendado').innerText;

    // Passa o nome limpo para a função
    abrirProntuario(nomePaciente.replace("Nome do paceinte: ", ""));
});
el.btnFecharProntuario.addEventListener('click', () => {
    el.btnAberturaProntuarioPaciente.close();
});

// 3. Função responsável por abrir o modal do prontuário
function abrirProntuario(nome) {
    console.log(`Abrindo o prontuario do paciente: ${nome}`);
    el.btnAberturaProntuarioPaciente.showModal();
}

// 4. Função para salvar o prontuário (ajustada para fechar o modal correto)
function salvarProntuario() {
    const btn = el.btnSalvarProntuario;
    if (!btn) return; // Proteção caso o botão salvar ainda não exista no HTML

    const original = btn.innerText;
    btn.innerText = "⌛ Salvando...";
    btn.disabled = true;

    setTimeout(() => {
        alert(`✅ Evolução salva!`);
        btn.innerText = original;
        btn.disabled = false;
        el.modalProntuario.close(); // Fecha o modal correto
    }, 800);
}


// ============================================================
// 9. EVENTOS
// ============================================================
function bindEvents() {

    // Abrir agendamento
    el.btnAbrir?.addEventListener("click", abrirModalAgendamento);

    // Confirmar
    el.btnConfirmar?.addEventListener("click", finalizarAgendamento);

    // Salvar prontuário
    el.btnSalvarProntuario?.addEventListener("click", salvarProntuario);

    // Delegação (cards dinâmicos)
    document.addEventListener("click", (e) => {
        const btn = e.target.closest(".btn-card-inline");

        if (btn) {
            const nome = btn.dataset.paciente;
            abrirProntuario(nome);
        }
    });

    // Navegação ativa
    document.querySelectorAll("nav a").forEach(link => {
        link.addEventListener("click", () => {
            document.querySelectorAll("nav a")
                .forEach(l => l.classList.remove("active"));

            link.classList.add("active");
        });
    });
}// 1. Função para definir a classe CSS baseada no status
const definirClasseStatus = (status) => {
    const s = status.toLowerCase();
    if (s === 'confirmado') return 'status-confirmado';
    if (s === 'pendente') return 'status-pendente';
    if (s === 'bloqueado') return 'status-bloqueado';
    return '';
};

// 2. Na sua função de renderizar cards, aplique assim:
const renderizarCards = () => {
    grid.innerHTML = "";

    consultas.forEach(c => {
        // Chamamos a função para pegar a classe dinâmica
        const classeStatus = definirClasseStatus(c.status);

        const card = document.createElement("div");
        card.className = "card-consulta";
        card.innerHTML = `
            <div class="card-horario">🕒 ${c.hora}</div>
            <h3 class="card-paciente-nome">${c.nome}</h3>
            
            <!-- Aqui aplicamos a classe dinâmica -->
            <div class="card-status ${classeStatus}">
                ${c.status}
            </div>

            <button class="btn-card-inline" onclick="abrirProntuario('${c.nome}')">
                Ver Prontuário
            </button>
        `;
        grid.appendChild(card);
    });
};
document.addEventListener("DOMContentLoaded", () => {
    // Captura as referências dos elementos do seu card
    const h2NomePaciente = document.getElementById("nomePacienteAgendado");
    const labelTerapeuta = document.querySelector(".card-consulta .campo-selecao label");

    // Lê todas as informações da memória do navegador
    const pacienteSalvo = localStorage.getItem("pacienteAgendado");
    const dataSalva = localStorage.getItem("dataAgendada");
    const horaSalva = localStorage.getItem("horaAgendada");
    const profissionalSalvo = localStorage.getItem("profissionalAgendado");

    // Se encontrou dados, injeta no card de forma organizada
    if (pacienteSalvo && h2NomePaciente) {
        h2NomePaciente.textContent = pacienteSalvo;
    }

    if (profissionalSalvo && labelTerapeuta) {
        // Exibe o nome do profissional e o horário/data no card
        labelTerapeuta.innerHTML = `<strong>Profissional:</strong> ${profissionalSalvo} <br> 
                                    <strong>Horário:</strong> ${dataSalva} às ${horaSalva}`;
    }
});