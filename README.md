# EasyReasy.FileStorage Repository Overview

A comprehensive .NET file storage solution providing both local and remote file storage capabilities with security features and user management.

## Overview

EasyReasy.FileStorage is a modular file storage system consisting of multiple projects that work together to provide secure, scalable file storage solutions. The system supports both local file operations and remote multi-tenant file storage with authentication.

## Projects

| Project | Description | README |
|---------|-------------|--------|
| **EasyReasy.FileStorage** | Core file system abstraction with local file operations and file watching capabilities | [README](EasyReasy.FileStorage/README.md) |
| **EasyReasy.FileStorage.Remote.Common** | Shared models and data structures for remote file storage operations | [README](EasyReasy.FileStorage.Remote.Common/README.md) |
| **EasyReasy.FileStorage.Server** | Multi-tenant file storage server with authentication and user management | [README](EasyReasy.FileStorage.Server/README.md) |
| **EasyReasy.FileStorage.Tests** | Unit tests for all components | - |

## Quick Start

### Local File Storage
```csharp
// Use the core file storage library for local operations
IFileSystem fileSystem = new LocalFileSystem("/path/to/storage");
await fileSystem.WriteFileAsTextAsync("example.txt", "Hello, World!");
```

## Dependencies
- **.NET 8.0+**: All projects target .NET 8.0

## License
MIT
