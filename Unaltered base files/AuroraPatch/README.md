# AuroraPatch

This allows patching of the Aurora executable, it supports Harmony. The patcher works with any Aurora version.

## For users

Grab the latest zip from the [releases page](https://github.com/Aurora-Modders/AuroraPatch/releases). Extract AuroraPatch.zip in Aurora's folder. Patches go into their own `\Patches\{name}\` subfolder where `name` is the name of the patch. Start the game by running AuroraPatch.exe.

## For patch creators

Your patch should be a Class Library targeting the .Net 4 Framework (same as Aurora itself). You create a patch by extending the `AuroraPatch.Patch` class. See the Example project.

When working on a patch, there may be patch-specific instructions on how to build/contribute to the project.

Otherwise, please see the [PATCH_CONTRIBUTORS.md](/PATCH_CONTRIBUTORS.md) file for help on how to get up and running.

### Lib

The Lib patch is a patch intended to provide services to other patches, a bit like HugsLib in RimWorld modding. In particular it attempts to provide methods for patch authors to interact with Aurora which are robust to Aurora's ever-changing code obfuscation. Patch creators are encouraged to add any deobfuscation knowledge they uncover to Lib's KnowledgeBase to make it available to other patchers, thereby minimizing the instances where patch creators have to do the same deobfuscation work twice.

## For contributors

AuroraPatch is the main patcher application, it takes over and runs the Aurora game itself. AuroraPatch is designed to be as robust as possible against Aurora updates, any code which may stop working when Aurora updates belongs in Lib and not in AuroraPatch.