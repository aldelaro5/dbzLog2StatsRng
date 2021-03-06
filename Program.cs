using System.Globalization;
using System.Text;

namespace Log2Rng
{
  class Program
  {
    enum Character
    {
      Goham = 0,
      Piccolo,
      Vegeta,
      Trunks,
      Goku,
      Hercules
    }

    enum NpcDirection
    {
      Down = 0,
      Up,
      Left,
      Right
    }

    enum ManipPreset
    {
      Trunks1 = 0,
      Piccolo,
      Trunks2,
      MANUAL
    }

    struct RngSeed
    {
      public uint seed1;
      public uint seed2;
    }

    struct StatsBoostRange
    {
      public short min;
      public short max;
    }

    struct CharacterLevelUpBoosts
    {
      public StatsBoostRange str;
      public StatsBoostRange ene;
      public StatsBoostRange def;
    }

    struct CharacterStats
    {
      public CharacterStats()
      {
        maxHpCoefficient = 1.0m;
        maxEp = 0;
        str = 0;
        ene = 0;
        def = 0;
      }

      public decimal maxHpCoefficient;
      public ushort maxEp;
      public ushort str;
      public ushort ene;
      public ushort def;
    }

    struct ManipParams
    {
      public Character character;
      public int levelFrom;
      public int levelTo;
      public int nbrExcessRolls;
    }

    static Dictionary<Character, CharacterLevelUpBoosts> allCharacterStatsBoostRanges = new Dictionary<Character, CharacterLevelUpBoosts>
    {
      {
        Character.Goham, new CharacterLevelUpBoosts
        {
          str = new StatsBoostRange {min = 0x13E, max = 0x206},
          ene = new StatsBoostRange {min = 0x12E, max = 0x1F6},
          def = new StatsBoostRange {min = 0x100, max = 0x1C8}
        }
      },
      {
        Character.Piccolo, new CharacterLevelUpBoosts
        {
          str = new StatsBoostRange {min = 0xE0, max = 0x1A8},
          ene = new StatsBoostRange {min = 0xF5, max = 0x1BD},
          def = new StatsBoostRange {min = 0xD0, max = 0x198}
        }
      },
      {
        Character.Vegeta, new CharacterLevelUpBoosts
        {
          str = new StatsBoostRange {min = 0xEA, max = 0x1B2},
          ene = new StatsBoostRange {min = 0xDB, max = 0x1A3},
          def = new StatsBoostRange {min = 0xF5, max = 0x1BD}
        }
      },
      {
        Character.Trunks, new CharacterLevelUpBoosts
        {
          str = new StatsBoostRange {min = 0x11F, max = 0x1E7},
          ene = new StatsBoostRange {min = 0xAC, max = 0x174},
          def = new StatsBoostRange {min = 0xB6, max = 0x17E}
        }
      },
      {
        Character.Goku, new CharacterLevelUpBoosts
        {
          str = new StatsBoostRange {min = 0xFF, max = 0x1C7},
          ene = new StatsBoostRange {min = 0xFF, max = 0x1C7},
          def = new StatsBoostRange {min = 0xC6, max = 0x18E}
        }
      },
      {
        Character.Hercules, new CharacterLevelUpBoosts
        {
          str = new StatsBoostRange {min = 0x40, max = 0xC0},
          ene = new StatsBoostRange {min = 0x40, max = 0xC0},
          def = new StatsBoostRange {min = 0x40, max = 0xC0}
        }
      }
    };

    static Dictionary<ManipPreset, ManipParams> allManipPresets = new Dictionary<ManipPreset, ManipParams>
    {
      {
        ManipPreset.Trunks1, new ManipParams
        {
          character = Character.Trunks,
          levelFrom = 1,
          levelTo = 6,
          nbrExcessRolls = 2
        }
      },
      {
        ManipPreset.Piccolo, new ManipParams
        {
          character = Character.Piccolo,
          levelFrom = 1,
          levelTo = 10,
          nbrExcessRolls = 16
        }
      },
      {
        ManipPreset.Trunks2, new ManipParams
        {
          character = Character.Trunks,
          levelFrom = 6,
          levelTo = 27,
          // There are JUST for the sprites on file select
          nbrExcessRolls = 3
        }
      },
    };

    const double gbaFramerate = 59.72750056960583;

