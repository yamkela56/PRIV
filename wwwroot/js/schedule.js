async function loadConfirmedBookings() {
    const res = await apiFetch('/bookings/my-schedule');
    const container = document.getElementById('confirmedList');

    if (res.ok) {
        const bookings = await res.json();
        if (bookings.length === 0) {
            container.innerHTML = '<p>No bookings found.</p>';
            return;
        }

        container.innerHTML = bookings.map(b => `
            <div class="card">
                <div class="list-item">
                    <div>
                        <h3>${b.bookingType} with ${b.isHost ? '@' + b.requester : '@' + b.host}</h3>
                        <p><strong>Time:</strong> ${new Date(b.startTime).toLocaleString()}</p>
                        ${b.confirmedLocation ? `<p><strong>Confirmed Location:</strong> ${b.confirmedLocation}</p>` : ''}
                        ${b.declineCancelReason ? `<p style="color:red;"><strong>Reason:</strong> ${b.declineCancelReason}</p>` : ''}
                    </div>
                    <div>
                        <span class="badge badge-${b.status.toLowerCase()}">${b.status}</span>
                    </div>
                </div>

                <!-- Host Location Approval Section -->
                ${b.isHost && b.status === 'Pending' ? `
                    <div style="margin-top:10px; background:#f8fafc; padding:10px; border-radius:6px;">
                        <h4>Select Location to Approve:</h4>
                        <select id="locSelect_${b.bookingId}">
                            <option value="${b.location1}">1. ${b.location1}</option>
                            ${b.location2 ? `<option value="${b.location2}">2. ${b.location2}</option>` : ''}
                            ${b.location3 ? `<option value="${b.location3}">3. ${b.location3}</option>` : ''}
                        </select>
                        <div style="display:flex; gap:10px; margin-top:5px;">
                            <button style="background:#10b981;" onclick="respondBooking(${b.bookingId}, 'Approved')">Approve with Selected Location</button>
                            <button style="background:#ef4444;" onclick="respondBooking(${b.bookingId}, 'Declined')">Decline</button>
                        </div>
                    </div>
                ` : ''}

                <!-- Cancel Option -->
                ${b.status === 'Approved' || b.status === 'Pending' ? `
                    <button class="btn-secondary" style="margin-top:10px;" onclick="respondBooking(${b.bookingId}, 'Cancelled')">Cancel Booking</button>
                ` : ''}
            </div>
        `).join('');
    }
}

async function respondBooking(bookingId, action) {
    let selectedLocation = null;
    let reason = null;

    if (action === 'Approved') {
        selectedLocation = document.getElementById(`locSelect_${bookingId}`).value;
    } else if (action === 'Declined' || action === 'Cancelled') {
        reason = prompt(`Please provide a mandatory reason for ${action.toLowerCase()}ing:`);
        if (!reason) {
            alert('Reason is required!');
            return;
        }
    }

    const res = await apiFetch('/bookings/respond', 'POST', {
        bookingId,
        action,
        selectedLocation,
        reason
    });

    if (res.ok) {
        loadConfirmedBookings();
    } else {
        alert(await res.text());
    }
}