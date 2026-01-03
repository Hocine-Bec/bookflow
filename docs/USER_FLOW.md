# Complete User Flow - Multi-Tenant Booking System

## Architecture Overview

```
                    bookease.com
                           |
        ┌──────────────────┼──────────────────┐
        |                  |                   |
   Landing Page      Business Flow        Customer Flow
```

---

## Flow 1: Business Owner Registration & Setup

```
Step 1: Sarah visits bookflow.com
┌─────────────────────────────────────────┐
│         Welcome to BookEase             │
│   Simple Appointment Booking for        │
│        Service Businesses               │
│                                         │
│      [Start Free Trial]  [Login]       │
│                                         │
│  ✓ Easy scheduling                     │
│  ✓ Email notifications                 │
│  ✓ 24/7 booking                        │
└─────────────────────────────────────────┘
              ↓ (Clicks "Start Free Trial")

Step 2: Registration Form
┌─────────────────────────────────────────┐
│         Create Your Account             │
│                                         │
│  Business Name: [Sarah's Hair Salon]   │
│  Email:         [sarah@salon.com]      │
│  Password:      [••••••••••••]         │
│  Phone:         [+1234567890]          │
│  Description:   [Premium styling...]   │
│                                         │
│         [Create Account]                │
└─────────────────────────────────────────┘
              ↓ (Submits form)

Step 3: Account Created - Success Page
┌─────────────────────────────────────────┐
│      🎉 Welcome to BookEase!            │
│                                         │
│  Your booking page is ready:           │
│  bookease.com/sarahs-hair-salon        │
│                                         │
│  Share this link with your customers!  │
│                                         │
│  Next steps:                           │
│  1. Add your services                  │
│  2. Set your working hours             │
│                                         │
│     [Go to Dashboard]                  │
└─────────────────────────────────────────┘
              ↓ (Clicks "Go to Dashboard")

Step 4: Business Dashboard (Empty State)
┌─────────────────────────────────────────┐
│  BookEase | Dashboard                   │
│  ──────────────────────────────────     │
│  Sarah's Hair Salon                     │
│  bookease.com/sarahs-hair-salon        │
│                                         │
│  📋 Services (0)  |  📅 Appointments    │
│  ⏰ Hours        |  ⚙️  Settings        │
│                                         │
│  Get Started:                          │
│  → Add your first service              │
│  → Set your working hours              │
└─────────────────────────────────────────┘
              ↓ (Adds services and hours)

Step 5: Dashboard After Setup
┌─────────────────────────────────────────┐
│  BookEase | Dashboard                   │
│  ──────────────────────────────────     │
│  Sarah's Hair Salon                     │
│  bookease.com/sarahs-hair-salon  [Copy] │
│                                         │
│  📋 Services (2)                        │
│  ├─ Women's Haircut ($50, 60min)       │
│  └─ Hair Coloring ($150, 120min)       │
│                                         │
│  ⏰ Working Hours                       │
│  Mon-Fri: 9:00 AM - 5:00 PM            │
│  Sat: 10:00 AM - 3:00 PM               │
│  Sun: Closed                           │
│                                         │
│  📅 Upcoming Appointments (0)           │
│  No appointments yet. Share your       │
│  booking link to get started!          │
└─────────────────────────────────────────┘
```

---

## Flow 2: Customer Booking Journey

