PRIV.requireAuth();
PRIV.renderNavbar(PRIV.getUser());

let activeTab = "incoming";
const listEl = document.getElementById("requests-list");

document.querySelectorAll(".tab").forEach(tab => {
    tab.addEventListener("click", () => {
        document.querySelectorAll(".tab").forEach(t => t.classList.remove("active"));
        tab.classList.add("active");
        activeTab = tab.dataset.tab;
        load();
    });
});

async function load() {
    listEl.innerHTML = `<p class="empty-state">Loading…</p>`;
    try {
        const items = await PRIV.request(`/connections/${activeTab}`);
        if (!items.length) {
            listEl.innerHTML = `<p class="empty-state">No ${activeTab} requests.</p>`;
            return;
        }
        listEl.innerHTML = items.map(r => renderRow(r)).join("");

        if (activeTab === "incoming") {
            document.querySelectorAll("[data-approve]").forEach(btn =>
                btn.addEventListener("click", () => respond(btn.dataset.approve, "approve")));
            document.querySelectorAll("[data-decline]").forEach(btn =>
                btn.addEventListener("click", () => respond(btn.dataset.decline, "decline")));
        }
    } catch (err) {
        listEl.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

function renderRow(r) {
    const actions = (activeTab === "incoming" && r.status === "Pending")
        ? `<div class="btn-row">
         <button class="btn btn-success" data-approve="${r.id}">Approve</button>
         <button class="btn btn-danger" data-decline="${r.id}">Decline</button>
       </div>`
        : "";

    return `
    <div class="list-item" style="display:flex; justify-content:space-between; align-items:center; flex-wrap: wrap; gap: 10px;">
      <div>
        <strong>${r.otherName}</strong> <span class="username-tag">@${r.otherUsername}</span>
        <div class="hint">${new Date(r.createdAt).toLocaleDateString()}</div>
      </div>
      <div style="display:flex; align-items:center; gap: 10px;">
        <span class="${PRIV.badgeClass(r.status)}">${r.status}</span>
        ${actions}
      </div>
    </div>`;
}

async function respond(id, action) {
    try {
        await PRIV.request(`/connections/${id}/${action}`, { method: "POST" });
        load();
    } catch (err) {
        alert(err.message);
    }
}

load();