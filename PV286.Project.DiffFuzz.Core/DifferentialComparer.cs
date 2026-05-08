using System.Text.Json;

internal sealed class DifferentialComparer
{
	private readonly bool includeDerive;
	private readonly bool includeDeriveEntropy;
	private readonly FailureWriter failures;
	private readonly OurMnemonicApi ours = new();
	private readonly global::Mnemonic.DifferentialMnemonicApi teamB = new();

	public DifferentialComparer(
		string failuresDir,
		bool includeDerive,
		bool includeDeriveEntropy)
	{
		this.includeDerive = includeDerive;
		this.includeDeriveEntropy = includeDeriveEntropy;
		failures = new FailureWriter(failuresDir);
	}

	public void Compare(string rawInput)
	{
		var input = DiffInput.FromRaw(rawInput, includeDerive, includeDeriveEntropy);

		switch (input.Operation)
		{
			case Operation.Encode:
				CompareEncode(input);
				break;
			case Operation.Decode:
				CompareDecode(input);
				break;
			case Operation.VerifyOk:
				CompareVerify(input, input.SeedBytes, "verify/ok");
				break;
			case Operation.VerifyNok:
				CompareVerify(input, input.WrongSeedBytes, "verify/nok");
				break;
			case Operation.DeriveSeed:
				CompareDeriveSeed(input);
				break;
			case Operation.DeriveEntropy:
				CompareDeriveEntropy(input);
				break;
			default:
				throw new InvalidOperationException($"Unsupported operation: {input.Operation}");
		}
	}

	private void CompareEncode(DiffInput input)
	{
		var left = Capture("ours encode", () => ours.GenerateFromEntropy(input.EntropyBytes));
		var right = Capture("teamB generate", () => teamB.GenerateFromEntropy(input.EntropyBytes));

		RequireSuccess("encode/generate", input, left.Run, right.Run);
		RequireEqual("encode/generate", "phrase", input, left.Value!.Phrase, right.Value!.Phrase, left.Run, right.Run);
		RequireEqual("encode/generate", "seed", input, left.Value.SeedHex, right.Value.SeedHex, left.Run, right.Run);
	}

	private void CompareDecode(DiffInput input)
	{
		var left = Capture("ours decode", () => ours.RecoverFromPhrase(input.Phrase));
		var right = Capture("teamB recover", () => teamB.RecoverFromPhrase(input.Phrase));

		RequireSuccess("decode/recover", input, left.Run, right.Run);
		RequireEqual("decode/recover", "phrase", input, left.Value!.Phrase, right.Value!.Phrase, left.Run, right.Run);
		RequireEqual("decode/recover", "entropy", input, left.Value.EntropyHex, right.Value.EntropyHex, left.Run, right.Run);
		RequireEqual("decode/recover", "seed", input, left.Value.SeedHex, right.Value.SeedHex, left.Run, right.Run);
	}

	private void CompareVerify(DiffInput input, byte[] seed, string operation)
	{
		var left = Capture("ours verify", () => ours.Verify(input.Phrase, seed));
		var right = Capture("teamB verify", () => teamB.Verify(input.Phrase, seed));

		RequireSuccess(operation, input, left.Run, right.Run);
		RequireEqual(operation, "result", input, left.Value!, right.Value!, left.Run, right.Run);
	}

	private void CompareDeriveSeed(DiffInput input)
	{
		var left = Capture("ours derive seed", () => ours.DeriveFromSeed(input.SeedBytes, input.Path));
		var right = Capture("teamB derive seed", () => teamB.DeriveFromSeed(input.SeedBytes, input.Path));

		RequireSuccess("derive/seed", input, left.Run, right.Run);
		RequireEqual("derive/seed", "xprv", input, left.Value!.ExtendedPrivateKey, right.Value!.ExtendedPrivateKey, left.Run, right.Run);
		RequireEqual("derive/seed", "xpub", input, left.Value.ExtendedPublicKey, right.Value.ExtendedPublicKey, left.Run, right.Run);
	}

	private void CompareDeriveEntropy(DiffInput input)
	{
		var left = Capture("ours derive entropy", () => ours.DeriveFromEntropy(input.EntropyBytes, input.Path));
		var right = Capture("teamB derive entropy", () => teamB.DeriveFromEntropy(input.EntropyBytes, input.Path));

		RequireSuccess("derive/entropy", input, left.Run, right.Run);
		RequireEqual("derive/entropy", "xprv", input, left.Value!.ExtendedPrivateKey, right.Value!.ExtendedPrivateKey, left.Run, right.Run);
		RequireEqual("derive/entropy", "xpub", input, left.Value.ExtendedPublicKey, right.Value.ExtendedPublicKey, left.Run, right.Run);
	}

	private static Captured<T> Capture<T>(string command, Func<T> action)
	{
		try
		{
			var value = action();
			return new Captured<T>(
				value,
				new RunResult(command, 0, JsonSerializer.Serialize(value), "", false));
		}
		catch (Exception ex)
		{
			return new Captured<T>(
				default,
				new RunResult(command, 1, "", ex.ToString(), false));
		}
	}

	private void RequireSuccess(string operation, DiffInput input, RunResult left, RunResult right)
	{
		if (!left.Success || !right.Success)
		{
			failures.WriteAndThrow(operation, input, "one implementation failed", left, right);
		}
	}

	private void RequireEqual(
		string operation,
		string field,
		DiffInput input,
		string left,
		string right,
		RunResult leftRun,
		RunResult rightRun)
	{
		if (!string.Equals(left, right, StringComparison.Ordinal))
		{
			failures.WriteAndThrow(
				operation,
				input,
				$"field '{field}' differs: ours='{left}', teamB='{right}'",
				leftRun,
				rightRun);
		}
	}

	private sealed record Captured<T>(T? Value, RunResult Run);
}