```
Step 1: Customer Discovers Business
Sarah shares her link on:
- Instagram bio: "Book online: bookease.com/sarahs-hair-salon"
- Business card with QR code
- Facebook post
- Email signature

Mike (customer) clicks the link or types URL

Step 2: Public Booking Page
bookease.com/sarahs-hair-salon
┌─────────────────────────────────────────┐
│      🪮 Sarah's Hair Salon              │
│      Premium styling services           │
│      📞 +1234567890                     │
│                                         │
│  Available Services                    │
│  ┌─────────────────────────────────┐  │
│  │ 💇 Women's Haircut              │  │
│  │ Professional cut and style      │  │
│  │ 60 minutes • $50.00            │  │
│  │         [Book Now] ────────────┐│  │
│  └────────────────────────────────┼┘  │
│  ┌─────────────────────────────────┼┐ │
│  │ 🎨 Hair Coloring                ││ │
│  │ Full color treatment            ││ │
│  │ 120 minutes • $150.00          ││ │
│  │         [Book Now]              ││ │
│  └─────────────────────────────────┘│ │
└─────────────────────────────────────┼──┘
                                      │
              ↓ (Clicks "Book Now")  │
                                      │
Step 3: Select Date & Time            │
bookease.com/sarahs-hair-salon/book/◄─┘
service-id-123
┌─────────────────────────────────────────┐
│  Book: Women's Haircut                  │
│  Duration: 60 minutes | Price: $50     │
│                                         │
│  Select Date:                          │
│  [📅 Calendar Widget]                  │
│  Selected: January 15, 2025            │
│                                         │
│  Available Times:                      │
│  ┌──────┐ ┌──────┐ ┌──────┐          │
│  │ 9:00 │ │10:00 │ │11:00 │  ← Available│
│  └──────┘ └──────┘ └──────┘          │
│  ┌──────┐          ┌──────┐          │
│  │ 1:00 │ [Booked] │ 3:00 │          │
│  └──────┘          └──────┘          │
│                                         │
│         [Continue] ────────────────┐   │
└────────────────────────────────────┼───┘
                                     │
              ↓ (Selects 10:00 AM)  │
                                     │
Step 4: Enter Details                │
┌────────────────────────────────────▼───┐
│  Confirm Your Appointment              │
│                                         │
│  Women's Haircut                       │
│  Wednesday, January 15, 2025           │
│  10:00 AM - 11:00 AM                   │
│                                         │
│  Your Information:                     │
│  Full Name:  [John Doe          ]     │
│  Email:      [john@example.com  ]     │
│  Phone:      [+1234567890       ]     │
│  Notes:      [First time        ]     │
│              [customer          ]     │
│                                         │
│  ☐ Send me booking reminders           │
│                                         │
│         [Confirm Booking]              │
└─────────────────────────────────────────┘
              ↓ (Submits form)

Step 5: Confirmation Page
┌─────────────────────────────────────────┐
│      ✅ Booking Confirmed!              │
│                                         │
│  Confirmation #: BK-20250115-ABC       │
│                                         │
│  Women's Haircut                       │
│  Sarah's Hair Salon                    │
│  📍 Downtown Location                   │
│  📅 Wed, Jan 15, 2025                  │
│  ⏰ 10:00 AM - 11:00 AM                │
│                                         │
│  📧 Confirmation sent to:              │
│  john@example.com                      │
│                                         │
│  Need to cancel or reschedule?        │
│  Check your email for the link         │
│                                         │
│  📞 Questions? Call +1234567890        │
└─────────────────────────────────────────┘

Step 6: Confirmation Email (Sent to Customer)
┌─────────────────────────────────────────┐
│  From: bookease.com                     │
│  To: john@example.com                  │
│  Subject: Booking Confirmed - Sarah's  │
│                                         │
│  Hi John,                              │
│                                         │
│  Your appointment is confirmed!        │
│                                         │
│  Service: Women's Haircut              │
│  Business: Sarah's Hair Salon          │
│  When: Wed, Jan 15, 2025 at 10:00 AM  │
│  Duration: 60 minutes                  │
│                                         │
│  Confirmation #: BK-20250115-ABC       │
│                                         │
│  [View Appointment]                    │
│  [Cancel Appointment]                  │
│                                         │
│  See you soon!                         │
└─────────────────────────────────────────┘
```

---

## Flow 3: Business Owner Receives Booking

```
Email Notification to Sarah:
┌─────────────────────────────────────────┐
│  From: bookease.com                     │
│  To: sarah@salon.com                   │
│  Subject: New Booking - John Doe        │
│                                         │
│  You have a new appointment!           │
│                                         │
│  Customer: John Doe                    │
│  Service: Women's Haircut              │
│  When: Wed, Jan 15, 2025 at 10:00 AM  │
│  Phone: +1234567890                    │
│  Email: john@example.com               │
│  Notes: First time customer            │
│                                         │
│  [View in Dashboard]                   │
└─────────────────────────────────────────┘

Sarah's Dashboard (Updated):
┌─────────────────────────────────────────┐
│  BookEase | Dashboard                   │
│  ──────────────────────────────────     │
│                                         │
│  📅 Upcoming Appointments (1)           │
│                                         │
│  Today - January 15, 2025              │
│  ┌─────────────────────────────────┐  │
│  │ 10:00 AM - 11:00 AM             │  │
│  │ Women's Haircut                 │  │
│  │ John Doe                        │  │
│  │ 📞 +1234567890                  │  │
│  │ 📧 john@example.com             │  │
│  │ "First time customer"           │  │
│  │                                 │  │
│  │ [Mark Completed] [Cancel]       │  │
│  └─────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

---

## Flow 4: Customer Cancels Appointment

```
Step 1: Customer clicks "Cancel" link in email
bookease.com/appointments/cancel?token=abc123

┌─────────────────────────────────────────┐
│      Cancel Appointment?                │
│                                         │
│  Women's Haircut                       │
│  Sarah's Hair Salon                    │
│  Wed, Jan 15, 2025 at 10:00 AM        │
│                                         │
│  Are you sure you want to cancel?     │
│                                         │
│  [Yes, Cancel]  [Go Back]              │
└─────────────────────────────────────────┘
              ↓ (Clicks "Yes, Cancel")

Step 2: Cancellation Confirmed
┌─────────────────────────────────────────┐
│      Appointment Cancelled              │
│                                         │
│  Your appointment has been cancelled.  │
│                                         │
│  Confirmation emails have been sent.   │
│                                         │
│  Need to book again?                   │
│  [Visit Sarah's Booking Page]          │
└─────────────────────────────────────────┘

Both Sarah and John receive cancellation emails.
The 10:00 AM time slot becomes available again.
```

---

## Key Technical Points

### URL Structure Summary

| URL | Who Sees It | Purpose |
|-----|-------------|---------|
| `bookease.com` | Everyone | Landing/marketing page |
| `bookease.com/register` | New businesses | Sign up form |
| `bookease.com/login` | Business owners | Login form |
| `bookease.com/{slug}` | Customers | Public booking page |
| `bookease.com/{slug}/book/{serviceId}` | Customers | Booking flow |
| `bookease.com/dashboard` | Business owners (auth) | Management panel |
| `bookease.com/appointments/cancel?token=xxx` | Customers | Cancel via email link |