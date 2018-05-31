# EditorConfig Visual Studio Plugin

[![Join the chat at https://gitter.im/Mpdreamz/editorconfig-visualstudio](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Mpdreamz/editorconfig-visualstudio?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This plugin allows you to have per project checked in Visual Studio settings using an `.editorconfig` settings file.

See the [project web site](https://editorconfig.org) for more information.

## VS 2017

As of [Visual Studio 2017](https://www.visualstudio.com/en-us/news/releasenotes/vs2017-relnotes) Visual Studio ships with `.editorconfig` support baked in! 

This is in fact part of roslyn and will mean that other editors relying on roslyn (omnisharp) to do the code formatting will also benefit from this. 

On top of that roslyn will introduce more editorconfig extension properties to control additional formatting options per project see e.g: https://github.com/dotnet/roslyn/pull/15029

For now this baked in implementation has 2 limitations:

* Does not work for XML files
* Does not support `insert_final_newline` and `trim_trailing_whitespace` (note that these options are supported from Visual Studio 2017 version 15.3)

If you feel strongly there is a need for editorconfig plugin (given these limitations still exists at the time of reading) and you want to submit a PR and become a maintainer ping one of the editorconfig team members!

## Resharper

The plugin and resharper tend to not play nicely, resharper will **also** support `.editorconfig` files in the near future see: https://youtrack.jetbrains.com/issue/RSRP-461746

## Installing

This plugin works with Visual Studio 2012, 2013, and 2015. Again, this plugin is not needed on 2017. The easiest way to install it is through Visual Studio's built-in Extension Manager. Just search for "EditorConfig" in the Online Gallery section. Or, download a copy from the [Visual Studio gallery](https://marketplace.visualstudio.com/items?itemName=EditorConfigTeam.EditorConfig) website.

## Bleeding edge [![Build status](https://ci.appveyor.com/api/projects/status/ad0dc6ldff3bbf3o?svg=true)](https://ci.appveyor.com/project/Mpdreamz/editorconfig-visualstudio/branch/master)

Bleeding edge vsix installer is build on every commit and can be found here:

https://ci.appveyor.com/project/Mpdreamz/editorconfig-visualstudio/build/artifacts

## Building

We adhere to the [F5 manifesto](http://www.khalidabuhakmeh.com/the-f5-manifesto-for-net-developers) so you should be able to clone, open in VS and build.

Building from the command line can be done calling `build.cmd` this will create a vsix packages under `artifacts` directory in the root of the checkout.

## Supported properties

The plugin supports the following EditorConfig [properties](https://editorconfig.org/#supported-properties):

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
