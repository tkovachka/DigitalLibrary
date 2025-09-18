# 📚 Digital Library System

A cloud-hosted web application for managing a digital library, developed in **ASP.NET Core MVC** with **Onion architecture**, and deployed on **Microsoft Azure**. The system enables administrators to manage the library catalog (CRUD operations and imports from the Google Books API), while users can borrow, return, or reserve books. All automation for loans and reservations is handled by **Azure Functions**, with **SendGrid** for notifications.

---

## ✅ Project Features

| Requirement | Status | Description |
|------------|--------|-------------|
| Onion Architecture | ✅ | Domain, Repository, Service, and Web layers are clearly separated. |
| Minimum 4 Models | ✅ | `Author`, `Publisher`, `Category`, `Book`, `Loan`, `Reservation`, `LibraryUser`. |
| CRUD Operations | ✅ | Admins can perform full CRUD on all models. Users can only loan or reserve books. |
| Extra Action | ✅ | Custom actions for `LoanBook` and `ReserveBook` include queueing logic, activation windows, and expiration rules. |
| External API Integration | ✅ | Admins can import books from the Google Books API directly into the system. |
| Hosting on Cloud | ✅ | Deployed on Azure using App Service, Cosmos DB (free tier), Azure Functions, and SendGrid. |

---

## 🧱 Architecture Overview

- **Domain Layer** – Core entities and business rules  
- **Repository Layer** – Data access (EF Core + Cosmos DB), API clients  
- **Service Layer** – Use cases (e.g. LoanBook, ReserveBook, ImportBooks)  
- **Web Layer** – ASP.NET Core MVC frontend with in-app authentication  

---

## 🧑‍💻 User Experience Flow

### Admin Flow
1. On first application run, if no admin exists in the database, the system prompts the user to create an **admin account**.  
2. The admin logs in using credentials.  
3. Admin can:
   - Manage books manually with CRUD operations.  
   - Import books via the Google Books API.  
   - Batch delete all models (system reset option).  

### User Flow
1. A visitor can browse all models (books, authors, categories, publishers) without logging in.  
2. To borrow or reserve books, the visitor must **register or log in** as a user.  
3. **Loan flow**:  
   - User can loan a book if it is available.  
   - Once finished, the user returns the book, no later than 14 days after loaning, otherwise the loan expires.  
4. **Reservation flow**:  
   - If a book is already loaned, users can reserve it.  
   - Reservations form a queue (FIFO). Each user can see their position.  
   - When a loaned book is returned, the first user in the reservation queue is notified:  
     - Their reservation is activated and the book is reserved for them for **3 days**.  
     - If they do not borrow within 3 days, the reservation expires and the next in line is notified.  
   - When a user borrows a book via reservation, their reservation is automatically deleted.  
   - Reservations are not stored long-term; only loans remain recorded.  
5. Users can **return loans** or **cancel reservations** (even active ones) at any time.  

---

## ☁ Azure Integration

| Component | Service |
|----------|---------|
| Web Application | Azure App Service |
| Database | Azure Cosmos DB (free tier) |
| Email Notifications | Azure SendGrid |
| Background Processing | Azure Functions (timer-triggered) |
| Event Notifications | Azure Event Grid (free tier) |

---

## 🔁 Automation Logic

- **Timer-triggered Azure Function** runs to update the state of loans and reservations.
  - Expire loans if 14 days are up.
  - Expire reservations after 3 days if not used.
- **Event Grid (free tier)** triggers email notifications when:  
  - A reservation becomes active.  
  - A user’s reservation has expired.  

---

## 🔐 Authentication

- In-app authentication with **two roles**:  
  - **Admin**: Full CRUD operations, book imports, batch deletes.  
  - **User**: Can borrow, return, or reserve books.  

---
