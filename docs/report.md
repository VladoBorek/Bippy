# PV286 Project Report - Team 33

## Part A: Our Implementation

**Technology Stack:** C# .NET 9.0, NBitcoin 9.0.5, Microsoft.Extensions DI

**Architecture:** We built a 5-tier layered architecture: Presentation (CLI handlers), Business Logic (Services), Parsers & Validators, DTOs, and Utility (Result Pattern). This structure uses the Command Pattern with dependency injection for extensibility, Service Layer Pattern for clean business logic abstraction, and an explicit Result Pattern for error handling.

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

## Collaboration
- **Team Coordination:** Flawless coordination
- **Lessons for Improvement:** We identified that some validators have higher cyclomatic complexity than ideal and could benefit from breaking them into smaller, more focused methods

# Used tools
- **Static Analysis (SecurityCodeScan, SonarAnalyzer):** These tools caught security issues and complexity hotspots we might have missed
- **xUnit.NET & Integration Tests:** We built comprehensive tests covering full workflows
- **SharpFuzz:** We used out-of-process fuzzing with our custom dictionary (cli.dict) to guide the fuzzer toward meaningful code paths
- **Differential Fuzzing (AFL++):** By comparing our implementation against Team B's, we found a critical bug in our cryptographic handling
- **Key Insight:** Fuzzing caught logic errors that traditional testing simply couldn't expose

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

## Part B: Comparing Our Approach with Team 34

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

Team 34 used the same tooling approach: NUnit for tests and static analysis during dev. The differential fuzzing runs validated correctness by comparing our application behaviour against theirs.

## Key Differences

Team 34 went for a simpler 3-tier design compared to our 5-tier architecture. They use exceptions (`ArgumentException`) for validation failures where we use an explicit `Result<T>` pattern. Both approaches are valid. 

## Overall project feedback

It was interesting that this project offered great freedom regarding programming languages. The project requirements are not extensive, in fact, the scope is quite small. The limited scope combined with the available BIP libraries made the implementation relatively straightforward. The most complex part was definitely designing a proper architecture for CLI software. In the SWE program, every project until now has been about webdev, so this was a refreshing change of perspective. Stefan, who is also enrolled in PA193, mentioned that the course is much better when studied alongside the seminars. The issue is that PA193 isn't part of the SWE curriculum, so PV286 should also work well as a standalone course. The real problem is that the course provides the theoretical knowledge but lacks practical examples of how to use them. Each language has completely different ecosystems with tools that vary wildly in usefulness and documentation. It would help significantly to narrow the scope to just one or two languages and provide working examples of the right tools and how to use them for those specific languages.

Overall, the project is well-structured and enables students to use proper security and code quality tools, but it could be improved with more focused guidance.