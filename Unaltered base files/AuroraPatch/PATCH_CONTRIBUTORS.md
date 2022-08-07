# Contributing to AuroraPatch or other Patches

THIS DOCUMENT IS A WORK-IN-PROGRESS

## Creating/Altering Patches

This is a snippet taken from a Discord conversation around creating one's own patch.

```
The easiest way to get up-and-running in order to create/alter a patch is probably this:
1) Clone a patch and open the solution/project file in Visual Studio
2) In the Solution Explorer on the right, expand the Project Name > Reference tree and see if any references are missing
    For Locale, you'll probably be missing a reference to 0Harmony and AuroraPatch. You'll want to add those in manually by right-clicking References > Add Reference and browsing to the location of those files (just needs to point to the existing 0Harmony.dll and AuroraPatch.exe that comes with the AuroraPatch download)
3) You should now be able to compile the patch (it'll show up in the project directory under \bin\Debug\ or \bin\Release\ depending on which way you're building
4) Take the files generated under \bin\Debug\ or \bin\Release\ and dump them in a new folder in your <AuroraFolder>\Patches\<ProjectName>\ directory
5) Fire up AuroraPatch and you should see your new patch in the list(edited)

There's a few configuration changes you can make that make your life easier when debugging (like pointing the references from other project build directories, setting build output to be inside the Aurora patches folder directly so you don't have to manually move them every time, and debugging your patches by running the AuroraPatch project directly in debug mode). But I'll have to hash those out in a long-form tutorial.
```

Stay tuned for future documentation.
