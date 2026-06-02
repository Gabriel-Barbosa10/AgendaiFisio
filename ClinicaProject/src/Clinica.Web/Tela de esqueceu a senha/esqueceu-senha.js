window.onload = function() {
    const btnRecuperar = document.getElementById("btnRecuperar");
    const inputEmail = document.getElementById("emailRecuperar");
    const URL_API = "http://localhost:8000/password-recovery"; // URL da sua API

    if (btnRecuperar) {
        btnRecuperar.onclick = async function() {
            const email = inputEmail.value.trim();

            // Validação simples de e-mail
            if (!email || !email.includes("@")) {
                alert("⚠️ Por favor, insira um e-mail válido.");
                inputEmail.classList.add("input-erro");
                inputEmail.style.borderColor = "red";
                return;
            }

            try {
                const response = await fetch(URL_API, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email: email })
                });

                if (response.ok) {
                    alert("✅ Se este e-mail estiver cadastrado, você receberá um link em breve!");
                    window.location.href = "./login.html";
                } else {
                    const errorData = await response.json();
                    alert(errorData.detail || "Ocorreu um erro ao processar a solicitação.");
                }
            } catch (err) {
                console.error("Erro de conexão:", err);
                alert("Erro ao conectar com o servidor.");
            }
        };
    }

    // Remove erro ao digitar
    inputEmail.oninput = () => {
        inputEmail.classList.remove("input-erro");
        inputEmail.style.borderColor = "#ddd";
    };
};
