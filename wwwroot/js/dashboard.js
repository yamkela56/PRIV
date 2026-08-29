PRIV.requireAuth();
const me = PRIV.getUser();
PRIV.renderNavbar(me);
document.getElementById("welcome-msg").textContent = `Welcome back, ${me.name}`;

async function loadUpcomingBookings() {
    const el = document.getElementById("upcoming-list");
    try {
        const bookings = await PRIV.request("/bookings/mine?status=Approved");
        if (!bookings.length) {
            el.innerHTML = `<p class="empty-state">No upcoming bookings yet.</p>`;
            return;
        }
        el.innerHTML = bookings.slice(0, 5).map(b => `
      <div class="list-item">
        <strong>${b.type === "Custom" ? b.customTypeLabel : b.type}</strong> with
        <span class="username-tag">@${b.otherUsername}</span>
        <div class="hint">${PRIV.formatDate(b.date)} · ${PRIV.formatTime(b.startTime)}–${PRIV.formatTime(b.endTime)}
          ${b.confirmedLocation ? " · " + b.confirmedLocation.name : ""}</div>
      </div>
    `).join("");
    } catch (err) {
        el.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

async function loadPendingApprovals() {
    const el = document.getElementById("pending-list");
    try {
        const requests = await PRIV.request("/connections/incoming");
        const pending = requests.filter(r => r.status === "Pending");
        if (!pending.length) {
            el.innerHTML = `<p class="empty-state">No pending access requests.</p>`;
            return;
        }
        el.innerHTML = pending.slice(0, 5).map(r => `
      <div class="list-item">
        <strong>${r.otherName}</strong> <span class="username-tag">@${r.otherUsername}</span> wants access to your schedule.
      </div>
    `).join("");
    } catch (err) {
        el.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

async function loadIncomingBookings() {
    const el = document.getElementById("incoming-bookings-list");
    try {
        const bookings = await PRIV.request("/bookings/mine?status=Pending");
        const incoming = bookings.filter(b => b.direction === "Incoming");
        if (!incoming.length) {
            el.innerHTML = `<p class="empty-state">No pending booking requests.</p>`;
            return;
        }
        el.innerHTML = incoming.slice(0, 5).map(b => `
      <div class="list-item">
        <strong>${b.type === "Custom" ? b.customTypeLabel : b.type}</strong> from
        <span class="username-tag">@${b.otherUsername}</span>
        <div class="hint">${PRIV.formatDate(b.date)} · ${PRIV.formatTime(b.startTime)}–${PRIV.formatTime(b.endTime)}</div>
      </div>
    `).join("");
    } catch (err) {
        el.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

async function loadBusyTimes() {
    const el = document.getElementById("busy-list");
    try {
        const items = await PRIV.request("/blocked-times");
        if (!items.length) {
            el.innerHTML = `<p class="empty-state">No busy times added yet.</p>`;
            return;
        }
        el.innerHTML = items.slice(0, 5).map(b => `
      <div class="list-item">
        <strong>${b.label}</strong>
        <div class="hint">${b.specificDate ? PRIV.formatDate(b.specificDate) : b.dayOfWeek}
          · ${PRIV.formatTime(b.startTime)}–${PRIV.formatTime(b.endTime)}</div>
      </div>
    `).join("");
    } catch (err) {
        el.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

loadUpcomingBookings();
loadPendingApprovals();
loadIncomingBookings();
loadBusyTimes();