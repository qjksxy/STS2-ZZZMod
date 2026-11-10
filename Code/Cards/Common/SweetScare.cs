using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     甜蜜惊吓 —— 1费技能，获得格挡。
///     若目标意图为攻击，则给予其1层「虚弱」；否则给予其1层「易伤」。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SweetScare() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(7, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 目标意图为攻击 → 虚弱；否则 → 易伤
        if (target.Monster?.IntendsToAttack == true)
        {
            await ApplyPowerTo<WeakPower>(choiceContext, target, 1, cardPlay.Card);
        }
        else
        {
            await ApplyPowerTo<VulnerablePower>(choiceContext, target, 1, cardPlay.Card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);   // 强防列
    }
}
