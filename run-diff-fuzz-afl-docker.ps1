param(
    # Parent folder containing both Project and Project_TEAM_B.
    [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,

    # Target .csproj path as seen inside Docker.
    [string]$TargetProject = "/work/Project_TEAM_B/team-34/Mnemonic/Mnemonic/Mnemonic.csproj",

    [int]$FuzzSeconds = 300,

    # Enables derive --seed comparison.
    [switch]$IncludeDerive,

    # Enables derive --entropy comparison.
    # This is expected to reveal a semantic mismatch if the implementations treat entropy differently.
    [switch]$IncludeDeriveEntropy
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ImageName = "pv286-diff-fuzz-afl"
$Dockerfile = Join-Path $PSScriptRoot ".ci\Dockerfile.diff-fuzz"

if (-not (Test-Path $Dockerfile)) {
    throw "Dockerfile not found: $Dockerfile"
}

$includeDeriveValue = if ($IncludeDerive) { "1" } else { "0" }
$includeDeriveEntropyValue = if ($IncludeDeriveEntropy) { "1" } else { "0" }

docker build -f $Dockerfile -t $ImageName $PSScriptRoot

docker run --rm `
  -v "${WorkspaceRoot}:/work" `
  -w "/work/Project" `
  -e "TEAM_B_PROJECT=$TargetProject" `
  -e "FUZZ_SECONDS=$FuzzSeconds" `
  -e "INCLUDE_DERIVE=$includeDeriveValue" `
  -e "INCLUDE_DERIVE_ENTROPY=$includeDeriveEntropyValue" `
  $ImageName `
  bash -lc "sed -i 's/\r$//' ./run-diff-fuzz-afl-container.sh && bash ./run-diff-fuzz-afl-container.sh"


if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}