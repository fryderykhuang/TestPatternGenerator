# TestPatternGenerator

Test pattern generator mainly for calibration of CRT monitors.

## Motivation

1. Most existing test pattern generators for monitor calibration is not Hi-DPI awareness, will be scaled by Windows automatically, result in blurry polygon edge and fonts.
2. As a hands-on WinForm practice session for me.

## Highlights

I'm quite satisfied by the ease of use and accurary of the gamma estimation function, please check it out.

## Building

### Dependencies

 .NET SDK 7.x  (https://dotnet.microsoft.com/en-us/download)

### Procedure

To get the Native AOT build, just go in the `src` folder, run

`dotnet publish -c Release -r win-x64`

In the `publish` folder, only the single `.exe` is needed to run the program.

## Things to do in the future

1. Localization
2. Add more calibration patterns
