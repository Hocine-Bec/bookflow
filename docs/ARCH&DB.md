# Appointment Booking System - Architecture & Database Design

## 1. Business Model: Multi-Tenant SaaS Platform

### How It Works

**Your Platform:** `bookease.com` (one website, many businesses)

**Business Registration Flow:**
1. Business owner visits `bookease.com`
2. Signs up with email/password + business details
3. Gets unique booking URL: `bookease.com/{business-slug}`
   - Example: `bookease.com/sarahs-hair-salon`
4. Business owner shares this URL with their customers
5. Customers visit that URL to book appointments (no login needed)
6. Business owner logs into `bookease.com/dashboard` to manage

**Key Concept: Business Slug**
- Each business gets a unique slug (URL-friendly identifier)
- Generated from business name: "Sarah's Hair Salon" → `sarahs-hair-salon`
- Used in URLs: `bookease.com/sarahs-hair-salon`
- Must be unique across all businesses

### Route Structure

**Public Routes (No Auth):**
- `/` - Landing page (marketing, sign up CTA)
- `/register` - Business owner registration
- `/login` - Business owner login
- `/{businessSlug}` - Public booking page for a business
- `/{businessSlug}/book/{serviceId}` - Booking flow
- `/appointments/view?token=xxx` - View appointment via email link
- `/appointments/cancel?token=xxx` - Cancel appointment

**Protected Routes (Business Owner Only):**
- `/dashboard` - Business owner dashboard
- `/dashboard/services` - Manage services
- `/dashboard/appointments` - View all appointments
- `/dashboard/hours` - Manage business hours
- `/dashboard/settings` - Business profile settings

