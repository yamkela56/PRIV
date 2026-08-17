async function searchUsers() {
    const q = document.getElementById('searchInput').value.trim();
    const container = document.getElementById('searchResults');

    if (!q) {
        container.innerHTML = '';
        return;
    }

    const query = q.startsWith('@') ? q.substring(1) : q;
    const res = await apiFetch(`/users/search?query=${query}`);

    if (res.ok) {
        const users = await res.json();
        if (users.length === 0) {
            container.innerHTML = '<p class="mt-2">No users found.</p>';
            return;
        }

        container.innerHTML = users.map(u => `
            <div class="list-item">
                <div>
                    <strong>${u.fullName || u.username}</strong><br>
                    <small>@${u.username}</small>
                </div>
                <div>
                    ${u.connectionStatus === 'None'
                ? `<button style="width:auto;" onclick="sendAccessRequest(${u.userId})">Request Access</button>`
                : `<span class="badge badge-${u.connectionStatus.toLowerCase()}">${u.connectionStatus}</span>`}
                </div>
            </div>
        `).join('');
    }
}

async function sendAccessRequest(targetId) {
    const res = await apiFetch(`/connections/request/${targetId}`, 'POST');
    if (res.ok) {
        alert('Access request sent!');
        searchUsers();
    } else {
        alert(await res.text());
    }
}