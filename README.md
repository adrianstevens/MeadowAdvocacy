# MeadowAdvocacy

Advocacy sample apps for [Wilderness Labs Meadow](https://www.wildernesslabs.co/) — visual demos and experiments targeting Meadow-supported hardware.

## Project Structure

Each demo follows a **Core/Head** separation pattern:

```
ProjectName/
├── ProjectName.Core/       # Shared logic (netstandard2.1)
├── ProjectName.Juego/      # Juego hardware head
├── ProjectName.ProjectLab/ # Project Lab hardware head
└── ProjectName.Silk/       # Silk hardware head (optional)
```

- **Core** contains all rendering logic, simulation state, and math — no hardware dependencies.
- **Heads** contain `MeadowApp<T>`, display initialization, and hardware wiring. They reference the Core project and a board-specific project (e.g., `Juego.csproj`).

### Creating a New Head

1. Create the head project directory (e.g., `MyDemo.Juego/`)
2. Use `Meadow.Sdk/1.1.0` as the SDK, with `AssemblyName=App` and `OutputType=Library`
3. Reference the board project (e.g., `..\..\Juego\Source\Juego\Juego.csproj`) and the Core project
4. Use a non-shadowing namespace: `MyDemoJuego`, not `MyDemo.Juego`
5. Add `meadow.config.yaml` with `MonoControl: Options: --jit`

### Build & Deploy

```bash
# Build
dotnet build

# Deploy (stays open for serial output)
meadow app run -c release
```

## Demos

| Project | Description | Hardware |
|---------|-------------|----------|
| Starfield | 3D starfield warp effect | Juego, ProjectLab |
| Mystify | Bouncing polygon screensaver | Juego, ProjectLab, Silk |
| GameOfLife | Conway's Game of Life | ProjectLab |
| RotatingCube | Wireframe 3D cube with accelerometer control | ProjectLab |
| FallingSand | Particle sand simulation — tilt to control gravity | Juego |
| Froggit | Frogger-style game | Juego |
| RogueLike | Roguelike dungeon game | Juego |
| Skeeball | Skeeball game | Juego |
| Gradients | Color gradient renderer | Juego |
| Arcs | Arc drawing demo | Juego |
| Mystify | Bouncing polygon screensaver | Juego, ProjectLab, Silk |

## Performance Guide for F7 / Juego

The Meadow F7 uses an STM32F7 microcontroller. Key constraints and patterns for getting smooth frame rates:

### Use Single-Precision Float

The F7 FPU is single-precision only. All math should use `float`, `MathF`, and cast `double`-returning APIs (like `TotalSeconds`) to `float` immediately.

### Integer Pixel Coordinates

Use `int` for all pixel positions. Floating-point pixel math wastes cycles and precision.

### Pre-compute, Don't Allocate

- Use `const` for compile-time values (enables constant folding)
- Build lookup tables (e.g., color palettes) at startup instead of constructing objects per frame
- Cache array references and properties as locals in tight loops

### Drawing Tips

- **Avoid anti-aliased drawing** — `DrawLineAntialiased` is extremely expensive; use `DrawLine`
- **Full-screen clear + redraw** is faster than selective erase-and-redraw because SPI transfer time dominates
- SPI transfer ceiling for 320x240 RGB444 is ~21-22 FPS — once you're there, software won't improve it further

### FPS Measurement

Use a `Stopwatch` + frame counter. Update the FPS string once per second to avoid string allocation overhead.