    const int nbrFramesFadeIn = 9;
    const int nbrFramesFadeOut = 33;

    const int nbrFramesTrunks2ManipFirstScreen = 286;

    static uint Rng(ref RngSeed seed)
    {
      seed.seed1 = seed.seed1 + 0x2cbbc71 ^ seed.seed2;
      seed.seed2 = (seed.seed2 >> 0x1d | seed.seed2 << 3) + 0x632d80f ^ seed.seed1;
      return seed.seed1 - seed.seed2;
    }

    // Returns a random short between 0 and max exclusive
    static ushort RngShort(ref RngSeed seed, ushort max)
    {
      return (ushort)(max * (Rng(ref seed) & 0xFFFF) >> 0x10);
    }

    static bool RngChanceOver65536(ref RngSeed seed, ushort chance)
    {
      return (ushort)(Rng(ref seed) & 0xFFFF) < chance;
    }

    static ushort levelUpStatsByRange(ref RngSeed seed, StatsBoostRange range, int levelAmount, ushort stat)
    {
      ushort nbrInRange = RngShort(ref seed, (ushort)(levelAmount * ((range.max - range.min) + 1)));
      ushort newStat = (ushort)(nbrInRange + levelAmount * range.min + stat);
      if (newStat >= 0x6400)
        newStat = 0x6400;
      return newStat;
    }

    static CharacterStats LevelUpCharacterStatsFromLevelToLevel(RngSeed seed, CharacterStats stats, Character character, int levelFrom, int levelTo)
    {
      CharacterStats newStats = new CharacterStats();
      int levelAmount = levelTo - levelFrom;
      newStats.str = levelUpStatsByRange(ref seed, allCharacterStatsBoostRanges[character].str, levelAmount, stats.str);
      newStats.ene = levelUpStatsByRange(ref seed, allCharacterStatsBoostRanges[character].ene, levelAmount, stats.ene);
      newStats.def = levelUpStatsByRange(ref seed, allCharacterStatsBoostRanges[character].def, levelAmount, stats.def);
      newStats.maxHpCoefficient = stats.maxHpCoefficient;
      newStats.maxEp = stats.maxEp;
      if (levelFrom < levelTo)
      {
        do
        {
          // This will increase the max HP by a percentage between 5.5% and 7.5%
          // Since we are just showing the delta, we just cumulate the coefficents
          newStats.maxHpCoefficient *= 1 + (decimal)(RngShort(ref seed, 0x51f) + 0xe14) / 65536m;
          newStats.maxEp += (ushort)(RngShort(ref seed, 3) + 2);
          levelFrom++;
        } while (levelFrom < levelTo);
      }
      return newStats;
    }

    static bool MrPopoDirectionCheck(ref RngSeed seed, ref NpcDirection direction)
    {
      // This is a less than 2% check
      if (RngChanceOver65536(ref seed, 0x4c0))
      {
        int newDirection = RngShort(ref seed, 5);
        if (newDirection != 4)
          direction = (NpcDirection)newDirection;
        return true;
      }
      return false;
    }

    // Trunks2 is special because there is an npc called Mr Popo that has a random walk cycle
    // which must be accounted for in the simulations. It goes like this: for every frame,
    // we try to walk with a certain chance defined by what seems to be the npc (less than 2% here)
    // and if it passes, the npc will generate a random direction (one is the current direction)
    // and if they can walk there, they will walk for 52 frames at which point no RNG will be called.
    // After the walk cycle is done, a frame is burned for the sprite idle RNG call and the cycle repeats.
    // The reason the NPC might not be able to walk has to do with either walls or its boundaries.
    static void RollRngThroughTrunks2FirstScreen(ref RngSeed seed)
    {
      NpcDirection currentDirection = NpcDirection.Down;
      int x = 0;
      int y = 0;
      int frameCount = 0;
      int noCheckTimeout = 0;
      while (frameCount < nbrFramesTrunks2ManipFirstScreen)
      {
        frameCount++;
        if (noCheckTimeout > 0)
        {
          noCheckTimeout--;
          // When we are done walking, burn the frame and simulate the sprite idle RNG call
          if (noCheckTimeout == 0)
          {
            frameCount++;
            Rng(ref seed);
          }
        }
        else if (MrPopoDirectionCheck(ref seed, ref currentDirection))
        {
          bool canWalk = false;
          // The boundaries of this NPC is set in such a way that he cannot walk more than
          // 1 tiles away from spawn vertically and not more than 2 away on the right.
          // He can walk a long distance on the left, enough to not care about it in this simulation
          switch (currentDirection)
          {
            case NpcDirection.Up:
              if (y < 1)
              {
                y++;
                canWalk = true;
              }
              break;
            case NpcDirection.Down:
              if (y > -1)
              {
                y--;
                canWalk = true;
              }
              break;
            case NpcDirection.Left:
              x--;
              canWalk = true;
              break;
            case NpcDirection.Right:
              if (x < 2)
              {
                x++;
                canWalk = true;
              }
              break;
          }

          if (canWalk)
            noCheckTimeout = 52;
        }
      }
    }

