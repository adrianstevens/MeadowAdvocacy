# MeadowAdvocacy — Claude Code Instructions

## Project Overview
Collection of Meadow experiment/demo projects, owned by the primary maintainer of Meadow.Foundation. Each project demonstrates a visual effect or hardware capability on Meadow-supported boards.

## Project Structure Conventions

### Core/Head Separation
- Shared logic lives in a `.Core` project targeting `netstandard2.1`
- Hardware-specific heads reference the Core project (e.g., `.Juego`, `.ProjectLab`, `.Silk`)
- Each head contains `MeadowApp`, display initialization, and hardware wiring

### Juego Head csproj Template
```xml
<Project Sdk="Meadow.Sdk/1.1.0">
  <PropertyGroup>
    <AssemblyName>App</AssemblyName>
    <OutputType>Library</OutputType>
    <TargetFramework>net7.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Juego\Source\Juego\Juego.csproj" />
    <ProjectReference Include="..\<ProjectName>.Core\<ProjectName>.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Meadow.F7" Version="*" />
  </ItemGroup>
</Project>
```

### Namespace Rules
Namespace must **not** shadow type names. Use a combined name for heads:
- `StarfieldJuego` not `Starfield.Juego` (avoids clashing with `Juego` class)
- `MystifyJuego` not `Mystify.Juego`

### Config & Deployment
- Every F7 project needs `meadow.config.yaml` with:
  ```yaml
  MonoControl:
    Options: --jit
  ```
- Build validation: `dotnet build` before deploying
- Deploy: `meadow app run -c release` (stays open for serial output, never returns)

## Performance Patterns for STM32F7

### Floating Point
- F7 has **single-precision FPU only** — use `float` not `double` for all math
- `TotalSeconds` returns `double` — cast to `float` immediately: `(float)stopwatch.Elapsed.TotalSeconds`
- Use `MathF` not `Math` (avoids double promotion)

### Integer Math for Pixels
- Use `int` for pixel coordinates, not `double` or `float`
- Cast to `int` at assignment, not inside draw calls

### Constants and Caching
- Prefer `const` over `readonly` for values known at compile time — compiler folds constants
- Cache array references and property values as locals in tight loops
- Pre-compute lookup tables (e.g., Color LUT indexed by intensity) instead of constructing objects per-frame

### Drawing
- Use `DrawLine` not `DrawLineAntialiased` — AA is extremely expensive on F7
- `graphics.Clear()` + full redraw + `Show()` is faster than selective erase-and-redraw for full-screen updates (SPI transfer time dominates)

### SPI Transfer Ceiling
- SPI transfer is the hard ceiling: ~21-22 FPS for 320x240 RGB444
- Once you hit this, further software optimization won't improve FPS

### FPS Counter Pattern
```csharp
Stopwatch sw = Stopwatch.StartNew();
int frameCount = 0;
string fpsText = "";

// In render loop:
frameCount++;
if (sw.ElapsedMilliseconds >= 1000)
{
    fpsText = $"{frameCount} fps";
    frameCount = 0;
    sw.Restart();
}
```
