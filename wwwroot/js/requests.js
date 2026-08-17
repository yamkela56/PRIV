async function loadRequests() {
    const res = await apiFetch('/connections/pending');
    const container = document.getElementById('pendingList');

    if (res.ok) {
        const requests = await res.json();
        if (requests.length === 0) {
            container.innerHTML = '<p>No pending requests.</p>';
            return;
        }

        container.innerHTML = requests.map(r => `
            <div class="list-item">
                <div>
                    <strong>Request from @${r.requesterUsername}</strong>
                </div>
                <div style="display:flex; gap:10px;">
                    <button style="width:auto; background:#10b981;" onclick="respondRequest(${r.connectionId}, 'Approved')">Approve</button>
                    <button style="width:auto; background:#ef4444;" onclick="respondRequest(${r.connectionId}, 'Declined')">Decline</button>
                </div>
            </div>
        `).join('');
    }
}

async function respondRequest(connectionId, action) {
    const res = await apiFetch('/connections/respond', 'POST', { connectionId, action });
    if (res.ok) {
        loadRequests();
    } else {
        alert(await res.text());
    }
}