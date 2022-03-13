# dbzLog2StatsRng
Simulates the stats generation of characters in Dragon Ball Z: The legacy of Goku

## System Requirements
The only requirement is to have the .NET runtime installed. For Windows, you can install the latest version of .NET by following [this link](https://dotnet.microsoft.com/download) (note, you ideally want to install .NET, not .NET Core or .NET Framework). For Linux, refer to your distribution's documentation for proper installation of the runtime

## Usage
Syntax: dbzLog2StatsRng characterId fromLevel toLevel nbrFrames [-d | --delta

Output the CSV data of the stats generated in Dragon Ball Z: The legacy of Goku 2 for the
character characterId going from level fromLevel to level toLevel for nbrFrames frames.

Valid characterId are:

0. Goham
1. Unk1
2. Unk2
3. Trunks
4. Unk4
5. Unk5

The -d or --delta option will output only the delta from all stats set to 0
rather than from the character's base stats
