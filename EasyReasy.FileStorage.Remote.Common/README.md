# EasyReasy.FileStorage.Remote.Common

[← Back to Repository Overview](../README.md)

[![NuGet](https://img.shields.io/badge/nuget-EasyReasy.FileStorage.Remote.Common-blue.svg)](https://www.nuget.org/packages/EasyReasy.FileStorage.Remote.Common)

A .NET library containing shared models and data structures for remote file storage operations.

## Overview

EasyReasy.FileStorage.Remote.Common provides shared data models and request/response structures that are used across the EasyReasy file storage system. This library contains the common types needed for user management and authentication in remote file storage scenarios.

**Key Features:**
- **User Management**: Models for user data and user creation requests
- **Shared Types**: Common data structures used across the file storage system
- **Authentication Support**: User models with password hashing support
- **Storage Limits**: Built-in support for per-user storage limits

## Quick Start

```csharp
// Create a user request
CreateUserRequest request = new CreateUserRequest(
    username: "john.doe",
    password: "securePassword123",
    isAdmin: false,
    storageLimitBytes: 2 * 1024 * 1024 * 1024 // 2GB
);

// Create a user record
User user = new User(
    id: "john.doe",
    passwordHash: "hashedPasswordHere",
    isAdmin: false,
    storageLimitBytes: 2 * 1024 * 1024 * 1024 // 2GB
);
```

## Core Concepts

### User
Represents a user in the file storage system:

```csharp
public record User(
    string Id,                    // The unique user identifier (username)
    string PasswordHash,          // The hashed password
    bool IsAdmin = false,         // Whether the user has admin privileges
    long StorageLimitBytes = 1024 * 1024 * 1024 // Default 1GB storage limit
);
```

### CreateUserRequest
Request model for creating a new user:

```csharp
public class CreateUserRequest
{
    public string Username { get; set; }           // The username for the new user
    public string Password { get; set; }           // The password for the new user
    public bool IsAdmin { get; set; }              // Whether the user should have admin privileges
    public long StorageLimitBytes { get; set; }    // The storage limit in bytes for this user
    
    public CreateUserRequest(string username, string password, bool isAdmin = false, long storageLimitBytes = 1024 * 1024 * 1024);
}
```

## Getting Started

### 1. Create a User Request

```csharp
// Basic user creation
CreateUserRequest request = new CreateUserRequest("alice", "password123");

// Admin user with custom storage limit
CreateUserRequest adminRequest = new CreateUserRequest(
    username: "admin",
    password: "adminPassword",
    isAdmin: true,
    storageLimitBytes: 10 * 1024 * 1024 * 1024 // 10GB
);
```

### 2. Work with User Records

```csharp
// Create a user record (typically after password hashing)
User user = new User(
    id: "alice",
    passwordHash: "hashedPasswordValue",
    isAdmin: false,
    storageLimitBytes: 1024 * 1024 * 1024 // 1GB
);

// Access user properties
string username = user.Id;
bool isAdmin = user.IsAdmin;
long storageLimit = user.StorageLimitBytes;
```

### 3. Storage Limit Management

```csharp
// Common storage limit constants
const long ONE_GB = 1024 * 1024 * 1024;
const long FIVE_GB = 5 * 1024 * 1024 * 1024;
const long TEN_GB = 10 * 1024 * 1024 * 1024;

// Create user with specific storage limit
CreateUserRequest request = new CreateUserRequest(
    username: "largeStorageUser",
    password: "password",
    storageLimitBytes: FIVE_GB
);
```

## Usage Patterns

### User Creation Workflow

```csharp
// 1. Create the request
CreateUserRequest request = new CreateUserRequest("newuser", "password");

// 2. Hash the password (typically done by the service layer)
string hashedPassword = HashPassword(request.Password);

// 3. Create the user record
User user = new User(
    id: request.Username,
    passwordHash: hashedPassword,
    isAdmin: request.IsAdmin,
    storageLimitBytes: request.StorageLimitBytes
);
```

### Admin User Management

```csharp
// Create admin user
CreateUserRequest adminRequest = new CreateUserRequest(
    username: "systemadmin",
    password: "adminPassword",
    isAdmin: true,
    storageLimitBytes: 50 * 1024 * 1024 * 1024 // 50GB for admin
);

// Check if user is admin
User user = GetUserFromDatabase("systemadmin");
if (user.IsAdmin)
{
    // Perform admin operations
}
```

## Data Types

### Storage Limits
Storage limits are specified in bytes as `long` values:

```csharp
// Common storage limit calculations
long oneMB = 1024 * 1024;
long oneGB = 1024 * 1024 * 1024;
long oneTB = 1024L * 1024 * 1024 * 1024; // Use L suffix for large values
```

### User Identifiers
User IDs are strings that typically represent usernames:

```csharp
// Valid user IDs
string userId1 = "john.doe";
string userId2 = "user123";
string userId3 = "admin@company.com";
```

## Dependencies

- **.NET 8.0+**: Modern .NET features and performance optimizations

## License
MIT