    struct ProgArgs
    {
      public ManipPreset preset;
      public ManipParams manipParams;
      public int nbrFrames;
      public RngSeed startingSeed;
    }

    static bool tryParseSeed(string strSeed, out RngSeed seed)
    {
      seed = new RngSeed();
      string[] seedParts = strSeed.Split('-');
      if (seedParts.Length != 2)
        return false;
      if (!uint.TryParse(seedParts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out seed.seed1))
        return false;
      if (!uint.TryParse(seedParts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out seed.seed2))
        return false;
      return true;
    }

    static bool TryParseArguments(string[] args, out ProgArgs progArgs)
    {
      progArgs = new ProgArgs();
      if (args.Length == 0 || (args.Length != 2 && args.Length != 3 && args.Length != 6))
        return false;

      if (!Enum.TryParse(args[0], true, out progArgs.preset))
        return false;

      if (args.Length == 2 || args.Length != 3)
      {
        if (progArgs.preset == ManipPreset.MANUAL)
          return false;

        if (!int.TryParse(args[1], out progArgs.nbrFrames))
          return false;
        if (progArgs.nbrFrames < 0)
          return false;

        progArgs.startingSeed.seed1 = 0;
        progArgs.startingSeed.seed2 = 0;
        if (args.Length == 3 && !tryParseSeed(args[1].ToUpper(), out progArgs.startingSeed))
          return false;

        progArgs.manipParams = allManipPresets[progArgs.preset];
        return true;
      }
      else
      {
        if (progArgs.preset != ManipPreset.MANUAL)
          return false;

        if (!Enum.TryParse(args[1], true, out progArgs.manipParams.character))
          return false;

        if (!int.TryParse(args[2], out progArgs.manipParams.levelFrom))
          return false;
        if (progArgs.manipParams.levelFrom < 0)
          return false;

        if (!int.TryParse(args[3], out progArgs.manipParams.levelTo))
          return false;
        if (progArgs.manipParams.levelTo < 0)
          return false;

        if (progArgs.manipParams.levelFrom >= progArgs.manipParams.levelTo)
          return false;

        progArgs.manipParams.nbrExcessRolls = 0;

        if (!int.TryParse(args[4], out progArgs.nbrFrames))
          return false;
        if (progArgs.nbrFrames < 0)
          return false;

        if (!tryParseSeed(args[5].ToUpper(), out progArgs.startingSeed))
          return false;
      }

      return true;
    }

    static void OutputStatsLine(int frame, RngSeed seed, CharacterStats stats, StringBuilder sb)
    {
      sb.Clear();
      sb.Append(frame + " (" + ((frame * (1 / gbaFramerate)) * 1000).ToString("0") + " ms)");
      sb.Append(";");
      sb.Append(seed.seed1.ToString("X8"));
      sb.Append("-");
      sb.Append(seed.seed2.ToString("X8"));
      sb.Append(";");
      sb.Append((stats.maxHpCoefficient * 100m).ToString("N6"));
      sb.Append(";");
      sb.Append(stats.maxEp);
      sb.Append(";");
      sb.Append(stats.str.ToString("X4"));
      sb.Append(";");
      sb.Append(stats.ene.ToString("X4"));
      sb.Append(";");
      sb.Append(stats.def.ToString("X4"));
      sb.Append(";");
      sb.Append(stats.maxHpCoefficient + stats.maxEp + stats.str + stats.ene + stats.def);
      Console.WriteLine(sb.ToString());
    }

