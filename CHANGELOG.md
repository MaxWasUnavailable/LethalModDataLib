# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Fixed

### Changed

### Removed

## [0.0.2] - 15/02/2024

### Added

- The API now supports properties across the board
- ModDataAttributes can now be used on non-static fields/properties, provided the class using them is instantiated, and
  registered with the ModDataHandler via `ModDataHandler.RegisterInstance(object instance, string keySuffix = "")`

### Fixed

- Fixed a bug where fields/properties in a ModDataContainer flagged with the ModDataIgnoreAttribute with no IgnoreFlags
  would not be ignored

### Changed

- Split up ModDataHandler into ModDataHandler, ModDataAttributeCollector, and SaveLoadHandler
    - ModDataHandler
        - Now only handles the registration of ModDataAttributes, and event hooking & handling
    - ModDataAttributeCollector
        - Now handles the collection of (static field/property) ModDataAttributes, calling the ModDataHandler to
          register them
    - SaveLoadHandler
        - Now handles the actual saving and loading of data
- The API now uses an IModDataKey interface for a single dictionary, rather than having separate field & property
  dictionaries. It also has a ModDataValue as the value type, rather than the ModDataAttribute. This allows me to
  store the relevant information in a unified way (e.g. instance can be null for static fields/properties, or an
  instance for non-static fields/properties)

## [0.0.1] - 04/02/2024

### Added

- Initial project setup
    - README
    - CHANGELOG
    - .gitignore
- ModDataAttribute
- ModDataContainer abstract class
- ModDataHandler system
    - SaveData (3 signatures)
    - LoadData (3 signatures)
    - Finding & registration of ModData attributes
    - Hooked into LethalEventsLib's hooks for saving, autosaving, loading, and file deletion
    - Guid / assembly fetching for manual saving/loading
- ModDataHelper
- Enums
    - LoadWhen
    - SaveWhen
    - SaveLocation