**How Frontend Knows Which Business:**
- When customer visits `/{businessSlug}`, fetch business data from API
- API endpoint: `GET /api/businesses/by-slug/{slug}`
- Returns business info + their services
- No authentication needed for this endpoint (it's public)

---

## 2. System Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                       │
│                                                              │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │  Customer Web UI │         │ Business Web UI  │         │
│  │   (React + Vite) │         │  (React + Vite)  │         │
│  └────────┬─────────┘         └────────┬─────────┘         │
│           │                             │                    │
│           └─────────────┬───────────────┘                   │
│                         │                                    │
└─────────────────────────┼────────────────────────────────────┘
                          │ HTTPS / REST API
                          │
┌─────────────────────────▼────────────────────────────────────┐
│                     APPLICATION LAYER                         │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           .NET 9 Web API (ASP.NET Core)              │  │
│  │                                                       │  │
│  │  ├─ Controllers (HTTP endpoints)                     │  │
│  │  ├─ Middleware (Auth, Error handling, Logging)       │  │
│  │  └─ DTOs (Data Transfer Objects)                     │  │
│  └──────────────────────┬───────────────────────────────┘  │
│                         │                                    │
└─────────────────────────┼────────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────────┐
│                      BUSINESS LAYER                          │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Core Business Logic                      │  │
│  │                                                       │  │
│  │  ├─ Services (Business operations)                   │  │
│  │  ├─ Domain Models (Entities)                         │  │
│  │  ├─ Interfaces (Contracts)                           │  │
│  │  └─ Business Rules & Validation                      │  │
│  └──────────────────────┬───────────────────────────────┘  │
│                         │                                    │
└─────────────────────────┼────────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                       │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Database   │  │    Email     │  │  File Storage│     │
│  │   (EF Core)  │  │   Service    │  │  (Optional)  │     │
│  │              │  │  (SendGrid)  │  │              │     │
│  │ PostgreSQL/  │  │              │  │              │     │
│  │ SQL Server   │  │              │  │              │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### Why This Architecture?

**Layered Architecture (3-Tier)**
- **Separation of Concerns:** Each layer has a specific responsibility
- **Maintainability:** Change database without touching business logic
- **Testability:** Test business logic without database or UI
- **Professional Standard:** This is what companies use in production

**Single Page Application (SPA) Frontend**
- **Better UX:** No page reloads, feels like a native app
- **API-First:** Same API can later support mobile apps
- **Modern Standard:** React is industry-standard for web apps

---

## 2. Technology Stack Decisions

### Backend: .NET 9 Web API

**Why .NET 9?**
- ✅ Fast performance (among top web frameworks)
- ✅ Built-in dependency injection
- ✅ Excellent async/await support (important for database operations)
- ✅ Strong typing catches errors at compile time
- ✅ Free, cross-platform, backed by Microsoft
- ✅ Entity Framework Core for database (no raw SQL needed)
- ✅ Built-in authentication/authorization

**Project Structure:**
```
BookingSystem/
├── BookingSystem.API/              # Web API project
│   ├── Controllers/                # HTTP endpoints
│   ├── Middleware/                 # Custom middleware
│   ├── DTOs/                       # Request/Response models
│   └── Program.cs                  # App configuration
│
├── BookingSystem.Core/             # Business logic
│   ├── Entities/                   # Domain models
│   ├── Interfaces/                 # Abstractions
│   ├── Services/                   # Business operations
│   └── Exceptions/                 # Custom exceptions
│
└── BookingSystem.Infrastructure/   # External concerns
    ├── Data/                       # Database context
    ├── Repositories/               # Data access
    ├── EmailService/               # Email sending
    └── Migrations/                 # Database migrations
```

### Frontend: React 19 + Vite + TypeScript

**Why React + Vite?**
- ✅ React: Most popular UI library, huge community, lots of resources
- ✅ Vite: Ultra-fast dev server and builds (much faster than Create React App)
- ✅ Component-based: Reusable UI pieces (Button, Calendar, etc.)
- ✅ Rich ecosystem: Libraries for calendars, date pickers, forms

**Project Structure:**
```
booking-system-frontend/
├── src/
│   ├── components/           # Reusable UI components
│   │   ├── common/          # Button, Input, Card, etc.
│   │   ├── booking/         # ServiceList, TimeSlotPicker
│   │   └── dashboard/       # AppointmentCalendar, etc.
│   │
│   ├── pages/               # Route components
│   │   ├── Home.jsx
│   │   ├── BookingPage.jsx
│   │   ├── Dashboard.jsx
│   │   └── Login.jsx
│   │
│   ├── services/            # API communication
│   │   └── api.js          # Axios instance, API calls
│   │
│   ├── context/             # React Context (auth state)
│   │   └── AuthContext.jsx
│   │
│   ├── hooks/               # Custom React hooks
│   │   └── useAuth.js
│   │
│   ├── utils/               # Helper functions
│   │   └── dateHelpers.js
│   │
│   └── App.jsx              # Main app component
│
└── package.json
```

### Database: PostgreSQL (Recommended) or SQL Server

**Why PostgreSQL?**
- ✅ Free and open source
- ✅ Excellent JSON support (for flexible data)
- ✅ Strong data integrity
- ✅ Works everywhere (Windows, Mac, Linux, cloud)
- ✅ Free hosting options (Supabase, Railway, Neon)

**Alternative: SQL Server**
- ✅ Great if you deploy to Azure
- ✅ Free tier (Express edition)
- ✅ Familiar if you know SQL Server

**For this project, I'll design for PostgreSQL, but it works with either.**

### Authentication: JWT (JSON Web Tokens)

**Why JWT?**
- ✅ Stateless: Server doesn't store sessions
- ✅ Works perfectly with SPAs
- ✅ Can scale horizontally (multiple servers)
- ✅ Token contains user info (no database lookup per request)

**Flow:**
```
1. User logs in → Server validates credentials
2. Server creates JWT with user info → Returns to client
3. Client stores JWT (in memory or localStorage)
4. Client sends JWT in header with every request
5. Server validates JWT → Allows/denies request
```

### Email: SendGrid (or SMTP)

**Why SendGrid?**
- ✅ Free tier: 100 emails/day (plenty for MVP)
- ✅ Reliable delivery
- ✅ Simple API
- ✅ Email templates

**Alternative: SMTP**
- Use Gmail SMTP or your hosting provider
- Free but less reliable, may go to spam

---

## 3. Database Design

### Entity Relationship Diagram (ERD)

```
┌─────────────────────────────────────┐
│              Users                  │
├─────────────────────────────────────┤
│ Id (PK, GUID)                       │
│ Email (Unique, Index)               │
│ PasswordHash                        │
│ Role (Enum: Business, Admin)       │
│ BusinessName                        │
│ BusinessPhone                       │
│ BusinessDescription                 │
│ IsEmailVerified                     │
│ CreatedAt                           │
│ UpdatedAt                           │
└──────────────┬──────────────────────┘
               │
               │ 1:N
               │
┌──────────────▼──────────────────────┐
│           Services                  │
├─────────────────────────────────────┤
│ Id (PK, GUID)                       │
│ BusinessId (FK → Users.Id)          │
│ Name                                │
│ Description                         │
│ DurationMinutes                     │
│ Price                               │
│ IsActive                            │
│ CreatedAt                           │
│ UpdatedAt                           │
└──────────────┬──────────────────────┘
               │
               │ 1:N
               │
┌──────────────▼──────────────────────┐
│         Appointments                │
├─────────────────────────────────────┤
│ Id (PK, GUID)                       │
│ ServiceId (FK → Services.Id)        │
│ BusinessId (FK → Users.Id)          │
│ CustomerName                        │
│ CustomerEmail (Index)               │
│ CustomerPhone                       │
│ AppointmentDate (Index)             │
│ StartTime                           │
│ EndTime                             │
│ Status (Enum: Pending, Confirmed,   │
│         Completed, Cancelled)       │
│ Notes                               │
│ CancellationToken (GUID, Unique)    │
│ CreatedAt                           │
│ UpdatedAt                           │
└─────────────────────────────────────┘


┌─────────────────────────────────────┐
│        BusinessHours                │
├─────────────────────────────────────┤
│ Id (PK, GUID)                       │
│ BusinessId (FK → Users.Id)          │
│ DayOfWeek (0=Sun, 1=Mon...6=Sat)   │
│ OpenTime (TimeSpan)                 │
│ CloseTime (TimeSpan)                │
│ IsClosed (Boolean)                  │
│ CreatedAt                           │
│ UpdatedAt                           │
└─────────────────────────────────────┘
       ▲
       │ 1:N
       │
       └─────────────── Users
```

### Detailed Table Schemas

#### **Users Table**
```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL,
    BusinessName VARCHAR(255),
    BusinessSlug VARCHAR(255) NOT NULL UNIQUE,
    BusinessPhone VARCHAR(20),
    BusinessDescription TEXT,
    IsEmailVerified BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON Users(Email);
CREATE UNIQUE INDEX idx_users_slug ON Users(BusinessSlug);
```

**Why These Fields?**
- `Id` as GUID: Secure, non-guessable IDs
- `PasswordHash`: NEVER store plain passwords
- `Role`: Distinguish business owners from potential future admin roles
- `BusinessName`: Display name (e.g., "Sarah's Hair Salon")
- `BusinessSlug`: URL-friendly unique identifier (e.g., "sarahs-hair-salon")
  - Generated from BusinessName during registration
  - Must be unique (enforced by UNIQUE constraint)
  - Used in public URLs: `bookease.com/sarahs-hair-salon`
- `BusinessPhone/Description`: Displayed to customers on booking page
- `IsEmailVerified`: Future feature for email verification
- `CreatedAt/UpdatedAt`: Audit trail, useful for debugging

---

#### **Services Table**
```sql
CREATE TABLE Services (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BusinessId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    DurationMinutes INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_services_businessid ON Services(BusinessId);
CREATE INDEX idx_services_active ON Services(BusinessId, IsActive);
```

**Why These Fields?**
- `BusinessId`: Who owns this service (foreign key)
- `DurationMinutes`: Critical for slot calculation (15, 30, 60, 90 min, etc.)
- `Price`: DECIMAL for money (not FLOAT - floating point causes rounding errors!)
- `IsActive`: Soft delete - hide service but keep historical appointment data
- **ON DELETE CASCADE**: If business account is deleted, delete their services too

---

#### **Appointments Table**
```sql
CREATE TABLE Appointments (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ServiceId UUID NOT NULL REFERENCES Services(Id),
    BusinessId UUID NOT NULL REFERENCES Users(Id),
    CustomerName VARCHAR(255) NOT NULL,
    CustomerEmail VARCHAR(255) NOT NULL,
    CustomerPhone VARCHAR(20) NOT NULL,
    AppointmentDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    Notes TEXT,
    CancellationToken UUID NOT NULL UNIQUE DEFAULT gen_random_uuid(),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_appointments_businessid_date ON Appointments(BusinessId, AppointmentDate);
CREATE INDEX idx_appointments_customer_email ON Appointments(CustomerEmail);
CREATE INDEX idx_appointments_date_status ON Appointments(AppointmentDate, Status);
CREATE INDEX idx_appointments_cancellation_token ON Appointments(CancellationToken);
```

**Why These Fields?**
- `ServiceId` AND `BusinessId`: Redundant but useful - can query by business directly
- `AppointmentDate`: Separate date field for easy date-based queries
- `StartTime/EndTime`: TIME type for time-of-day (09:00:00, 10:30:00)
- `Status`: Track appointment lifecycle
- `CancellationToken`: Unique GUID for secure cancellation links (no login needed)
- **Multiple Indexes**: This table will be queried heavily, indexes speed up queries

**Critical Index:** `(BusinessId, AppointmentDate)` - used for slot availability calculation

---

#### **BusinessHours Table**
```sql
CREATE TABLE BusinessHours (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BusinessId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    DayOfWeek INT NOT NULL CHECK (DayOfWeek >= 0 AND DayOfWeek <= 6),
    OpenTime TIME,
    CloseTime TIME,
    IsClosed BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(BusinessId, DayOfWeek)
);

CREATE INDEX idx_businesshours_businessid ON BusinessHours(BusinessId);
```

**Why These Fields?**
- `DayOfWeek`: 0=Sunday, 1=Monday, ... 6=Saturday (matches .NET DayOfWeek enum)
- `OpenTime/CloseTime`: When business is open that day (09:00, 17:00)
- `IsClosed`: If TRUE, business is closed that day (ignore OpenTime/CloseTime)
- **UNIQUE(BusinessId, DayOfWeek)**: One row per business per day
- Example rows:
  ```
  BusinessId: abc123, DayOfWeek: 0 (Sun), IsClosed: true
  BusinessId: abc123, DayOfWeek: 1 (Mon), OpenTime: 09:00, CloseTime: 17:00
  BusinessId: abc123, DayOfWeek: 2 (Tue), OpenTime: 09:00, CloseTime: 17:00
  ```

---

### Database Relationships

```
Users (1) ────< (N) Services
Users (1) ────< (N) Appointments
Users (1) ────< (N) BusinessHours
Services (1) ──< (N) Appointments
```

**Referential Integrity:**
- If a User is deleted → All their Services, Appointments, and BusinessHours are deleted (CASCADE)
- If a Service is deleted → Set IsActive = false instead (soft delete) to preserve appointment history
- Appointments reference both Service AND Business for easier querying

---

## 4. API Design (REST Endpoints)

### Authentication Endpoints

```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
GET    /api/auth/me
```

**Detailed Specification:**

#### `POST /api/auth/register`
**Request Body:**
```json
{
  "email": "sarah@salon.com",
  "password": "SecurePass123!",
  "businessName": "Sarah's Hair Salon",
  "businessPhone": "+1234567890",
  "businessDescription": "Premium hair styling services"
}
```

**Response: 201 Created**
```json
{
  "userId": "uuid",
  "email": "sarah@salon.com",
  "businessName": "Sarah's Hair Salon",
  "businessSlug": "sarahs-hair-salon",
  "bookingUrl": "https://bookease.com/sarahs-hair-salon",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-15T12:00:00Z"
}
```

**Response: 400 Bad Request**
```json
{
  "errors": {
    "email": ["Email is already registered"],
    "password": ["Password must be at least 8 characters"],
    "businessName": ["Business name is required"]
  }
}
```

**Slug Generation Logic:**
```csharp
// In your service layer
public string GenerateSlug(string businessName)
{
    // "Sarah's Hair Salon" → "sarahs-hair-salon"
    var slug = businessName.ToLower()
        .Replace("'", "")
        .Replace(" ", "-")
        .Replace("&", "and");
    
    // Remove any non-alphanumeric except hyphens
    slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
    
    // Check if slug exists, if yes, append number
    var finalSlug = slug;
    var counter = 1;
    while (await _context.Users.AnyAsync(u => u.BusinessSlug == finalSlug))
    {
        finalSlug = $"{slug}-{counter}";
        counter++;
    }
    
    return finalSlug;
}
```

**Why Slug Generation:**
- Makes shareable URLs: `bookease.com/sarahs-hair-salon` (user-friendly)
- Better than: `bookease.com/business/a8f2e9d1-4c3b-4a5e-8f9d-1e2c3d4a5b6c` (ugly)
- Helps with SEO (search engines prefer readable URLs)
- Must handle duplicates: If "Sarah's Hair Salon" exists, next one becomes "sarahs-hair-salon-2"

---

#### `POST /api/auth/login`
**Request Body:**
```json
{
  "email": "sarah@salon.com",
  "password": "SecurePass123!"
}
```

**Response: 200 OK** (Same as register)

**Response: 401 Unauthorized**
```json
{
  "message": "Invalid email or password"
}
```

---

### Business Public Profile Endpoints

```
GET    /api/businesses/by-slug/{slug}                 (Public)
```

#### `GET /api/businesses/by-slug/{slug}`
**Purpose:** Get business info and services for public booking page

**Example:** `GET /api/businesses/by-slug/sarahs-hair-salon`

**Response: 200 OK**
```json
{
  "id": "uuid",
  "businessName": "Sarah's Hair Salon",
  "businessSlug": "sarahs-hair-salon",
  "businessPhone": "+1234567890",
  "businessDescription": "Premium hair styling services in downtown",
  "services": [
    {
      "id": "uuid",
      "name": "Women's Haircut",
      "description": "Cut and style",
      "durationMinutes": 60,
      "price": 50.00
    },
    {
      "id": "uuid",
      "name": "Hair Coloring",
      "description": "Full color treatment",
      "durationMinutes": 120,
      "price": 150.00
    }
  ]
}
```

**Response: 404 Not Found**
```json
{
  "message": "Business not found"
}
```

**Why This Endpoint:**
- Customer visits `bookease.com/sarahs-hair-salon`
- Frontend extracts `sarahs-hair-salon` from URL
- Calls this API to get business info and services
- Renders the booking page

---

### Service Management Endpoints

```
GET    /api/businesses/{businessId}/services          (Public)
GET    /api/services/{id}                             (Public)
POST   /api/services                                  (Auth: Business)
PUT    /api/services/{id}                             (Auth: Business, Owner)
DELETE /api/services/{id}                             (Auth: Business, Owner)
```

#### `GET /api/businesses/{businessId}/services`
**Response: 200 OK**
```json
{
  "businessName": "Sarah's Hair Salon",
  "businessPhone": "+1234567890",
  "services": [
    {
      "id": "uuid",
      "name": "Women's Haircut",
      "description": "Cut and style",
      "durationMinutes": 60,
      "price": 50.00
    },
    {
      "id": "uuid",
      "name": "Hair Coloring",
      "description": "Full color treatment",
      "durationMinutes": 120,
      "price": 150.00
    }
  ]
}
```

---

#### `POST /api/services`
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "name": "Men's Haircut",
  "description": "Classic cut",
  "durationMinutes": 30,
  "price": 35.00
}
```

**Response: 201 Created**
```json
{
  "id": "uuid",
  "businessId": "uuid",
  "name": "Men's Haircut",
  "description": "Classic cut",
  "durationMinutes": 30,
  "price": 35.00,
  "isActive": true,
  "createdAt": "2025-01-10T10:00:00Z"
}
```

---

### Business Hours Endpoints

```
GET    /api/businesses/{businessId}/hours             (Public)
POST   /api/business-hours                            (Auth: Business)
PUT    /api/business-hours/{id}                       (Auth: Business, Owner)
```

#### `GET /api/businesses/{businessId}/hours`
**Response: 200 OK**
```json
{
  "hours": [
    {
      "dayOfWeek": 0,
      "dayName": "Sunday",
      "isClosed": true
    },
    {
      "dayOfWeek": 1,
      "dayName": "Monday",
      "isClosed": false,
      "openTime": "09:00:00",
      "closeTime": "17:00:00"
    },
    ...
  ]
}
```

---

### Appointment Endpoints

```
GET    /api/appointments/available-slots              (Public)
POST   /api/appointments                              (Public - Book)
GET    /api/appointments                              (Auth: Business - List)
GET    /api/appointments/{id}                         (Public with token)
PUT    /api/appointments/{id}/cancel                  (Public with token)
PUT    /api/appointments/{id}/status                  (Auth: Business, Owner)
```

#### `GET /api/appointments/available-slots`
**Query Parameters:**
- `serviceId` (required): UUID
- `date` (required): YYYY-MM-DD

**Response: 200 OK**
```json
{
  "serviceId": "uuid",
  "serviceName": "Women's Haircut",
  "durationMinutes": 60,
  "date": "2025-01-15",
  "availableSlots": [
    {
      "startTime": "09:00:00",
      "endTime": "10:00:00"
    },
    {
      "startTime": "09:15:00",
      "endTime": "10:15:00"
    },
    {
      "startTime": "11:00:00",
      "endTime": "12:00:00"
    }
  ]
}
```

**Algorithm for this endpoint (Critical!):**
```csharp
1. Get service duration
2. Get business hours for that day of week
3. If business is closed that day → return empty array
4. Generate all 15-minute slots from open to close time
   - Filter out slots where appointment wouldn't fit before closing
