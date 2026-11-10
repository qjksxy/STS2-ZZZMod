using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Daze;

namespace ZZZMod.Code.Powers;

/// <summary>
///     协奏狙杀 —— 由「协奏狙杀」施加。
///     打出单目标攻击牌时，额外对目标造成1点失衡。
///     若目标失衡值已满或处于失衡状态时，则额外对目标造成5点伤害。
/// </summary>
[RegisterPower]
public sealed class HarmonizingShotPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/harmonizing_shot.png",
        BigIconPath: "res://ZZZMod/images/powers/harmonizing_shot_big.png"
    );

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只在玩家打出卡牌时触发
        if (cardPlay.Player != Owner.Player) return;
        // 排除自动打出的牌（连携等）
        if (cardPlay.IsAutoPlay) return;
        // 只对单目标攻击牌生效
        if (cardPlay.Card.Type != CardType.Attack) return;

        var target = cardPlay.Target;
        if (target == null || target.Side != CombatSide.Enemy || !target.IsAlive) return;

        // 检查目标是否失衡值已满或处于失衡状态
        bool isDazedOrFull = DazeStore.TryGet(target, out var daze) && (daze.IsFull || daze.IsDazed);

        if (isDazedOrFull)
        {
            // 额外对目标造成5点伤害
            await CreatureCmd.Damage(choiceContext, target, 5,
                MegaCrit.Sts2.Core.ValueProps.ValueProp.Move, Owner, null, null);
        }
        else
        {
            // 额外对目标造成1点失衡
            DazeStore.Get(target).AddDaze(1);
        }
    }
}
