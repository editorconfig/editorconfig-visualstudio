# Release Notes

## 0.4.0

* Enforces settings on document open (i.e., trim trailing whitespace, fix line endings and fix mixed tabs/spaces).
* Analyzes document for indent size trend and, if inconsistent, adjusts to fit settings.
* Enforces settings before save (i.e., trim trailing whitespace, fix line endings and insert final newline).
* Restores global settings when document loses focus.

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
