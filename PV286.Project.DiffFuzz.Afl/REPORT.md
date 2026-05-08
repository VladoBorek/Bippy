# Differential Fuzzing Report

## Setup

We implemented differential fuzzing between our and team-34's implementation. The fuzzer uses AFL++ with SharpFuzz.

Each fuzz input is a structured text file with `op`, `entropy`, and `path` fields. AFL++ mutates this text; the harness normalises invalid or partial inputs so every mutation still produces a meaningful comparison rather than a parse error. A CLI dictionary (`cli.dict`) provides domain-specific tokens to help AFL++ reach relevant code paths faster.
 
---

## Results

We ran the fuzzer across all operations. The longest run tested **73,243 inputs** over 11 minutes at ~109 exec/sec.

For all operations except `derive --entropy`, both implementations produced identical outputs on every input tested. No crashes or timeouts were recorded.

---

## Finding — `derive --entropy` Semantic Mismatch

**Input:**
```
op=derive-entropy
entropy=00000000000000000000000000000000
path=m/44'/0'/0'/0/0
```

**Outputs differed:**

| | xprv |
|---|---|
| Ours | `xprvA4EMaq49eKGKGK2k3kAsiqTowWrNuidQTx5DaYm669TjJUtsEARurRTwXiP1PXsNkxL4pLijwktqb9gSWHccdm92nKDKznNUCSKwvktQLp2` |
| Team B | `xprvA2cWYEXRrpaYZmR4Mat3aHw7ARSGFAtb5LQNfSuyQCCGVJXRNWA3zkkHZcBM4voi9TBrb9WaC65HGv5e8gZgfnjzH71WofaXT3haLw8LYqQ` |

**Root cause:** our implementation passed the raw entropy bytes directly into BIP32 as the seed. The correct pipeline per the assignment and BIP39 spec is:

```
entropy → BIP39 mnemonic → BIP39 seed (PBKDF2-HMAC-SHA512) → BIP32 master key → BIP32 derivation
```

Team B follows this pipeline. 
Our `derive --seed` behavior was correct; only `derive --entropy` was wrong.

Repro: `diff-fuzz-findings/failure_derive_entropy_179B88AC0081EF36.json`
