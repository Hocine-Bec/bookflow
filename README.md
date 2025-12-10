# BookFlow

A simple, modern appointment booking system for service-based businesses.

BookFlow enables salons, consultants, tutors, clinics, and other service providers to let customers book appointments online 24/7. Built with a clean UI, fast API, and a focus on eliminating double bookings.

## Features

### For Business Owners

* Manage services (name, duration, price)
* Set weekly working hours and break times
* View all appointments in a dashboard
* Email notifications for new bookings
* Mark appointments as completed or cancelled

### For Customers

* No login required
* Browse available services
* Select date + time with real-time availability
* Book in under 2 minutes
* Cancel via unique secure link
* Instant confirmation email

## Project Structure

```
bookflow/
    backend/      # API, business logic, DB
    frontend/     # Customer booking UI + owner dashboard
    docs/         # PRD, SRS, architecture, API specs
    infra/        # Deployment, Docker, Nginx, etc.
    .gitignore
    LICENSE
    README.md
```

## Tech Stack

### Backend

* Node.js + TypeScript
* Express or Fastify
* PostgreSQL
* Prisma ORM
* JWT Authentication
* Nodemailer for email

### Frontend

* React (Vite or Next.js)
* TailwindCSS
* React Query / Zustand / Context API

### Infrastructure

* Docker
* Nginx reverse proxy
* Optional: Terraform / Kubernetes

## Core Booking Logic

The availability engine computes time slots using:

1. Business working hours
2. Service duration
3. 15-minute increments
4. Existing appointments
5. Breaks
6. Booking rules (min notice, max window)

## Getting Started

### 1. Clone the repo

```bash
git clone https://github.com/your-username/bookflow.git
cd bookflow
```

### 2. Backend Setup

```bash
cd backend
npm install
npm run dev
```

### 3. Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

## Environment Variables

### Backend (`.env`)

```
DATABASE_URL=
JWT_SECRET=
SMTP_HOST=
SMTP_USER=
SMTP_PASS=
FRONTEND_URL=
```

### Frontend (`.env`)

```
VITE_API_URL=
```

## Testing

### Backend tests

```bash
cd backend
npm run test
```

### Frontend tests

```bash
cd frontend
npm run test
```

## Documentation

All documentation lives in `docs/`:

* PRD.md
* SRS.md
* API_SPEC.md
* SYSTEM_DESIGN.md
* DB_SCHEMA.md

## Roadmap

* Multi-staff support
* Payment integrations
* SMS notifications
* Customer accounts
* Calendar integrations
* Analytics dashboard

## License

MIT License.

## Contributing

PRs welcome!
Please open an issue before submitting large changes.