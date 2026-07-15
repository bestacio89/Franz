# ServiceCloner

A PowerShell utility for creating new .NET services from a stable template repository.

ServiceCloner does **not modify the original template**. Instead, it creates a new project instance from the template, updates the project identity, cleans generated artifacts, and prepares the repository for independent development.

The intended workflow is:

```
Franz (Template Repository)
        |
        +---- HeroService
        |
        +---- UserService
        |
        +---- InventoryService
        |
        +---- Any future service
```

Each generated service becomes its own Git repository while preserving the architectural standards, structure, and conventions of the template.

---

# Why ServiceCloner Exists

Large .NET ecosystems often suffer from service creation drift:

* inconsistent folder structures
* missing architectural layers
* different dependency registration patterns
* duplicated setup work
* forgotten configuration files
* inconsistent CI/CD foundations

ServiceCloner solves this by making the template repository the single source of truth.

A new service starts from a proven architecture instead of being assembled manually.

---

# Features

## Solution cloning

Creates a complete copy of the template solution.

Included:

* solution files
* projects
* source code
* configuration files
* documentation
* build configuration

Excluded:

* `.git`
* `bin`
* `obj`
* generated build artifacts

---

## Project identity replacement

Updates the generated project name across:

* solution files
* `.csproj` files
* assembly names
* root namespaces
* project references

Example:

```
Template:
Franz.Domain
Franz.Application
Franz.Persistence

Generated:
HeroService.Domain
HeroService.Application
HeroService.Persistence
```

---

## Namespace migration

Updates C# namespaces and using statements safely.

Example:

Before:

```csharp
namespace Franz.Domain.Users;
```

After:

```csharp
namespace HeroService.Domain.Users;
```

The original template remains untouched.

---

## Repository isolation

The generated project is intended to become an independent repository.

Typical workflow:

```powershell
ServiceCloner.ps1 -Source Franz -Target HeroService
```

Then:

```powershell
cd HeroService

git init
git remote add origin <repository-url>
git add .
git commit -m "Initial service creation"
git push -u origin main
```

The template and generated service now evolve independently.

---

# Installation

Requirements:

* PowerShell 7+
* .NET SDK installed
* Git installed

The script should be executed from the template repository.

Example:

```
Franz/
 |
 ├── scripts/
 │    └── ServiceCloner.ps1
 |
 ├── Franz.slnx
 |
 └── src/
```

Run:

```powershell
./ServiceCloner.ps1
```

---

# Parameters

| Parameter               | Description                           | Default          |
| ----------------------- | ------------------------------------- | ---------------- |
| `SourceProjectName`     | Template project name                 | Current template |
| `TargetProjectName`     | Generated service name                | Required         |
| `TargetOutputDirectory` | Where the new service is created      | Parent directory |
| `DryRun`                | Preview changes without writing files | Disabled         |

Example:

```powershell
./ServiceCloner.ps1 `
    -SourceProjectName Franz `
    -TargetProjectName HeroService
```

Creates:

```
../HeroService
```

without changing:

```
../Franz
```

---

# Recommended Workflow

## 1. Maintain the template

The template repository should contain:

* architecture foundations
* common project structure
* dependency conventions
* CI/CD configuration
* testing setup
* coding standards

Example:

```
Franz
 ├── Domain
 ├── Application
 ├── Infrastructure
 ├── Persistence
 ├── API
 └── Tests
```

---

## 2. Generate a service

Run ServiceCloner:

```powershell
./ServiceCloner.ps1 `
    -TargetProjectName UserService
```

---

## 3. Connect the new repository

Create an empty repository in your Git provider.

Then:

```powershell
cd ../UserService

git init
git remote add origin <remote-url>

git add .
git commit -m "Initial UserService creation"

git push -u origin main
```

---

# Design Principles

## Template first

The template repository is the architectural reference.

Changes that improve all services should happen in the template first.

---

## No destructive operations

ServiceCloner never:

* renames the original repository
* modifies the source template
* deletes source files
* rewrites Git history

It only creates a new project.

---

## Repeatable service creation

A new service should be predictable.

Given:

```
Template + Service Name
```

the result should always be:

```
Consistent Service Repository
```

---

# Example Generated Structure

After cloning:

```
HeroService
 |
 ├── HeroService.slnx
 |
 ├── src
 │    ├── HeroService.Domain
 │    ├── HeroService.Application
 │    ├── HeroService.Infrastructure
 │    ├── HeroService.Persistence
 │    └── HeroService.Api
 |
 ├── tests
 |
 └── README.md
```

---

# Troubleshooting

## Target already exists

ServiceCloner does not overwrite existing projects.

Choose another output directory or remove the existing target manually.

---

## Source and target are identical

The template repository cannot be cloned into itself.

Example of invalid usage:

```
Source:
Franz

Target:
Franz
```

Use a different service name.

---

## Namespace replacement issues

The replacement process is designed for .NET projects following standard namespace conventions.

Custom generated code or external generated files may require manual review.

---

# License

Use according to the license of the template repository.

---

# Summary

ServiceCloner turns a mature .NET architecture into a reusable service factory.

Instead of repeatedly rebuilding foundations, create new services from a validated baseline and let each service evolve independently.
