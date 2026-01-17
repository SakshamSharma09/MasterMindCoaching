# The MasterMind Coaching Classes

A comprehensive coaching institute management system built with .NET 10 and Vue.js 3.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)
![Vue.js](https://img.shields.io/badge/Vue.js-3.x-green.svg)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16.x-blue.svg)

## 📋 Overview

MasterMind Coaching Classes is a full-featured management system designed for coaching institutes. It provides tools for managing students, teachers, attendance, finances, and leads with role-based access for administrators, teachers, and parents.

## ✨ Features

### Administrator Features
- 👨‍🎓 Student Management (CRUD operations)
- 📚 Class/Section Management
- 📊 Attendance Tracking with Visual Reports
- 💰 Finance Management (Fees, Payments)
- 👨‍🏫 Teacher Management
- 📞 Lead/Inquiry Management
- 📈 Dashboard Analytics

### Teacher Features
- 👁️ View Assigned Students
- 📝 Add Chapter-wise Remarks
- 📊 Track Student Performance
- ✅ Mark Attendance

### Parent Features
- 👀 View Child's Attendance
- 📊 View Performance Reports
- 💳 Check Fee Status
- 📱 Receive Notifications

## 🛠️ Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10 (ASP.NET Core Web API) |
| Frontend | Vue.js 3 + TypeScript |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core 9 |
| State Management | Pinia |
| Styling | Tailwind CSS |
| Containerization | Docker |
| Authentication | JWT + OTP |

## 📁 Project Structure

```
MasterMindCoaching/
├── docs/                          # Documentation
├── src/
│   ├── backend/
│   │   └── MasterMind.API/        # .NET 10 Web API
│   └── frontend/
│       └── mastermind-web/        # Vue.js 3 App
├── docker/
│   ├── Dockerfile.api
│   ├── Dockerfile.web
│   └── docker-compose.yml
├── database/
│   └── scripts/
├── .github/
│   └── workflows/
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/get-started)
- [PostgreSQL 16](https://www.postgresql.org/) (or use Docker)

### Quick Start with Docker

```bash
# Clone the repository
git clone https://github.com/yourusername/mastermind-coaching-classes.git
cd mastermind-coaching-classes

# Start all services
docker-compose up -d

# Access the application
# Frontend: http://localhost:3000
# API: http://localhost:5000
# API Docs: http://localhost:5000/swagger
```

### Manual Setup

#### Backend Setup

```bash
# Navigate to backend directory
cd src/backend/MasterMind.API

# Restore packages
dotnet restore

# Update database
dotnet ef database update

# Run the API
dotnet run
```

#### Frontend Setup

```bash
# Navigate to frontend directory
cd src/frontend/mastermind-web

# Install dependencies
npm install

# Run development server
npm run dev
```

## 🔧 Configuration

### Backend Configuration

Update `appsettings.json` or use environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mastermind;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-characters",
    "Issuer": "MasterMindCoaching",
    "Audience": "MasterMindCoaching",
    "ExpiryMinutes": 60
  }
}
```

### Frontend Configuration

Create `.env` file:

```env
VITE_API_URL=http://localhost:5000/api
VITE_APP_NAME=MasterMind Coaching Classes
```

## 📚 API Documentation

Once the backend is running, access Swagger documentation at:
- Development: `http://localhost:5000/swagger`

## 🧪 Testing

```bash
# Backend tests
cd src/backend
dotnet test

# Frontend tests
cd src/frontend/mastermind-web
npm run test
```

## 🐳 Docker Commands

```bash
# Build and start all services
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

## 📊 Database Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Revert last migration
dotnet ef database update PreviousMigrationName
```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Your Name** - *Initial work*

## 🙏 Acknowledgments

- Thanks to all contributors
- Inspired by the need for efficient coaching institute management

---

Made with ❤️ for The MasterMind Coaching Classes