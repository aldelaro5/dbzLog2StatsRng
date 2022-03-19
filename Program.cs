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
      Unk4,
      Unk5,
      COUNT
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
      public ushort maxHp;
      public ushort maxEp;
      public ushort str;
      public ushort ene;
      public ushort def;
    }

    struct CharacterData
    {
      public CharacterLevelUpBoosts levelUpBoosts;
      public CharacterStats baseStats;
    }

    static Dictionary<Character, CharacterData> allCharacterData = new Dictionary<Character, CharacterData>
    {
      {
        Character.Goham, new CharacterData
        {
          levelUpBoosts = new CharacterLevelUpBoosts
          {
            str = new StatsBoostRange {min = 0x13E, max = 0x206},
            ene = new StatsBoostRange {min = 0x12E, max = 0x1F6},
            def = new StatsBoostRange {min = 0x100, max = 0x1C8}
          },
          baseStats = new CharacterStats
          {
            maxHp = 85,
            maxEp = 20,
            str = 0x300,
            ene = 0x500,
            def = 0x300
          }
        }
      },
      {
        Character.Piccolo, new CharacterData
        {
          levelUpBoosts = new CharacterLevelUpBoosts
          {
            str = new StatsBoostRange {min = 0xE0, max = 0x1A8},
            ene = new StatsBoostRange {min = 0xF5, max = 0x1BD},
            def = new StatsBoostRange {min = 0xD0, max = 0x198}
          },
          baseStats = new CharacterStats
          {
            maxHp = 95,
            maxEp = 20,
            str = 0x400,
            ene = 0x400,
            def = 0x500
          }
        }
      },
      {
        Character.Vegeta, new CharacterData
        {
          levelUpBoosts = new CharacterLevelUpBoosts
          {
            str = new StatsBoostRange {min = 0xEA, max = 0x1B2},
            ene = new StatsBoostRange {min = 0xDB, max = 0x1A3},
            def = new StatsBoostRange {min = 0xF5, max = 0x1BD}
          },
          baseStats = new CharacterStats
          {
            maxHp = 105,
            maxEp = 20,
            str = 0x600,
            ene = 0x400,
            def = 0x600
          }
        }
      },
      {
        Character.Trunks, new CharacterData
        {
          levelUpBoosts = new CharacterLevelUpBoosts
          {
            str = new StatsBoostRange {min = 0x11F, max = 0x1E7},
            ene = new StatsBoostRange {min = 0xAC, max = 0x174},
            def = new StatsBoostRange {min = 0xB6, max = 0x17E}
          },
          baseStats = new CharacterStats
          {
            maxHp = 110,
            maxEp = 20,
            str = 0x600,
            ene = 0x300,
            def = 0x400
          }
        }
      },
      {
        Character.Unk4, new CharacterData
        {
          levelUpBoosts = new CharacterLevelUpBoosts
          {
            str = new StatsBoostRange {min = 0xFF, max = 0x1C7},
            ene = new StatsBoostRange {min = 0xFF, max = 0x1C7},
            def = new StatsBoostRange {min = 0xC6, max = 0x18E}
          },
          baseStats = new CharacterStats
          {
            maxHp = 100,
            maxEp = 20,
            str = 0x500,
            ene = 0x700,
            def = 0x600
          }
        }
      },
      {
        Character.Unk5, new CharacterData
        {
          levelUpBoosts = new CharacterLevelUpBoosts
          {
            str = new StatsBoostRange {min = 0x40, max = 0xC0},
            ene = new StatsBoostRange {min = 0x40, max = 0xC0},
            def = new StatsBoostRange {min = 0x40, max = 0xC0}
          },
          baseStats = new CharacterStats
          {
            maxHp = 50,
            maxEp = 20,
            str = 0x800,
            ene = 0x100,
            def = 0x600
          }
        }
      }
    };

    const double gbaFramerate = 59.72750056960583;

    const int nbrFramesFadeIn = 9;
    const int nbrFramesFadeOut = 33;

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
      newStats.str = levelUpStatsByRange(ref seed, allCharacterData[character].levelUpBoosts.str, levelAmount, stats.str);
      newStats.ene = levelUpStatsByRange(ref seed, allCharacterData[character].levelUpBoosts.ene, levelAmount, stats.ene);
      newStats.def = levelUpStatsByRange(ref seed, allCharacterData[character].levelUpBoosts.def, levelAmount, stats.def);
      newStats.maxHp = stats.maxHp;
      newStats.maxEp = stats.maxEp;
      if (levelFrom < levelTo)
      {
        do
        {
          // This will increase the max HP by a percentage between 5.5% and 7.5%
          newStats.maxHp += (ushort)((newStats.maxHp * (RngShort(ref seed, 0x51f) + 0xe14)) >> 0x10);
          newStats.maxEp += (ushort)(RngShort(ref seed, 3) + 2);
          levelFrom++;
        } while (levelFrom < levelTo);
      }
      return newStats;
    }

    struct ProgArgs
    {
      public Character character;
      public int levelFrom;
      public int levelTo;
      public int nbrFrames;
      public int nbrExcessRolls;
      public bool delta;
    }

    static bool TryParseArguments(string[] args, out ProgArgs progArgs)
    {
      progArgs = new ProgArgs();
      if (args.Length > 6)
        return false;

      if (args.Length > 5)
      {
        if (args[5] != "-d" && args[5] != "--delta")
          return false;
        progArgs.delta = true;
      }

      if (!Enum.TryParse(args[0], out progArgs.character))
        return false;
      if (progArgs.character < 0 || progArgs.character >= Character.COUNT)
        return false;

      if (!int.TryParse(args[1], out progArgs.levelFrom))
        return false;
      if (progArgs.levelFrom < 0)
        return false;

      if (!int.TryParse(args[2], out progArgs.levelTo))
        return false;
      if (progArgs.levelTo < 0)
        return false;

      if (progArgs.levelFrom >= progArgs.levelTo)
        return false;

      if (!int.TryParse(args[3], out progArgs.nbrFrames))
        return false;
      if (progArgs.nbrFrames < 0)
        return false;
      
      if (!int.TryParse(args[4], out progArgs.nbrExcessRolls))
        return false;
      if (progArgs.nbrExcessRolls < 0)
        return false;

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
      sb.Append(stats.maxHp);
      sb.Append(";");
      sb.Append(stats.maxEp);
      sb.Append(";");
      sb.Append(stats.str.ToString("X4"));
      sb.Append(";");
      sb.Append(stats.ene.ToString("X4"));
      sb.Append(";");
      sb.Append(stats.def.ToString("X4"));
      sb.Append(";");
      sb.Append(stats.maxHp + stats.maxEp + stats.str + stats.ene + stats.def);
      Console.WriteLine(sb.ToString());
    }

    static void printHelp()
    {
      Console.WriteLine("Syntax: dbzLog2StatsRng characterId fromLevel toLevel nbrFrames nbrExcessRolls [-d | --delta\n");
      Console.WriteLine("Output the CSV data of the stats generated in Dragon Ball Z: The legacy of Goku 2 for the");
      Console.WriteLine("character characterId going from level fromLevel to level toLevel for nbrFrames frames");
      Console.WriteLine("with nbrExcessRolls additional RNG calls after file select fade out. This assumes frame 0");
      Console.WriteLine("is pressing A upon skipping the intro cutscene and the frame output is when pressing A on file select\n");
      Console.WriteLine("Valid characterId are:");
      Console.WriteLine("0: Goham");
      Console.WriteLine("1: Piccolo");
      Console.WriteLine("2: Unk2");
      Console.WriteLine("3: Trunks");
      Console.WriteLine("4: Unk4");
      Console.WriteLine("5: Unk5\n");
      Console.WriteLine("The -d or --delta option will output only the delta from all stats set to 0");
      Console.WriteLine("rather than from the character's base stats");
    }

    static void Main(string[] args)
    {
      ProgArgs progArgs = new ProgArgs();
      if (!TryParseArguments(args, out progArgs) || (args.Length == 1 && (args[0] == "-h" || args[1] == "--help")))
      {
        printHelp();
        return;
      }

      RngSeed seed = new RngSeed { seed1 = 0, seed2 = 0 };
      // How to read this: the frame at which flowtimer will start is when you press the button to skip the intro,
      // but this intro skip takes a few frames where no RNG calls happens which offsets all the timings, this is
      // the fade in factor. Additionally, the final beep occur when pressing A on file select, but right after,
      // there are additional frames of fade out where there are still RNG calls happening. They don't hinder the
      // manip, but they must be taken into account by moving all the frames backwards (since one frame = 1 call)
      int effectiveOffset = nbrFramesFadeIn - nbrFramesFadeOut;
      
      // We account for a fixed amount of calls before the generation is done, but this amount changes depending
      // on which generation we are simulating
      effectiveOffset -= progArgs.nbrExcessRolls;

      // Burn through impossible seeds
      if (effectiveOffset < 0)
      {
        for (int i = effectiveOffset; i < 0; i++)
          Rng(ref seed);
      }

      CharacterStats baseStats = progArgs.delta ? new CharacterStats() : allCharacterData[progArgs.character].baseStats;
      CharacterStats stats = baseStats;
      StringBuilder sb = new StringBuilder();
      Console.WriteLine("Frame;Seeds;Max HP;Max EP;Str (hex);Ene (hex);Def (hex);Sum All");

      for (int i = 0; i < progArgs.nbrFrames; i++)
      {
        stats = LevelUpCharacterStatsFromLevelToLevel(seed, stats, progArgs.character, progArgs.levelFrom, progArgs.levelTo);
        OutputStatsLine(i, seed, stats, sb);
        Rng(ref seed);
        stats = baseStats;
      }
    }
  }
}
