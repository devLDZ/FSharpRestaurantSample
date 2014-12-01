# FSharp Restaurant Sample

The aim of project is to provide comprehensive sample of modern application based on such principles as TDD, DDD, CQRS and Event Sourcing focusing also on functional programming style. Chosen domain is enough simple and easy to understand to not be a problem against understanding of program while being easily expandable and being complex enough to not be just a naive sample

#Project structure and tooling
* Project structure is based on [Project Scaffold](https://github.com/fsprojects/ProjectScaffold). It's just simpler version of it which should not make any problems for people not used to complex build systems.
* Project is using [FAKE](https://github.com/fsharp/FAKE) -  automatic build system for building, running tests and pushing solution into GitHub
* Project is using [Paket](https://github.com/fsprojects/Paket) - package dependency manager  with support for NuGet packages, GitHub repositories and arbitrary source files in the internet.
* Project is using [FsUnit](https://github.com/fsprojects/FsUnit) which provides functional API on top of chosen test runner engine (in this case NUnit)

#Persistence Layer
Project is using [EventStore](https://github.com/EventStore/EventStore) database which is working very well in CQRS applications.

* Download EventStore from [http://geteventstore.com/downloads/](http://geteventstore.com/downloads/) and extract it somewhere on the PC
* Run database from command line / powershell consele using: `.\EventStore.ClusterNode.exe --db ./db --log ./logs --run-projections=all`
*  Use internet browser to access database web ui [http://127.0.0.1:2113/](http://127.0.0.1:2113/) login: admin, password: changeit
*  Go to projection tab and enable all projections
*  Create projections defined in src/FSharpRestaurantSample/Projections folder - allow emit, enable, type: continuous, name same as projection name without extension

#Contribution Guide

* Run `build.cmd` file to download all external libraries and ensure project is building correctly
* `build.cmd` also runs unit tests
* Describe changes in `RELEASE_NOTES.md` file.
* Publish changes to GitHub using `build.cmd Publish` command