# Differential Fuzzing with AFL++ and SharpFuzz

This fuzz target compares **our CLI** against **Team B's CLI** using differential fuzzing. The fuzzer detects behavioral discrepancies between the two implementations when given equivalent inputs.

AFL++ does not pass the exact same CLI string to both projects because the command names differ. Instead, AFL++ mutates a structured input file, SharpFuzz passes the mutated input into the C# harness, and the harness constructs equivalent commands for each implementation independently.

---

## Command Mapping

| Operation | Our CLI | Team B CLI |
|---|---|---|
| Encode entropy | `encode --entropy <hex> --format hex` | `generate --entropy <hex>` |
| Decode mnemonic | `decode --words <phrase> --format hex` | `recover --phrase <phrase>` |
| Verify valid seed | `verify --phrase <phrase> --seed <seed> --format hex` | `verify --phrase <phrase> --seed <seed>` |
| Derive from seed | `derive --seed <seed> --path <path> --format hex` | `derive --seed <seed> --path <path>` |
| Derive from entropy | `derive --entropy <entropy> --path <path> --format hex` | `derive --entropy <entropy> --path <path>` |

---

## Docker

Docker provides a self-contained Linux environment with all required tooling pre-installed:

- .NET 10 SDK
- .NET 9 SDK
- AFL++
- SharpFuzz CLI

---

## Running the Fuzzer

All commands are run from the **`Project`** folder on Windows.

**Basic run (300 seconds):**
```powershell
.\run-diff-fuzz-afl-docker.ps1 -FuzzSeconds 300
```

**With seed-based derive:**
```powershell
.\run-diff-fuzz-afl-docker.ps1 -FuzzSeconds 300 -IncludeDerive
```

**With entropy-based derive too:**
```powershell
.\run-diff-fuzz-afl-docker.ps1 -FuzzSeconds 300 -IncludeDerive -IncludeDeriveEntropy
```

---

## Custom Target Project Path

The default layout assumes this folder structure:

```
PV286 Secure Coding priciples/
  Project/
  Project_TEAM_B/
```

If Team B's project is in a different location, pass the Docker-internal path explicitly:

```powershell
.\run-diff-fuzz-afl-docker.ps1 `
  -TargetProject "/work/Project_TEAM_B/team-34/Mnemonic/Mnemonic/Mnemonic.csproj"
```

---

## Outputs

| Path | Contents |
|---|---|
| `diff-bin-afl/` | Compiled build outputs |
| `diff-fuzz-findings/` | AFL++ crash/hang findings and JSON repros |




```text
The in-process harness requires a small local adapter in Team B's project.
The adapter template is stored in PV286.Project.DiffFuzz.Afl/TeamBAdapterPatch/.
Copy it into Project_TEAM_B before running the in-process fuzzer.

# Team B csproj patch

Apply these changes to:

`Project_TEAM_B/team-34/Mnemonic/Mnemonic/Mnemonic.csproj`

1. Change:

```xml
<TargetFramework>net10.0</TargetFramework>

to:

<TargetFrameworks>net9.0;net10.0</TargetFrameworks>


Add:
<ItemGroup>
  <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
    <_Parameter1>PV286.Project.DiffFuzz.Core</_Parameter1>
  </AssemblyAttribute>
</ItemGroup>