    static void printHelp()
    {
      Console.WriteLine("Syntax: dbzLog2StatsRng preset nbrFrames [startingSeed]");
      Console.WriteLine("        dbzLog2StatsRng 'manual' character fromLevel toLevel nbrFrames startingSeed\n");
      Console.WriteLine("Output the CSV data of the delta stats generated in Dragon Ball Z: The legacy of Goku 2 from a");
      Console.WriteLine("preset RTA manip optionally starting from startingSeed (0-0 by default) or for character going from level"); 
      Console.WriteLine("fromLevel to level toLevel for nbrFrames frames starting at startingSeed. The startingSeed");
      Console.WriteLine("has the following format: seed1-seed2 where both seed1 and seed2 are hexadecimal numbers. The");
      Console.WriteLine("preset format is more suited for RTA manips while manual is more suited for TASing.\n");
      Console.WriteLine("For the RTA preset format, assumes frame 0 is pressing A upon skipping the intro cutscene and");
      Console.WriteLine("the frame output is when pressing A on file select\n");
      Console.WriteLine("Valid presets are:");
      Console.WriteLine("trunks1");
      Console.WriteLine("piccolo");
      Console.WriteLine("trunks2\n");
      Console.WriteLine("For the manual format, all parameters and a starting seed must be provided without the ability");
      Console.WriteLine("to have excess calls\n");
      Console.WriteLine("Valid characterId are:");
      Console.WriteLine("goham");
      Console.WriteLine("piccolo");
      Console.WriteLine("vegeta");
      Console.WriteLine("trunks");
      Console.WriteLine("goku");
      Console.WriteLine("hercules\n");
    }

    static void Main(string[] args)
    {
      ProgArgs progArgs = new ProgArgs();
      if (!TryParseArguments(args, out progArgs) || (args.Length == 1 && (args[0] == "-h" || args[1] == "--help")))
      {
        printHelp();
        return;
      }

      RngSeed seed = progArgs.startingSeed;
      if (progArgs.preset != ManipPreset.MANUAL)
      {
        // How to read this: the frame at which flowtimer will start is when you press the button to skip the intro,
        // but this intro skip takes a few frames where no RNG calls happens which offsets all the timings, this is
        // the fade in factor. Additionally, the final beep occur when pressing A on file select, but right after,
        // there are additional frames of fade out where there are still RNG calls happening. They don't hinder the
        // manip, but they must be taken into account by moving all the frames backwards (since one frame = 1 call)
        int effectiveOffset = nbrFramesFadeIn - nbrFramesFadeOut;

        // We account for a fixed amount of calls before the generation is done, but this amount changes depending
        // on which generation we are simulating
        effectiveOffset -= progArgs.manipParams.nbrExcessRolls;

        // Burn through impossible seeds
        if (effectiveOffset < 0)
        {
          for (int i = effectiveOffset; i < 0; i++)
            Rng(ref seed);
        }
      }

      CharacterStats stats = new CharacterStats();
      StringBuilder sb = new StringBuilder();
      Console.WriteLine("Frame;Seeds;Max HP increase (%);Max EP;Str (hex);Ene (hex);Def (hex);Sum All");

      for (int i = 0; i < progArgs.nbrFrames; i++)
      {
        RngSeed actualSeed = seed;
        CharacterStats actualStats = stats;
        // Trunks2 has complications caused by the Mr Popo NPC that has a walk cycle influenced by RNG.
        // To properly track the seed, we need to simulate his RNG patterns and make a few assumptions namely
        // that the player will never run into him or that he won't try to run into the player all while the player
        // never releases the dpad and only goes up with the possibility to use the diuagonals
        if (progArgs.preset == ManipPreset.Trunks2)
        {
          for (int j = 0; j < 4; j++)
            Rng(ref actualSeed);

          RollRngThroughTrunks2FirstScreen(ref actualSeed);

          for (int j = 0; j < 12; j++)
            Rng(ref actualSeed);
        }
        actualStats = LevelUpCharacterStatsFromLevelToLevel(actualSeed, actualStats, progArgs.manipParams.character,
                                                            progArgs.manipParams.levelFrom, progArgs.manipParams.levelTo);
        OutputStatsLine(i, actualSeed, actualStats, sb);
        Rng(ref seed);
      }
    }
  }
}
