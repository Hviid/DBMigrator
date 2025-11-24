# .NET 9.0 Migration Plan

## 1. Executive Summary

### Scenario
Upgrade the DBMigrator solution from multiple .NET versions (.NET Framework 4.7.2, .NET 5.0, .NET 6.0/netstandard2.0) to **.NET 9.0 (Standard Term Support)**.

### Scope
- **Total Projects**: 4
- **Current State**: 
  - DBMigrator.Console: .NET Framework 4.7.2
  - DBMigrator.Test: .NET 5.0
  - DBMigrator: .NET 6.0 + netstandard2.0 (multi-targeted)
  - DBMigratorVsix: .NET Framework 4.7.2 (Classic project format)

### Target State
After migration, the solution will have:
- **DBMigrator**: net6.0;netstandard2.0;net9.0 (add net9.0 to multi-targeting)
- **DBMigrator.Console**: net9.0 (upgrade from net472)
- **DBMigrator.Test**: net9.0 (upgrade from net5.0)
- **DBMigratorVsix**: net9.0-windows (convert to SDK-style, upgrade from .NET Framework 4.7.2)

### Selected Strategy
**Big Bang Strategy** - All projects upgraded simultaneously in a single coordinated operation.

**Rationale**: 
- Small solution with only 4 projects
- Clear dependency structure (DBMigrator is the foundation library)
- Total codebase is manageable (~3,200 LOC)
- All projects need framework updates
- Packages have clear upgrade paths
- Single security vulnerability to address

### Complexity Assessment
**Medium Complexity**

**Justification**:
- One project (DBMigratorVsix) requires SDK-style conversion
- Security vulnerability in System.Data.SqlClient must be addressed
- One deprecated package (Microsoft.Extensions.CommandLineUtils) needs attention
- Multiple Microsoft.Extensions packages need coordinated upgrade (3.1.8 → 9.0.11)
- One incompatible package (Microsoft.VSSDK.BuildTools) in VSIX project
- Multi-targeting scenario for DBMigrator library

### Critical Issues
1. **Security Vulnerability** 🔒: System.Data.SqlClient 4.8.2 has known security issues - must upgrade to 4.9.0
2. **Deprecated Package**: Microsoft.Extensions.CommandLineUtils is deprecated (may need replacement or acceptance of deprecation)
3. **SDK-style Conversion Required**: DBMigratorVsix must be converted from classic to SDK-style project
4. **Incompatible Package**: Microsoft.VSSDK.BuildTools 16.7.3069 flagged as incompatible (suggests downgrade to 15.7.104)

### Recommended Approach
**Big Bang Strategy** - All projects will be upgraded simultaneously because:
- Solution is small enough to manage as a single operation
- Clear, simple dependency graph (DBMigrator → Console/Test)
- Faster overall completion time
- Reduces complexity of managing multi-targeting during transition
- All developers can adopt the new framework at once

---

## 2. Migration Strategy

### 2.1 Approach Selection

**Chosen Strategy**: Big Bang Strategy

**Justification**:
- **Project Count**: 4 projects is small enough for simultaneous upgrade
- **Codebase Size**: ~3,200 total LOC is manageable
- **Dependency Structure**: Simple and clear - DBMigrator is the foundation library with 2 dependants (Console and Test), DBMigratorVsix is independent
- **Package Updates**: All packages have clear upgrade paths (except deprecated CommandLineUtils)
- **Risk Level**: Medium complexity with one SDK conversion, but no blocking issues
- **Team Impact**: Faster completion benefits outweigh coordination overhead

### 2.2 Dependency-Based Ordering

**Critical Path**: DBMigrator → DBMigrator.Console, DBMigrator.Test

The migration will respect this order:
1. **Phase 1 (Foundation)**: DBMigrator - Must be upgraded first as it's the dependency for Console and Test projects
2. **Phase 2 (Applications)**: DBMigrator.Console, DBMigrator.Test, DBMigratorVsix - Can be upgraded in parallel after Phase 1

**Key Relationships**:
- DBMigrator.Console depends on DBMigrator
- DBMigrator.Test depends on DBMigrator
- DBMigratorVsix is independent (no project dependencies)

**No Circular Dependencies**: The dependency graph is clean and straightforward.

### 2.3 Parallel vs Sequential Execution

**Sequential**:
- Phase 1: DBMigrator (must complete first)

**Parallel** (after Phase 1):
- Phase 2: DBMigrator.Console, DBMigrator.Test, DBMigratorVsix can be upgraded simultaneously

**Reasoning**: 
- DBMigrator must be upgraded first because Console and Test depend on it
- Once DBMigrator is stable on net9.0, all dependent projects can be upgraded in parallel
- DBMigratorVsix is independent and can be done in parallel with others

### Strategy-Specific Considerations

**Big Bang Batching**:
- All project file updates happen in a single task
- All package updates happen in the same task
- Single build and fix cycle for the entire solution
- One comprehensive testing phase at the end

**Atomic Operation**:
The upgrade will be one coordinated operation:
1. Update all TargetFramework properties
2. Update all package references
3. Convert DBMigratorVsix to SDK-style
4. Restore dependencies
5. Build solution and fix all compilation errors
6. Run all tests

---

## 3. Detailed Dependency Analysis

### 3.1 Dependency Graph Summary

**Migration Waves**:

**Wave 1 (Foundation Layer)**:
- DBMigrator (net6.0;netstandard2.0 → net6.0;netstandard2.0;net9.0)
  - 0 dependencies
  - 2 dependants (blocks Console and Test)

**Wave 2 (Application Layer)**:
- DBMigrator.Console (net472 → net9.0) - depends on DBMigrator
- DBMigrator.Test (net5.0 → net9.0) - depends on DBMigrator
- DBMigratorVsix (.NET Framework 4.7.2 → net9.0-windows) - independent

### 3.2 Project Groupings

#### Phase 0: Preparation
- Verify .NET 9.0 SDK installation
- Check for global.json compatibility

#### Phase 1: Atomic Framework and Package Upgrade
All projects updated simultaneously:
- **DBMigrator** (foundation library)
- **DBMigrator.Console** (main application)
- **DBMigrator.Test** (test project)
- **DBMigratorVsix** (Visual Studio extension)

#### Phase 2: Test Validation
- Run DBMigrator.Test project
- Validate all functionality

**Strategy-Specific Grouping Notes**: 
Big Bang strategy requires all projects to be upgraded in a single atomic operation. The phases above represent logical stages within that single operation, not separate task boundaries.

