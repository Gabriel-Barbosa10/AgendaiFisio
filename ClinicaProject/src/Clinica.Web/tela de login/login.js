// INTERCEPTA O ALERT TRADICIONAL E CONVERTE EM POPUP MODERNO
window.alert = function (mensagemErros) {
    // Agora o Swal sempre estará disponível instantaneamente
    Swal.fire({
        text: mensagemErros,
        icon: 'error',
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#C8DAA0', 
        background: '#ffffff',
        border: 'none',
        customClass: {
            popup: 'arredondar-popup',
        }
    });
};

window.onload = function() {
    const btnEntrar = document.getElementById("btnEntrar");
    const inputCPF = document.getElementById("cpfLogin");
    const inputSenha = document.getElementById("senha");

    // --- MÁSCARA DE CPF ---
    if (inputCPF) {
        inputCPF.addEventListener("input", function(e) {
            let v = e.target.value.replace(/\D/g, "");
            if (v.length > 11) v = v.slice(0, 11);
            v = v.replace(/(\d{3})(\d)/, "$1.$2");
            v = v.replace(/(\d{3})(\d)/, "$1.$2");
            v = v.replace(/(\d{3})(\d{1,2})$/, "$1-$2");
            e.target.value = v;
        });
    }


    // --- LÓGICA DE LOGIN ---
    if (btnEntrar) {
        btnEntrar.addEventListener("click", function(e) {
            e.preventDefault(); 
            e.stopPropagation();

            const cpfLimpio = inputCPF.value.replace(/\D/g, "");
            const senhaDigitada = inputSenha.value.trim();
            
            if (cpfLimpio.length < 11 || senhaDigitada < 8 ) {
                alert("Por favor, preencha o CPF e a senha corretamente.");
                return;
            }

            // SIMULAÇÃO DE SUCESSO
            const tokenSimulado = "jwt_" + Math.random().toString(36).substr(2);
            localStorage.setItem('token_acesso', tokenSimulado);
            localStorage.setItem('usuario_nome', "Arnaldo Candido");

            btnEntrar.innerText = "Acessando...";
            btnEntrar.style.opacity = "0.7";
            btnEntrar.disabled = true;

            setTimeout(() => {
                window.location.href = "../Tela de Agendamento/index.html";
            }, 500);
        });
    }

    // Atalho Enter
    [inputCPF, inputSenha].forEach(input => {
        input.addEventListener("keypress", (e) => {
            if (e.key === "Enter") {
                e.preventDefault();
                btnEntrar.click();
            }
        });
    });
};
