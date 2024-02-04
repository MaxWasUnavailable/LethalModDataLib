# LethalModDataLib

*A library for mod data saving & loading.*

## What is this?

This library provides a standardised way to save and load persistent data for mods. It is designed to be easy to use and
flexible; offering multiple different ways to interact with the system, depending on your needs.

Data is saved in `.moddata` files, which are stored in the same location as the vanilla save files. Instead of having a
single file, or a file per mod, the library has a file for each save file, and a file for general data - essentially
mimicking the vanilla save system. This ensures that mods do no pollute the vanilla save files. The library makes use
of ES3 to handle the actual saving and loading of the data, which should be compatible with most Unity types.

When saving and loading data through the library, keys are automatically generated based on your mod's GUID and assembly
information (depending on the approach used - see below). This ensures that your data does not conflict with other mods'
data, and that it is easy to find and debug.

## Usage

There are 3 ways to use this library. They can all be used in the same project, with some caveats.

### 1. Using the `ModData` attribute

This is the easiest and most automated way to use the library. Unless you need to manually handle saving and loading,
this is the way to go. Note that this method still allows you to manually handle saving and/or loading if you need to,
so you are not limited to the automated part.

Depending on the attribute configuration, the library will take care of saving and loading data for you, in a way that
is seamless and "invisible" / does not require you to add any additional code beyond the attribute.

The `ModData` attribute can be used to mark fields that should be saved and loaded through the handler's event hooks.
This is its constructor signature:

```csharp
public ModDataAttribute(SaveWhen saveWhen, LoadWhen loadWhen, SaveLocation saveLocation, string? baseKey = null)
```

These are options for its 4 parameters:

- `SaveWhen` (enum) - When the data should be saved
    - `OnSave` - When the game is saved (Most frequent - also called by autosaves)
    - `OnAutoSave` - When the game is autosaved (= Whenever the ship returns to orbit)
    - `Manual` - Manually handled by you, the modder
- `LoadWhen` (enum) - When the data should be loaded
    - `OnLoad` - When a save file is loaded, right after all vanilla loading is done
    - `OnRegister` - When the attribute is registered, as soon as possible
    - `Manual` - Manually handled by you, the modder
- `SaveLocation` (enum) - Where the data should be saved
    - `GeneralSave` - In a .moddata file that fulfills the same purpose as vanilla's LCGeneralSaveData file
    - `CurrentSave` - In a .moddata file that is specific to the current save file
- `BaseKey` - **Strongly recommended to leave default unless you know what you're doing** - The base key for the data.
  This is used to create the key for the field in the .moddata file. If not set, the library will sort this out. In
  general, you should not need to set this unless you are e.g. trying to access the data from another mod (in which
  case, you should probably be using the `GetFieldInfo` method in `ModDataHelper` instead, since duplicate attribute
  keys can cause issues).

> Example usage:

```csharp
public class SomeClass
{
    [ModData(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.GeneralSave)]
    private int __someInt;
    
    [ModData(SaveWhen.OnAutoSave, LoadWhen.OnLoad, SaveLocation.CurrentSave)]
    private string __someString;
    
    [ModData(SaveWhen.Manual, LoadWhen.OnLoad, SaveLocation.GeneralSave)]
    private float __someFloat;
    
    // Some method in which we manually handle __someFloat's saving, since its attribute is set to SaveWhen.Manual
    private void SomeMethod()
    {
        // (...)
        
        ModDataHandler.SaveData(GetFieldInfo(this, nameof(__someFloat)));
        
        // (...)
    }
}
```

The ModData attribute can be used on fields, both static and instanced ones, as well as public, private and internal
ones.

> [!WARNING]
>
> When using the Manual parameter for saving and/or loading, you **need** to use the methods that take FieldInfo as a
> parameter. This is because the other save/load methods will result in a different key being used, which will cause
> your data to be saved and loaded in a different location.

### 2. Using the `ModDataContainer` abstract class

This way of using the library requires you to set up a class that inherits from `ModDataContainer`. Any fields in this
class will be saved and loaded automatically, without the need for any attributes. You are essentially creating a
"container" for your mod data.

The ModDataContainer class has a number of properties and methods that you can override to customize its behavior:

- Properties:
    - `SaveLocation` - Where the data should be saved. Defaults to `SaveLocation.CurrentSave`
    - `OptionalPrefixSuffix` - A string that will be appended to the prefix for keys of fields in the container. This is
      useful in case you want to have different instances of the same container in the same save file; for example a
      container per player. Defaults to `string.Empty`
- Methods:
    - `GetPrefix` - **Strongly recommended to leave default unless you know what you're doing** - Returns the prefix for
      keys of fields in the container. Defaults to the assembly name and the class name, separated by a dot. (
      e.g. `MyMod.MyContainer`). If `OptionalPrefixSuffix` is set, it will be appended to the prefix like
      so: `MyMod.MyContainer.MyOptionalPrefixSuffix`
    - `Save` - **Strongly recommended to leave default unless you know what you're doing** - Saves the data in the
      container. Should be called by the modder when the data should be saved.
    - `Load` - **Strongly recommended to leave default unless you know what you're doing** - Loads the data in the
      container. Should be called by the modder when the data should be loaded.
    - Pre/PostSave/Load - Methods that are called before and after the saving and loading of the container's data. Can
      be
      used to perform additional operations, such as logging or data validation.

