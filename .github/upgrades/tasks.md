# DBMigrator.Aspire.Example: Upgrade to .NET 10.0 and Aspire 13.0.0

## Overview

Upgrade DBMigrator.Aspire.Example, DBMigrator.Aspire.Hosting, and DBMigrator.Aspire.Tests projects from .NET 9.0/Aspire 9.0.0 to .NET 10.0/Aspire 13.0.0, including package updates, breaking changes review, build and test verification. Tasks are batched for maximum consolidation (single upgrade task for all projects, separate testing and commit tasks) per standard upgrade strategy.

**Progress**: 0/4 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

## Tasks

### [ ] TASK-001: Upgrade all projects to .NET 10.0 and Aspire 13.0.0
**References**: Plan §1, §2, §3, §4; Assessment #ProjectDetails

- [ ] (1) Update TargetFramework to net10.0 in:
      - DBMigrator.Aspire.Hosting\DBMigrator.Aspire.Hosting.csproj
      - Examples\DBMigrator.Aspire.Example\DBMigrator.Aspire.Example.csproj
      - DBMigrator.Aspire.Tests\DBMigrator.Aspire.Tests.csproj
      (see Plan §1.1, §2.1, §3.1)
- [ ] (2) Update Aspire package references per Plan §1.2, §2.2, §3.2:
      - Replace Aspire.Hosting with Aspire.Hosting.AppHost 13.0.0
      - Update Aspire.Hosting.SqlServer to 13.0.0
      - Update Aspire.Hosting.Testing to 13.0.0
      (reference plan for details)
- [ ] (3) Review and update code for breaking changes per Plan §4.1, §4.2:
      - Check for API changes and lifecycle hook compatibility
      - Update files listed in Plan §4.2 as needed
- [ ] (4) Restore NuGet packages for all three projects
- [ ] (5) Build all three projects and fix any compilation errors found
- [ ] (6) All projects build successfully with 0 errors (**Verify**)
- [ ] (7) Commit changes with message: "TASK-001: Upgrade projects to .NET 10.0 and Aspire 13.0.0"
- [ ] (8) Changes committed successfully (**Verify**)

### [ ] TASK-002: Run and verify test suite
**References**: Plan §5.2, §3; Assessment #ProjectDetails

- [ ] (1) Run all tests in DBMigrator.Aspire.Tests project
- [ ] (2) Fix any test failures resulting from upgrade (reference Plan §4 for common issues)
- [ ] (3) Re-run tests after fixes
- [ ] (4) All tests passed with 0 failures (**Verify**)
- [ ] (5) Commit test fixes with message: "TASK-002: Fix tests after Aspire/.NET 10 upgrade"
- [ ] (6) Changes committed successfully (**Verify**)

### [ ] TASK-003: Final review and clean-up
**References**: Plan §7; Assessment #RiskAssessment

- [ ] (1) Confirm no deprecated package warnings in build output
- [ ] (2) Confirm all success criteria in Plan §7 are met (**Verify**)
- [ ] (3) Commit any final clean-up changes with message: "TASK-003: Finalize Aspire/.NET 10 upgrade"
- [ ] (4) Changes committed successfully (**Verify**)

### [ ] TASK-004: Rollback (if required)
**References**: Plan §6

- [ ] (1) If upgrade fails, follow rollback steps in Plan §6:
      - Checkout previous branch: `git checkout feature/aspire_integration`
      - Delete upgrade branch: `git branch -D upgrade-aspire-to-net10`
- [ ] (2) Projects restored to .NET 9.0 / Aspire 9.0.0 (**Verify**)