---

## 4. Project-by-Project Migration Plans

### Project: DBMigrator

**Current State**:
- **Target Framework**: net6.0;netstandard2.0
- **Project Type**: ClassLibrary (SDK-style)
- **Dependencies**: 0 project dependencies
- **Dependants**: DBMigrator.Console, DBMigrator.Test
- **Package Count**: 8 packages
- **LOC**: 1,704

**Target State**:
- **Target Framework**: net6.0;netstandard2.0;net9.0 (add net9.0 to multi-targeting)
- **Updated Packages**: 7 packages need updates

**Migration Steps**:

1. **Prerequisites**
   - None - this is the foundation project with no dependencies
   - .NET 9.0 SDK must be installed

2. **Framework Update**
   - Update `DBMigrator\DBMigrator.csproj`:
     - Change `<TargetFrameworks>net6.0;netstandard2.0</TargetFrameworks>`
     - To: `<TargetFrameworks>net6.0;netstandard2.0;net9.0</TargetFrameworks>`
   - This adds net9.0 as an additional target while maintaining existing targets for backward compatibility

3. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.Extensions.DependencyInjection | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.DependencyInjection.Abstractions | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.Logging | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.Logging.Abstractions | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.Logging.Console | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.Logging.Debug | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| System.Data.SqlClient | 4.8.2 | 4.9.0 | **Security vulnerability fix** 🔒 |

**Packages to Remove**:
- System.ValueTuple 4.5.0 (functionality included in .NET 9.0 framework)

4. **Expected Breaking Changes**

- **Microsoft.Extensions.* 3.1.8 → 9.0.11**: Major version jump (6 versions)
  - Potential API changes in DependencyInjection and Logging
  - Configuration patterns may have evolved
  - Service registration methods may have new overloads or changes
  - Check for obsolete attributes on existing method calls

- **System.Data.SqlClient 4.8.2 → 4.9.0**: Minor security update
  - Low risk of breaking changes
  - Primarily a security patch

- **System.ValueTuple removal**: 
  - No code changes expected - functionality is built into modern .NET

**Areas to Review**:
- Database connection initialization (Database.cs)
- Logging configuration and usage patterns
- Dependency injection container setup

5. **Code Modifications**

Expected changes based on framework upgrade:
- **Nullable reference types**: .NET 9.0 has improved nullable analysis - may see new warnings
- **Logging**: Review ILogger usage patterns for any deprecated methods
- **DI Container**: Check ServiceCollection and ServiceProvider usage
- **SQL Client**: Verify connection string handling remains compatible

Specific files to review:
- `Database\Database.cs` (uses System.Data.SqlClient)
- Any files using ILogger or IServiceProvider

6. **Testing Strategy**

**Unit Tests**: 
- DBMigrator.Test project contains tests for this project
- All tests must pass after upgrade

**Key Scenarios**:
- Database connection and command execution
- Version tracking functionality
- Migration execution

**Manual Testing**:
- Verify database connectivity works
- Test migration script execution

7. **Validation Checklist**
- [ ] Project builds for net6.0 target without errors
- [ ] Project builds for netstandard2.0 target without errors
- [ ] Project builds for net9.0 target without errors
- [ ] No build warnings related to obsolete APIs
- [ ] System.ValueTuple package removed successfully
- [ ] Security vulnerability resolved (System.Data.SqlClient 4.9.0)
- [ ] Dependencies resolve correctly for all target frameworks
- [ ] DBMigrator.Test project can reference net9.0 target

---

### Project: DBMigrator.Console

**Current State**:
- **Target Framework**: net472
- **Project Type**: DotNetCoreApp (SDK-style)
- **Dependencies**: DBMigrator
- **Dependants**: 0
- **Package Count**: 6 packages
- **LOC**: 306

**Target State**:
- **Target Framework**: net9.0
- **Updated Packages**: 5 packages need updates, 1 deprecated package

**Migration Steps**:

1. **Prerequisites**
   - DBMigrator must be upgraded first and building successfully on net9.0
   - .NET 9.0 SDK installed

2. **Framework Update**
   - Update `DBMigrator.Console\DBMigrator.Console.csproj`:
     - Change `<TargetFramework>net472</TargetFramework>`
     - To: `<TargetFramework>net9.0</TargetFramework>`

3. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.Extensions.DependencyInjection | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.Logging | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.Logging.Console | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.Logging.Debug | 3.1.8 | 9.0.11 | Framework compatibility, major version upgrade |
| Microsoft.Extensions.CommandLineUtils | 1.1.1 | **Keep** | ⚠️ **Deprecated** - no replacement version available |
| Nerdbank.GitVersioning | 3.6.133 | **Keep** | Already compatible |

**Deprecated Package Note**:
- **Microsoft.Extensions.CommandLineUtils** is deprecated as part of the .NET Package Deprecation effort
- **Recommendation**: Keep 1.1.1 for now, plan future replacement with System.CommandLine or another alternative
- **Risk**: Low - package still functions, just no longer maintained

4. **Expected Breaking Changes**

- **Microsoft.Extensions.* 3.1.8 → 9.0.11**: 
  - Same concerns as DBMigrator project
  - DI and logging setup in Program.cs may need updates
  - Check for new patterns in host builder configuration

- **.NET Framework 4.7.2 → .NET 9.0**:
  - Configuration system changes (app.config → appsettings.json if applicable)
  - Removed .NET Framework APIs (check for any Windows-specific code)
  - Path handling changes (cross-platform considerations)

- **Microsoft.Extensions.CommandLineUtils**:
  - No breaking changes expected (keeping same version)
  - Consider planning migration to System.CommandLine in future

**Areas to Review**:
- Program.cs entry point and host configuration
- Command-line argument parsing (CommandLineUtils usage)
- File system operations (ensure cross-platform compatibility)
- Any Windows-specific API usage

5. **Code Modifications**

Expected changes:
- **Program.cs**: Review Main method, host builder setup, DI configuration
- **Command-line parsing**: Verify CommandLineUtils API usage still works
- **Logging setup**: Update logging configuration for .NET 9.0 patterns
- **File paths**: Ensure path operations work cross-platform

Specific concerns:
- Top-level statements (optional modernization)
- Async Main support (if not already using)
- Global using directives (optional modernization)

6. **Testing Strategy**

**Manual Testing Required**:
- Run console application with various command-line arguments
- Verify database migration functionality works end-to-end
- Test all command-line options

