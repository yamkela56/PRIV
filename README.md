# PRIV 
Build Status

License: MIT

Version
## Overview
PRIV is a privacy-first booking and scheduling platform designed to give users full control over their availability and location disclosures. By eliminating publicly visible calendars and requiring mutual approval for time slots, PRIV solves the privacy risks associated with traditional open scheduling tools. It allows users to control access to their available slots, propose custom meeting locations, and manage incoming connection requests securely.

## Target Users
 * **Privacy-Conscious Professionals:** Individuals who want to prevent open public indexing of their daily schedules.
 * **Service Providers & Consultants:** Hosts who require location verification and client approval before confirming bookings.
 * **Private Networks:** Communities and teams needing secure, request-based availability sharing.
   
## Key Features
 * **Granular Availability Control:** Hide schedules behind custom permission levels and connection request approvals.
 * **Dynamic Location Proposals:** Propose and approve custom meeting locations during the response workflow.
 * **Modular JavaScript Architecture:** Dedicated, lightweight page scripts (auth.js, search.js, booking.js, requests.js) built on a central API wrapper.
 * **High-Contrast Editorial UI:** Modern dark obsidian glassmorphism UI styled with CSS custom properties, responsive grids, and subtle micro-interactions.
 * **RESTful Swagger API:** Built-in API documentation and endpoint testing out of the box.
   
## Tech Stack
 * **Backend:** C# / .NET 8 / ASP.NET Core Web API
 * **Database:** SQL Server Express / Entity Framework Core
 * **Frontend:** Modern HTML5, CSS3 (Custom Properties & Glassmorphism Design System), Vanilla JavaScript (ES Modular)
 * **IDE & Tooling:** Visual Studio Community 2022, Swagger UI

## Installation
### Prerequisites
 * .NET 8.0 SDK
 * Visual Studio Community 2022
 * SQL Server Express
### Setup Steps
 1. **Clone the repository:**
   ```bash
   git clone https://github.com/yamkela56/PRIV.git
   cd PRIV
   
   ```
 2. **Configure Database Connection:**
   Update appsettings.json with your local SQL Server connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YAMKELA\\SQLEXPRESS;Database=PrivDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   
   ```
 3. **Apply EF Core Migrations:**
   Run the following command in the Package Manager Console or via CLI:
   ```bash
   dotnet ef database update
   
   ```
 4. **Restore Dependencies:**
   ```bash
   dotnet restore
   
   ```
## Usage
### Running Locally
 1. Open PRIV.sln in **Visual Studio Community 2022**.
 2. Press **F5** or click **Start** to launch the server at https://localhost:7269.
Alternatively, run via the .NET CLI:
```bash
dotnet run --project PRIV

```
### API Endpoint Example
To respond to a pending booking request:
```http
POST /api/bookings/respond
Content-Type: application/json

{
  "bookingId": 102,
  "action": "Approved",
  "selectedLocation": "Private Office 3B",
  "reason": null
}

```
### Interface Screenshots

*Figure 1: PRIV Dashboard & Pending Requests UI.*
## Project Structure
```text
PRIV/
├── Controllers/ # REST API Controllers (BookingsController, AuthController)
├── DTOs/ # Data Transfer Objects (BookingResponseActionDto.cs)
├── Models/ # Entity Framework Core Data Models
├── Data/ # DbContext and Database Configurations
├── wwwroot/ # Static Web Assets
│ ├── styles.css # Glassmorphism Design System Stylesheet
│ ├── js/
│ │ ├── api.js # Central API request wrapper & auth handling
│ │ ├── auth.js # Authentication logic
│ │ ├── booking.js # Slot booking logic
│ │ ├── requests.js # Connection request handling
│ │ └── search.js # User search functionality
│ ├── images/ # Design assets and background images
│ ├── dashboard.html # Main user dashboard
│ ├── requests.html # Pending connection requests
│ ├── search.html # User search portal
│ └── settings.html # User profile and privacy settings
├── appsettings.json # Configuration settings
└── Program.cs # Application entry point & service DI


```
## Contributing
Contributions are welcome! Please follow these steps:
 1. Fork the Project.
 2. Create your Feature Branch (git checkout -b feature/AmazingFeature).
 3. Commit your Changes (git commit -m 'Add some AmazingFeature').
 4. Push to the Branch (git push origin feature/AmazingFeature).
 5. Open a Pull Request.
    
## License
Distributed under the MIT License. See LICENSE for more information.

## Contact
 * **Maintainer:** Yamkela Khumalo
 * **Email:** khumaloyamkela56@gmail.com]
 * **Project Link:** https://github.com/yamkela56/PRIV
