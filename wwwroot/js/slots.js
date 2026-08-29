PRIV.requireAuth();
PRIV.renderNavbar(PRIV.getUser());

const params = new URLSearchParams(window.location.search);
const targetUsername = params.get("u");
document.getElementById("slots-heading").textContent = targetUsername
    ? `Available slots — @${targetUsername}`
    : "Available slots";

if (!targetUsername) {
    document.getElementById("slots-results").innerHTML =
        `<p class="error-text">No user specified. Go to their profile and click "View available slots".</p>`;
}

// Default date range: today through +6 days.
const today = new Date();
const in6days = new Date(today.getTime() + 6 * 86400000);
document.getElementById("from-date").value = toDateInput(today);
document.getElementById("to-date").value = toDateInput(in6days);

function toDateInput(d) {
    return d.toISOString().slice(0, 10);
}

document.getElementById("load-slots-btn").addEventListener("click", loadSlots);

async function loadSlots() {
    const resultsEl = document.getElementById("slots-results");
    if (!targetUsername) return;

    const from = document.getElementById("from-date").value;
    const to = document.getElementById("to-date").value;
    if (!from || !to) return;

    resultsEl.innerHTML = `<p class="empty-state">Loading…</p>`;

    try {
        const days = await PRIV.request(
            `/slots/${encodeURIComponent(targetUsername)}?from=${from}&to=${to}`
        );

        if (!days.length) {
            resultsEl.innerHTML = `<p class="empty-state">No availability data.</p>`;
            return;
        }

        resultsEl.innerHTML = days.map(day => `
      <div class="card">
        <h3>${PRIV.formatDate(day.date)}</h3>
        <div class="slot-grid">
          ${day.slots.map(s => `
            <div class="slot ${s.available ? "available" : "unavailable"}">
              ${PRIV.formatTime(s.startTime)}
            </div>
          `).join("")}
        </div>
      </div>
    `).join("");
    } catch (err) {
        resultsEl.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

if (targetUsername) loadSlots();