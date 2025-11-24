# Upgrade Plan: DBMigrator.Aspire.Example to .NET 10.0 and Aspire 13.0.0

## Overview

This plan covers upgrading the DBMigrator.Aspire.Example project from .NET 9.0 / Aspire 9.0.0 to .NET 10.0 / Aspire 13.0.0.

**Scope**: Single project upgrade with dependency consideration
**Target Framework**: .NET 10.0 (Preview)
**Target Aspire Version**: 13.0.0
**Branch**: upgrade-aspire-to-net10 (from feature/aspire_integration)

## Prerequisites

- [x] .NET 10.0 SDK is installed and compatible
- [x] Changes committed: d1b48dc "feat: Add Aspire integration support with lifecycle hooks and examples"
- [x] New branch created: upgrade-aspire-to-net10

## Affected Projects

Based on the analysis, the following projects need updates:

1. **Examples\DBMigrator.Aspire.Example\DBMigrator.Aspire.Example.csproj** (Primary target)
   - Update TFM: net9.0 → net10.0
   - Update Aspire.Hosting.AppHost: 9.0.0 → 13.0.0
   - Update Aspire.Hosting.SqlServer: 9.0.0 → 13.0.0

2. **DBMigrator.Aspire.Hosting\DBMigrator.Aspire.Hosting.csproj** (Dependency)
   - Update TFM: net9.0 → net10.0
   - Update Aspire.Hosting: 9.0.0 → Aspire.Hosting.AppHost 13.0.0 (package rename)
   - Update Aspire.Hosting.SqlServer: 9.0.0 → 13.0.0

3. **DBMigrator.Aspire.Tests\DBMigrator.Aspire.Tests.csproj** (Related tests)
   - Update TFM: net9.0 → net10.0
   - Update Aspire.Hosting.Testing: 9.0.0 → 13.0.0
   - Update Aspire.Hosting.SqlServer: 9.0.0 → 13.0.0

## §1. Update DBMigrator.Aspire.Hosting Project

### §1.1 Update Target Framework
**File**: DBMigrator.Aspire.Hosting\DBMigrator.Aspire.Hosting.csproj

Change:
```xml
<TargetFramework>net9.0</TargetFramework>
```
To:
```xml
<TargetFramework>net10.0</TargetFramework>
```

### §1.2 Update Aspire Packages

**Important**: Aspire.Hosting package is deprecated and replaced with Aspire.Hosting.AppHost in version 13.0.0

**File**: DBMigrator.Aspire.Hosting\DBMigrator.Aspire.Hosting.csproj

Remove:
```xml
<PackageReference Include="Aspire.Hosting" Version="9.0.0" />
```

Add:
```xml
<PackageReference Include="Aspire.Hosting.AppHost" Version="13.0.0" />
```

Update:
```xml
<PackageReference Include="Aspire.Hosting.SqlServer" Version="9.0.0" />
```
To:
```xml
<PackageReference Include="Aspire.Hosting.SqlServer" Version="13.0.0" />
```

## §2. Update DBMigrator.Aspire.Example Project

### §2.1 Update Target Framework
**File**: Examples\DBMigrator.Aspire.Example\DBMigrator.Aspire.Example.csproj

Change:
```xml
<TargetFramework>net9.0</TargetFramework>
```
To:
```xml
<TargetFramework>net10.0</TargetFramework>
```

### §2.2 Update Aspire Packages
**File**: Examples\DBMigrator.Aspire.Example\DBMigrator.Aspire.Example.csproj

Update:
```xml
<PackageReference Include="Aspire.Hosting.AppHost" Version="9.0.0" />
<PackageReference Include="Aspire.Hosting.SqlServer" Version="9.0.0" />
```
To:
```xml
<PackageReference Include="Aspire.Hosting.AppHost" Version="13.0.0" />
<PackageReference Include="Aspire.Hosting.SqlServer" Version="13.0.0" />
```

## §3. Update DBMigrator.Aspire.Tests Project

### §3.1 Update Target Framework
**File**: DBMigrator.Aspire.Tests\DBMigrator.Aspire.Tests.csproj

Change:
```xml
<TargetFramework>net9.0</TargetFramework>
```
To:
```xml
<TargetFramework>net10.0</TargetFramework>
```

### §3.2 Update Aspire Packages
**File**: DBMigrator.Aspire.Tests\DBMigrator.Aspire.Tests.csproj

Update:
```xml
<PackageReference Include="Aspire.Hosting.Testing" Version="9.0.0" />
<PackageReference Include="Aspire.Hosting.SqlServer" Version="9.0.0" />
```
To:
```xml
<PackageReference Include="Aspire.Hosting.Testing" Version="13.0.0" />
<PackageReference Include="Aspire.Hosting.SqlServer" Version="13.0.0" />
```

## §4. Code Changes Assessment

### §4.1 Breaking Changes Review

Based on Aspire 13.0.0 release notes, key changes to review:

1. **Aspire.Hosting package rename**: The `Aspire.Hosting` package has been deprecated and replaced with `Aspire.Hosting.AppHost`
2. **API compatibility**: Review if any Aspire APIs used in custom extensions have changed
3. **Lifecycle hooks**: Verify lifecycle hook implementation remains compatible

### §4.2 Files to Review for Breaking Changes

1. **DBMigrator.Aspire.Hosting\DBMigratorResourceBuilderExtensions.cs**
   - Uses `IDistributedApplicationBuilder`, `IResourceBuilder<T>` interfaces
   - Check for any API signature changes

2. **DBMigrator.Aspire.Hosting\DBMigratorLifecycleHook.cs**
   - Implements `IDistributedApplicationLifecycleHook`
   - Verify lifecycle hook contract hasn't changed

3. **DBMigrator.Aspire.Hosting\DBMigratorAnnotations.cs**
   - Custom annotations implementation
   - Check if annotation system has changed

4. **Examples\DBMigrator.Aspire.Example\Program.cs**
   - Uses `DistributedApplication.CreateBuilder`
   - Verify all extension methods remain available

## §5. Verification Steps

### §5.1 Build Verification
- Restore NuGet packages for all three projects
- Build DBMigrator.Aspire.Hosting project
- Build DBMigrator.Aspire.Example project
- Build DBMigrator.Aspire.Tests project
- Verify 0 errors

### §5.2 Test Verification
- Run tests in DBMigrator.Aspire.Tests project
- Verify all tests pass

### §5.3 Runtime Verification (Optional)
- Run DBMigrator.Aspire.Example application
- Verify SQL Server container starts
- Verify migrations execute successfully
- Check for any runtime errors or warnings

## §6. Rollback Plan

If issues are encountered:
1. Checkout previous branch: `git checkout feature/aspire_integration`
2. Delete upgrade branch: `git branch -D upgrade-aspire-to-net10`
3. Projects remain on .NET 9.0 / Aspire 9.0.0

## §7. Success Criteria

- [ ] All three projects build successfully with 0 errors
- [ ] All tests in DBMigrator.Aspire.Tests pass
- [ ] No deprecated package warnings
- [ ] Example application runs successfully
- [ ] Changes committed to upgrade-aspire-to-net10 branch

## Risk Assessment

**Low Risk Areas**:
- Target framework change (net9.0 → net10.0)
- Aspire package version updates
- SQL Server integration

**Medium Risk Areas**:
- Aspire.Hosting → Aspire.Hosting.AppHost package rename
- Potential API changes in Aspire 13.0.0
- Custom lifecycle hook compatibility

**Mitigation**:
- Sequential project updates (dependency-first)
- Build verification after each project update
- Comprehensive test suite execution
- Easy rollback via Git branch management
