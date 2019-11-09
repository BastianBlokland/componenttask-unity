# Installation


### Add package
Add a reference to this repository to your package dependencies (`Packages/manifest.json`)
```
"dependencies": {
"com.bastianblokland.componenttask": "https://github.com/BastianBlokland/componenttask-unity.git#v1.8",
    ...
}
```

Use the tag at the end to specify the version to use.

Latest version:

[![GitHub release](https://img.shields.io/github/release/BastianBlokland/componenttask-unity.svg)](https://github.com/BastianBlokland/componenttask-unity/releases/)

Avoid adding a git path without a tag as latest `master` is not guaranteed to be stable, also it will
make package resolving non-deterministic as every time you resolve `master` it might be a different
commit.


### Upgrading
To update to a newer version simply open your package manifest (`Packages/manifest.json`) and set
the tag at the end of the git url (for example `v1.8.1`) to a later version.
