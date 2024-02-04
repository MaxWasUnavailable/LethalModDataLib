# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Fixed

### Changed

### Removed

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