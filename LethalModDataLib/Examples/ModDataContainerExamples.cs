using System.Collections.Generic;
using LethalModDataLib.Attributes;
using LethalModDataLib.Base;
using LethalModDataLib.Enums;

#pragma warning disable CS0169 // Field is never used

namespace LethalModDataLib.Examples;

public class ModDataContainerExampleSimple : ModDataContainer
{
    public int TestInt;
    public string? TestString;

    public int SomeReadOnlyIntProperty { get; } = 100;

    public int SomeWriteOnlyIntProperty
    {
        set => TestInt = value;
    }
}

public class ModDataContainerExampleGeneralSave : ModDataContainer
{
    public int SaveCount;

    protected override SaveLocation SaveLocation => SaveLocation.GeneralSave;

    protected override void PreSave()
    {
        SaveCount++;
    }
}

public sealed class ModDataContainerExampleInstanced : ModDataContainer
{
    [ModDataIgnore] private static int _instanceCount;

    [ModDataIgnore(IgnoreFlag.IfNull | IgnoreFlag.IfDefault)]
    private readonly int _instanceId;

    public int TestInt;
    public string? TestString;

    public ModDataContainerExampleInstanced(string optionalPrefixSuffix)
    {
        OptionalPrefixSuffix = optionalPrefixSuffix;
        _instanceId = _instanceCount++;
    }

    protected override void PostLoad()
    {
        if (TestInt <= 0) TestInt = _instanceId;
    }
}

public static class ModDataContainerExamples
{
    public static void RunExample()
    {
        var simple = new ModDataContainerExampleSimple();
        var generalSave = new ModDataContainerExampleGeneralSave();
        
        List<ModDataContainerExampleInstanced> instances = new();
        for (var i = 0; i < 10; i++)
            instances.Add(new ModDataContainerExampleInstanced($"OptionalPrefixSuffix{i}"));

        // Load everything
        LethalModDataLib.Logger?.LogDebug("Loading example data...");
        simple.Load();
        generalSave.Load();
        foreach (var instance in instances)
            instance.Load();

        // Print current values
        LethalModDataLib.Logger?.LogDebug($"simple.TestInt: {simple.TestInt}");
        LethalModDataLib.Logger?.LogDebug($"simple.TestString: {simple.TestString}");
        LethalModDataLib.Logger?.LogDebug($"generalSave.SaveCount: {generalSave.SaveCount}");
        foreach (var instance in instances)
        {
            LethalModDataLib.Logger?.LogDebug($"instance.TestInt: {instance.TestInt}");
            LethalModDataLib.Logger?.LogDebug($"instance.TestString: {instance.TestString}");
        }

        // Increment or set values
        LethalModDataLib.Logger?.LogDebug("Incrementing or setting example data...");
        simple.TestInt++;
        simple.TestString = "Hello, World!";
        foreach (var instance in instances)
        {
            instance.TestInt++;
            instance.TestString = $"Hello, instance {nameof(instance)}!";
        }

        // Save everything
        LethalModDataLib.Logger?.LogDebug("Saving example data...");
        simple.Save();
        generalSave.Save();
        foreach (var instance in instances)
            instance.Save();
    }
}