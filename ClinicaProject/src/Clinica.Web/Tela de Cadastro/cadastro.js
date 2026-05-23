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
        erroDialog: document.getElementById("modalErro"),
        campoObrigatorio: document.getElementById("meuFormularioCadastro"),
        crefitoReq: document.getElementById("crefitoReq")
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

    // --- 1.5 MÁSCARA CREFITO ---
    const aplicarMascaraCrefito = (valor) => {
        let v = valor.toUpperCase();
        let numeros = v.replace(/\D/g, "").substring(0, 6);
        let letras = v.replace(/[^A-Z]/g, "").substring(0, 2);
        
        if (numeros.length === 6) {
            if (letras.length > 0) {
                return numeros + "-" + letras;
            } else if (valor.endsWith("-")) {
                return numeros + "-";
            }
            return numeros;
        }
        return numeros;
    };

    if (el.crefitoReq) {
        el.crefitoReq.addEventListener("input", (e) => {
            e.target.value = aplicarMascaraCrefito(e.target.value);
            el.crefitoReq.classList.remove("input-erro");
        });
    }

    // Remove o erro visual dos outros campos assim que o usuário digita algo
    [el.inputNomeCadastro, el.inputEmailCadastro, el.inputSenhaCadastro, el.crefitoReq].forEach(input => {
        if (input) {
            input.addEventListener("input", () => {
                input.classList.remove("input-erro");
                const msgAntiga = input.parentNode.querySelector(".mensagem-erro-texto");
                if (msgAntiga) msgAntiga.remove();
            });
        }
    });

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
        const regexEmail = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
        
        const dados = {
            nomeCadastro: el.inputNomeCadastro ? el.inputNomeCadastro.value.trim() : "",
            cpf: el.inputCPFCadastro ? el.inputCPFCadastro.value.replace(/\D/g, "") : "",
            emailCadastro: el.inputEmailCadastro ? el.inputEmailCadastro.value.trim() : "",
            senhaCadastro: el.inputSenhaCadastro ? el.inputSenhaCadastro.value.trim() : "",
            crefitoReq: el.crefitoReq ? el.crefitoReq.value.trim() : "",
        };

        // CORREÇÃO 1: Incluído o el.crefitoReq na lista de limpeza para apagar erros antigos
        [el.inputNomeCadastro, el.inputCPFCadastro, el.inputEmailCadastro, el.inputSenhaCadastro, el.crefitoReq].forEach(input => {
            if (input) {
                input.classList.remove("input-erro");
                const msgAntiga = input.parentNode.querySelector(".mensagem-erro-texto");
                if (msgAntiga) msgAntiga.remove();
            }
        });

        let formInvalido = false;
        let primeiroCampoErro = null;

        // 2. FUNÇÃO AUXILIAR: Adiciona a borda e insere o texto dinamicamente abaixo do campo
        const marcarErro = (campo, mensagem) => {
            if (campo) {
                campo.classList.add("input-erro");

                // Cria o elemento de texto para o erro
                const textoErro = document.createElement("span");
                textoErro.className = "mensagem-erro-texto";
                textoErro.innerText = mensagem;

                // Insere o texto logo após o campo de input correspondente
                campo.parentNode.insertBefore(textoErro, campo.nextSibling);

                if (!primeiroCampoErro) primeiroCampoErro = campo;
                formInvalido = true;
            }
        };

        // --- VALIDAÇÕES INDIVIDUAIS COM MENSAGENS PERSONALIZADAS ---
        
        // Valida Nome
        if (!dados.nomeCadastro) {
            marcarErro(el.inputNomeCadastro, "O campo Nome é obrigatório.");
        }
        
        // Valida CPF
        if (!dados.cpf) {
            marcarErro(el.inputCPFCadastro, "O campo CPF é obrigatório.");
        } else if (dados.cpf.length !== 11 || !validarCPF(dados.cpf)) {
            marcarErro(el.inputCPFCadastro, "Por favor, insira um CPF válido.");
        }

        // CORREÇÃO 2: Validação de 6 números, hífen e 2 letras (9 caracteres no total)
        if (!dados.crefitoReq) {
            marcarErro(el.crefitoReq, "O campo CREFITO é obrigatório.");
        } else if (!/^\d{6}-[A-Z]{2}$/.test(dados.crefitoReq)) {
            marcarErro(el.crefitoReq, "Por favor, insira um CREFITO válido (ex: 123456-TO).");
        }
        
        // Valida E-mail
        if (!dados.emailCadastro) {
            marcarErro(el.inputEmailCadastro, "O campo E-mail é obrigatório.");
        } else if (!regexEmail.test(dados.emailCadastro)) {
            marcarErro(el.inputEmailCadastro, "Insira um formato de e-mail válido (ex: nome@email.com).");
        }
        
        // Valida Senha
        if (!dados.senhaCadastro) {
            marcarErro(el.inputSenhaCadastro, "O campo Senha é obrigatório.");
        } else if (dados.senhaCadastro.length < 8) {
            marcarErro(el.inputSenhaCadastro, "A senha deve conter no mínimo 8 caracteres.");
        }

        // Se houver erro, para a execução, abre o modal geral e foca no primeiro campo
        if (formInvalido) {
            if (el.erroDialog) el.erroDialog.showModal();
            if (primeiroCampoErro) primeiroCampoErro.focus();
            return;
        }

        // Fluxo de Sucesso
        if (el.sucessoModal){
            el.sucessoModal.showModal();
        }

        setTimeout(() => {
            window.location.href = "/ClinicaProject/src/Clinica.Web/tela de login/login.html";
        }, 3000);
    };

    // --- 4. EVENTOS ---
    if (el.btnCadastrar) {
        el.btnCadastrar.onclick = async (e) => {
            e.preventDefault();
            enviarCadastro();
        };
    }

    if (el.btnRedirecionar) {
        el.btnRedirecionar.addEventListener("click", () => {
            window.location.href = "/ClinicaProject/src/Clinica.Web/tela de login/login.html";
        });
    }
};

function selecionarPerfil(tipo) {
    const card = tipo === 'paciente' ? 
        document.getElementById('cardPaciente') : 
        document.getElementById('cardProfissional');

    if (card) card.style.transform = "scale(0.95)";

    setTimeout(() => {
        if (tipo === 'paciente') {
            window.location.href = "cadastro-paciente.html";
        } else {
            window.location.href = "cadastro-profissional.html";
        }
    }, 150);
}
