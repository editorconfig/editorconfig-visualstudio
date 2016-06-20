# EditorConfig Visual Studio Plugin

[![Join the chat at https://gitter.im/Mpdreamz/editorconfig-visualstudio](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Mpdreamz/editorconfig-visualstudio?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This plugin allows you to have per project checked in Visual Studio settings using an `.editorconfig` settings file.

See the [project web site](http://editorconfig.org) for more information.


## Installing

This plugin works with Visual Studio 2012 or later. The easiest way to install it is through Visual Studio's built-in Extension Manager. Just search for "EditorConfig" in the Online Gallery section. Or, download a copy from the [Visual Studio gallery](http://visualstudiogallery.msdn.microsoft.com/c8bccfe2-650c-4b42-bc5c-845e21f96328) website.

## Bleeding edge [![Build status](https://ci.appveyor.com/api/projects/status/ad0dc6ldff3bbf3o?svg=true)](https://ci.appveyor.com/project/Mpdreamz/editorconfig-visualstudio/branch/master)

Bleeding edge vsix installer is build on every commit and can be found here:

https://ci.appveyor.com/project/Mpdreamz/editorconfig-visualstudio/build/artifacts

## Building

We adhere to the [F5 manifesto](http://www.khalidabuhakmeh.com/the-f5-manifesto-for-net-developers) so you should be able to clone, open in VS and build.

Building from the command line can be done calling `build.cmd` this will create a vsix packages under `artifacts` directory in the root of the checkout.

## Supported properties

The plugin supports the following EditorConfig [properties](http://editorconfig.org/#supported-properties):

* indent_style
* indent_size
* tab_width
* end_of_line
* insert_final_newline
* trim_trailing_whitespace
* root (only used by EditorConfig core)

## Reporting problems

If you encounter any problems, feel free to report them at the [issue tracker](https://github.com/editorconfig/editorconfig-visualstudio/issues).

Please note that you shouldn't expect to see settings from your .editorconfig file's configuration in the text editor options dialog. Those are your global settings that we try to restore whenever we can so we do not bleed into your projects not using editorconfig.
