async function fetchAvailableSlots(hostId, dateStr) {
    const grid = document.getElementById('slotsGrid');
    grid.innerHTML = 'Computing available slots...';

    const res = await apiFetch(`/bookings/availability/${hostId}?date=${dateStr}`);

    if (res.ok) {
        const slots = await res.json();
        if (slots.length === 0) {
            grid.innerHTML = '<p>No available slots found for this day.</p>';
            return;
        }

        grid.innerHTML = slots.map(s => {
            const start = new Date(s.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            const end = new Date(s.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            return `
                <div class="slot-btn" onclick="navigateToBooking(${hostId}, '${s.startTime}', '${s.endTime}')">
                    ${start} - ${end}
                </div>
            `;
        }).join('');
    } else {
        grid.innerHTML = `<p style="color:red;">${await res.text()}</p>`;
    }
}

function navigateToBooking(hostId, start, end) {
    window.location.href = `/book.html?hostId=${hostId}&start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`;
}