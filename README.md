# 📚 Digital Library System

A cloud-hosted web application for managing a digital library, developed in **ASP.NET Core MVC** with **Onion architecture**, and deployed on **Microsoft Azure**. The system enables administrators to import books via the Google Books API, while users can borrow, return, or reserve books that are currently unavailable.

---

## ✅ Project Features

| Requirement | Status | Description |
|------------|--------|-------------|
| Onion Architecture | ✅ | Domain, Repository, Service, and Web layers are clearly separated. |
| Minimum 4 Models | ✅ | `Book`, `User`, `Loan`, `Reservation`. |
| CRUD Operations | ✅ | Full Create, Read, Update, Delete on all core entities. |
| Extra Action | ✅ | Custom action: `ReserveBook`, includes queueing logic and time-limited locks. |
| External API Integration | ✅ | Google Books API is used to import books into the system, with transformation of response data (rating scale, metadata cleanup, etc). |
| Hosting on Cloud | ✅ | Deployed on Azure using App Service, Azure SQL Database, Blob Storage, Azure Functions, and Azure Service Bus. |

---

## 🧱 Architecture Overview

- **Domain Layer** – Core entities and business rules
- **Repository Layer** – Data access (EF Core + Azure SQL), API clients, Blob storage
- **Service Layer** – Use cases (e.g. BorrowBook, ReserveBook)
- **Web Layer** – ASP.NET Core MVC frontend with Identity authentication

---

## 🧑‍💻 User Experience Flow

1. User registers/logs in using Microsoft identity (Azure AD).
2. Browses available books.
3. Borrows any book that is free (max 5 active loans).
4. If the book is loaned, reserves it (max 10 active reservations).
5. Upon book return, the next user in the reservation queue is notified and granted 48 hours to borrow it.
6. If they do not act within the window, the reservation expires and the next user in line is notified automatically.

---

## ☁ Azure Integration

| Component | Service |
|----------|---------|
| Web Application | Azure App Service |
| Database | Azure SQL Database |
| Book Covers | Azure Blob Storage |
| Email Notifications | Azure SendGrid |
| Background Processing | Azure Functions |
| Event-Driven Architecture | Azure Service Bus |
| Real-Time Updates | Azure SignalR Service |

---

## 🔁 Automation Logic

- **Timer-triggered Azure Function** runs daily to auto-import books from the Google Books API.
- **Event-based Functions** listen to loan and reservation events via Service Bus to handle notifications, reservation activation, and expiration.
- All infrastructure is configured for scalable, serverless execution using Azure consumption-based plans.

---

## 📦 Models

- `Book`: ISBN, title, author, description, cover, rating
- `User`: Full name, email, current loans, reservations
- `Loan`: Book ID, user ID, start/end/return dates
- `Reservation`: FIFO queue per book, user ID, timestamps, status flags

---

## 🔐 Authentication

- Uses **Microsoft Identity Platform** (Azure AD)
- Only users with a Microsoft work, school, or personal account can log in

---

## 📤 Data Migration

- Local SQL Server data is migrated to **Azure SQL** via SQL Server Management Studio
- Cover images are uploaded to **Azure Blob Storage**

---

## 💡 Notes

- Users can see their position in reservation queues and estimated wait time
- Reservations automatically expire after 48h if unused
- All concurrency is handled with EF Core row versioning to avoid race conditions