**Key Scenarios**:
- Help text display (-h, --help)
- Database connection string handling
- Migration execution commands
- Error handling and logging output

**No Automated Tests**: This project has no unit tests - manual validation is critical

7. **Validation Checklist**
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] Console application starts successfully
- [ ] Command-line argument parsing works
- [ ] Database connection succeeds
- [ ] Migration commands execute correctly
- [ ] Help text displays properly
- [ ] Error handling works as expected
- [ ] Logging output appears correctly

---

### Project: DBMigrator.Test

**Current State**:
- **Target Framework**: net5.0
- **Project Type**: DotNetCoreApp (SDK-style)
- **Dependencies**: DBMigrator
- **Dependants**: 0
- **Package Count**: 4 packages
- **LOC**: 821

**Target State**:
- **Target Framework**: net9.0
- **Updated Packages**: 1 package to remove (included in framework)

**Migration Steps**:

1. **Prerequisites**
   - DBMigrator must be upgraded and building on net9.0
   - .NET 9.0 SDK installed

2. **Framework Update**
   - Update `DBMigrator.Test\DBMigrator.Test.csproj`:
     - Change `<TargetFramework>net5.0</TargetFramework>`
     - To: `<TargetFramework>net9.0</TargetFramework>`

3. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.NET.Test.Sdk | 16.7.1 | **Keep** | Compatible with .NET 9.0 |
| MSTest.TestAdapter | 2.1.2 | **Keep** | Compatible with .NET 9.0 |
| MSTest.TestFramework | 2.1.2 | **Keep** | Compatible with .NET 9.0 |

**Packages to Remove**:
- System.Runtime.Extensions 4.3.1 (functionality included in .NET 9.0 framework)

**Optional Updates** (for latest features):
- Consider updating MSTest packages to latest versions (2.2.x or newer) for improved test features
- Consider updating Microsoft.NET.Test.Sdk to 17.x for latest tooling support

4. **Expected Breaking Changes**

- **.NET 5.0 → .NET 9.0**:
  - Minimal breaking changes expected for test code
  - Test framework APIs are stable
  - MSTest 2.1.2 is compatible but older

- **System.Runtime.Extensions removal**:
  - No code changes expected - functionality built into .NET 9.0

**Areas to Review**:
- Test initialization and cleanup methods
- Assert methods usage
- Test data attributes
- Async test methods

5. **Code Modifications**

Expected changes:
- **DatabaseTest.cs**: Review test methods for any .NET version-specific behaviors
- **LocalDB connection strings**: Verify (localdb)\mssqllocaldb still works on dev machines
- **Test attributes**: Check for any deprecated test attributes

Potential improvements:
- Consider using `Assert.ThrowsException<T>` for exception testing
- Review async test methods for proper async/await patterns
- Consider adding more test coverage

6. **Testing Strategy**

**Unit Tests**: This IS the test project
- All tests must pass after upgrade
- No new test failures should occur
- Test execution performance should be similar

**Key Test Classes**:
- DatabaseTest.cs (database operations)
- Other test files (20 files total)

**Test Scenarios**:
- `Versions_noversions_test`: Verify empty version tracking
- `Versions_one_versions_test`: Verify single version migration
- `Message_test`: Verify SQL output handling
- All other test methods across 20 files

**LocalDB Dependency**:
- Tests require SQL Server LocalDB to be installed
- Connection string: `(localdb)\mssqllocaldb`
- Ensure LocalDB is available in .NET 9.0 environment

7. **Validation Checklist**
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] All tests discovered by test runner
- [ ] All tests pass (no regressions)
- [ ] Test execution time is acceptable
- [ ] LocalDB connection works
- [ ] System.Runtime.Extensions removed successfully
- [ ] Test output window shows expected results

---

### Project: DBMigratorVsix

**Current State**:
- **Target Framework**: .NETFramework,Version=v4.7.2
- **Project Type**: ClassicWpf (⚙️ Classic project format)
- **SDK-style**: False
- **Dependencies**: 0
- **Dependants**: 0
- **Package Count**: 2 packages
- **LOC**: 367

**Target State**:
- **Target Framework**: net9.0-windows
- **Project Type**: SDK-style
- **Updated Packages**: 1 incompatible package

**Migration Steps**:

1. **Prerequisites**
   - .NET 9.0 SDK installed
   - Visual Studio 2022 with VSIX development workload
   - None - this project has no project dependencies

2. **SDK-style Conversion** ⚠️ **CRITICAL STEP**
   - This project MUST be converted from classic to SDK-style project format before framework upgrade
   - Use Visual Studio's built-in conversion tool or manual conversion
   - Steps:
     1. Back up current `DBMigratorVsix\DBMigratorVsix.csproj`
     2. Convert to SDK-style project format
     3. Ensure VSIX-specific elements are preserved
     4. Update PackageReferences to modern format

   **Conversion Considerations**:
   - VSIX projects have special build targets and imports
   - Must preserve `.vsixmanifest` file references
   - VSPackage registration attributes must remain
   - Assets like icons and VSCTs must be included correctly

3. **Framework Update**
   - After SDK conversion, update target framework:
     - From: `.NETFramework,Version=v4.7.2`
     - To: `<TargetFramework>net9.0-windows</TargetFramework>`
   - Note: `net9.0-windows` is required for Visual Studio extensions (not just `net9.0`)

4. **Package Updates**

| Package | Current Version | Target Version | Reason |
|---------|----------------|----------------|---------|
| Microsoft.VisualStudio.SDK | 16.0.206 | **Keep or update to 17.x** | Base VS SDK package |
| Microsoft.VSSDK.BuildTools | 16.7.3069 | ⚠️ **15.7.104** or **17.x** | Flagged as incompatible |

**Package Notes**:
- **Microsoft.VSSDK.BuildTools**: Analysis suggests 15.7.104, but this seems like a downgrade
  - **Recommendation**: Try keeping 16.7.3069 or upgrade to 17.x for VS 2022 compatibility
  - Visual Studio 2022 typically uses 17.x packages
  - May need to update to Microsoft.VSSDK.BuildTools 17.x for .NET 9.0 support

**Recommended Package Versions for VS 2022 + .NET 9.0**:
- Microsoft.VisualStudio.SDK: 17.10.x or later
- Microsoft.VSSDK.BuildTools: 17.10.x or later

5. **Expected Breaking Changes**

