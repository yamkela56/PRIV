PRIV.requireAuth();
PRIV.renderNavbar(PRIV.getUser());

const params = new URLSearchParams(window.location.search);
const targetUsername = params.get("u");

document.getElementById("booking-heading").textContent = targetUsername
    ? `Book time with @${targetUsername}`
    : "Book time";
document.getElementById("slots-link").href = targetUsername
    ? `/slots.html?u=${encodeURIComponent(targetUsername)}`
    : "/slots.html";

if (!targetUsername) {
    document.getElementById("booking-form").style.display = "none";
    document.getElementById("booking-msg").style.display = "block";
    document.getElementById("booking-msg").className = "error-text";
    document.getElementById("booking-msg").textContent =
        "No user specified. Go to their profile and click \"Book time\".";
}

document.getElementById("booking-type").addEventListener("change", (e) => {
    document.getElementById("custom-type-wrap").style.display =
        e.target.value === "Custom" ? "block" : "none";
});

let optionCount = 1;
document.getElementById("add-location-btn").addEventListener("click", () => {
    if (optionCount >= 3) return;
    optionCount++;
    const wrap = document.getElementById("location-options");
    const row = document.createElement("div");
    row.className = "location-option-row";
    row.innerHTML = `<input type="text" class="location-input" placeholder="Location ${optionCount}" maxlength="200" required />`;
    wrap.appendChild(row);
    if (optionCount >= 3) document.getElementById("add-location-btn").style.display = "none";
});

document.getElementById("booking-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const msgEl = document.getElementById("booking-msg");
    msgEl.style.display = "none";

    const locationOptions = Array.from(document.querySelectorAll(".location-input"))
        .map(i => i.value.trim())
        .filter(Boolean);

    const type = document.getElementById("booking-type").value;
    const customTypeLabel = document.getElementById("custom-type-label").value.trim();

    if (type === "Custom" && !customTypeLabel) {
        msgEl.className = "error-text";
        msgEl.textContent = "Please enter a name for your custom booking type.";
        msgEl.style.display = "block";
        return;
    }

    try {
        await PRIV.request("/bookings", {
            method: "POST",
            body: {
                targetUsername,
                type,
                customTypeLabel: type === "Custom" ? customTypeLabel : null,
                date: document.getElementById("booking-date").value,
                startTime: document.getElementById("start-time").value + ":00",
                endTime: document.getElementById("end-time").value + ":00",
                locationOptions
            }
        });
        msgEl.className = "success-text";
        msgEl.textContent = "Booking request sent! You'll be notified once they respond.";
        msgEl.style.display = "block";
        document.getElementById("booking-form").reset();
        setTimeout(() => { window.location.href = "/confirmed.html"; }, 1200);
    } catch (err) {
        msgEl.className = "error-text";
        msgEl.textContent = err.message;
        msgEl.style.display = "block";
    }
});