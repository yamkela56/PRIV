PRIV.requireAuth();
const me = PRIV.getUser();
PRIV.renderNavbar(me);

// Profile 
async function loadProfileIntoForm() {
    try {
        const profile = await PRIV.request(`/users/u/${encodeURIComponent(me.username)}`);
        document.getElementById("settings-name").value = profile.name;
        document.getElementById("settings-bio").value = profile.bio || "";
        document.getElementById("new-username").value = profile.username;
        if (profile.discoverableInSearch !== null && profile.discoverableInSearch !== undefined) {
            discoverableToggle.checked = profile.discoverableInSearch;
        }
    } catch (err) {
        console.error(err);
    }
}

document.getElementById("profile-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    try {
        await PRIV.request("/users/me/profile", {
            method: "PUT",
            body: {
                name: document.getElementById("settings-name").value,
                bio: document.getElementById("settings-bio").value
            }
        });
        alert("Profile saved.");
    } catch (err) {
        alert(err.message);
    }
});

//Privacy 
const discoverableToggle = document.getElementById("discoverable-toggle");
discoverableToggle.checked = true; // default; will refine below once it can read it back

discoverableToggle.addEventListener("change", async () => {
    try {
        await PRIV.request("/users/me/privacy", {
            method: "PUT",
            body: { discoverableInSearch: discoverableToggle.checked }
        });
    } catch (err) {
        alert(err.message);
        discoverableToggle.checked = !discoverableToggle.checked;
    }
});

// Username 
document.getElementById("username-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const msgEl = document.getElementById("username-msg");
    msgEl.style.display = "none";
    try {
        const newUsername = document.getElementById("new-username").value;
        await PRIV.request("/users/me/username", { method: "PUT", body: { newUsername } });
        const updatedUser = { ...me, username: newUsername };
        localStorage.setItem("priv_user", JSON.stringify(updatedUser));
        msgEl.className = "success-text";
        msgEl.textContent = "Username updated.";
        msgEl.style.display = "block";
        PRIV.renderNavbar(updatedUser);
    } catch (err) {
        msgEl.className = "error-text";
        msgEl.textContent = err.message;
        msgEl.style.display = "block";
    }
});

// Password 
document.getElementById("password-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const msgEl = document.getElementById("password-msg");
    msgEl.style.display = "none";
    try {
        await PRIV.request("/users/me/password", {
            method: "PUT",
            body: {
                currentPassword: document.getElementById("current-password").value,
                newPassword: document.getElementById("new-password").value
            }
        });
        msgEl.className = "success-text";
        msgEl.textContent = "Password changed.";
        msgEl.style.display = "block";
        document.getElementById("password-form").reset();
    } catch (err) {
        msgEl.className = "error-text";
        msgEl.textContent = err.message;
        msgEl.style.display = "block";
    }
});

// Blocked times
const blockMode = document.getElementById("block-mode");
blockMode.addEventListener("change", () => {
    const recurring = blockMode.value === "recurring";
    document.getElementById("recurring-fields").style.display = recurring ? "block" : "none";
    document.getElementById("specific-fields").style.display = recurring ? "none" : "block";
});

async function loadBlockedTimes() {
    const el = document.getElementById("blocked-times-list");
    try {
        const items = await PRIV.request("/blocked-times");
        if (!items.length) {
            el.innerHTML = `<p class="empty-state">No busy times added yet.</p>`;
            return;
        }
        el.innerHTML = items.map(b => `
      <div class="list-item" style="display:flex; justify-content:space-between; align-items:center;">
        <div>
          <strong>${b.label}</strong>
          <div class="hint">${b.specificDate ? PRIV.formatDate(b.specificDate) : "Every " + b.dayOfWeek}
            · ${PRIV.formatTime(b.startTime)}–${PRIV.formatTime(b.endTime)}</div>
        </div>
        <button class="btn btn-danger" data-delete-block="${b.id}">Remove</button>
      </div>
    `).join("");

        document.querySelectorAll("[data-delete-block]").forEach(btn =>
            btn.addEventListener("click", () => deleteBlockedTime(btn.dataset.deleteBlock)));
    } catch (err) {
        el.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

async function deleteBlockedTime(id) {
    try {
        await PRIV.request(`/blocked-times/${id}`, { method: "DELETE" });
        loadBlockedTimes();
    } catch (err) {
        alert(err.message);
    }
}

document.getElementById("blocked-time-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const isRecurring = blockMode.value === "recurring";
    const start = document.getElementById("block-start").value;
    const end = document.getElementById("block-end").value;

    if (!start || !end) {
        alert("Please provide both a start and end time.");
        return;
    }

    const body = {
        label: document.getElementById("block-label").value || "Busy",
        startTime: start + ":00",
        endTime: end + ":00",
        dayOfWeek: isRecurring ? document.getElementById("block-day").value : null,
        specificDate: isRecurring ? null : (document.getElementById("block-date").value || null)
    };

    if (!isRecurring && !body.specificDate) {
        alert("Please choose a date.");
        return;
    }

    try {
        await PRIV.request("/blocked-times", { method: "POST", body });
        document.getElementById("blocked-time-form").reset();
        loadBlockedTimes();
    } catch (err) {
        alert(err.message);
    }
});

loadProfileIntoForm();
loadBlockedTimes();