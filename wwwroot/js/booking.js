document.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const hostId = urlParams.get('hostId');
    const start = urlParams.get('start');
    const end = urlParams.get('end');

    if (hostId && start && end) {
        document.getElementById('hostId').value = hostId;
        document.getElementById('slotStart').value = new Date(start).toLocaleString();
        document.getElementById('slotEnd').value = new Date(end).toLocaleString();

        
        document.getElementById('bookingForm').dataset.startIso = start;
        document.getElementById('bookingForm').dataset.endIso = end;
    }
});

async function submitBooking(e) {
    e.preventDefault();
    const form = document.getElementById('bookingForm');

    const payload = {
        hostId: parseInt(document.getElementById('hostId').value),
        bookingType: document.getElementById('bookingType').value,
        startTime: form.dataset.startIso,
        endTime: form.dataset.endIso,
        location1: document.getElementById('loc1').value,
        location2: document.getElementById('loc2').value || null,
        location3: document.getElementById('loc3').value || null
    };

    const res = await apiFetch('/bookings/request', 'POST', payload);

    if (res.ok) {
        alert('Booking request submitted!');
        window.location.href = '/confirmed.html';
    } else {
        alert(await res.text());
    }
}