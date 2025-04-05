using HPHelper;

namespace NewGameMode
{
    public class HealthPatch
    {
        [HPHelper(typeof(AgentModel), nameof(AgentModel.OnDie))]
        [HPPrefix]
        public static bool AgentModel_OnDie(ref AgentModel __instance)
        {
            if (GlobalGameManager.instance.gameMode != Harmony_Patch.rougeLike)
            {
                return true;
            }
            if (__instance.DeadType == DeadType.EXECUTION)
            {
                return true;
            }
            if (HealthManager.Instance.GetHealth() > 0)
            {
                HealthManager.Instance.AddHealth(-1);
                AngelaConversationUI.instance.AddAngelaMessage(string.Format(LocalizeTextDataModel.instance.GetText("HealthManager_Relive"), __instance._agentName.GetName(), HealthManager.Instance.GetHealth()));
                __instance.hp = __instance.maxHp;
                __instance.AddUnitBuf(new Health_ReliveBuf());
                __instance.StopPanic();
                return false;
            }
            return true;
        }
        public class Health_ReliveBuf : UnitBuf
        {
            public override void Init(UnitModel model)
            {
                this.model = model;
                this.remainTime = 5f;
            }
            public override float OnTakeDamage(UnitModel attacker, DamageInfo damageInfo)
            {
                return 0f;
            }
        }
    }
}
