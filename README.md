        
# Formulatrix Homework

![C#](https://img.shields.io/badge/c%23-%23239120.svg?logo=c-sharp&logoColor=white&style=for-the-badge)![.NET](https://img.shields.io/badge/.NET-5C2D91?logo=.net&logoColor=white&style=for-the-badge)

A collection of C# programming assignments demonstrating various software development concepts and best practices. This repository contains three distinct projects, each showcasing different aspects of software engineering.

## Projects Overview

### [CameraFrameProcessor](https://github.com/salcad/FormulatrixHomework/tree/main/CameraFrameProcessor) 

A real-time camera frame processing system that calculates average pixel values and provides performance monitoring.

**Key Features:**
- Real-time frame processing with thread safety
- Memory management for unmanaged pointers
- Frame rate monitoring and performance statistics
- Camera simulation for testing and demonstration

**Technologies:** C#, .NET Framework, ThreadPool

### [CodingCompetency](https://github.com/salcad/FormulatrixHomework/tree/main/CodingCompetency) 

A flexible C# console application implementing a customizable number generator with rule-based output substitution.

**Key Features:**
- Flexible rule system with `AddRule(divisor, output)` method
- Priority handling for exact matches and divisibility rules
- Multiple rule combinations with automatic output concatenation
- Clean, extensible API design

**Technologies:** C#, .NET Framework

### [RepositoryManager](https://github.com/salcad/FormulatrixHomework/tree/main/RepositoryManager) 

A thread-safe .NET class library for storing and retrieving JSON or XML strings, identified by unique string keys.

**Key Features:**
- Thread-safe operations using concurrent data structures
- Built-in validation for JSON and XML content
- Extensible architecture with dependency injection support
- Comprehensive error handling and initialization control

**Technologies:** C#, .NET Framework, Newtonsoft.Json, MSTest

## Getting Started

### Prerequisites
- .NET Framework 4.7.2 or higher
- Visual Studio 2017 or later
- MSTest framework (for RepositoryManager tests)

### Building the Solution

1. Clone the repository:
```bash
git clone https://github.com/salcad/FormulatrixHomework.git
```

2. Open the solution in Visual Studio:
```bash
start FormulatrixHomework.sln
```

3. Build the entire solution:
```bash
msbuild FormulatrixHomework.sln
```

### Running Individual Projects

Each project can be run independently:

- **CameraFrameProcessor**: Real-time frame processing demonstration
- **CodingCompetency**: Console application with multiple test cases
- **RepositoryManager**: Run the test project to see the library in action

## Architecture Highlights

- **Thread Safety**: All projects demonstrate proper concurrent programming practices
- **Clean Code**: Well-structured, maintainable code with clear separation of concerns
- **Testing**: Comprehensive unit tests with high coverage
- **Performance**: Optimized implementations with performance monitoring
- **Extensibility**: Modular design with dependency injection support

## Documentation

Each project includes detailed documentation:
- Individual README files with specific implementation details
- Code comments and XML documentation
- Usage examples and test cases
- Architecture diagrams and screenshots