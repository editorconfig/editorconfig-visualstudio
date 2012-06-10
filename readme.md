# EditorConfig Visual Studio Plugin

This plugin causes Visual Studio to load it's indentation options from a standard `.editorconfig` settings file. See the [project web site](http://editorconfig.org) for more information.

## Building

To build this software, first download and build the [EditorConfig core library](https://github.com/editorconfig/editorconfig-core) in the `Core` directory. To download the core library, use the git commands:

    git submodule init
    git submodule update

Follow the build instructions for the core library as normal, but include the `-DMSVC_MD=ON` option when invoking CMake:

    cd Core/
    cmake . -DMSVC_MD=ON

Once the core library is built, open the solution file `EditorConfigVS.sln` and compile the plugin. You may need to install the [Visual Studio SDK](https://www.microsoft.com/en-us/download/details.aspx?displaylang=en&id=2680) for this to work. The resulting plugin is named `Plugin/bin/(Debug|Release)/VSEditorConfig.vsix`, and double-clicking installs it into Visual Studio.
