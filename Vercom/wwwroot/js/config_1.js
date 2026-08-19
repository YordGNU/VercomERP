(() => {
    const STORAGE_KEY = "__THEME_CONFIG__";
    const FULLSCREEN_KEY = "__FULLSCREEN__";

    var e = document.documentElement,
        i = sessionStorage.getItem(STORAGE_KEY),
        t = {
            dir: "ltr",
            skin: "default",
            theme: "light",
            width: "fluid",
            position: "fixed",
            orientation: "vertical",
            "sidenav-size": "default",
            "sidenav-user": true,
            "topbar-color": "light",
            "sidenav-color": "dark",
            monochrome: false
        };

    function a() {
        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    // ✅ Corrección: usar t.skin en vez de t completo
    t = {
        dir: e.getAttribute("dir") || t.dir,
        skin: e.getAttribute("data-skin") || t.skin,
        theme: e.getAttribute("data-bs-theme") === "system"
            ? a()
            : e.getAttribute("data-bs-theme") || (t.theme === "system" ? a() : t.theme),
        "topbar-color": e.getAttribute("data-topbar-color") || t["topbar-color"],
        "sidenav-color": e.getAttribute("data-menu-color") || t["sidenav-color"],
        "sidenav-size": e.getAttribute("data-sidenav-size") || t["sidenav-size"],
        "sidenav-user": e.hasAttribute("data-sidenav-user") || t["sidenav-user"],
        position: e.getAttribute("data-layout-position") || t.position,
        width: e.getAttribute("data-layout-width") || t.width,
        monochrome: e.classList.contains("monochromea") || t.monochrome || false
    };

    // Guardar configuración inicial
    window.defaultConfig = structuredClone(t);
    i = i ? JSON.parse(i) : t;
    window.config = i;

    // Aplicar atributos al <html>
    e.setAttribute("dir", i.dir);
    e.setAttribute("data-skin", i.skin);
    e.setAttribute("data-bs-theme", i.theme);
    e.setAttribute("data-topbar-color", i["topbar-color"]);
    e.setAttribute("data-menu-color", i["sidenav-color"]);
    e.setAttribute("data-layout-position", i.position);
    e.setAttribute("data-layout-width", i.width);
    e.classList.toggle("monochromea", i.monochrome);

    if (i["sidenav-user"] === true) {
        e.setAttribute("data-sidenav-user", "true");
    } else {
        e.removeAttribute("data-sidenav-user");
    }

    if (i["sidenav-size"]) {
        let size = i["sidenav-size"];
        if (window.innerWidth <= 1140) size = "offcanvas";
        e.setAttribute("data-sidenav-size", size);
    }

    // ✅ Enganchar botones topbar
    document.addEventListener("DOMContentLoaded", () => {
        // Modo oscuro
        document.getElementById("light-dark-mode")?.addEventListener("click", () => {
            const currentTheme = e.getAttribute("data-bs-theme") || "light";
            const newTheme = currentTheme === "dark" ? "light" : "dark";
            i.theme = newTheme;
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(i));
            e.setAttribute("data-bs-theme", newTheme);
            e.setAttribute("data-theme", newTheme);
        });

        // Fullscreen
        document.querySelector("#fullscreen-toggler button")?.addEventListener("click", () => {
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen();
                sessionStorage.setItem(FULLSCREEN_KEY, "true");
            } else {
                document.exitFullscreen();
                sessionStorage.setItem(FULLSCREEN_KEY, "false");
            }
        });

        // Monocromo
        document.getElementById("monochrome-mode")?.addEventListener("click", () => {
            i.monochrome = !i.monochrome;
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(i));
            e.classList.toggle("monochromea", i.monochrome);
        });
    });

    // ✅ Al cargar, sincronizar fullscreen iconos
    if (sessionStorage.getItem(FULLSCREEN_KEY) === "true") {
        const maximizeIcon = document.querySelector("#fullscreen-toggler .ti-maximize");
        const minimizeIcon = document.querySelector("#fullscreen-toggler .ti-minimize");
        maximizeIcon?.classList.add("d-none");
        minimizeIcon?.classList.remove("d-none");
    }
})();
