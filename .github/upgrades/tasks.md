# .NET 9.0 Migration: Atomic Upgrade of DBMigrator Solution

## Overview

This scenario upgrades all four projects in the DBMigrator solution to .NET 9.0 using the **Big Bang Strategy**. All project and package updates, build, and fix operations are performed in a single atomic task, followed by automated test validation and a final commit. Manual validation steps are excluded; only automatable actions are included.

**Progress**: 0/3 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

## Tasks

### [▶] TASK-001: Verify prerequisites for .NET 9.0 migration
**References**: Plan §3.2, Plan §8.1

- [▶] (1) Verify .NET 9.0 SDK is installed (`dotnet --list-sdks`)
- [ ] (2) Check for global.json compatibility with .NET 9.0 (if present)
- [ ] (3) Prerequisites confirmed (**Verify**)

### [ ] TASK-002: Atomic upgrade of all projects and packages to .NET 9.0
**References**: Plan §2.3, Plan §4, Plan §5, Plan §6

- [ ] (1) Update all project files to target .NET 9.0 per Plan §4 (multi-target DBMigrator, net9.0 for others, net9.0-windows for DBMigratorVsix)
- [ ] (2) Convert DBMigratorVsix to SDK-style project format per Plan §4 (preserve VSIX manifest and assets)
- [ ] (3) Update all package references per Plan §5 (Microsoft.Extensions.*, System.Data.SqlClient, remove deprecated/included packages)
- [ ] (4) Restore all dependencies for the solution
- [ ] (5) Build the entire solution and fix all compilation errors per Plan §6 (address breaking changes, obsolete APIs, multi-targeting issues)
- [ ] (6) Solution builds with 0 errors (**Verify**)
- [ ] (7) Commit all changes with message: "Upgrade solution to .NET 9.0 - all projects and packages updated"
- [ ] (8) Changes committed successfully (**Verify**)

### [ ] TASK-003: Automated test validation after upgrade
**References**: Plan §8.1, Plan §4 (DBMigrator.Test)

- [ ] (1) Run all automated tests in DBMigrator.Test project
- [ ] (2) All tests pass with 0 failures (**Verify**)

---

**Note:**  
- Manual validation steps (e.g., starting applications, visual confirmation, manual VSIX testing) are excluded per strategy and automation requirements.
- All large lists (projects, packages, breaking changes) are referenced by plan section, not duplicated.
- Tasks are flat, sequential, and fully automatable.  
- Status indicators and checkboxes are on the left for all tasks and actions.