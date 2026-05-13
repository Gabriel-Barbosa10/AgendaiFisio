// --- 1. CONFIGURAÇÕES INICIAIS E MAPEAMENTO ---
const elements = {
    btnAgendarPrincipal: document.getElementById('btnAgendar'),
    selectFisio: document.getElementById('selectFisio'),
    toastErro: document.getElementById('toastErro'),
    
    // Modais
    modalAgenda: document.getElementById('modalAgenda'),
    modalTriagem: document.getElementById('modalTriagem'),
    modalSucesso: document.getElementById('modalSucesso'),
    modalNavegacao: document.getElementById('modalNavegacao'),
    
    // Elementos Internos
    modalEspecialista: document.getElementById('modalEspecialista'),
    modalData: document.getElementById('modalData'),
    gridHorarios: document.getElementById('gridHorarios'),
    btnConfirmarAgendamento: document.getElementById('btnConfirmarAgendamento'),
    nomePaciente: document.getElementById('nomePaciente'),
    
    // Alertas de Erro
    erroModal: document.getElementById('erroModal'),
    textoErroModal: document.getElementById('textoErroModal'),
    erroTriagem: document.getElementById('erroTriagem'),
    
    // Inputs e Resumo
    btnFinalizarTriagem: document.getElementById('btnFinalizarTriagem'),
    resumoMedico: document.getElementById('resumoMedico'),
    resumoData: document.getElementById('resumoData'),
    resumoHora: document.getElementById('resumoHora'),
    btnProximoPasso: document.getElementById('btnProximoPasso'),
    tiposSintomas: document.getElementById('tiposSintomas')
};

let horaSelecionada = null;
const gradeBase = ["10:00", "11:00", "12:00", "13:00","14:00", "15:00","16:00","17:00", "18:00"];

// Utilitário para pegar data ISO local corrigida
const getHojeISO = () => {
    const d = new Date();
    return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().split('T')[0];
};

// --- 2. FUNÇÕES DE LIMPEZA (UX IMPROVEMENT) ---
const resetarErros = () => {
    elements.erroModal.style.display = "none";
    elements.erroTriagem.style.display = "none";
};

// --- 3. LOGICA DE ABERTURA ---
elements.btnAgendarPrincipal.onclick = () => {
    if (!elements.selectFisio.value) {
        elements.toastErro.classList.add("show");
        setTimeout(() => elements.toastErro.classList.remove("show"), 3000);
        return;
    }
    
    resetarErros();
    elements.modalEspecialista.value = elements.selectFisio.value;
    elements.modalData.setAttribute("min", getHojeISO());
    elements.modalData.value = "";
    elements.gridHorarios.innerHTML = "<p style='grid-column:1/-1; color:gray; text-align:center;'>Escolha uma data primeiro...</p>";
    
    document.body.style.overflow = 'hidden'; // Evita scroll ao fundo
    elements.modalAgenda.showModal();
};

// --- 4. RENDERIZAÇÃO DE HORÁRIOS ---
elements.modalData.onchange = () => {
    const dataSel = elements.modalData.value;
    if (!dataSel) return;
    
    resetarErros();
    const agora = new Date();
    const hojeStr = getHojeISO();
    
    // Recupera horários já ocupados
    const agendaGlobal = JSON.parse(localStorage.getItem('agendaFisioData')) || {};
    const dadosDaData = agendaGlobal[dataSel] || {};

    elements.gridHorarios.innerHTML = "";
    horaSelecionada = null;
    
    gradeBase.forEach(hora => {
        const isOcupado = dadosDaData[hora];
        let isPassado = false;

        if (dataSel === hojeStr) {
            const [h, m] = hora.split(':');
            const dataHoraBotao = new Date();
            dataHoraBotao.setHours(parseInt(h), parseInt(m), 0, 0);
            if (dataHoraBotao <= agora) isPassado = true;
        }

        const btn = document.createElement("button");
        btn.type = "button";
        btn.className = "btn-hora";
        

        //BLOQUEIA HORARIO JÁ SELECIONADO//
        if (isPassado) {
            btn.classList.add("encerrado"); 
            btn.disabled = true;
            btn.innerText = `Encerrado ${hora}`;
        } else if (isOcupado) {
            btn.classList.add("ocupado"); 
            btn.disabled = true;
            btn.innerText = `🚫 ${hora}`;
        } else {
            btn.classList.add("disponivel");
            btn.innerText = hora;
            btn.onclick = () => {
                document.querySelectorAll('.btn-hora').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                horaSelecionada = hora;
                elements.erroModal.style.display = "none";
            };
        }
        elements.gridHorarios.appendChild(btn);
    });
};

// --- 5. FLUXO DE CONFIRMAÇÃO ---

// PASSO A: Validação Data/Hora -> Abre Triagem
elements.btnConfirmarAgendamento.onclick = () => {
    if (!elements.modalData.value || !horaSelecionada) {
        elements.textoErroModal.innerText = "⚠️ Selecione a data e o horário.";
        elements.erroModal.style.display = "block";
        return;
    }
    elements.modalAgenda.close();
    elements.modalTriagem.showModal();
};

// PASSO B: Validação Triagem -> Salva e Sucesso
elements.btnFinalizarTriagem.onclick = () => {


    const dataRaw = elements.modalData.value;
    const dataFormatada = dataRaw.split("-").reverse().join("/");

    const novaConsulta = {
        id: Date.now(),
        nome: "Davi Gusmão",
        especialista: elements.modalEspecialista.value,
        data: dataFormatada,
        dataISO: dataRaw,
        hora: horaSelecionada,
        status: "Confirmado"
    };

    // 1. Salva na lista de consultas (Tela do Paciente e Profissional)
    const consultas = JSON.parse(localStorage.getItem("consultas_fisio")) || [];
    consultas.push(novaConsulta);
    localStorage.setItem("consultas_fisio", JSON.stringify(consultas));

    // 2. Ocupa o horário na Agenda Geral
    const agendaGlobal = JSON.parse(localStorage.getItem('agendaFisioData')) || {};
    if (!agendaGlobal[dataRaw]) agendaGlobal[dataRaw] = {};
    agendaGlobal[dataRaw][horaSelecionada] = true;
    localStorage.setItem('agendaFisioData', JSON.stringify(agendaGlobal));

    // 3. Atualiza Resumo e Transição
    elements.resumoMedico.innerText = novaConsulta.especialista;
    elements.resumoData.innerText = novaConsulta.data;
    elements.resumoHora.innerText = novaConsulta.hora;

    elements.modalTriagem.close();
    elements.modalSucesso.showModal();
};

// PASSO C: Navegação Pós-Sucesso
elements.btnProximoPasso.onclick = () => {
    elements.modalSucesso.close();
    
    // Pequeno delay para garantir que o navegador processe a fechada do modal anterior
    setTimeout(() => {
        if (elements.modalNavegacao) {
            elements.modalNavegacao.showModal();
        }
    }, 150);
};

// Monitorar fechamento global para restaurar o scroll do body
const fecharDialog = (modal) => {
    if(!modal) return;
    modal.addEventListener('close', () => {
        const algumAberto = elements.modalAgenda.open || 
                           elements.modalTriagem.open || 
                           elements.modalSucesso.open || 
                           (elements.modalNavegacao && elements.modalNavegacao.open);
        
        if (!algumAberto) document.body.style.overflow = 'auto';
    });
};

[elements.modalAgenda, elements.modalTriagem, elements.modalSucesso, elements.modalNavegacao].forEach(fecharDialog);