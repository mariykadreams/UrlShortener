# TASK
The goal of this project is to develop a URL Shortener application that allows users to generate shortened versions of long URLs and navigate to the original URLs using the short links.

### ✅ Login View

Requirements:

- [x] Input fields for Login and Password
- [x] Authentication logic
- [x] Role-based authorization (Admin vs. User)

### ✅ Short URLs Table View

Requirements:

- [x] Visible to everyone (authorized and anonymous users)

- Display a table with:

     - [x] Original URL

     - [x] Shortened URL

     - [x] Action buttons (View Details, Delete – where allowed)

- “Add New URL” section:

     - [x] Only visible to authorized users

     - [x] Allows entering a long URL

-  On submit:
     - [x]  Generates a short code
     - [x] Adds the new record to the table without reloading the page
     - [x] If the URL already exists → show an error message

-  Navigation to URL Info view using correct ID

Permissions:

-  Authorized Regular Users can:
     - [x] Add new URLs
     - [x] View details of their own URLs
     - [x] Delete only their own URLs

-  Admin Users can:
     - [x] Add new URLs
     - [x] View details of any URL
     - [x] Delete any URL

-  Anonymous Users can:
     - [x] Only view the table (no access to details, add, or delete)

### ✅ Short URL Info View

Requirements:
- [x] Accessible only to authenticated users (not anonymous)

-  Display the following information:
     - [x] CreatedBy (username or user ID)
     - [x] CreatedDate
     - [x] Any other available metadata (e.g., short code, original URL)

### ✅ About View

Requirements:

- [x] Visible to everyone, including anonymous users

- [ ] Editable (only by Admin users)

- Contains:

     - [x] Description of the algorithm (e.g., hashing, Base62 encoding, collision handling)

Submit action to allow editing (only for admins)

# URL Shortener Algorithm Description
The URL Shortener Service generates a unique, fixed-length short code for each original URL. The algorithm ensures that each short URL is compact, unique, and easy to use.

### How the Algorithm Works
1. For every long URL submitted, the application generates a unique short code.
   
2. The short code is always 7 characters long.
  
3. It uses a Base62 character set: 0-9, A-Z, and a-z.
  
4. The original URL is hashed using the SHA-256 algorithm.
  
5.  A portion of the hash is converted into a Base62 string.
  
6. Length Adjustment

- If the code is longer than 7 characters, it is truncated.

- If it is shorter, it is padded with 0s.

7. If the code already exists in the database, the algorithm retries up to 5 times using a slightly different input (by appending a GUID).

### From Short Code to Full Short URL
The generated short code is just a 7-character string, for example:
```
9Alk8DE
```
To make it usable, the short code is combined with a domain name:
```
https://your-domain.com/9Alk8DE
```
During local development, it may look like:
```
https://localhost:7283/9Alk8DE
```
### Clean URL Routing in ASP.NET Core
The application configures a simple route that maps any request with a short code at the root path:

```
https://localhost:7283/9Alk8DE
```
When a request is received:

- The app extracts the short code from the URL.

- It looks up the corresponding original URL in the database.

- If found, it redirects the user to the original URL.

- If not found, it returns a 404 Not Found error.

This setup avoids longer paths like /api/urls/redirect/{code} and produces cleaner, more shareable URLs.