- **Classic → SDK-style conversion**:
  - Project file structure completely different
  - References handling changes
  - Build customizations need migration
  - VSIX manifest integration may need adjustments

- **.NET Framework 4.7.2 → .NET 9.0**:
  - Removed .NET Framework-only APIs
  - COM interop changes (important for VS extensions)
  - Threading model changes
  - Package loading and initialization changes

- **Visual Studio SDK 16.x → 17.x** (if upgrading):
  - Async package loading patterns
  - Updated service interfaces
  - Threading context changes (JoinableTaskFactory)
  - Updated VS SDK APIs

**High-Risk Areas**:
- VSPackage class and async initialization
- Command handlers and menu integration
- Tool window implementations
- COM interop with Visual Studio services
- Thread affinity and UI thread access

6. **Code Modifications**

Expected changes:
- **Package class**: Update to async package pattern if not already
- **Commands**: Review command handler registration
- **Services**: Update service acquisition to use modern patterns
- **UI Threading**: Ensure proper use of JoinableTaskFactory
- **Manifest**: Update VSIX manifest for VS 2022 compatibility

Specific files to review:
- VSPackage implementation class
- Command handler classes
- Tool window implementations
- `.vsixmanifest` file (prerequisite versions, supported VS versions)

**VSIX Manifest Updates**:
- Update `InstallationTarget` to include VS 2022 version ranges
- Update `Prerequisites` to match .NET 9.0 requirements
- Update supported Visual Studio versions

7. **Testing Strategy**

**Manual Testing Required** (No automated tests for VSIX):
- Install VSIX in Visual Studio 2022
- Verify extension loads without errors
- Test all menu commands
- Test tool windows
- Verify no performance degradation

**Key Scenarios**:
- Extension installation
- Extension activation
- Menu command execution
- Tool window display
- Integration with Visual Studio services

**Test Environments**:
- Visual Studio 2022 (latest version)
- Clean VS instance (experimental instance)
- Verify no conflicts with other extensions

8. **Validation Checklist**
- [ ] Project converted to SDK-style successfully
- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] VSIX package generates successfully
- [ ] VSIX installs in VS 2022
- [ ] Extension loads without errors in VS
- [ ] All commands appear in correct menus
- [ ] Commands execute correctly
- [ ] Tool windows display properly
- [ ] No exceptions in Activity Log
- [ ] Extension can be uninstalled cleanly

**Special Considerations**:
- This project has highest risk due to SDK conversion + framework + VS version changes
- Recommend testing in VS experimental instance first
- Keep backup of original working VSIX
- May require significant debugging and iteration

---

## 5. Package Update Reference

### Common Package Updates (affecting multiple projects)

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| Microsoft.Extensions.DependencyInjection | 3.1.8 | 9.0.11 | 2 (DBMigrator, DBMigrator.Console) | Framework compatibility, 6 major versions |
| Microsoft.Extensions.Logging | 3.1.8 | 9.0.11 | 2 (DBMigrator, DBMigrator.Console) | Framework compatibility, 6 major versions |
| Microsoft.Extensions.Logging.Console | 3.1.8 | 9.0.11 | 2 (DBMigrator, DBMigrator.Console) | Framework compatibility, 6 major versions |
| Microsoft.Extensions.Logging.Debug | 3.1.8 | 9.0.11 | 2 (DBMigrator, DBMigrator.Console) | Framework compatibility, 6 major versions |

### Project-Specific Updates

**DBMigrator Only**:
- Microsoft.Extensions.DependencyInjection.Abstractions: 3.1.8 → 9.0.11
- Microsoft.Extensions.Logging.Abstractions: 3.1.8 → 9.0.11
- System.Data.SqlClient: 4.8.2 → 4.9.0 (**SECURITY FIX** 🔒)

**DBMigrator.Console Only**:
- Microsoft.Extensions.CommandLineUtils: 1.1.1 (⚠️ deprecated, keep as-is)
- Nerdbank.GitVersioning: 3.6.133 (keep, compatible)

**DBMigrator.Test Only**:
- Microsoft.NET.Test.Sdk: 16.7.1 (keep, compatible)
- MSTest.TestAdapter: 2.1.2 (keep, compatible)
- MSTest.TestFramework: 2.1.2 (keep, compatible)

**DBMigratorVsix Only**:
- Microsoft.VisualStudio.SDK: 16.0.206 → Consider 17.x for VS 2022
- Microsoft.VSSDK.BuildTools: 16.7.3069 → Resolve incompatibility (try 17.x)

### Packages to Remove

| Package | Current Version | Projects | Reason |
|---------|----------------|----------|---------|
| System.ValueTuple | 4.5.0 | DBMigrator | Included in .NET 9.0 framework |
| System.Runtime.Extensions | 4.3.1 | DBMigrator.Test | Included in .NET 9.0 framework |

---

## 6. Breaking Changes Catalog

### Framework Breaking Changes

#### .NET Framework 4.7.2 → .NET 9.0 (DBMigrator.Console, DBMigratorVsix)
- **Configuration System**: app.config no longer used, migrate to appsettings.json if applicable
- **Windows-Only APIs**: Some APIs removed (check for Registry, WinForms-specific code)
- **Binary Serialization**: BinaryFormatter removed in .NET 9.0
- **Code Access Security (CAS)**: Completely removed
- **AppDomain**: Limited functionality compared to .NET Framework
- **Path Handling**: More cross-platform, may behave differently on edge cases

#### .NET 5.0 → .NET 9.0 (DBMigrator.Test)
- **Minimal impact**: Both are modern .NET, breaking changes are minor
- **Performance improvements**: Better performance, no code changes needed
- **C# 12 features**: New language features available (if targeting C# 12)

#### .NET 6.0 → .NET 9.0 (DBMigrator multi-targeting)
- **Minimal impact**: Close versions, mostly additive changes
- **TFM selection**: Ensure projects select net9.0 TFM when available

### Package Breaking Changes

#### Microsoft.Extensions.* 3.1.8 → 9.0.11

**DependencyInjection**:
- `IServiceCollection` extensions may have new overloads
- Service lifetime and scope behavior refinements
- Potential changes in service resolution order

**Logging**:
- `ILogger` generic parameter conventions
- New logging levels or categories
- Configuration binding changes
- Performance improvements that may change behavior

**Potential Code Impacts**:
```csharp
// Old pattern (may still work)
services.AddLogging(config => config.AddConsole());

// Check for new patterns in .NET 9.0 documentation
services.AddLogging(builder => 
{
    builder.AddConsole();
    builder.AddDebug();
});
```

