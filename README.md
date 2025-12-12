# [Bike Management System]

> **Note:** This project was created as a submission for a school assignment/university course . It serves as a demonstration of learning outcomes regarding full-stack web development using .NET and Blazor.

## 📖 Overview

Bike Management System is a web application designed to simulate an e-commerce bike store which handles purchasing, servicing, sales and returns. 

The application utilizes a modern tech stack, separating the backend API logic from the client-side UI to ensure a clean architecture.

## 🛠️ Tech Stack

**Backend:**
* **ASP.NET Core Web API:** Handles data processing, business logic, and database communication.
* **Entity Framework Core:** ORM for database management.

**Frontend:**
* **Blazor (WebAssembly or Server):** C# based Single Page Application framework.
* **MudBlazor:** A Material Design component library for Blazor that provides the UI/UX.

## ✨ Features

* [Feature 1: e.g., User Authentication and Authorization]
* [Feature 2: e.g., Interactive Dashboard using MudBlazor charts]
* [Feature 3: e.g., CRUD operations for data management]
* [Feature 4: e.g., Dark Mode support]

## 🚀 How to Run the Application

Follow these steps to get the project running on your local machine.

### Prerequisites

Ensure you have the following installed:
* [.NET SDK](https://dotnet.microsoft.com/download) (Version  8.0 )
* [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
* 
### Installation Steps

1.  **Clone the Repository**
    ```bash
    git clone https://github.com/mesailesh7/Bike-Management-System.git
    cd your-repo-name
    ```

2.  **Database Configuration**
    * Navigate to the /ProjectWebApp/appsettings.json .
    * Open `appsettings.json`.
    * Update the `ConnectionStrings` to point to your local SQL Server instance.
    * Run the migrations to create the database:
        ```bash
        dotnet ef database update
        ```

3.  **Running via Visual Studio (Recommended)**
    * Open the `.sln` file in Visual Studio.
    * Right-click the Solution in the Solution Explorer and select **"Set Startup Projects"**.
    * Select **"Multiple startup projects"** and set both the **Server/API** project and the **Client/Blazor** project to "Start".
    * Press `F5` or click the green "Start" button.

4.  **Running via CLI (Command Line)**
    * **Start the Backend:**
        Open a terminal in the Server/API project folder and run:
        ```bash
        dotnet run
        ```
        *Note the localhost URL (e.g., https://localhost:5001).*

    * **Start the Frontend:**
        Open a new terminal in the Client/Blazor project folder and run:
        ```bash
        dotnet run
        ```
        *If using Blazor WASM, you may only need to run the Server project if it serves the client.*


## 📄 License

This project is for educational purposes.

## 👤 Author

* **Sunny** - 

---
*Thanks for checking out my project!*
