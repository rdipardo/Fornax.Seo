## Contributing

### Development

_All platforms need a .NET SDK at version 5.0.200 or later_

_Linux users should consider 5.0.202 the [safest minimum version][]_

> **NOTE**
>
> .NET SDK 5.0.300 and later [may be incompatible][] with `fsdocs`
>
> Run `dotnet --list-sdks` and `dotnet --version` before trying to build
> the project's documentation


Start by checking out the source tree and submodules:

    git clone --recursive https://github.com/rdipardo/Fornax.Seo


#### Windows

To run unit tests and build a sample website:

~~~bat
  scripts\ci
  :: or, to also serve the site at localhost:8080
  scripts\ci live
~~~

To browse a local copy of [the documentation][]:

~~~bat
  scripts\gendocs live
~~~


#### Linux, macOS

Run tests with:

~~~sh
  scripts/ci
  # or
  scripts/ci live
~~~

Browse docs with:

~~~sh
  scripts/gendocs live
~~~


### Pull Requests

Before committing your changes, make sure your code is formatted:

       # on Windows
       scripts\format

       # or, on Linux and macOS
       scripts/format

GitHub users can follow the [well-documented steps][] to creating forks
and pull requests.


#### No GitHub account?

All contributions are welcome. If you have the [required development tools](#development)
installed, you can simply follow these steps:

* commit your changes and generate a patch file:

  *Note*. Pass the number of commits you want to share as an option, e.g.
  `-1` for the most recent commit, `-2` for the latest and second most
  recent commit, etc.

        git format-patch -k -1

* email your patch to the [the project owner][]

GitHub users are also welcome to upload patch files to [the issue tracker][]
if they prefer.


### Licensing your contributions

This project is licensed under the [Mozilla Public License, Version 2.0][].
If you contribute new code files (not including test files and scripts), please
put a [license notice][] at the top, like this:

~~~
//
// Copyright (c) <year> <your_name>
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this file,
// You can obtain one at http://mozilla.org/MPL/2.0/.
//
~~~


[safest minimum version]: https://github.com/NuGet/Announcements/issues/56#issue-847215025
[may be incompatible]: https://github.com/ArtemyB/FsDocsSample/issues/1#issuecomment-878835846
[well-documented steps]: https://docs.github.com/en/github/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request-from-a-fork
[the issue tracker]: https://github.com/rdipardo/Fornax.Seo/issues
[the project owner]: mailto:dipardo.r@gmail.com
[the documentation]: https://heredocs.io/Fornax.Seo
[Mozilla Public License, Version 2.0]: https://www.mozilla.org/en-US/MPL/2.0/
[license notice]: https://www.mozilla.org/en-US/MPL/headers/
