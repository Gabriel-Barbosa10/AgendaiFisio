window.onload = function() {
    const el = {
        btnCadastrar: document.getElementById("btnCadastrar"),
        inputNomeCadastro: document.getElementById("nomeCadastro"),
        inputCPFCadastro: document.getElementById("cpfCadastro"),
        inputEmailCadastro: document.getElementById("emailCadastro"),
        inputSenhaCadastro: document.getElementById("senhaCadastro"),
        checkLGPD: document.getElementById("checkLGPD"), // Novo elemento
        URL_API: "http://localhost:8000/register",
        sucessoModal: document.getElementById("modalSucessoCadastro"),
        btnRedirecionar: document.getElementById("btnRedirecionar"),
        dialogTerapeuta: document.getElementById("btnCadastrar"),
        erroDialog: document.getElementById("modalErro")
    };

    // --- 1. MÁSCARA DE CPF (Lógica Funcional) ---
    const aplicarMascaraCPF = (valor) => {
        return valor
            .replace(/\D/g, "") // Remove tudo que não é número
            .replace(/(\d{3})(\d)/, "$1.$2") // Coloca ponto após o 3º dígito
            .replace(/(\d{3})(\d)/, "$1.$2") // Coloca ponto após o 6º dígito
            .replace(/(\d{3})(\d{1,2})$/, "$1-$2") // Coloca traço após o 9º dígito
            .substring(0, 14); // Limita o tamanho total
    };

    if (el.inputCPFCadastro) {
        el.inputCPFCadastro.addEventListener("input", (e) => {
            e.target.value = aplicarMascaraCPF(e.target.value);
            el.inputCPFCadastro.classList.remove("input-erro");
        });
    }
            function validarCPF(cpf) {
            cpf = cpf.replace(/[^\d]+/g, ''); // Remove pontos e traços
            if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false; // Verifica tamanho e repetidos
            
            // Lógica matemática de validação
            let soma = 0;
            let resto;
            for (let i = 1; i <= 9; i++) soma = soma + parseInt(cpf.substring(i - 1, i)) * (11 - i);
            resto = (soma * 10) % 11;
            if ((resto === 10) || (resto === 11)) resto = 0;
            if (resto !== parseInt(cpf.substring(9, 10))) return false;
            
            soma = 0;
            for (let i = 1; i <= 10; i++) soma = soma + parseInt(cpf.substring(i - 1, i)) * (12 - i);
            resto = (soma * 10) % 11;
            if ((resto === 10) || (resto === 11)) resto = 0;
            if (resto !== parseInt(cpf.substring(10, 11))) return false;
            
            return true;
        }

        function checkForm() {
            let cpf = document.getElementById('cpf').value;
            if (!validarCPF(cpf)) {
                alert('CPF Inválido!');
                return false;
            }
            alert('CPF Válido!');
            return true;
        }


    // --- 2. LÓGICA LGPD (Habilitar Botão) ---
    if (el.checkLGPD && el.btnCadastrar) {
        el.checkLGPD.addEventListener("change", () => {
            el.btnCadastrar.disabled = !el.checkLGPD.checked;
        });
    }



    // --- 3. ENVIO DO FORMULÁRIO ---
    const enviarCadastro = async () => {
        const regexEmail =  /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
        const dados = {
            nomeCadastro: el.inputNomeCadastro.value.trim(),
            cpf: el.inputCPFCadastro.value.replace(/\D/g, ""), // Limpa para a API
            emailCadastro: el.inputEmailCadastro.value.trim(),
            senhaCadastro: el.inputSenhaCadastro.value.trim()

        };



        // Validações 
        if (!dados.nomeCadastro || dados.cpf.length !== 11 || !dados.emailCadastro.includes("@") || dados.senhaCadastro.length < 8 ) {
            alert("⚠️ Preencha todos os campos corretamente!");
            el.erroDialog.showModal();
            return;
        }
        if (el.sucessoModal){
            el.sucessoModal.showModal();
        }

        setTimeout(() => {
            window.location.href = "/ClinicaProject/src/Clinica.Web/tela de login/login.html"
        }, 3000);

 

    };
    
    ///4. EVENTOS///

if (el.btnCadastrar) {
        // Usamos async aqui para garantir que ele espere a função terminar se houver uma API
        el.btnCadastrar.onclick = async (e) => {
            e.preventDefault();
            // Apenas chama a função. Toda a lógica de validar, 
            // mostrar o modal e redirecionar está dentro dela.
            enviarCadastro();
        };
    }

    if (el.btnRedirecionar) {
        el.btnRedirecionar.addEventListener("click", () => {
            // Caso o usuário clique no botão dentro do modal para ir mais rápido
            window.location.href = "/ClinicaProject/src/Clinica.Web/tela de login/login.html";
        });
    }
};

function selecionarPerfil(tipo) {
    // Adiciona uma pequena animação de clique antes de redirecionar
    const card = tipo === 'paciente' ? 
        document.getElementById('cardPaciente') : 
        document.getElementById('cardProfissional');

    card.style.transform = "scale(0.95)";

    setTimeout(() => {
        if (tipo === 'paciente') {
            // Redireciona para a tela de cadastro de paciente
            window.location.href = "cadastro-paciente.html";
        } else {
            // Redireciona para a tela de cadastro de profissional
            window.location.href = "cadastro-profissional.html";
        }
    }, 150);
}
