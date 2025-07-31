# Music-Backend-C#

A .NET 9.0 Web API backend service for a comprehensive music learning application. This backend provides audio track management, user authentication, and file storage capabilities for the Music-TS-React frontend.

## 🎵 Features

- **User Authentication**: JWT-based authentication with secure token validation
- **Audio Track Management**: Full CRUD operations for audio tracks with metadata
- **Azure Blob Storage Integration**: Efficient audio file storage and retrieval
- **SQL Server Database**: Data persistence using Dapper ORM
- **CORS Configuration**: Support for both development and production environments
- **Swagger Documentation**: Interactive API documentation
- **Environment Configuration**: Secure configuration management with .env files

## 🏗️ Architecture

- **Framework**: ASP.NET Core 9.0
- **Database**: SQL Server with Dapper ORM
- **File Storage**: Azure Blob Storage
- **Authentication**: JWT Bearer tokens
- **Documentation**: Swagger/OpenAPI

## 📋 Prerequisites

- .NET 9.0 SDK
- SQL Server (local or Azure)
- Azure Storage Account (for blob storage)
- Visual Studio 2022 or VS Code

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Music-Backend-C#
```

### 2. Configuration

Create a `.env` file in the root directory with the following variables:

```env
# Database Connection
DB_CONNECTION_STRING=your_sql_server_connection_string

# Azure Blob Storage
AZURE_STORAGE_CONNECTION_STRING=your_azure_storage_connection_string
AZURE_STORAGE_CONTAINER_NAME=your_container_name

# JWT Configuration
JWT_TOKEN_KEY=your_secret_token_key_at_least_32_characters_long

# CORS Origins
ALLOWED_ORIGINS=https://cgmusic.netlify.app,http://localhost:5173
```

### 3. Database Setup

Run the SQL scripts in the root directory to set up your database:

```sql
-- Run update_table.sql and z_blob_storage.sql
```

### 4. Install Dependencies

```bash
dotnet restore
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at `https://localhost:7000` (or the configured port).

## 📚 API Documentation

### Authentication Endpoints

#### POST /Auth/Login

Authenticate a user and receive a JWT token.

**Request Body:**

```json
{
  "username": "user@example.com",
  "password": "password123"
}
```

**Response:**

```json
{
  "token": "jwt_token_here",
  "userId": 1,
  "username": "user@example.com"
}
```

#### POST /Auth/Register

Register a new user account.

**Request Body:**

```json
{
  "username": "user@example.com",
  "password": "password123"
}
```

### Audio Track Endpoints

#### GET /AudioTrack/GetAudioTracks

Retrieve all audio tracks for the authenticated user.

**Headers:** `Authorization: Bearer <jwt_token>`

**Response:**

```json
[
  {
    "audioTrackId": 1,
    "userId": 1,
    "songName": "My Song",
    "songTip": "Helpful tip",
    "songKey": "C",
    "songChords": "C G Am F",
    "songInstrument": "Guitar",
    "songDifficulty": "Beginner",
    "songArtist": "Artist Name",
    "songAlbum": "Album Name",
    "songLength": "3:45",
    "recordingQuality": "High",
    "songBlobUrl": "https://storage.blob.core.windows.net/container/file.mp3",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
]
```

#### PUT /AudioTrack/UpsertAudioTrack

Create or update an audio track.

**Headers:** `Authorization: Bearer <jwt_token>`

**Request Body:**

```json
{
  "audioTrackId": 0,
  "songName": "New Song",
  "songTip": "Practice tip",
  "songKey": "G",
  "songChords": "G C D",
  "songInstrument": "Guitar",
  "songDifficulty": "Intermediate",
  "songArtist": "Artist",
  "songAlbum": "Album",
  "songLength": "4:20",
  "recordingQuality": "Medium",
  "songData": "base64_encoded_audio_file"
}
```

#### DELETE /AudioTrack/DeleteAudioTrack/{audioTrackId}

Delete an audio track.

**Headers:** `Authorization: Bearer <jwt_token>`

### User Endpoints

#### GET /User/GetUsers

Retrieve all users (admin only).

**Headers:** `Authorization: Bearer <jwt_token>`

## 🔧 Development

### Project Structure

```
Music-Backend-C#/
├── Controllers/          # API endpoints
├── Models/              # Data models
├── Services/            # Business logic
├── Data/               # Data access layer
├── Dtos/               # Data transfer objects
├── Helpers/            # Utility classes
├── Properties/         # Project properties
└── Program.cs          # Application entry point
```

### Adding New Features

1. Create models in the `Models/` directory
2. Add DTOs in the `Dtos/` directory
3. Implement services in the `Services/` directory
4. Create controllers in the `Controllers/` directory
5. Update database schema as needed

## 🚀 Deployment

### Azure Deployment

1. Create an Azure App Service
2. Configure application settings with your environment variables
3. Deploy using Azure CLI or Visual Studio

```bash
az webapp up --name your-app-name --resource-group your-resource-group
```

### Docker Deployment

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Music-Backend-CSharp.csproj", "./"]
RUN dotnet restore "Music-Backend-CSharp.csproj"
COPY . .
RUN dotnet build "Music-Backend-CSharp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Music-Backend-CSharp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Music-Backend-CSharp.dll"]
```

## 🔒 Security

- JWT tokens are used for authentication
- Passwords are hashed before storage
- CORS is configured for specific origins
- Environment variables are used for sensitive configuration
- Azure Blob Storage provides secure file storage

## 📝 Environment Variables

| Variable                          | Description                                  | Required |
| --------------------------------- | -------------------------------------------- | -------- |
| `DB_CONNECTION_STRING`            | SQL Server connection string                 | Yes      |
| `AZURE_STORAGE_CONNECTION_STRING` | Azure Storage connection string              | Yes      |
| `AZURE_STORAGE_CONTAINER_NAME`    | Blob storage container name                  | Yes      |
| `JWT_TOKEN_KEY`                   | Secret key for JWT token signing             | Yes      |
| `ALLOWED_ORIGINS`                 | Comma-separated list of allowed CORS origins | Yes      |

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions, please open an issue in the repository or contact the development team.
