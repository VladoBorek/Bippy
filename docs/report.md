# PV286 Project Report - Team 33

## Part A: Implementation Description

**Technology Stack:** C# .NET 9.0, NBitcoin 9.0.5, Microsoft.Extensions DI

**Architecture:** 5-tier layered design: Presentation (CLI handlers), Business Logic (Services), Parsers & Validators, DTOs, and Utility (Result Pattern). Uses Command Pattern with dependency injection for extensibility, Service Layer Pattern for business logic abstraction, and Result Pattern for explicit error handling.

## Quality Assessment

### Strengths
- Clear layered separation of concerns with distinct presentation, business logic, validation, and utility tiers
- Comprehensive test coverage including unit tests, integration tests, and differential fuzzing
- Strong input validation preventing invalid operations before execution
- Static analysis integration (SecurityCodeScan, SonarAnalyzer) for security and complexity detection
- All services abstracted behind interfaces for testability
- Consistent code structure and naming conventions across commands

### Weaknesses
- No caching mechanism for repeated derivations, impacting performance on repeated operations
- Some validators have high cyclomatic complexity and would benefit from refactoring into smaller methods
- Batch command error handling lacks granularity (no fail-fast or continue-on-error options)

## Collaboration & Design Improvements
- **Team Collaboration:** Flawless coordination and knowledge sharing
- **Design Improvements:** Refactor high-complexity validators into smaller, focused methods to reduce cyclomatic complexity

## Tools & Fuzzing Strategy
- **Static Analysis (SecurityCodeScan, SonarAnalyzer):** Identified security vulnerabilities and code complexity patterns, reducing manual review burden
- **xUnit.NET & Integration Tests:** Comprehensive test coverage including end-to-end command workflows; caught integration issues unit tests missed
- **SharpFuzz:** Out-of-process fuzzing with domain-specific dictionary (cli.dict) guiding fuzzer to relevant code paths
- **Differential Fuzzing (AFL++):** Comparative testing against Team B's implementation; discovered critical cryptographic correctness issue
- **Impact:** Fuzzing identified semantic errors traditional testing missed

## Most Interesting Issues: 

#### 1. Derive-Entropy BIP-39 Pipeline Bug

**Problem:** `derive --entropy` produced incorrect output; differential fuzzer detected mismatch vs. Team B's implementation

**Root Cause:** Implementation bypassed BIP-39 mnemonic generation, passing raw entropy directly to BIP32 derivation instead of following proper pipeline

**Correct Pipeline:** entropy → BIP-39 mnemonic → PBKDF2-HMAC-SHA512 seed → BIP32 derivation

**Resolution:** Implemented proper BIP-39/BIP-32 pipeline; verified against Team B's reference implementation (73,243+ fuzz iterations)

**Outcome:** Differential fuzzing identified semantic correctness issues unit tests missed. Only `derive --entropy` affected; `derive --seed` was correct throughout.

#### 2. Extensible CLI Architecture via Command Pattern + DI

**Problem:** Adding new commands required modification of central dispatcher, creating tight coupling and violating Open/Closed Principle

**Solution:** Implemented `ICliCommand` interface with command-specific parsers registered in DI container, enabling runtime discovery and execution without modifying dispatcher

**Benefit:** New commands added by creating parser class and registering in DI; no changes to existing command handlers or dispatcher logic required

#### 3. Parser Builder Pattern for Shared Validation Logic

**Problem:** Each command duplicated argument parsing, validation, and error handling logic, creating maintenance burden and inconsistencies

**Solution:** Extracted shared logic into `ArgParser` base class with `ValidatorUtils` for reusable validation chains; builders construct parser configuration dynamically

**Benefit:** Consolidated parsing logic, consistent validation rules across commands, centralized maintenance, reduced boilerplate significantly

---

## Part B: Team 34 Implementation Review

**Technology Stack:** C# .NET 10.0, NBitcoin 10.0.1, NUnit 4.3.2, Microsoft.Extensions DI

**Architecture:** 3-tier design with CliRunner/CliParser (presentation), ICommand handlers via CommandFactory (execution), IBip39Service/IBip32Service (business logic). Uses records for immutable data structures and DI for decoupled components.

## Quality Assessment

### Strengths
- Clean separation of concerns between CLI parsing, command execution, and cryptographic operations
- Comprehensive DI configuration with all services abstracted behind interfaces for testability
- Strong test coverage including fuzzing and integration tests

### Weaknesses
- Initially emphasized unit/component testing with limited integration test coverage; now resolved with added end-to-end CLI workflows

## Tools & Fuzzing Strategy

Team 34 employed same tools: NUnit for testing, static analysis during development, and differential fuzzing with AFL++. Fuzzing provided equivalent value in validating cryptographic correctness through comparative testing against Team 33.

## Key Implementation Notes

Team 34's architecture prioritizes simplicity with 3-tier design vs. Team 33's 5-tier layered approach. Error handling uses exceptions (`ArgumentException`) for validation failures rather than explicit `Result<T>` pattern. Both approaches passed differential fuzzing validation, confirming correctness of cryptographic implementations.
