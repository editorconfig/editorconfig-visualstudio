# Release Notes

## 0.6.0

* No longer relies on C++ editorconfig but on the .NET port
* Moved to @jedmao branch that relies on codemaid based architecture so we can restore global settings more aggressively
  This to relieve a common nuisance that having two visual studios one with and one without .editorconfig settings do not respect global
  (default) settings.
* Add file template to easily add `.editorconfig` file to a project
* Re added @jaredpars autocomplete and syntax highlight work
* Moved to paket and fake for builds and dependency management
* Moved to VS SDK nuget packages
* Fixed SO exception when renaming a file (only occured in development version not in previously published release)

## 0.5.0

* Autocomplete now accepts domain properties
* VS 2015 support

## 0.4.0

* Enforces settings on document open (i.e., trim trailing whitespace, fix line endings and fix mixed tabs/spaces).
* Analyzes document for indent size trend and, if inconsistent, adjusts to fit settings.
* Enforces settings before save (i.e., trim trailing whitespace, fix line endings and insert final newline).
* Restores global settings when document loses focus.

## 0.3.2

* Bug fixes

## 0.3.1

* Visual Studio 2013 support


## 0.3.0

* Built against Core 0.11.0

## 0.2.6

* Visual Studio 2012 support

## 0.2.5

* Ignore certain unsaved files (fix by Karaken12)

## 0.2.4

* Handle config files that have a Unicode BOM mark
* Ignore paths that begin with "http:"

## 0.2.3

* Fully handle the `indent_size=tab` case

## 0.2.2

* Fix bug #56

## 0.2.1

* Display core messages in the Visual Studio error window

## 0.2

* Initial release after re-writing the plugin from scratch
