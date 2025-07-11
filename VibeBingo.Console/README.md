# VibeBingo.Console

This is the console application for the VibeBingo project. It uses the shared `VibeBingo.Core` library for all BINGO logic. Run this app to use a text-based BINGO caller with auto-call and TTS support (on Windows).

## How to Build and Run

1. Make sure you have .NET 9 SDK installed.
2. In the root of this project, run:
   ```
   dotnet build
   dotnet run --project VibeBingo.Console
   ```

## Features
- Start new BINGO rounds
- Auto-call balls with selectable delay and pause/resume
- Text-to-speech for called balls (Windows only)

## Project Structure
- `VibeBingo.Console` - Console app (this project)
- `VibeBingo.Core` - Shared BINGO logic
- `VibeBingo.Blazor` - Blazor WebAssembly UI (separate project)
