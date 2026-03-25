# 🏥 Doctor Appointment System — R1

A production-grade, multi-tenant **Doctor Appointment System** built with **.NET 8**, **Angular**, and **PostgreSQL**, following **Clean Architecture** and **Domain-Driven Design (DDD)** principles.

---

## 🚀 Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | .NET 8 (ASP.NET Core) |
| Frontend | Angular |
| Database | PostgreSQL |
| ORM | Entity Framework Core |
| Auth | ASP.NET Core Identity + JWT |
| Messaging | MediatR (CQRS) |
| Containerization | Docker / Docker Compose |

---

## 🏗️ Architecture

This project uses a **Modular Monolith** architecture designed for future migration to microservices.

```
src/
├── Modules/
│   ├── Appointments/       # Booking, scheduling, availability
│   ├── Doctors/            # Doctor profiles, specializations
│   ├── Patients/           # Patient registration, history
│   ├── Notifications/      # Email / SMS event-driven alerts
│   └── Identity/           # Auth, roles, multi-tenancy
├── Shared/
│   ├── Domain/             # Base entities, value objects
│   └── Infrastructure/     # Cross-cutting concerns
└── API/                    # Entry point, controllers
```

---

## ✨ Key Features

- 🏢 **Multi-Tenant** — White-label support per clinic/hospital
- 🌍 **Timezone-Aware** — Global appointment scheduling across timezones
- 🔔 **Event-Driven Notifications** — Email & SMS on booking/cancellation
- 🔒 **Role-Based Access** — Admin, Doctor, Patient roles
- 📅 **Smart Scheduling** — Availability slots, conflict detection
- 📊 **Clean Architecture** — Domain → Application → Infrastructure → API

---

## 🛠️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js & npm](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)
- [Docker](https://www.docker.com/) *(optional)*

### Run with Docker Compose
```bash
docker-compose up --build
```

### Run Locally
```bash
# Backend
cd src/API
dotnet run

# Frontend
cd frontend
npm install
ng serve
```

---

## 📄 License

MIT © 2026 SachinPShahMCA
