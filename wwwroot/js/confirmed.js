PRIV.requireAuth();
PRIV.renderNavbar(PRIV.getUser());

let activeStatus = "";
const listEl = document.getElementById("bookings-list");

document.querySelectorAll(".tab").forEach(tab => {
    tab.addEventListener("click", () => {
        document.querySelectorAll(".tab").forEach(t => t.classList.remove("active"));
        tab.classList.add("active");
        activeStatus = tab.dataset.status;
        load();
    });
});

async function load() {
    listEl.innerHTML = `<p class="empty-state">Loading…</p>`;
    try {
        const path = activeStatus ? `/bookings/mine?status=${activeStatus}` : "/bookings/mine";
        const bookings = await PRIV.request(path);
        if (!bookings.length) {
            listEl.innerHTML = `<p class="empty-state">No bookings here yet.</p>`;
            return;
        }
        listEl.innerHTML = bookings.map(renderBooking).join("");
        attachHandlers(bookings);
    } catch (err) {
        listEl.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

function renderBooking(b) {
    const typeLabel = b.type === "Custom" ? b.customTypeLabel : b.type;
    const location = b.confirmedLocation
        ? b.confirmedLocation.name
        : `${b.locationOptions.length} option${b.locationOptions.length > 1 ? "s" : ""} proposed`;

    let actions = "";
    if (b.status === "Pending" && b.direction === "Incoming") {
        const locationChoices = b.locationOptions.map(o =>
            `<option value="${o.id}">${o.name}</option>`).join("");
        actions = `
      <div class="btn-row">
        <select class="location-select" data-booking="${b.id}">${locationChoices}</select>
        <button class="btn btn-success" data-approve="${b.id}">Approve</button>
        <button class="btn btn-danger" data-decline="${b.id}">Decline</button>
      </div>`;
    } else if (b.status === "Pending" || b.status === "Approved") {
        actions = `<div class="btn-row"><button class="btn btn-danger" data-cancel="${b.id}">Cancel</button></div>`;
    }

    const reasonLine = b.declineReason
        ? `<div class="hint">Decline reason: ${b.declineReason}</div>`
        : b.cancelReason
            ? `<div class="hint">Cancel reason: ${b.cancelReason}</div>`
            : "";

    return `
    <div class="list-item">
      <div style="display:flex; justify-content:space-between; flex-wrap:wrap; gap:10px;">
        <div>
          <strong>${typeLabel}</strong>
          <span class="${PRIV.badgeClass(b.status)}">${b.status}</span>
          <div class="hint">
            ${b.direction === "Outgoing" ? "with" : "from"} <span class="username-tag">@${b.otherUsername}</span>
            · ${PRIV.formatDate(b.date)} · ${PRIV.formatTime(b.startTime)}–${PRIV.formatTime(b.endTime)}
          </div>
          <div class="hint">Location: ${location}</div>
          ${reasonLine}
        </div>
      </div>
      ${actions}
    </div>`;
}

function attachHandlers(bookings) {
    document.querySelectorAll("[data-approve]").forEach(btn =>
        btn.addEventListener("click", () => approveBooking(btn.dataset.approve)));
    document.querySelectorAll("[data-decline]").forEach(btn =>
        btn.addEventListener("click", () => openReasonModal(btn.dataset.decline, "decline")));
    document.querySelectorAll("[data-cancel]").forEach(btn =>
        btn.addEventListener("click", () => openReasonModal(btn.dataset.cancel, "cancel")));
}

async function approveBooking(id) {
    const select = document.querySelector(`.location-select[data-booking="${id}"]`);
    const selectedLocationOptionId = parseInt(select.value, 10);
    try {
        await PRIV.request(`/bookings/${id}/approve`, {
            method: "POST",
            body: { selectedLocationOptionId }
        });
        load();
    } catch (err) {
        alert(err.message);
    }
}

//Reason modal (used for both decline and cancel) 
const modal = document.getElementById("reason-modal");
const reasonInput = document.getElementById("reason-input");
const reasonTitle = document.getElementById("reason-modal-title");
let pendingAction = null; // { id, type: "decline" | "cancel" }

function openReasonModal(id, type) {
    pendingAction = { id, type };
    reasonTitle.textContent = type === "decline" ? "Reason for declining" : "Reason for cancelling";
    reasonInput.value = "";
    modal.style.display = "flex";
}

document.getElementById("reason-cancel-btn").addEventListener("click", () => {
    modal.style.display = "none";
    pendingAction = null;
});

document.getElementById("reason-submit-btn").addEventListener("click", async () => {
    const reason = reasonInput.value.trim();
    if (!reason) {
        alert("A reason is required.");
        return;
    }
    if (!pendingAction) return;

    try {
        await PRIV.request(`/bookings/${pendingAction.id}/${pendingAction.type}`, {
            method: "POST",
            body: { reason }
        });
        modal.style.display = "none";
        pendingAction = null;
        load();
    } catch (err) {
        alert(err.message);
    }
});

load();