PRIV.requireAuth();
PRIV.renderNavbar(PRIV.getUser());

const input = document.getElementById("search-input");
const resultsList = document.getElementById("results-list");
let debounceTimer = null;

input.addEventListener("input", () => {
    clearTimeout(debounceTimer);
    const q = input.value.trim();
    if (!q) {
        resultsList.innerHTML = `<p class="empty-state">Start typing a username to search.</p>`;
        return;
    }
    debounceTimer = setTimeout(() => runSearch(q), 300);
});

async function runSearch(q) {
    resultsList.innerHTML = `<p class="empty-state">Searching…</p>`;
    try {
        const results = await PRIV.request(`/users/search?q=${encodeURIComponent(q)}`);
        if (!results.length) {
            resultsList.innerHTML = `<p class="empty-state">No users found.</p>`;
            return;
        }
        resultsList.innerHTML = results.map(u => `
      <div class="list-item" style="display:flex; justify-content:space-between; align-items:center;">
        <div>
          <strong>${u.name}</strong> <span class="username-tag">@${u.username}</span>
        </div>
        <div>
          ${renderActionButton(u)}
        </div>
      </div>
    `).join("");

        document.querySelectorAll("[data-request-username]").forEach(btn => {
            btn.addEventListener("click", () => sendRequest(btn.dataset.requestUsername, btn));
        });
    } catch (err) {
        resultsList.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

function renderActionButton(u) {
    if (u.connectionStatus === "Approved") {
        return `<a href="/u/${encodeURIComponent(u.username)}" class="btn btn-success">View profile</a>`;
    }
    if (u.connectionStatus === "Pending") {
        return `<button class="btn" disabled>Request pending</button>`;
    }
    return `<button class="btn btn-primary" data-request-username="${u.username}">Request Access</button>`;
}

async function sendRequest(username, btn) {
    btn.disabled = true;
    btn.textContent = "Sending…";
    try {
        await PRIV.request("/connections/request", { method: "POST", body: { targetUsername: username } });
        btn.textContent = "Request pending";
    } catch (err) {
        btn.disabled = false;
        btn.textContent = "Request Access";
        alert(err.message);
    }
}