# dbzLog2StatsRng
Simulates the stats generation of characters in Dragon Ball Z: The legacy of Goku

## System Requirements
The only requirement is to have the .NET runtime installed. For Windows, you can install the latest version of .NET by following [this link](https://dotnet.microsoft.com/download) (note, you ideally want to install .NET, not .NET Core or .NET Framework). For Linux, refer to your distribution's documentation for proper installation of the runtime

## Usage
Syntax: 

```dbzLog2StatsRng preset nbrFrames [startingSeed]```

```dbzLog2StatsRng 'manual' character fromLevel toLevel nbrFrames startingSeed```

Output the CSV data of the delta stats generated in Dragon Ball Z: The legacy of Goku 2 from a preset RTA manip optionally starting from startingSeed (0-0 by default) or for character going from level fromLevel to level toLevel for nbrFrames frames starting at startingSeed. The startingSeed has the following format: seed1-seed2 where both seed1 and seed2 are hexadecimal numbers. The preset format is more suited for RTA manips while manual is more suited for TASing.

For the RTA preset format, assumes frame 0 is pressing A upon skipping the intro cutscene and
the frame output is when pressing A on file select

Valid presets are:
- trunks1
- piccolo
- trunks2

For the manual format, all parameters and a starting seed must be provided without the ability
to have excess calls

Valid characterId are:
- goham
- piccolo
- vegeta
- trunks
- unk4
- unk5
