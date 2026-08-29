// Shared API helper. All frontend pages load this before their own page script.
const PRIV = (() => {
    const TOKEN_KEY = "priv_token";
    const USER_KEY = "priv_user";

    function getToken() {
        return localStorage.getItem(TOKEN_KEY);
    }

    function getUser() {
        const raw = localStorage.getItem(USER_KEY);
        return raw ? JSON.parse(raw) : null;
    }

    function setSession(authResponse) {
        localStorage.setItem(TOKEN_KEY, authResponse.token);
        localStorage.setItem(USER_KEY, JSON.stringify({
            id: authResponse.userId,
            username: authResponse.username,
            name: authResponse.name
        }));
    }

    function clearSession() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
    }

    function isLoggedIn() {
        return !!getToken();
    }

    // Redirects to login if not authenticated. Call at the top of any protected page.
    function requireAuth() {
        if (!isLoggedIn()) {
            window.location.href = "/login.html";
        }
    }

    async function request(path, { method = "GET", body = null, auth = true } = {}) {
        const headers = { "Content-Type": "application/json" };
        if (auth) {
            const token = getToken();
            if (token) headers["Authorization"] = `Bearer ${token}`;
        }

        const res = await fetch(`/api${path}`, {
            method,
            headers,
            body: body ? JSON.stringify(body) : null
        });

        if (res.status === 401) {
            clearSession();
            window.location.href = "/login.html";
            return null;
        }

        let data = null;
        const text = await res.text();
        if (text) {
            try { data = JSON.parse(text); } catch { data = text; }
        }

        if (!res.ok) {
            const message = (data && data.message) ? data.message : `Request failed (${res.status})`;
            throw new Error(message);
        }

        return data;
    }

    function logout() {
        clearSession();
        window.location.href = "/index.html";
    }

    function badgeClass(status) {
        return `badge badge-${(status || "none").toLowerCase()}`;
    }

    function formatTime(ts) {
        // ts comes back as "HH:MM:SS" from the API (TimeSpan)
        if (!ts) return "";
        const parts = ts.split(":");
        let h = parseInt(parts[0], 10);
        const m = parts[1];
        const suffix = h >= 12 ? "PM" : "AM";
        h = h % 12; if (h === 0) h = 12;
        return `${h}:${m} ${suffix}`;
    }

    function formatDate(dateStr) {
        if (!dateStr) return "";
        const d = new Date(dateStr + "T00:00:00");
        return d.toLocaleDateString(undefined, { weekday: "short", month: "short", day: "numeric", year: "numeric" });
    }

    function renderNavbar(activeUser) {
        const nav = document.getElementById("priv-navbar");
        if (!nav) return;

        if (activeUser) {
            nav.innerHTML = `
        <div class="brand"><a href="/index.html">PRIV</a></div>
        <nav>
          <a href="/dashboard.html">Dashboard</a>
          <a href="/search.html">Search</a>
          <a href="/requests.html">Requests</a>
          <a href="/confirmed.html">Bookings</a>
          <a href="/settings.html">Settings</a>
          <a href="/u/${encodeURIComponent(activeUser.username)}">My Profile</a>
          <button class="btn-ghost" id="priv-logout-btn">Logout</button>
        </nav>`;
            document.getElementById("priv-logout-btn").addEventListener("click", logout);
        } else {
            nav.innerHTML = `
        <div class="brand"><a href="/index.html">PRIV</a></div>
        <nav>
          <a href="/index.html#how-it-works">How it works</a>
          <a href="/login.html">Login</a>
          <a href="/register.html" class="btn btn-primary">Register</a>
        </nav>`;
        }
    }

    function renderFooter() {
        const footer = document.getElementById("priv-footer");
        if (!footer) return;
        const year = new Date().getFullYear();
        footer.innerHTML = `
      <span>&copy; ${year} PRIV. All rights reserved.</span>
      <span>Engineered for privacy-first scheduling.</span>`;
    }

    return {
        getToken, getUser, setSession, clearSession, isLoggedIn, requireAuth,
        request, logout, badgeClass, formatTime, formatDate, renderNavbar, renderFooter
    };
})();