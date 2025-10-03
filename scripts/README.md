Got it ✅
Here’s a **README.md** tailored for this PowerShell script that explains clearly how to use it to rename/clone a .NET solution template into a new project.

---

# 🛠 Project Cloner & Renamer Script

This PowerShell script automates the process of creating a new .NET project from an existing template solution.
It **copies the template**, **renames the solution**, **updates namespaces**, and **fixes references** — all in one go.

---

## 📌 Features

* Copy an existing solution as a base.
* Rename solution files, folders, and references.
* Update project namespaces and using statements.
* Update `AssemblyInfo.cs` if provided.
* Works with `.sln`, `.csproj`, and `.cs` files.

---

## 🚀 Usage

### 1. Open PowerShell

Run the script from the root folder where your **source solution** is located.

### 2. Parameters

| Parameter                            | Alias   | Description                                                                        | Default   |
| ------------------------------------ | ------- | ---------------------------------------------------------------------------------- | --------- |
| `-NomProjetSource`                   | `-sp`   | Name of the existing source project (template).                                    | `Minerva` |
| `-NomProjetCible`                    | `-tp`   | Name of the new target project.                                                    | `Franz`   |
| `-RepertoireRacineSortieProjetCible` | `-odtp` | Output directory for the new project (leave empty to place it next to the source). | `""`      |
| `-CheminRelatifVersAssemblyInfo`     | `-ai`   | Relative path to `AssemblyInfo.cs` (empty string = skip).                          | `""`      |

---

### 3. Example Commands

**Basic usage (clone template `Minerva` into `Franz` in the same folder):**

```powershell
.\Clone-Project.ps1 -sp Minerva -tp Franz
```

**Clone into a different directory:**

```powershell
.\Clone-Project.ps1 -sp Minerva -tp Franz -odtp "..\NewProjects\"
```

**Clone and also update AssemblyInfo.cs:**

```powershell
.\Clone-Project.ps1 -sp Minerva -tp Franz -ai ".\Properties\AssemblyInfo.cs"
```

---

## ⚠️ Notes

* The script excludes `.git` and `scripts` folders when copying the base solution.
* If the **source solution file** cannot be found, the script will exit with an error.
* Namespace and using statements will be replaced everywhere in `.cs` files.

---

## ✅ Output

When executed successfully, you’ll see logs like:

```
=====---- Starting configuration of your template.
---------.... Copying if target is different from source. - OK
---------.... Renaming files and directories in the new solution: Minerva to Franz. - OK
---------.... Updating references in the target solution file: Franz.sln. - OK
---------.... Renaming assemblies and root namespaces in target: ..\Franz. - OK
---------.... Renaming namespaces and usings in cs files: ..\Franz. - OK
---------.... Updating AssemblyInfo if it exists: ..\Franz. - OK
=====---- Template configuration completed.
```

Your new project is ready to use 🎉

---

👉 Do you want me to also make a **French version of this README.md** so you can switch depending on the repo’s target audience?
