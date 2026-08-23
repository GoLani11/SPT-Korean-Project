using KoreanPatchFix;

var clientCases = new (string? Version, bool Expected)[]
{
    (null, false),
    ("3.8.3", true),
    ("3.9.8", true),
    ("3.10.5", true),
    ("3.11.4", true),
    ("4.0.13", true),
    ("4.1.0", false),
    ("4.1.1", false),
    ("4.1.2", true),
    ("4.1.3", true),
    ("4.1.99", true),
    ("4.1.3-pre", false),
    ("4.2.0", false),
};

foreach (var (version, expected) in clientCases)
{
    var actual = SptCompatibilityPolicy.IsSupportedStableRelease(version!);
    if (actual != expected)
    {
        Console.Error.WriteLine(
            $"Client compatibility mismatch for '{version ?? "null"}': expected={expected}, actual={actual}");
        return 1;
    }
}

var serverRange = new SemanticVersioning.Range(SptCompatibilityPolicy.FourOneServerRange);
var serverCases = new (string Version, bool Expected)[]
{
    ("4.1.0", false),
    ("4.1.1", false),
    ("4.1.2", true),
    ("4.1.3", true),
    ("4.1.99", true),
    ("4.1.3-pre", false),
    ("4.2.0", false),
};

foreach (var (version, expected) in serverCases)
{
    var actual = serverRange.IsSatisfied(version, includePrerelease: false, loose: false);
    if (actual != expected)
    {
        Console.Error.WriteLine(
            $"Server compatibility mismatch for '{version}': expected={expected}, actual={actual}");
        return 1;
    }

    var clientActual = SptCompatibilityPolicy.IsSupportedStableRelease(version);
    if (clientActual != actual)
    {
        Console.Error.WriteLine(
            $"Server/client compatibility differs for '{version}': server={actual}, client={clientActual}");
        return 1;
    }
}

Console.WriteLine(
    $"Validated {clientCases.Length} client cases and {serverCases.Length} equivalent SPT 4.1 server/client cases.");
return 0;
