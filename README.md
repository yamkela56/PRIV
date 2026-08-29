# PRIV

A booking system where User A never sees User B's calendar — only computed
Available/Unavailable slots, and only after User B approves an access request.

## Target Users
* Privacy-Conscious Professionals: Individuals who want to prevent open public indexing of their daily schedules.
* Service Providers & Consultants: Hosts who require location verification and client approval before confirming bookings.
* Private Networks: Communities and teams needing secure, request-based availability sharing.

## Stack
* Backend: C# / ASP.NET Core 8 Web API
* Database: SQL Server (via EF Core), designed for LocalDB in Visual Studio
* Frontend: Static HTML/CSS/JS served from `wwwroot/`
* Auth: JWT bearer tokens, passwords hashed with BCrypt

## Project layout

PRIV/
├── PRIV.sln                
│   ├── Controllers/           
│   ├── Models/                
│   ├── DTOs/                 
│   ├── Services/               
│   ├── Data/AppDbContext.cs

│   ├── wwwroot/                
│   │                             
│   ├── Program.cs

│   └── appsettings.json

└── PrivDb.sql        


## Setup in Visual Studio

1. Open `PRIV.sln` in Visual Studio 2022 (17.8+, with the ASP.NET and web
   development workload, and the "Data storage and processing" workload for
   LocalDB tools).

2. Restore NuGet packages. Visual Studio usually does this automatically
   on open. If not: right-click the solution → Restore NuGet Packages.
   Packages used: `Microsoft.EntityFrameworkCore.SqlServer`,
   `Microsoft.EntityFrameworkCore.Design`,
   `Microsoft.AspNetCore.Authentication.JwtBearer`, `BCrypt.Net-Next`,
   `Swashbuckle.AspNetCore`.

3. Create the database. The connection string in `appsettings.json`
   points at LocalDB (`(localdb)\mssqllocaldb`), which ships with Visual
   Studio — no separate SQL Server install needed. Two ways to create the
   schema:

   Option A — EF Core Migrations (recommended):
   Open Tools → NuGet Package Manager → Package Manager Console and run:
   ```powershell
   Add-Migration InitialCreate
   Update-Database

   This generates and applies migrations matching the model classes exactly.


4. Run the project (F5 or Ctrl+F5, using the `https` launch
   profile). Visual Studio opens a browser to `https://localhost:7080`, which serves the frontend directly — the whole app is
   one project, one process.

5. Try it out:
   - Register two accounts (e.g. `alice`, `bob`) in two browser
     sessions/tabs (or one normal + one incognito window, since the JWT is
     stored in `localStorage`).
   - As `alice`, search for `bob` and click **Request Access**.
   - As `bob`, go to **Requests → Incoming** and **Approve**.
   - As `alice`, go to `bob`'s profile (`/u/bob`) → **View available slots**
     to confirm only Available/Unavailable shows (no event names) → **Book
     time**, choose a type, date/time, and 1–3 locations, and submit.
   - As `bob`, go to **Requests / Dashboard** to see the incoming booking,
     pick one of the proposed locations, and **Approve** (or **Decline**
     with a reason).
   - Both users can see it under **Bookings**, and either can **Cancel**
     with a reason, which frees the slot again.

## Key design notes (useful if you need to explain/defend the project)

- Privacy boundary: `SlotsController` + `SlotService` never return event
  names, blocked-time labels, or booking details to the requester — only
  `Available: true/false` per time slot. That logic lives in one place
  (`SlotService.GetAvailableSlotsAsync`) so the privacy guarantee isn't
  duplicated/re-implemented per endpoint.
- Case-insensitive unique usernames: `User.UsernameNormalized` is a
  lowercase, uniquely-indexed column used for all lookups; `User.Username`
  keeps the original casing for display.
- Approval gating: Both `SlotsController` and `BookingsController`
  independently re-check that a `ConnectionRequest` with
  `Status == Approved` exists before returning slots or accepting a booking
  — so even a direct API call can't skip the approval step.
- Locations: `BookingLocationOption` stores 1–3 proposed locations per
  booking; approving sets `BookingRequest.ConfirmedLocationOptionId` to
  whichever option User B picked.
- Decline/cancel reasons: both are required fields on their respective
  DTOs (`DeclineBookingRequestDto`, `CancelBookingRequestDto`) — the API
  rejects the request with 400 if empty.

## Known simplifications
- Slot increments and working hours are per-user fields with defaults
  (08:00–20:00, 60-minute increments) rather than a full custom-hours UI —
  extend `User.WorkDayStart/End/SlotIncrementMinutes` + a settings form if
  you want that configurable in the UI.
- Email is optional and unverified (no confirmation email flow).
- No password reset flow.
- Frontend uses plain `localStorage` for the JWT (fine for a class project;
  swap for httpOnly cookies if this goes further).

## License

Distributed under the MIT License. See LICENSE for more information.

## Contact
 * **Maintainer:** Yamkela Khumalo
 * **Email:** khumaloyamkela56@gmail.com
 * **Project Link:** https://github.com/yamkela56/PRIV
