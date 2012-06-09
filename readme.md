# EditorConfig Visual Studio Plugin

This plugin causes Visual Studio to load it's indentation options from a standard `.editorconfig` settings file. See the [project web site](http://editorconfig.org) for more information.

## Building

To build this software, first download and build the [core library](https://github.com/editorconfig/editorconfig-core) from Github. Follow the build instructions as normal, but include the `-DMSVC_MD=ON` option when invoking CMake.

Once the core library is built, edit its location into the `core.props` configuration file.

Finally, open the Visual Studio solution file and build it. You may need to install the [Visual Studio SDK](https://www.microsoft.com/en-us/download/details.aspx?displaylang=en&id=2680) for this to work. Once the plugin is built, you will have a file named `Plugin/bin/(Debug|Release)/VSEditorConfig.vsix`. This is the plugin, and double-clicking it installs it into Visual Studio.
