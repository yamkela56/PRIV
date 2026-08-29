PRIV.requireAuth();
PRIV.renderNavbar(PRIV.getUser());

function getUsernameFromUrl() {
    // Path looks like /u/yamkelakhumalo
    const parts = window.location.pathname.split("/").filter(Boolean);
    const idx = parts.indexOf("u");
    return idx >= 0 && parts[idx + 1] ? decodeURIComponent(parts[idx + 1]) : null;
}

async function loadProfile() {
    const card = document.getElementById("profile-card");
    const username = getUsernameFromUrl();

    if (!username) {
        card.innerHTML = `<p class="error-text">No username specified.</p>`;
        return;
    }

    try {
        const profile = await PRIV.request(`/users/u/${encodeURIComponent(username)}`);
        card.innerHTML = `
      <h2>${profile.name}</h2>
      <p class="username-tag">@${profile.username}</p>
      ${profile.bio ? `<p>${profile.bio}</p>` : `<p class="hint">No bio yet.</p>`}
      <div id="profile-action"></div>
    `;
        renderAction(profile);
    } catch (err) {
        card.innerHTML = `<p class="error-text">${err.message}</p>`;
    }
}

function renderAction(profile) {
    const actionEl = document.getElementById("profile-action");

    if (profile.isSelf) {
        actionEl.innerHTML = `<p class="hint">This is your profile. <a href="/settings.html">Edit settings</a></p>`;
        return;
    }

    if (profile.connectionStatus === "Approved") {
        actionEl.innerHTML = `
      <div class="btn-row">
        <a href="/booking.html?u=${encodeURIComponent(profile.username)}" class="btn btn-primary">Book time</a>
        <a href="/slots.html?u=${encodeURIComponent(profile.username)}" class="btn">View available slots</a>
      </div>`;
        return;
    }

    if (profile.connectionStatus === "Pending") {
        actionEl.innerHTML = `<button class="btn" disabled>Request pending</button>`;
        return;
    }

    actionEl.innerHTML = `<button class="btn btn-primary" id="request-access-btn">Request Access</button>`;
    document.getElementById("request-access-btn").addEventListener("click", async (e) => {
        e.target.disabled = true;
        e.target.textContent = "Sending…";
        try {
            await PRIV.request("/connections/request", { method: "POST", body: { targetUsername: profile.username } });
            e.target.textContent = "Request pending";
        } catch (err) {
            e.target.disabled = false;
            e.target.textContent = "Request Access";
            alert(err.message);
        }
    });
}

loadProfile();