#### System.Data.SqlClient 4.8.2 → 4.9.0
- **Low risk**: Security patch, minimal API changes
- **Connection strings**: Verify all connection string formats still work
- **Encryption**: Check for new encryption defaults (TLS 1.2+)

**Areas to verify**:
- Connection opening and closing
- Command execution
- Transaction handling
- Connection pooling behavior

#### Microsoft.Extensions.CommandLineUtils (Deprecated)
- **No immediate impact**: Package still works
- **Future risk**: No security updates or bug fixes
- **Recommendation**: Plan migration to System.CommandLine

**Potential replacements**:
- System.CommandLine (Microsoft's new CLI library)
- CommandLineParser (popular community library)
- Write custom argument parser

### Visual Studio SDK Changes (DBMigratorVsix)

#### VS SDK 16.x → 17.x (if upgrading)

**Async Patterns**:
```csharp
// Old synchronous package
public class MyPackage : Package
{
    protected override void Initialize() { }
}

// New async package (required for VS 2022)
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
public class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        // Initialization code
    }
}
```

**Service Acquisition**:
```csharp
// Old pattern
var service = GetService(typeof(SVsService)) as IVsService;

// New async pattern
var service = await GetServiceAsync(typeof(SVsService)) as IVsService;
```

**Threading**:
- Must use `JoinableTaskFactory` for UI thread access
- Avoid blocking UI thread with synchronous calls
- Use `ThreadHelper.ThrowIfNotOnUIThread()` assertions

---

## 7. Risk Management

### 7.1 High-Risk Changes

| Project | Risk | Mitigation |
|---------|------|------------|
| DBMigratorVsix | **HIGH** - SDK conversion + framework upgrade + VS version | Test in VS experimental instance; keep backup; extensive manual testing |
| DBMigrator | **MEDIUM** - Security vulnerability + multi-targeting | Address security issue immediately; test all target frameworks |
| DBMigrator.Console | **MEDIUM** - Deprecated package + major framework jump | Accept deprecation for now; plan future CommandLineUtils replacement |
| DBMigrator.Test | **LOW** - Simple framework upgrade | Run all tests; verify LocalDB compatibility |

### 7.2 Risk Assessment by Category

**Security Risks**:
- System.Data.SqlClient vulnerability must be fixed immediately
- No new vulnerabilities introduced by updated packages

**Compatibility Risks**:
- DBMigratorVsix: High risk due to VS SDK changes and SDK conversion
- Microsoft.Extensions packages: Medium risk due to 6 major version jump
- Microsoft.Extensions.CommandLineUtils: Low risk (deprecated but functional)

**Dependency Risks**:
- LocalDB may need updates for .NET 9.0 (verify on dev machines)
- Visual Studio 2022 required for DBMigratorVsix development

**Data Risks**:
- Database migrations should not be affected by framework upgrade
- Test thoroughly with actual databases

### 7.3 Contingency Plans

**If SDK Conversion Fails (DBMigratorVsix)**:
- Alternative 1: Keep project on .NET Framework 4.7.2 temporarily
- Alternative 2: Use CsprojToVs2017 or similar tools for automated conversion
- Alternative 3: Manually create new SDK-style project and migrate code incrementally

**If Tests Fail After Upgrade**:
- Revert framework changes per project
- Upgrade DBMigrator first, validate tests, then proceed
- Check for LocalDB version compatibility

**If Microsoft.Extensions Packages Cause Issues**:
- Try intermediate versions (e.g., 6.x, 7.x, 8.x before 9.x)
- Review breaking changes documentation for each major version
- Consult GitHub issues for known migration problems

**If CommandLineUtils Becomes Blocking**:
- Immediate: Keep deprecated package and continue
- Short-term: Wrap CommandLineUtils usage in abstraction
- Long-term: Migrate to System.CommandLine

**If Performance Degrades**:
- Profile application before and after
- Check for new default behaviors in logging or DI
- Review .NET 9.0 performance guidance
- Consider targeting .NET 8 LTS instead if 9.0 has issues

---

## 8. Testing and Validation Strategy

### 8.1 Phase-by-Phase Testing

#### Phase 0: Preparation
- [ ] Verify .NET 9.0 SDK installed: `dotnet --list-sdks`
- [ ] Check global.json (if present) allows .NET 9.0
- [ ] Backup current solution state
- [ ] Document current test baseline (all tests passing)

#### Phase 1: Atomic Upgrade
After all project files and packages updated:

**Build Validation**:
- [ ] Clean solution: `dotnet clean`
- [ ] Restore packages: `dotnet restore`
- [ ] Build DBMigrator for all targets (net6.0, netstandard2.0, net9.0)
- [ ] Build DBMigrator.Console (net9.0)
- [ ] Build DBMigrator.Test (net9.0)
- [ ] Build DBMigratorVsix (net9.0-windows) after SDK conversion
- [ ] Entire solution builds with 0 errors

**Compilation Error Fixes**:
- Address any breaking changes from Microsoft.Extensions packages
- Fix any obsolete API usage
- Resolve any multi-targeting issues
- Fix VSIX-specific compilation issues

**Warning Cleanup**:
- Review and address nullable reference type warnings
- Fix any obsolete API warnings
- Address any multi-targeting warnings

#### Phase 2: Test Validation
**Automated Testing**:
- [ ] Run all tests in DBMigrator.Test
- [ ] All tests pass (100% success rate)
- [ ] No new test failures
- [ ] Test execution time acceptable

**Test Project Specific**:
- [ ] LocalDB connection succeeds
- [ ] Database operations work correctly
- [ ] Version tracking functions properly
- [ ] Migration execution works

### 8.2 Smoke Tests

#### DBMigrator (Library)
- [ ] Library loads in dependent projects
- [ ] Database class instantiates correctly
- [ ] Can connect to SQL Server / LocalDB
- [ ] Logging works correctly
- [ ] DI container can resolve services

#### DBMigrator.Console (Application)
- [ ] Application starts without errors
- [ ] Help text displays: `DBMigrator.Console.exe --help`
- [ ] Can parse command-line arguments
- [ ] Logging outputs correctly to console
- [ ] Can connect to database
- [ ] Can execute migration commands

#### DBMigrator.Test (Test Project)
- [ ] Test project builds
- [ ] Test runner discovers all tests
- [ ] Tests execute and pass
- [ ] LocalDB available and working

#### DBMigratorVsix (Visual Studio Extension)
- [ ] VSIX package builds successfully
- [ ] VSIX installs in VS 2022 experimental instance
- [ ] Extension loads without errors
- [ ] Menu commands appear
- [ ] Commands execute without exceptions
- [ ] No errors in VS Activity Log

### 8.3 Comprehensive Validation

#### Functional Testing
**DBMigrator.Console**:
- Test all command-line options
- Execute database migrations on test database
- Verify logging output is correct
- Test error handling (invalid connection strings, etc.)
- Verify version tracking updates correctly

**DBMigratorVsix**:
- Install VSIX in clean VS instance
- Open a project and use extension features
- Test all menu commands
- Verify tool windows display correctly
- Check for proper async behavior (no UI freezing)

#### Performance Testing
- Compare application startup time before/after
- Measure migration execution time before/after
- Verify no performance degradation
- Check memory usage patterns

#### Security Validation
- [ ] System.Data.SqlClient updated to 4.9.0 (security fix applied)
- [ ] No new security vulnerabilities in dependencies
- [ ] Run `dotnet list package --vulnerable`
- [ ] All security warnings addressed

#### Multi-Targeting Validation (DBMigrator)
- [ ] Library builds for net6.0
- [ ] Library builds for netstandard2.0
- [ ] Library builds for net9.0
- [ ] Dependent projects can reference appropriate target
- [ ] No cross-targeting issues

---

## 9. Timeline and Effort Estimates

### 9.1 Per-Project Estimates

| Project | Complexity | Estimated Time | Dependencies | Risk Level |
|---------|------------|---------------|--------------|------------|
| DBMigrator | Medium | 2-3 hours | None (foundation) | Medium |
| DBMigrator.Console | Medium | 1-2 hours | DBMigrator | Medium |
| DBMigrator.Test | Low | 30-60 minutes | DBMigrator | Low |
| DBMigratorVsix | High | 4-6 hours | None | High |

**Note**: Big Bang strategy means all project file updates and package updates happen simultaneously in the same task.

### 9.2 Phase Durations

#### Phase 0: Preparation (30 minutes)
- SDK verification
- Solution backup
- Baseline documentation

#### Phase 1: Atomic Upgrade (4-6 hours)
- Update all project files (30 minutes)
- Update all package references (30 minutes)
- Convert DBMigratorVsix to SDK-style (1-2 hours)
- Restore and initial build (15 minutes)
- Fix compilation errors (2-3 hours)
  - Microsoft.Extensions API changes
  - VSIX SDK conversion issues
  - Multi-targeting issues
- Rebuild and verify (15 minutes)

#### Phase 2: Test Validation (2-3 hours)
- Run DBMigrator.Test (15 minutes)
- Fix any test failures (1-2 hours)
- Manual testing DBMigrator.Console (30 minutes)
- Manual testing DBMigratorVsix (1 hour)
- Final validation (30 minutes)

### Total Estimated Timeline
- **Minimum**: 6.5 hours (optimistic)
- **Expected**: 8-12 hours (realistic with some issues)
- **Maximum**: 15 hours (if major VSIX conversion issues)

**Buffer Recommendation**: Add 25% buffer = **10-15 hours total**

### 9.3 Resource Requirements

**Developer Skills**:
- Experience with .NET migrations
- Familiarity with Visual Studio extension development (for VSIX)
- Understanding of SDK-style project format
- Knowledge of Microsoft.Extensions DI and Logging
- SQL Server / LocalDB setup

**Parallel Work Capacity**:
- Big Bang approach: Single developer working through phases sequentially
- Cannot parallelize within atomic upgrade task
- Test validation can be parallelized (different developers test different components)

**Testing Resources**:
- Development machine with .NET 9.0 SDK
- SQL Server LocalDB installed
- Visual Studio 2022 (latest version)
- Test database for migration validation

---

## 10. Source Control Strategy

### 10.1 Branching Strategy
- **Main upgrade branch**: `feature/net_upgrade` (current branch - will be used for upgrade)
- **Source branch**: `feature/net_upgrade` (starting point)
- **No feature branches**: All work done directly on `feature/net_upgrade` branch

**Rationale**: 
- Already on the target branch for upgrade work
- Small solution benefits from straightforward approach
- Single atomic upgrade aligns with single branch approach

### 10.2 Commit Strategy

**Big Bang Approach - Single Commit Recommended**:
- All changes in one atomic commit after successful build
- Commit message: "Upgrade solution to .NET 9.0 - all projects and packages updated"

**Alternative - Checkpoint Commits (if needed)**:
If issues arise, consider these checkpoint commits:

1. **Preparation Commit** (optional):
   - Message: "Prepare for .NET 9.0 upgrade - backup and baseline"
   - Files: Documentation, notes

2. **Atomic Upgrade Commit** (MAIN):
   - Message: "Upgrade all projects to .NET 9.0 and update packages
   
   - Updated DBMigrator to net6.0;netstandard2.0;net9.0 multi-targeting
   - Updated DBMigrator.Console from net472 to net9.0
   - Updated DBMigrator.Test from net5.0 to net9.0
   - Converted and upgraded DBMigratorVsix to net9.0-windows (SDK-style)
   - Upgraded Microsoft.Extensions.* packages from 3.1.8 to 9.0.11
   - Fixed security vulnerability: System.Data.SqlClient 4.8.2 → 4.9.0
   - Removed System.ValueTuple and System.Runtime.Extensions (included in framework)
   - Fixed all compilation errors
   - Solution builds with 0 errors"
   
   - Files: All .csproj files, package references

3. **Test Fixes Commit** (if needed):
   - Message: "Fix test failures after .NET 9.0 upgrade"
   - Files: Test code, test configurations

4. **Final Cleanup Commit** (if needed):
   - Message: "Clean up warnings and code quality issues after upgrade"
   - Files: Code files with warning fixes

**Recommended Pattern for Big Bang**:
- **Single comprehensive commit** after everything builds and tests pass
- Detailed commit message documenting all changes
- This aligns with atomic upgrade approach

### 10.3 Commit Message Template

```
Upgrade DBMigrator solution to .NET 9.0

Summary of changes:
- DBMigrator: net6.0;netstandard2.0 → net6.0;netstandard2.0;net9.0 (added net9.0)
- DBMigrator.Console: net472 → net9.0
- DBMigrator.Test: net5.0 → net9.0
- DBMigratorVsix: .NET Framework 4.7.2 → net9.0-windows (SDK-style conversion)

Package updates:
- Microsoft.Extensions.DependencyInjection: 3.1.8 → 9.0.11
- Microsoft.Extensions.DependencyInjection.Abstractions: 3.1.8 → 9.0.11
- Microsoft.Extensions.Logging: 3.1.8 → 9.0.11
- Microsoft.Extensions.Logging.Abstractions: 3.1.8 → 9.0.11
- Microsoft.Extensions.Logging.Console: 3.1.8 → 9.0.11
- Microsoft.Extensions.Logging.Debug: 3.1.8 → 9.0.11
- System.Data.SqlClient: 4.8.2 → 4.9.0 (SECURITY FIX)

Packages removed (included in framework):
- System.ValueTuple
- System.Runtime.Extensions

Deprecated packages (kept as-is):
- Microsoft.Extensions.CommandLineUtils 1.1.1 (deprecated, plan future replacement)

Breaking changes addressed:
- [List specific code changes made]
- [List any API replacements]

Testing:
- All tests pass in DBMigrator.Test
- DBMigrator.Console runs successfully
- DBMigratorVsix installs and works in VS 2022
- No security vulnerabilities remaining
```

### 10.4 Review and Merge Process

**Before Merge**:
- [ ] All projects build successfully
- [ ] All tests pass
- [ ] No security vulnerabilities
- [ ] Manual testing completed for Console and VSIX
- [ ] Code review completed (if team requires)
- [ ] Documentation updated (README, CHANGELOG)

**Pull Request Checklist**:
- [ ] Detailed PR description with upgrade summary
- [ ] Link to assessment.md and plan.md
- [ ] List of all breaking changes addressed
- [ ] Test results attached
- [ ] Screenshots of working VSIX (if applicable)
- [ ] Performance comparison (if measured)

**Merge Criteria**:
- ✅ All builds pass
- ✅ All tests pass
- ✅ Security vulnerability fixed
- ✅ Manual validation completed
- ✅ No regressions identified
- ✅ Approved by reviewers (if required)

### 10.5 Integration Validation Steps

**After Merge to Main**:
1. Verify CI/CD pipeline builds on main branch
2. Deploy to test environment (if applicable)
3. Run integration tests against test environment
4. Verify no deployment issues
5. Monitor for any runtime issues

**Rollback Plan**:
- Keep previous branch/commit tagged for easy revert
- Document rollback procedure
- Test rollback process before merge if critical system

---

## 11. Success Criteria

### 11.1 Technical Success Criteria

#### Build Success
- [ ] All projects build without errors
- [ ] All projects build without warnings (or warnings documented and accepted)
- [ ] Solution builds successfully in Release configuration
- [ ] Solution builds successfully in Debug configuration

#### Framework Migration
- [ ] DBMigrator multi-targets net6.0, netstandard2.0, and net9.0
- [ ] DBMigrator.Console targets net9.0
- [ ] DBMigrator.Test targets net9.0
- [ ] DBMigratorVsix targets net9.0-windows (SDK-style)

#### Package Updates
- [ ] All Microsoft.Extensions.* packages upgraded to 9.0.11
- [ ] System.Data.SqlClient upgraded to 4.9.0 (security fix applied)
- [ ] System.ValueTuple removed (DBMigrator)
- [ ] System.Runtime.Extensions removed (DBMigrator.Test)
- [ ] No security vulnerabilities in dependencies: `dotnet list package --vulnerable` returns clean

#### Test Success
- [ ] All automated tests in DBMigrator.Test pass (100% pass rate)
- [ ] No new test failures introduced
- [ ] Test execution time within acceptable range (no significant slowdown)

#### Manual Validation
- [ ] DBMigrator.Console starts and runs successfully
- [ ] DBMigrator.Console help text displays correctly
- [ ] DBMigrator.Console can execute database migrations
- [ ] DBMigratorVsix builds VSIX package successfully
- [ ] DBMigratorVsix installs in Visual Studio 2022
- [ ] DBMigratorVsix loads without errors
- [ ] DBMigratorVsix commands work correctly

### 11.2 Quality Criteria

#### Code Quality
- [ ] No regressions in code quality
- [ ] No new compiler warnings (or documented/suppressed)
- [ ] Nullable reference type warnings addressed or suppressed
- [ ] No obsolete API usage warnings

#### Performance
- [ ] Application startup time not significantly degraded
- [ ] Database migration performance maintained or improved
- [ ] Test execution time maintained or improved
- [ ] No memory leaks or excessive allocations detected

#### Security
- [ ] Security vulnerability CVE fixed (System.Data.SqlClient)
- [ ] No new vulnerabilities introduced
- [ ] Dependency security scan clean
- [ ] No high-severity security warnings

### 11.3 Process Criteria

#### Big Bang Strategy Principles
- [ ] All projects upgraded in single atomic operation
- [ ] All package updates applied simultaneously
- [ ] Single coordinated build and fix cycle completed
- [ ] No intermediate unstable states

#### Source Control
- [ ] All work committed to `feature/net_upgrade` branch
- [ ] Commit message(s) clearly document upgrade changes
- [ ] No uncommitted changes remaining
- [ ] Branch ready for review/merge

#### Documentation
- [ ] README updated with .NET 9.0 requirements
- [ ] CHANGELOG updated with upgrade notes
- [ ] Breaking changes documented
- [ ] Known issues documented (if any)
- [ ] Deprecated package plan documented (CommandLineUtils)

### 11.4 Acceptance Criteria

**The migration is complete and successful when**:

1. **All Technical Criteria Met**: 
   - 100% build success
   - 100% test pass rate
   - 0 security vulnerabilities

2. **All Quality Criteria Met**:
   - No performance regressions
   - Code quality maintained
   - Security improved (vulnerability fixed)

3. **All Process Criteria Met**:
   - Big Bang strategy followed
   - Source control strategy followed
   - Documentation updated

4. **User Acceptance**:
   - DBMigrator.Console works for end users
   - DBMigratorVsix works for developers
   - No reported regressions
   - Team confident in stability

---

## 12. Post-Migration Tasks

### 12.1 Immediate Follow-up

**Documentation**:
- Update README.md with .NET 9.0 requirement
- Update build instructions
- Document any new setup steps
- Update CHANGELOG with upgrade details

**CI/CD Pipeline**:
- Update build agents to .NET 9.0 SDK
- Update pipeline configurations (YAML, etc.)
- Verify all CI jobs pass
- Update deployment scripts if needed

**Dependencies**:
- Monitor for any runtime issues
- Set up automated security scanning
- Plan for future package updates

### 12.2 Medium-Term Follow-up (1-3 months)

**Deprecated Package Replacement**:
- Plan replacement for Microsoft.Extensions.CommandLineUtils
- Evaluate System.CommandLine or alternatives
- Create migration plan and timeline
- Update DBMigrator.Console to use new CLI library

**Optional Package Updates**:
- Consider updating MSTest packages to latest (currently on 2.1.2)
- Consider updating Microsoft.NET.Test.Sdk (currently on 16.7.1)
- Evaluate benefits vs. risks of updates

**Code Modernization**:
- Adopt C# 12 features where beneficial
- Consider file-scoped namespaces
- Consider global using directives
- Review nullable reference type annotations

### 12.3 Long-Term Follow-up (3-6 months)

**Performance Optimization**:
- Profile application on .NET 9.0
- Identify optimization opportunities
- Leverage new .NET 9.0 performance features

**Visual Studio Extension Updates**:
- Monitor for VS SDK updates
- Consider adding new VS 2022 features
- Improve async patterns in VSIX

**.NET Updates**:
- Monitor .NET 9.0 updates and patches
- Plan for .NET 10 when released
- Stay current with security patches

---

## 13. Risks and Mitigation Summary

### Critical Risks

| Risk | Probability | Impact | Mitigation | Owner |
|------|------------|---------|------------|-------|
| DBMigratorVsix SDK conversion fails | Medium | High | Manual conversion; VS tools; backup plan | Dev Team |
| Security vulnerability not fully resolved | Low | High | Verify System.Data.SqlClient 4.9.0; test thoroughly | Dev Team |
| Microsoft.Extensions breaking changes | Medium | Medium | Review migration docs; test DI and logging | Dev Team |
| Tests fail after upgrade | Low | Medium | LocalDB verification; incremental troubleshooting | Dev Team |
| VSIX doesn't work in VS 2022 | Medium | High | Extensive testing in experimental instance | Dev Team |

### Known Issues to Monitor

1. **Microsoft.Extensions.CommandLineUtils Deprecation**:
   - Not an immediate blocker
   - Plan replacement in next 6 months
   - Document in technical debt backlog

2. **Microsoft.VSSDK.BuildTools Incompatibility**:
   - Analysis suggests downgrade, but may be false positive
   - Try keeping or upgrading to 17.x first
   - Fallback: Use suggested 15.7.104 if necessary

3. **LocalDB Compatibility**:
   - Verify LocalDB works with .NET 9.0
   - Update LocalDB if needed on dev machines
   - Document LocalDB version requirements

---

## 14. Appendix

### 14.1 Reference Links

**.NET 9.0 Resources**:
- [.NET 9.0 Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- [.NET 9.0 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/9.0)
- [Breaking Changes in .NET 9.0](https://docs.microsoft.com/en-us/dotnet/core/compatibility/9.0)

**Migration Guides**:
- [Upgrade from .NET Framework to .NET](https://docs.microsoft.com/en-us/dotnet/core/porting/)
- [Migrate from .NET 5 to .NET 9](https://docs.microsoft.com/en-us/aspnet/core/migration)
- [SDK-style project conversion](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview)

**Package Documentation**:
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection)
- [System.Data.SqlClient Security Advisory](https://github.com/dotnet/SqlClient/security/advisories)
- [System.CommandLine](https://github.com/dotnet/command-line-api) (replacement for CommandLineUtils)

**Visual Studio Extension Development**:
- [VS SDK Documentation](https://docs.microsoft.com/en-us/visualstudio/extensibility/)
- [Async Packages in VS 2022](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage)
- [VS 2022 Extension Migration Guide](https://docs.microsoft.com/en-us/visualstudio/extensibility/migration)

### 14.2 Tools and Commands

**Useful Commands**:
```bash
# Verify .NET SDK versions
dotnet --list-sdks

# Check for security vulnerabilities
dotnet list package --vulnerable

# Clean and restore
dotnet clean
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Pack VSIX (if applicable)
msbuild /t:CreateVsixContainer /p:Configuration=Release
```

**Visual Studio Commands**:
- Convert project to SDK-style: Right-click project → Upgrade
- View Activity Log: `devenv /log` or `%AppData%\Microsoft\VisualStudio\17.0\ActivityLog.xml`
- Experimental Instance: `devenv /rootsuffix Exp`

### 14.3 Glossary

- **Big Bang Strategy**: Migration approach where all projects are upgraded simultaneously
- **LTS**: Long-Term Support (e.g., .NET 8.0)
- **STS**: Standard Term Support (e.g., .NET 9.0)
- **SDK-style**: Modern .NET project format using simplified XML
- **Classic project**: Legacy .NET Framework project format
- **Multi-targeting**: Building a library for multiple target frameworks
- **TFM**: Target Framework Moniker (e.g., net9.0, netstandard2.0)
- **VSIX**: Visual Studio Extension package format
- **LocalDB**: Lightweight SQL Server instance for development

### 14.4 Contact and Support

**Questions or Issues**:
- Review assessment.md and plan.md first
- Consult .NET 9.0 documentation
- Check GitHub issues for known problems
- Reach out to team lead for guidance

**Escalation Path**:
1. Development team discussion
2. Architecture review (if major changes needed)
3. Microsoft support (for framework/SDK issues)
4. Community forums (Stack Overflow, GitHub)

---

## Summary

This migration plan provides a comprehensive roadmap for upgrading the DBMigrator solution to .NET 9.0 using the **Big Bang Strategy**. The atomic upgrade approach ensures all projects move to the new framework simultaneously, reducing complexity and accelerating completion.

**Key Highlights**:
- ✅ All 4 projects upgraded to .NET 9.0 or net9.0-windows
- ✅ Security vulnerability fixed (System.Data.SqlClient)
- ✅ All Microsoft.Extensions packages updated to 9.0.11
- ✅ DBMigratorVsix converted to SDK-style project format
- ✅ Clear testing and validation strategy
- ✅ Risk mitigation plans in place

**Next Steps**:
1. Review and approve this plan
2. Begin Phase 0 (Preparation)
3. Execute Phase 1 (Atomic Upgrade)
4. Complete Phase 2 (Test Validation)
5. Merge to main branch

Good luck with the migration! 🚀