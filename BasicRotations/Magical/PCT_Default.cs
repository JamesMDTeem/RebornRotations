using System.ComponentModel;

namespace DefaultRotations.Magical;

[Rotation("Default", CombatType.PvE, GameVersion = "7.0", Description = "Updated Pictomancer rotation based on new guidelines")]
[SourceCode(Path = "main/DefaultRotations/Magical/PCT_Default.cs")]
[Api(3)]
public sealed class PCT_Default : PictomancerRotation
{
    public override MedicineType MedicineType => MedicineType.Intelligence;
    public static IBaseAction RainbowPrePull { get; } = new BaseAction((ActionID)34688);

    [RotationConfig(CombatType.PvE, Name = "Use HolyInWhite or CometInBlack while moving")]
    public bool HolyCometMoving { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Use swiftcast on")]
    public MotifSwift MotifSwiftCast { get; set; } = MotifSwift.NoMotif;

    [RotationConfig(CombatType.PvE, Name = "Burst Strategy")]
    public BurstStrategy BurstStrategySelection { get; set; } = BurstStrategy.SingleMuse;

    public enum MotifSwift : byte
    {
        [Description("CreatureMotif")] CreatureMotif,
        [Description("WeaponMotif")] WeaponMotif,
        [Description("LandscapeMotif")] LandscapeMotif,
        [Description("AllMotif")] AllMotif,
        [Description("NoMotif(ManualSwifcast)")] NoMotif
    }

    public enum BurstStrategy : byte
    {
        [Description("Single Muse Burst")] SingleMuse,
        [Description("Triple Muse Burst")] TripleMuse
    }

    #region Countdown logic
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= 3 && RainbowPrePull.CanUse(out var act))
        {
            return act;
        }

        // Prepare Motifs
        if (!CreatureMotifDrawn && CreatureMotifPvE.CanUse(out act)) return act;
        if (!WeaponMotifDrawn && HammerMotifPvE.CanUse(out act)) return act;
        if (!LandscapeMotifDrawn && StarrySkyMotifPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }
    #endregion

    #region Opener
    private bool ExecuteOpener(out IAction? act)
    {
        act = null;
        if (CombatTime > 30) return false;

        var opener = new IAction[]
        {
            RainbowDripPvE,
            StrikingMusePvE,
            StarryMusePvE,
            PomMotifPvE,
            SubtractivePalettePvE,
            BlizzardInCyanPvE,
            StoneInYellowPvE,
            ThunderInMagentaPvE,
            CometInBlackPvE,
            PomMusePvE,
            StarPrismPvE,
            HammerStampPvE,
            HammerBrushPvE,
            PolishingHammerPvE
        };

        foreach (var action in opener)
        {
            if (CanUse(action, out act))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Burst Window
    private bool ExecuteSingleMuseBurst(out IAction? act)
    {
        act = null;
        if (!Player.HasStatus(true, StatusID.StarryMuse)) return false;

        var burst = new IAction[]
        {
            StarryMusePvE,
            SubtractivePalettePvE,
            CometInBlackPvE,
            StoneInYellowPvE,
            ThunderInMagentaPvE,
            CometInBlackPvE,
            LivingMusePvE, // This should be the appropriate Creature Muse
            StarPrismPvE,
            HammerStampPvE,
            HammerBrushPvE,
            PolishingHammerPvE
        };

        foreach (var action in burst)
        {
            if (CanUse(action, out act))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    // Helper method to check if an action can be used
    private bool CanUse(IAction action, out IAction? act)
    {
        act = null;
        if (action is BaseAction baseAction)
        {
            return baseAction.CanUse(out act, skipAoeCheck: true);
        }
        return false;
    }

    #region General Rotation
    protected override bool GeneralGCD(out IAction? act)
    {
        // Execute opener if in combat for less than 30 seconds
        if (CombatTime < 30 && ExecuteOpener(out act)) return true;

        // Execute burst window
        if (ExecuteSingleMuseBurst(out act)) return true;

        // Basic combo
        if (WaterInBluePvE.CanUse(out act)) return true;
        if (AeroInGreenPvE.CanUse(out act)) return true;
        if (FireInRedPvE.CanUse(out act)) return true;

        // Subtractive combo
        if (PaletteGauge >= 50 && SubtractivePalettePvE.CanUse(out act)) return true;
        if (Player.HasStatus(true, StatusID.SubtractivePalette))
        {
            if (ThunderInMagentaPvE.CanUse(out act)) return true;
            if (StoneInYellowPvE.CanUse(out act)) return true;
            if (BlizzardInCyanPvE.CanUse(out act)) return true;
        }

        // Movement options
        if (IsMoving && HolyCometMoving)
        {
            if (CometInBlackPvE.CanUse(out act)) return true;
            if (HolyInWhitePvE.CanUse(out act)) return true;
        }

        // Prepare Motifs
        if (!CreatureMotifDrawn && CreatureMotifPvE.CanUse(out act)) return true;
        if (!WeaponMotifDrawn && HammerMotifPvE.CanUse(out act)) return true;
        if (!LandscapeMotifDrawn && StarrySkyMotifPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }
    #endregion

    #region oGCD Logic
    protected override bool AttackAbility(IAction nextGCD, out IAction? act)
    {
        // Use Muses
        if (StarryMusePvE.CanUse(out act)) return true;
        if (StrikingMusePvE.CanUse(out act)) return true;
        if (LivingMusePvE.CanUse(out act)) return true;

        // Use Portraits
        if (RetributionOfTheMadeenPvE.CanUse(out act)) return true;
        if (MogOfTheAgesPvE.CanUse(out act)) return true;

        // Other oGCDs
        if (SubtractivePalettePvE.CanUse(out act)) return true;

        return base.AttackAbility(nextGCD, out act);
    }
    #endregion

    #region Defensive Utility
    protected override bool DefenseAreaAbility(IAction nextGCD, out IAction? act)
    {
        if (TemperaCoatPvE.CanUse(out act)) return true;
        if (TemperaGrassaPvE.CanUse(out act)) return true;
        return base.DefenseAreaAbility(nextGCD, out act);
    }
    #endregion

    #region Movement
    [RotationDesc(ActionID.SmudgePvE)]
    protected override bool MoveForwardAbility(IAction nextGCD, out IAction? act)
    {
        if (SmudgePvE.CanUse(out act)) return true;
        return base.MoveForwardAbility(nextGCD, out act);
    }
    #endregion
}