5. Get all existing appointments for that business on that date
6. Remove slots that conflict with existing appointments
7. Return remaining slots
```

---

#### `POST /api/appointments`
**Request Body:**
```json
{
  "serviceId": "uuid",
  "appointmentDate": "2025-01-15",
  "startTime": "10:00:00",
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "customerPhone": "+1234567890",
  "notes": "First time customer"
}
```

**Response: 201 Created**
```json
{
  "id": "uuid",
  "confirmationNumber": "BK-20250115-ABC",
  "serviceName": "Women's Haircut",
  "businessName": "Sarah's Hair Salon",
  "appointmentDate": "2025-01-15",
  "startTime": "10:00:00",
  "endTime": "11:00:00",
  "status": "Pending",
  "cancellationUrl": "https://yourdomain.com/appointments/cancel?token=uuid"
}
```

**Response: 409 Conflict**
```json
{
  "message": "This time slot is no longer available",
  "suggestedSlots": ["10:15:00", "10:30:00", "11:00:00"]
}
```

**Validation Rules:**
- Cannot book in the past
- Cannot book outside business hours
- Cannot double-book (check for conflicts)
- Date must be within 90 days
- Must be at least 2 hours in future

---

#### `GET /api/appointments?date=2025-01-15&status=Pending`
**Headers:** `Authorization: Bearer {token}`

**Query Parameters (Optional):**
- `date`: Filter by date
- `status`: Filter by status
- `startDate` & `endDate`: Date range

**Response: 200 OK**
```json
{
  "appointments": [
    {
      "id": "uuid",
      "serviceName": "Women's Haircut",
      "customerName": "John Doe",
      "customerEmail": "john@example.com",
      "customerPhone": "+1234567890",
      "appointmentDate": "2025-01-15",
      "startTime": "10:00:00",
      "endTime": "11:00:00",
      "status": "Pending",
      "notes": "First time customer",
      "createdAt": "2025-01-10T14:30:00Z"
    }
  ],
  "totalCount": 1
}
```

---

#### `GET /api/appointments/{id}?token={cancellationToken}`
**No Auth Required** (uses cancellation token from email)

**Response: 200 OK**
```json
{
  "id": "uuid",
  "serviceName": "Women's Haircut",
  "businessName": "Sarah's Hair Salon",
  "businessPhone": "+1234567890",
  "appointmentDate": "2025-01-15",
  "startTime": "10:00:00",
  "endTime": "11:00:00",
  "status": "Pending",
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "canCancel": true
}
```

---

#### `PUT /api/appointments/{id}/cancel?token={cancellationToken}`
**No Auth Required** (uses cancellation token)

**Response: 200 OK**
```json
{
  "message": "Appointment cancelled successfully",
  "appointmentId": "uuid",
  "status": "Cancelled"
}
```

**Response: 400 Bad Request**
```json
{
  "message": "Cannot cancel appointment within 2 hours of start time"
}
```

---

## 5. .NET Project Structure (Clean Architecture)

### Layer Responsibilities

**API Layer (BookingSystem.API)**
- Receives HTTP requests
- Validates input (model validation)
- Calls business logic services
- Returns HTTP responses
- Handles authentication/authorization
- **Does NOT contain business logic**

**Core Layer (BookingSystem.Core)**
- Contains ALL business logic
- Domain entities (models)
- Interfaces (contracts)
- Business rules and validation
- **Independent of database, UI, external services**

**Infrastructure Layer (BookingSystem.Infrastructure)**
- Database access (EF Core)
- Email sending
- File storage
- External API calls
- **Implements interfaces from Core**

### Dependency Flow
```
API → Core ← Infrastructure
```

API and Infrastructure both depend on Core.
Core depends on NOTHING (pure business logic).

---

## 6. Frontend Architecture

### Component Structure

```
Common Components (Reusable):
- Button
- Input
- Card
- Modal
- DatePicker
- TimePicker
- LoadingSpinner

