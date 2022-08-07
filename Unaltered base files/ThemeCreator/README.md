# ThemeCreator

An AuroraPatch helper patch primarily used by theme developers to ease the theme creation process.

## Usage

This patch doesn't do anything on its own. It's meant to be used as a dependency of a proper theme patch.
It exposes a handful of helper methods to adjust Aurora colors:

```c#
AddColorChange(Color current, Color swap)
AddColorChange(Type type, ColorChange colorChange)
AddColorChange(string name, ColorChange colorChange)
AddColorChange(Regex regex, ColorChange colorChange)
AddColorChange(Func<Control, bool> predicate, ColorChange colorChange)
AddFontChange(Font font)
AddFontChange(Type type, Font font)
AddFontChange(string name, Font font)
AddFontChange(Regex regex, Font font)
AddFontChange(Func<Control, bool> predicate, Font font)
AddImageChange(Func<Control, bool> predicate, Image image)
AddImageChange(AuroraButton auroraButton, Image image)
AddImageChange(AuroraButton auroraButton, string imagePath)
DrawEllipsePrefixAction(Action<Graphics, Pen> action)
FillEllipsePrefixAction(Action<Graphics, Brush> action)
SetOrbitColor(Color color)
SetPlanetColor(Color color)
SetStarSystemColor(Color color)
SetStarColor(Color color)
SetLagrangePointColor(Color color)
DrawStringPrefixAction(Action<Graphics, string, Font, Brush> action)
DrawLinePrefixAction(Action<Graphics, Pen> action)
SetDistanceRulerLineColor(Color color)
SetCometTailColor(Color color)
SetMapTextColor(Color color)
```

## Example

See the [T2DTheme](https://github.com/Aurora-Modders/T2DTheme) for example usage.

## Developers

In order to build this library, you will need to update the project references so the dependencies can be found.

0Harmony should be pulled from NuGet automatically.

AuroraPatch.exe and Lib.dll >= 0.1.2 will need to be referenced.

If creating your own theme, you'll need to reference this project's DLL to your new theme project.
