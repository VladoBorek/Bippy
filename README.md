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