Feature Components:
- ServiceCard (display a service)
- ServiceList (list all services)
- TimeSlotGrid (show available slots)
- AppointmentCard (display appointment)
- CalendarView (calendar for appointments)
- BookingForm (booking form)

Page Components:
- HomePage (landing page)
- ServicesPage (browse services)
- BookingPage (select time and book)
- ConfirmationPage (after booking)
- DashboardPage (business owner dashboard)
- LoginPage
- RegisterPage
```

### State Management

**Options:**
1. **React Context** (Recommended for this size)
   - AuthContext (user login state)
   - Simple, built-in, no extra libraries

2. **Local Component State** (useState)
   - For UI state (modals open/closed, form inputs)
   - Fetched data from API

**What NOT to do:**
- Don't use Redux (overkill for this project)
- Don't store everything in Context (only auth state)

### Routing Structure

```
/ (Home)
/services/:businessId (Browse services for a business)
/book/:serviceId (Select date/time and book)
/confirmation/:appointmentId (After booking)
/appointments/view?token=xxx (View appointment via email link)
/appointments/cancel?token=xxx (Cancel appointment)
/login
/register
/dashboard (Business owner, protected route)
  /dashboard/services
  /dashboard/appointments
  /dashboard/hours
```

---

## 7. Security Considerations

### Password Security
- **NEVER** store plain text passwords
- Use BCrypt or PBKDF2 for hashing
- In .NET: Use `PasswordHasher<T>` from `Microsoft.AspNetCore.Identity`

### JWT Security
- Set reasonable expiration (1 hour for access token)
- Use strong secret key (256-bit minimum)
- Store secret in environment variables, NOT in code
- Validate token on every protected endpoint

### CORS (Cross-Origin Resource Sharing)
- API and Frontend will be on different domains in production
- Configure CORS in .NET to allow your frontend domain
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://your-frontend-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Input Validation
- Validate all input on backend (never trust frontend)
- Use Data Annotations in DTOs
- Sanitize user input (prevent SQL injection, XSS)

### Rate Limiting
- Prevent abuse of booking endpoint (someone booking 100 times)
- Use `AspNetCoreRateLimit` NuGet package
- Example: Max 10 bookings per IP per hour

---

## 8. Development Environment Setup Checklist

Before coding, set up:

**Backend:**
- [ ] Install .NET 8 SDK
- [ ] Install Visual Studio / VS Code / Rider
- [ ] Install PostgreSQL (or SQL Server)
- [ ] Install Postman or Thunder Client (API testing)

**Frontend:**
- [ ] Install Node.js (v18 or higher)
- [ ] Install VS Code
- [ ] Install React DevTools browser extension

**Database Tool:**
- [ ] pgAdmin (PostgreSQL) or Azure Data Studio (SQL Server)

**Version Control:**
- [ ] Git installed
- [ ] GitHub repository created
- [ ] Clone repository locally

---

## 9. Implementation Roadmap

### Phase 2A: Backend Setup (Week 1)
1. Create .NET solution with 3 projects (API, Core, Infrastructure)
2. Set up Entity Framework Core
3. Create database models (entities)
4. Create initial migration
5. Seed database with test data

### Phase 2B: Authentication (Week 1)
6. Implement User registration endpoint
7. Implement Login endpoint
8. JWT token generation
9. Test with Postman

### Phase 2C: Service Management (Week 2)
10. Service CRUD endpoints