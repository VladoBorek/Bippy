# PV286-project

## Basic Usage

To run the application, navigate to the `PV286-project` directory and use the `dotnet run` command.

```powershell
cd PV286-project
```

### Encode

You can use the `encode` command to encode entropy data into various formats:

```powershell
dotnet run -- encode --entropy 78ba6f96c8a70f71c4acff1c9dc7b35d8988734180d9502eeada775b7cca103e --format hex
dotnet run -- encode
```

### Decode

Decode a mnemonic phrase to retrieve the entropy and seed.

```powershell
dotnet run -- decode "photo memory captain decline vendor heavy seminar gloom mouse economy awkward tilt" --format hex
```

### Verify

Verify if a given mnemonic phrase generates the expected seed.

```powershell
dotnet run -- verify --phrase "judge square toss mule ill rib bargain paper broken until under roast obtain defy alcohol brass expand jar repair upgrade result govern domain solid" --seed 897f9beefb28fa6660e65a6b77518547d1bf8ad203cae84cf5614174fce86d8c8329547779a319090c4557fd330b36b294a1cc9bcaaf5c3f2b48eefbe5142340 --format hex
```

### Batch Commands

The `batch` command allows you to execute multiple commands either sequentially from a string (separated by `|`) or from a file.

**Inline Batch:**
```powershell
dotnet run -- batch - "encode --format bin | encode --format hex | encode | encode --entropy 010101 --format bin"
```

**Batch from a File:**
```powershell
dotnet run -- batch "C:\batch.txt"
```

### Help

To see all available options and commands:

```powershell
dotnet run -- --help
```
