# API Design (REST Endpoints)

> **Implementation Note**: This API is implemented using **ASP.NET Core 9** (C#) with Entity Framework Core. All endpoints follow RESTful conventions and return JSON responses.

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
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-15T12:00:00Z"
}
```

**Response: 400 Bad Request**
```json
{
  "errors": {
    "email": ["Email is already registered"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

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