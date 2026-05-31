// ============================================================
// 1. ESTADO GLOBAL E UTILITÁRIOS
// ============================================================
const statePacientes = {
    get lista() {
        return JSON.parse(localStorage.getItem("pacientes_terapeuta")) || [];
    },
    salvar(novaLista) {
        localStorage.setItem("pacientes_terapeuta", JSON.stringify(novaLista));
    }
};

const elPacientes = {
    grid: document.getElementById("gridPacientes"),
    inputBusca: document.getElementById("inputBuscaPaciente"),
    modalNovo: document.getElementById("modalNovoPaciente"),
    formNovo: document.getElementById("formNovoPaciente"),
    nomeInput: document.getElementById("novoPacienteNome"),
    telefoneInput: document.getElementById("novoPacienteTelefone")
};

// ============================================================
// 2. RENDERIZAÇÃO
// ============================================================
function renderizarPacientes(filtro = "") {
    if (!elPacientes.grid) return;
    elPacientes.grid.innerHTML = "";

    const pacientes = statePacientes.lista;
    const pacientesFiltrados = pacientes.filter(p => p.nome.toLowerCase().includes(filtro.toLowerCase()));

    if (pacientesFiltrados.length === 0) {
        elPacientes.grid.innerHTML = `<div class="card-vazio-msg" style="grid-column: 1 / -1; text-align: center; color: #64748b; padding: 3rem; font-style: italic;">Nenhum paciente encontrado.</div>`;
        return;
    }

    pacientesFiltrados.forEach(p => {
        const card = document.createElement("div");
        card.className = "card-consulta";
        card.innerHTML = `
            <div class="card-info">
                <h3 class="card-paciente-nome" style="font-size: 1.3rem; margin-bottom: 5px; color: var(--brand); font-weight: 800;">${p.nome}</h3>
                <p class="card-especialista" style="color: var(--texto-muted); font-weight: 600;">📞 ${p.telefone}</p>
                <p class="card-especialista" style="font-size: 0.8rem; margin-top: 5px; color: var(--texto-muted);">Cadastrado em: ${p.dataCadastro}</p>
            </div>
            <div class="card-status status-verde" style="margin-top: 15px;">● Ativo</div>
            <div class="card-acoes" style="margin-top: 15px;">
                <button class="btn-card-inline" onclick="abrirProntuarioDoPaciente('${p.id}')">
                    Ver Prontuário
                </button>
            </div>
        `;
        elPacientes.grid.appendChild(card);
    });
}

// ============================================================
// 3. AÇÕES
// ============================================================
window.salvarNovoPaciente = (e) => {
    e.preventDefault();
    
    const nome = elPacientes.nomeInput.value.trim();
    const telefone = elPacientes.telefoneInput.value.trim();
    
    if(!nome || !telefone) return;

    const novoPaciente = {
        id: Date.now().toString(),
        nome: nome,
        telefone: telefone,
        dataCadastro: new Date().toLocaleDateString('pt-br'),
        evolucao: ""
    };

    const lista = statePacientes.lista;
    lista.push(novoPaciente);
    statePacientes.salvar(lista);

    elPacientes.formNovo.reset();
    elPacientes.modalNovo.close();
    renderizarPacientes(elPacientes.inputBusca ? elPacientes.inputBusca.value : "");
    
    alert("Paciente cadastrado com sucesso!");
};

window.abrirProntuarioDoPaciente = (id) => {
    const paciente = statePacientes.lista.find(p => p.id === id);
    if(paciente) {
        const modal = document.getElementById("modalProntuario");
        document.getElementById("nomePacienteProntuario").innerText = paciente.nome;
        const textarea = document.getElementById("textoProntuario");
        textarea.value = paciente.evolucao || "";
        
        document.getElementById("btnSalvarProntuario").onclick = () => {
            paciente.evolucao = textarea.value;
            const lista = statePacientes.lista;
            const idx = lista.findIndex(p => p.id === id);
            if(idx !== -1) {
                lista[idx] = paciente;
                statePacientes.salvar(lista);
                
                const btn = document.getElementById("btnSalvarProntuario");
                btn.innerText = "⌛ Salvando...";
                setTimeout(() => {
                    alert("Prontuário salvo com sucesso!");
                    btn.innerText = "Salvar Evolução";
                    modal.close();
                }, 500);
            }
        };
        
        modal.showModal();
    }
};

// ============================================================
// 4. LISTENERS E INICIALIZAÇÃO
// ============================================================
if (elPacientes.inputBusca) {
    elPacientes.inputBusca.addEventListener("input", (e) => {
        renderizarPacientes(e.target.value);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    renderizarPacientes();
});