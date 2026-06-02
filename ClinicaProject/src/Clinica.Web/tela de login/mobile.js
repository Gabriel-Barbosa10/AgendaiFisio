/**
 * mobile.js - Controle de Interatividade Responsiva para AgendaiFisio
 */
document.addEventListener("DOMContentLoaded", () => {
    inicializarMenuHamburguer();
    inicializarDropdownsMobile();
});

/**
 * Inicializa o menu hambúrguer para as telas que possuem sidebar (Tela do Terapeuta)
 */
function inicializarMenuHamburguer() {
    const sidebar = document.querySelector(".sidebar");
    const container = document.querySelector(".app-container");
    
    if (!sidebar || !container) return; // Só executa em páginas com estrutura de sidebar

    // 1. Cria a barra superior móvel (mobile topbar) se não existir
    let topbar = document.querySelector(".mobile-topbar");
    if (!topbar) {
        topbar = document.createElement("div");
        topbar = document.createElement("div");
        topbar.className = "mobile-topbar";
        topbar.innerHTML = `
            <button class="hamburger-btn" aria-label="Abrir menu">☰</button>
            <div class="mobile-logo">🌿 AgendaiFisio</div>
            <div style="width: 40px;"></div> <!-- Espaçador para centralizar o título -->
        `;
        container.parentNode.insertBefore(topbar, container);
    }

    // 2. Cria o backdrop (fundo escurecido) para fechar o menu ao clicar fora
    let backdrop = document.querySelector(".sidebar-backdrop");
    if (!backdrop) {
        backdrop = document.createElement("div");
        backdrop.className = "sidebar-backdrop";
        document.body.appendChild(backdrop);
    }

    // 3. Seleciona o botão de hambúrguer recém-criado
    const menuBtn = topbar.querySelector(".hamburger-btn");

    // 4. Adiciona eventos para abrir/fechar a sidebar
    const toggleSidebar = () => {
        sidebar.classList.toggle("open");
        backdrop.classList.toggle("active");
        document.body.classList.toggle("sidebar-open");
    };

    const closeSidebar = () => {
        sidebar.classList.remove("open");
        backdrop.classList.remove("active");
        document.body.classList.remove("sidebar-open");
    };

    menuBtn.addEventListener("click", toggleSidebar);
    backdrop.addEventListener("click", closeSidebar);

    // Fecha a sidebar ao clicar em qualquer link da navegação
    const navLinks = sidebar.querySelectorAll("nav a");
    navLinks.forEach(link => {
        link.addEventListener("click", closeSidebar);
    });
}

/**
 * Ajusta o comportamento de menus de perfil em telas pequenas
 */
function inicializarDropdownsMobile() {
    const perfilMenu = document.getElementById("perfilMenu");
    if (!perfilMenu) return;

    // No mobile, o comportamento de hover pode ser ruim. Vamos adicionar clique.
    perfilMenu.addEventListener("click", (e) => {
        if (window.innerWidth <= 768) {
            e.stopPropagation();
            perfilMenu.classList.toggle("mobile-dropdown-active");
            
            const dropdown = perfilMenu.querySelector(".dropdown-conta");
            if (dropdown) {
                dropdown.style.display = perfilMenu.classList.contains("mobile-dropdown-active") ? "block" : "none";
            }
        }
    });

    // Fechar ao clicar fora
    document.addEventListener("click", () => {
        if (perfilMenu.classList.contains("mobile-dropdown-active")) {
            perfilMenu.classList.remove("mobile-dropdown-active");
            const dropdown = perfilMenu.querySelector(".dropdown-conta");
            if (dropdown) dropdown.style.display = "none";
        }
    });
}
