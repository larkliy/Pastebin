# Pastebin API Clone 📋

Welcome to the Pastebin API clone! This is a comprehensive backend service built with modern .NET 10, showcasing best practices for building robust and scalable web APIs. It emulates the core functionality of services like Pastebin, allowing users to create, share, and manage text snippets (pastes).

## Features ✨

*   **👤 User Management**: Secure user registration and login with JWT-based authentication and refresh tokens.
*   **✅ Email Confirmation (new)**: Users receive a confirmation email upon registration to verify their account.
*   **🛡️ Policy-Based Authorization (new)**: Certain actions, like liking or commenting, are restricted to users with a confirmed email address.
*   **📝 Full CRUD for Pastes**: Create, read, update, and delete pastes.
*   **🔒 Privacy Control**: Create public pastes or private, password-protected pastes.
*   **👍 Likes**: Users can like and unlike pastes.
*   **💬 Commenting System**:
    *   Users can comment on pastes.
    *   Supports nested comments (replies).
    *   Users can edit and delete their own comments.
*   **🗳️ Comment Voting**: Upvote and downvote comments to rank them.
*   **📄 Pagination**: All list endpoints are paginated for efficient data retrieval.
*   **🛡️ Global Exception Handling**: Centralized and clean error handling for a better developer experience.
*   **🧪 Unit Tests**: Includes unit tests for key services to ensure reliability.

## Tech Stack 🛠️

*   **Backend**: .NET 10 / ASP.NET Core (using Minimal APIs)
*   **Database**: Entity Framework Core 9 with SQLite
*   **Authentication**: JSON Web Tokens (JWT)
*   **Password Hashing**: BCrypt.Net
*   **Email Service (new)**: MailKit for sending emails.
*   **Testing**: xUnit, Moq, FluentAssertions
*   **API Documentation**: Swagger / OpenAPI

## Getting Started 🚀

Follow these instructions to get the project up and running on your local machine.

### Prerequisites

*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   An IDE like [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
*   [EF Core Tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) (`dotnet tool install --global dotnet-ef`)

### Installation & Running

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/larkliy/Pastebin
    cd Pastebin/Pastebin
    ```

2.  **Configure your settings (improved):**
    Open `appsettings.json`. This file contains crucial settings for the database connection, JWT, email service, and application URL.

    *   **`ConnectionStrings`**: The default settings use a local SQLite database (`pastebin.db`) which will be created automatically.
    *   **`JwtSettings`**: Defaults are fine for local development, but you should use strong, unique keys for production.
    *   **`ApplicationSettings`**:
        *   `FrontendUrl`: Set this to the base URL of your frontend application. It's used for generating links in emails. The default is `https://localhost:7172`.
    *   **`EmailSettings`**: **(Important!)** To enable email confirmation, you must configure this section with your SMTP server details. For local testing, you can use a service like [Mailtrap.io](https://mailtrap.io/).
        ```json
        "EmailSettings": {
          "SmtpServer": "smtp.mailtrap.io",
          "Port": 2525,
          "SenderName": "Pastebin Clone",
          "SenderEmail": "noreply@pastebin.clone",
          "Username": "your-mailtrap-username",
          "Password": "your-mailtrap-password"
        }
        ```

3.  **Apply database migrations:**
    This will create the SQLite database and apply the schema.
    ```sh
    dotnet ef database update
    ```

4.  **Run the application:**
    ```sh
    dotnet run
    ```

5.  **Access the API:**
    The API will be running at `https://localhost:7172` and `http://localhost:5211`.
    You can explore and test all the endpoints using the Swagger UI at:
    **https://localhost:7172/swagger**

## API Endpoints 🗺️

All endpoints are prefixed with `/api`. Actions marked with a 🔒 require the user's email to be confirmed.

---

### 👤 Users API (`/users`)

| Method | Path                  | Description                                       | Auth Required |
| :----- | :-------------------- | :------------------------------------------------ | :------------ |
| `POST` | `/register`           | Creates a new user account and sends a confirmation email. | No            |
| `GET`  | `/confirm-email`      | **(new)** Confirms a user's email via token.          | No            |
| `POST` | `/login`              | Authenticates a user and returns JWT tokens.      | No            |
| `POST` | `/refresh-token`      | Refreshes the access token using a refresh token. | No            |
| `GET`  | `/`                   | Gets a paginated list of all users.               | Yes           |
| `PUT`  | `/me`                 | Updates the current authenticated user's profile. | Yes           |
| `POST` | `/logout`             | Clears the user's refresh token and its expiry.   | Yes           |
| `DELETE`| `/me`                | Deletes the current authenticated user's account. | Yes           |

---

### 📝 Pastes API (`/pastes`)
**(improved)** Creating, updating, or deleting pastes now requires a confirmed email. Viewing pastes remains open.

| Method | Path                  | Description                                                              | Auth Required |
| :----- | :-------------------- | :----------------------------------------------------------------------- | :------------ |
| `POST` | `/`                   | Creates a new paste. Authenticated users must have a confirmed email 🔒.  | Optional      |
| `GET`  | `/`                   | Gets a paginated list of all public pastes.                              | No            |
| `GET`  | `/my-pastes`          | Gets a paginated list of the current user's pastes. 🔒                     | Yes           |
| `GET`  | `/{id}`               | Gets details of a single paste. Requires password in `X-Password` header for private pastes. | No            |
| `PUT`  | `/{id}`               | Updates a paste owned by the current user. 🔒                            | Yes           |
| `DELETE`| `/{id}`              | Deletes a paste owned by the current user. 🔒                            | Yes           |

---

### 👍 Likes API (`/likes`)
**(improved)** All actions in this group require a confirmed email.

| Method | Path                  | Description                                       | Auth Required |
| :----- | :-------------------- | :------------------------------------------------ | :------------ |
| `POST` | `/paste?pasteId={id}` | Likes a specific paste. 🔒                        | Yes           |
| `DELETE`| `/paste/{pasteId}`   | Removes a like from a specific paste. 🔒          | Yes           |
| `GET`  | `/my-likes`           | Gets a paginated list of the current user's likes. 🔒 | Yes           |
| `GET`  | `/paste/{pasteId}`    | Gets a paginated list of likes for a paste.       | No            |

---

### 💬 Comments API (`/comments`)
**(improved)** Creating, updating, or deleting comments requires a confirmed email.

| Method | Path                  | Description                                       | Auth Required |
| :----- | :-------------------- | :------------------------------------------------ | :------------ |
| `POST` | `/paste/{pasteId}`    | Creates a new comment on a paste. Can be a reply. 🔒 | Yes           |
| `GET`  | `/paste/{pasteId}`    | Gets paginated top-level comments for a paste.    | No            |
| `GET`  | `/user/{userId}`      | Gets paginated comments made by a specific user.  | No            |
| `GET`  | `/{commentId}`        | Gets a single comment and its direct replies.     | No            |
| `PUT`  | `/{commentId}`        | Updates a comment owned by the current user. 🔒   | Yes           |
| `DELETE`| `/{commentId}`       | Deletes a comment owned by the current user. 🔒   | Yes           |

---

### 🗳️ Comment Votes API (`/comments/{commentId}/vote`)
**(improved)** Voting on comments requires a confirmed email.

| Method | Path                  | Description                                       | Auth Required |
| :----- | :-------------------- | :------------------------------------------------ | :------------ |
| `POST` | `/`                   | Upvotes or downvotes a comment. 🔒                | Yes           |