> Example usage:

```csharp
public class SomeContainer : ModDataContainer
{
    private int __someInt;
    private string __someString;
    private float __someFloat;
    private List<int> __someList;
    
    // Use the constructor to set the OptionalPrefixSuffix, so we can have multiple instances of this container without them overwriting each other
    public SomeContainer(string name)
    {
        OptionalPrefixSuffix = name;
    }
    
    // Override the PostLoad method to ensure that the list is not null
    protected override void PostLoad()
    {
        if (__someList == null)
        {
            __someList = new List<int>();
        }
    }
}

// In some other class
public class SomeClass
{
    private SomeContainer __container;
    
    public SomeClass()
    {
        __container = new SomeContainer("SomeName"); // Create a new instance of the container
        __container.Load(); // Load the container's data, if any exists
    }
    
    // Some method in which we manually handle saving the container's data
    private void SomeMethod()
    {
        // (...)
        
        __container.Save(); // Save the container's data
        
        // (...)
    }
}
```

> [!WARNING]
>
> Note: You should **not** use the ModData attribute on fields in a class that inherits from ModDataContainer. This will
> cause the fields to be saved/loaded twice, once by the container and once by the attribute. Additionally, the keys for
> the fields will be different, which can cause inconsistencies depending on when the data is saved and loaded.

### 3. Using the `ModDataHandler` save & load methods

This is the "good old" manual way of saving and loading data. You can use the `ModDataHandler` class' methods to
manually handle saving and loading of data. This is useful if you need to save and load data in a way / at a time that
is not covered by the other options, or if you want to build your own handler for saving and loading.

The `ModDataHandler` class has a SaveData & LoadData method, with three public signatures each:

```csharp
// Primarily for internal use by the handler, not recommended for use by modders. Only exposed in case you *really* need it.
public static bool SaveData<T>(T? data, string key, string fileName)

// The recommended method to use for manual saving.
// It is recommended to leave autoAddGuid as true, since this will automatically add your mod's guid to the key; preventing conflicts with other mods.
public static bool SaveData<T>(T? data, string key, SaveLocation saveLocation = SaveLocation.CurrentSave, bool autoAddGuid = true)
    
// For usage with the SaveWhen.Manual attribute parameter. You will need to fetch the FieldInfo for the field you want to save.
// This can be done using the GetFieldInfo method in ModDataHelper.
// Note: This will save the data from the field, rather than requiring you to pass it through the method.
public static bool SaveData(FieldInfo field)
```

```csharp
// Primarily for internal use by the handler, not recommended for use by modders. Only exposed in case you *really* need it.
public static T? LoadData<T>(string key, string fileName, T? defaultValue = default)
    
// The recommended method to use for manual loading.
// It is recommended to leave autoAddGuid as true, since this will automatically add your mod's guid to the key; preventing conflicts with other mods.
public static T? LoadData<T>(string key, T? defaultValue = default, SaveLocation saveLocation = SaveLocation.CurrentSave, bool autoAddGuid = true)
    
// For usage with the LoadWhen.Manual attribute parameter. You will need to fetch the FieldInfo for the field you want to load.
// This can be done using the GetFieldInfo method in ModDataHelper.
// Note: This will store the loaded data in the field, rather than returning it through the method.
public static bool LoadData(FieldInfo field)
```

> Example usage:

```csharp
public class SomeClass
{
    private int __someInt;
    private string __someString;
    
    [ModData(SaveWhen.Manual, LoadWhen.Manual, SaveLocation.GeneralSave)]
    private float __someFloat;
    
    // Some method in which we manually handle saving __someInt
    private void SomeMethod()
    {
        // (...)
        
        ModDataHandler.SaveData(__someInt, "SomeIntKey");
        
        // (...)
    }
    
    // Some method in which we manually handle loading __someString
    private void SomeOtherMethod()
    {
        // (...)
        
        __someString = ModDataHandler.LoadData<string>("SomeStringKey", "SomeDefaultValue");
        
        // (...)
    }
    
    // Some method in which we manually handle saving __someFloat
    private void YetAnotherMethod()
    {
        // (...)
        
        ModDataHandler.SaveData(GetFieldInfo(this, nameof(__someFloat)));
        
        // (...)
    }
    
    // Some method in which we manually handle loading __someFloat
    private void AndAnotherMethod()
    {
        // (...)
        
        ModDataHandler.LoadData(GetFieldInfo(this, nameof(__someFloat)));
        
        // (...)
    }
}
```

## Tips

- The library automatically removes the paired .moddata file when a save is deleted, so handle this accordingly in your
  mod. (e.g. by hooking into the `PostDeleteFileEvent` event from `LethalEventsLib`)
- Validate your data after loading, if you expect it to be in a certain state. If a value is missing when it is loaded,
  it will be set to the type's `default` value (0, null, etc...). This can be done in e.g. the `PostLoad` method of a
  `ModDataContainer` or in the method that loads the data. For attribute-based saving and loading, you can use the
  `PostLoadGameEvent` event from `LethalEventsLib` to validate the data after it has been loaded.
- Lethal Company sets its current save file to the last selected/loaded save file on game start. Keep this in mind
  if you are using the `SaveLocation.CurrentSave` parameter, and are manually handling saving and/or loading. This is
  not a concern if you are using the attribute without manual handling, or if you are using
  the `SaveLocation.GeneralSave` parameter.