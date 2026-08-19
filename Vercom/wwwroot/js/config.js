; (function () {
    const STORAGE_KEY = '__THEME_CONFIG__'
    const FULLSCREEN_KEY = '__FULLSCREEN__'

    const BASE_DEFAULTS = {
        skin: 'default',
        theme: 'light',
        orientation: 'vertical',
        sidenavSize: 'default',
        sidenavColor: 'dark',
        sidenavUser: true,
        topbarColor: 'light',
        width: 'fluid',
        position: 'fixed',
        dir: 'ltr',
        monochrome: false
    }

    const SKIN_PRESETS = {
        default: { theme: 'light', sidenavColor: 'dark', topbarColor: 'light', sidenavUser: true },
        minimal: { theme: 'light', sidenavColor: 'gray', topbarColor: 'light', sidenavUser: false },
        material: { theme: 'light', topbarColor: 'dark', sidenavColor: 'light', sidenavUser: true },
        saas: { theme: 'light', topbarColor: 'light', sidenavColor: 'dark', sidenavUser: true },
        modern: { theme: 'light', topbarColor: 'light', sidenavColor: 'gradient', sidenavUser: true },
        flat: { theme: 'light', sidenavColor: 'dark', topbarColor: 'light', sidenavUser: false },
        galaxy: { theme: 'dark', topbarColor: 'dark', sidenavColor: 'light', sidenavUser: true },
        luxe: { theme: 'light', topbarColor: 'dark', sidenavColor: 'light', sidenavUser: true },
        retro: { theme: 'light', sidenavColor: 'gradient', topbarColor: 'light', sidenavUser: true },
        neon: { theme: 'light', sidenavColor: 'gray', topbarColor: 'gray', sidenavUser: false },
        pixel: { theme: 'light', sidenavColor: 'gradient', topbarColor: 'light', sidenavUser: true },
    }

    const ATTR_MAP = {
        theme: 'data-theme',
        skin: 'data-skin',
        orientation: 'data-layout',
        sidenavColor: 'data-menu-color',
        sidenavUser: 'data-sidenav-user',
        sidenavSize: 'data-sidenav-size',
        topbarColor: 'data-topbar-color',
        width: 'data-layout-width',
        position: 'data-layout-position',
        dir: 'dir',
    }

    function toKebabCase(str) {
        return str.replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase()
    }

    function readStorage() {
        try {
            const raw = sessionStorage.getItem(STORAGE_KEY)
            if (!raw || raw === 'undefined' || raw === 'null') return { ...BASE_DEFAULTS }
            return Object.assign({}, BASE_DEFAULTS, JSON.parse(raw))
        } catch {
            return { ...BASE_DEFAULTS }
        }
    }

    function writeStorage(state) {
        try {
            const clean = {}
            Object.keys(BASE_DEFAULTS).forEach((key) => {
                if (key in state) clean[key] = state[key]
            })
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(clean))
        } catch (e) {
            console.warn('[config.js] sessionStorage write failed:', e)
        }
    }

    function applyToDOM(state) {
        const html = document.documentElement
        Object.keys(state).forEach((key) => {
            if (key === 'skin') return
            const value = state[key]
            if (value === undefined || value === null) return
            const attrName = ATTR_MAP[key] || ('data-' + toKebabCase(key))
            html.setAttribute(attrName, String(value))
            if (key === 'theme') {
                html.setAttribute('data-bs-theme', String(value))
            }
            if (key === 'monochrome') {
                html.classList.toggle('monochromea', !!value)
            }
        })
    }

    function applySkin(skinName, queryPatch) {
        if (!skinName || typeof skinName !== 'string') return
        const preset = SKIN_PRESETS[skinName]
        if (!preset) return
        const current = readStorage()
        const next = Object.assign({}, current, preset, { skin: skinName }, queryPatch || {})
        // ✅ Respetar theme guardado si no viene en queryPatch
        if (!('theme' in queryPatch)) next.theme = current.theme
        // ✅ Respetar monocromo guardado
        next.monochrome = current.monochrome
        writeStorage(next)
        applyToDOM(next)
    }


    function initObserver() {
        const html = document.documentElement
        const queryPatch = getQueryPatch()
        let lastAppliedSkin = null

        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'attributes' && mutation.attributeName === 'data-skin') {
                    const skin = html.getAttribute('data-skin')
                    if (skin && skin !== lastAppliedSkin) {
                        lastAppliedSkin = skin
                        applySkin(skin, queryPatch)
                    }
                }
            })
        })
        observer.observe(html, { attributes: true, attributeFilter: ['data-skin'] })

        const bootSkin = queryPatch.skin || html.getAttribute('data-skin')
        if (bootSkin) {
            lastAppliedSkin = bootSkin
            if (bootSkin !== html.getAttribute('data-skin')) {
                html.setAttribute('data-skin', bootSkin)
            }
            applySkin(bootSkin, queryPatch)
        } else {
            const current = readStorage()
            const next = Object.assign({}, current, queryPatch)
            writeStorage(next)
            applyToDOM(next)
        }

        // ✅ Sincronizar fullscreen iconos
        if (sessionStorage.getItem(FULLSCREEN_KEY) === 'true') {
            const maximizeIcon = document.querySelector('#fullscreen-toggler .ti-maximize')
            const minimizeIcon = document.querySelector('#fullscreen-toggler .ti-minimize')
            maximizeIcon?.classList.add('d-none')
            minimizeIcon?.classList.remove('d-none')
        }
    }

    function getQueryPatch() {
        const params = new URLSearchParams(window.location.search)
        const patch = {}
        if (params.has('dark')) patch.theme = 'dark'
        if (params.has('rtl')) patch.dir = 'rtl'
        for (const skin of Object.keys(SKIN_PRESETS)) {
            if (params.has(skin)) { patch.skin = skin; break }
        }
        return patch
    }

    // ✅ Botones topbar
    document.addEventListener('DOMContentLoaded', () => {
        const html = document.documentElement

        // Botón modo oscuro
        document.getElementById('light-dark-mode')?.addEventListener('click', () => {
            const config = readStorage()
            const currentTheme = html.getAttribute('data-bs-theme') || 'light'
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark'
            config.theme = newTheme
            writeStorage(config)
            applyToDOM(config)
        })

        // Botón fullscreen
        document.querySelector('#fullscreen-toggler button')?.addEventListener('click', () => {
            const maximizeIcon = document.querySelector('#fullscreen-toggler .ti-maximize')
            const minimizeIcon = document.querySelector('#fullscreen-toggler .ti-minimize')

            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen()
                sessionStorage.setItem(FULLSCREEN_KEY, 'true')
                maximizeIcon?.classList.add('d-none')
                minimizeIcon?.classList.remove('d-none')
            } else {
                document.exitFullscreen()
                sessionStorage.setItem(FULLSCREEN_KEY, 'false')
                maximizeIcon?.classList.remove('d-none')
                minimizeIcon?.classList.add('d-none')
            }
        })

        // Botón monocromo
        document.getElementById('monochrome-mode')?.addEventListener('click', () => {
            const config = readStorage()
            config.monochrome = !config.monochrome
            writeStorage(config)
            applyToDOM(config)
        })
    })

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initObserver)
    } else {
        initObserver()
    }
})()
