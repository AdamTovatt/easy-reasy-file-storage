# EasyReasy.FileStorage.Server

A multi-tenant file storage server with secure authentication and user management. Built with ASP.NET Core and EasyReasy.Auth for JWT-based authentication.

## Overview

This server provides a secure, multi-tenant file storage system where:
- Each tenant has isolated file storage
- Users are managed within their tenant
- Admin users can create and manage other users (within their tenant)
- All operations are secured with JWT authentication
- File storage uses a hierarchical directory structure

## System Architecture

### Multi-Tenant Structure
The system is designed around tenant isolation:
- **Tenant ID**: Extracted from `X-Tenant-ID` header during login
- **User Isolation**: Users can only access files within their tenant
- **Admin Scope**: Admins can only manage users within their own tenant

### Authentication Flow
1. **Login**: Client sends `POST /api/auth/login` with `X-Tenant-ID` header
2. **Validation**: Server validates credentials against tenant-specific user data
3. **JWT Token**: Returns token with `user_id` and `tenant_id` claims
4. **Subsequent Requests**: Use JWT token for authentication (no tenant header needed)

## File System Structure

The server creates a hierarchical file structure for storing user data and files:

```
BASE_STORAGE_PATH/
├── tenant1/
│   ├── user1/
│   │   ├── user.json          # User data (password hash, admin status, storage limits)
│   │   └── files/             # User's file storage directory
│   │       ├── document1.pdf
│   │       ├── images/
│   │       │   └── photo.jpg
│   │       └── documents/
│   │           └── report.docx
│   ├── user2/
│   │   ├── user.json
│   │   └── files/
│   └── admin/
│       ├── user.json          # Admin user data
│       └── files/
└── tenant2/
    ├── user1/
    │   ├── user.json
    │   └── files/
    └── admin/
        ├── user.json
        └── files/
```

### User Data Structure (`user.json`)
```json
{
  "id": "username",
  "passwordHash": "hashed-password-using-pbkdf2",
  "isAdmin": false,
  "storageLimitBytes": 1073741824
}
```

## Environment Variables

The following environment variables are required:

```bash
# Base directory for file storage
# All tenant and user data will be stored under this path
BASE_STORAGE_PATH=/path/to/storage

# JWT signing secret for authentication tokens
# Must be at least 32 characters (256 bits) for HS256 security
JWT_SIGNING_SECRET=your-super-secure-jwt-signing-secret-at-least-32-chars-long
```

## API Endpoints

### Authentication Endpoints

#### `POST /api/auth/login`
Authenticates a user and returns a JWT token.

**Headers:**
```
X-Tenant-ID: tenant1
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "user1",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-15T10:30:00.000Z"
}
```

**Response (401 Unauthorized):**
```json
{
  "error": "Invalid credentials"
}
```

### Admin Endpoints

#### `POST /api/admin/users`
Creates a new user in the admin's tenant. Requires admin role.

**Headers:**
```
Authorization: Bearer <admin-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "newuser",
  "password": "securepassword",
  "isAdmin": false,
  "storageLimitBytes": 1073741824
}
```

**Response (201 Created):**
```json
{
  "username": "newuser",
  "tenantId": "tenant1",
  "isAdmin": false,
  "storageLimitBytes": 1073741824,
  "createdAt": "2024-01-15T10:30:00.000Z"
}
```

#### `GET /api/admin/users/{username}`
Retrieves user information from the admin's tenant. Requires admin role.

**Headers:**
```
Authorization: Bearer <admin-jwt-token>
```

**Response (200 OK):**
```json
{
  "username": "user1",
  "tenantId": "tenant1",
  "isAdmin": false,
  "storageLimitBytes": 1073741824,
  "hasPassword": true
}
```

## User Management

### User Types
- **Regular Users**: Can access their own files, no admin privileges
- **Admin Users**: Can create and manage other users within their tenant

### User Properties
- **Username**: Unique identifier within the tenant
- **Password**: Securely hashed using PBKDF2 with HMAC-SHA512
- **IsAdmin**: Boolean flag for admin privileges
- **StorageLimitBytes**: Maximum storage allocation (default: 1GB)

### Role Assignment
- **Regular Users**: Assigned `"user"` role
- **Admin Users**: Assigned both `"admin"` and `"user"` roles

## Security Features

### Password Security
- Uses PBKDF2 with HMAC-SHA512 and 100,000 iterations
- Username as additional salt for extra security
- Constant-time comparison to prevent timing attacks

### Tenant Isolation
- Complete file system isolation between tenants
- Users cannot access files outside their tenant
- Admin operations are scoped to the admin's tenant

### JWT Security
- Tokens include both `user_id` and `tenant_id` claims
- 1-hour expiration by default
- Cryptographically signed with secure secret

## Development

### Prerequisites
- .NET 8.0 SDK
- EasyReasy.Auth NuGet package
- Microsoft.IdentityModel.JsonWebTokens

### Running the Server
```bash
# Set environment variables
export BASE_STORAGE_PATH=/path/to/storage
export JWT_SIGNING_SECRET=your-super-secure-jwt-signing-secret-at-least-32-chars-long

# Run the server
dotnet run
```

### CLI Commands

The application can also be used as a command-line tool for administrative tasks:

#### Create Tenant
```bash
# Create a new tenant
EasyReasy.FileStorage.Server create-tenant --name="tenant01"
```

#### Create User
```bash
# Create a regular user
EasyReasy.FileStorage.Server create-user --tenant="tenant01" --name="user1" --password="password123"

# Create an admin user
EasyReasy.FileStorage.Server create-user --tenant="tenant01" --name="admin" --password="admin123" --isAdmin="true"

# Create user with custom storage limit
EasyReasy.FileStorage.Server create-user --tenant="tenant01" --name="user2" --password="password456" --storageLimit="10gb"

# Using short options
EasyReasy.FileStorage.Server create-user -t="tenant01" -n="user3" -p="password789" -s="5gb"
```

#### Storage Limit Formats
The `--storageLimit` parameter accepts various formats:
- `10gb` - 10 gigabytes
- `500mb` - 500 megabytes  
- `100kb` - 100 kilobytes
- `1024` - 1024 bytes
- Default: `1gb` if not specified

### Testing
```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "FullyQualifiedName~FileSystemUserServiceTests"
```

## File Storage Implementation

The server uses a file system-based approach for simplicity and reliability:
- **No Database Required**: All data stored in files
- **Easy Backup**: Simple file system backup
- **Portable**: Can be moved between systems easily
- **Scalable**: Can be mounted on different storage systems

### Data Persistence
- **User Data**: Stored in `user.json` files
- **File Storage**: Direct file system access
- **Tenant Structure**: Hierarchical directory organization

## Future Enhancements

- **File Upload/Download Endpoints**: For actual file operations
- **Storage Quota Enforcement**: Based on `StorageLimitBytes`
- **User Management**: Update/delete user endpoints
- **Audit Logging**: Track file access and user operations
- **Backup/Restore**: Automated backup procedures 