﻿
using FFXIVClassic.Common;
using FFXIVClassic_Map_Server.actors.area;
using FFXIVClassic_Map_Server.actors.group;
using FFXIVClassic_Map_Server.Actors.Chara;
using FFXIVClassic_Map_Server.packets.send.actor;
using FFXIVClassic_Map_Server.utils;
using FFXIVClassic_Map_Server.actors.chara.ai;
using System;
using System.Collections.Generic;
using FFXIVClassic_Map_Server.actors.chara;
using FFXIVClassic_Map_Server.packets.send.actor.battle;
using FFXIVClassic_Map_Server.packets.send;
using FFXIVClassic_Map_Server.actors.chara.ai.state;
using FFXIVClassic_Map_Server.actors.chara.ai.utils;

namespace FFXIVClassic_Map_Server.Actors
{
    /// <summary> Which Character types am I friendly with </summary>
    enum CharacterTargetingAllegiance
    {
        /// <summary> Friendly to Players </summary>
        Player,
        /// <summary> Friendly to BattleNpcs </summary>
        BattleNpcs
    }

    class Character : Actor
    {
        public const int CLASSID_PUG = 2;
        public const int CLASSID_GLA = 3;
        public const int CLASSID_MRD = 4;
        public const int CLASSID_ARC = 7;
        public const int CLASSID_LNC = 8;
        public const int CLASSID_THM = 22;
        public const int CLASSID_CNJ = 23;

        public const int SIZE = 0;
        public const int COLORINFO = 1;
        public const int FACEINFO = 2;
        public const int HIGHLIGHT_HAIR = 3;
        public const int VOICE = 4;
        public const int MAINHAND = 5;
        public const int OFFHAND = 6;
        public const int SPMAINHAND = 7;
        public const int SPOFFHAND = 8;
        public const int THROWING = 9;
        public const int PACK = 10;
        public const int POUCH = 11;
        public const int HEADGEAR = 12;
        public const int BODYGEAR = 13;
        public const int LEGSGEAR = 14;
        public const int HANDSGEAR = 15;
        public const int FEETGEAR = 16;
        public const int WAISTGEAR = 17;
        public const int NECKGEAR = 18;
        public const int L_EAR = 19;
        public const int R_EAR = 20;
        public const int R_WRIST = 21;
        public const int L_WRIST = 22;
        public const int R_RINGFINGER = 23;
        public const int L_RINGFINGER = 24;
        public const int R_INDEXFINGER = 25;
        public const int L_INDEXFINGER = 26;
        public const int UNKNOWN = 27;

        public bool isStatic = false;

        public bool isMovingToSpawn = false;

        public uint modelId;
        public uint[] appearanceIds = new uint[28];

        public uint animationId = 0;

        public uint currentTarget = 0xC0000000;
        public uint currentLockedTarget = 0xC0000000;

        public uint currentActorIcon = 0;

        public Work work = new Work();
        public CharaWork charaWork = new CharaWork();

        public Group currentParty = null;
        public ContentGroup currentContentGroup = null;

        //public DateTime lastAiUpdate;

        public AIContainer aiContainer;
        public StatusEffectContainer statusEffects;

        public CharacterTargetingAllegiance allegiance;

        public Pet pet;

        public Dictionary<Modifier, Int64> modifiers = new Dictionary<Modifier, long>();

        protected ushort hpBase, hpMaxBase, mpBase, mpMaxBase, tpBase;
        protected BattleTemp baseStats = new BattleTemp();
        public ushort currentJob;

        public Character(uint actorID) : base(actorID)
        {
            //Init timer array to "notimer"
            for (int i = 0; i < charaWork.statusShownTime.Length; i++)
                charaWork.statusShownTime[i] = 0xFFFFFFFF;

            this.statusEffects = new StatusEffectContainer(this);

            // todo: move this somewhere more appropriate
            ResetMoveSpeeds();
            // todo: base this on equip and shit
            SetMod((uint)Modifier.AttackRange, 3);
            SetMod((uint)Modifier.AttackDelay, (Program.Random.Next(30, 60) * 100));
        }

        public SubPacket CreateAppearancePacket()
        {
            SetActorAppearancePacket setappearance = new SetActorAppearancePacket(modelId, appearanceIds);
            return setappearance.BuildPacket(actorId);
        }

        public SubPacket CreateInitStatusPacket()
        {
            return (SetActorStatusAllPacket.BuildPacket(actorId, charaWork.status));
        }

        public SubPacket CreateSetActorIconPacket()
        {
            return SetActorIconPacket.BuildPacket(actorId, currentActorIcon);
        }

        public SubPacket CreateIdleAnimationPacket()
        {
            return SetActorSubStatPacket.BuildPacket(actorId, 0, 0, 0, 0, 0, 0, animationId);
        }

        public void SetQuestGraphic(Player player, int graphicNum)
        {
            player.QueuePacket(SetActorQuestGraphicPacket.BuildPacket(actorId, graphicNum));
        }

        public void SetCurrentContentGroup(ContentGroup group)
        {
            if (group != null)
                charaWork.currentContentGroup = group.GetTypeId();
            else
                charaWork.currentContentGroup = 0;

            currentContentGroup = group;

            ActorPropertyPacketUtil propPacketUtil = new ActorPropertyPacketUtil("charaWork/currentContentGroup", this);
            propPacketUtil.AddProperty("charaWork.currentContentGroup");
            zone.BroadcastPacketsAroundActor(this, propPacketUtil.Done());

        }

        public List<SubPacket> GetActorStatusPackets()
        {
            var propPacketUtil = new ActorPropertyPacketUtil("charaWork/status", this);
            var i = 0;
            foreach (var effect in statusEffects.GetStatusEffects())
            {
                propPacketUtil.AddProperty($"charaWork.statusShownTime[{i}]");
                propPacketUtil.AddProperty(String.Format("charaWork.statusShownTime[{0}]", i));
                i++;
            }
            return propPacketUtil.Done();
        }

        public void PlayAnimation(uint animId, bool onlySelf = false)
        {
            if (onlySelf)
            {
                if (this is Player)
                    ((Player)this).QueuePacket(PlayAnimationOnActorPacket.BuildPacket(actorId, animId));
            }
            else
                zone.BroadcastPacketAroundActor(this, PlayAnimationOnActorPacket.BuildPacket(actorId, animId));
        }

        public void DoBattleAction(ushort commandId, uint animationId)
        {
            zone.BroadcastPacketAroundActor(this, BattleActionX00Packet.BuildPacket(actorId, animationId, commandId));
        }

        public void DoBattleAction(ushort commandId, uint animationId, BattleAction action)
        {
            zone.BroadcastPacketAroundActor(this, BattleActionX01Packet.BuildPacket(actorId, animationId, commandId, action));
        }

        public void DoBattleAction(ushort commandId, uint animationId, BattleAction[] actions)
        {
            int currentIndex = 0;

            while (true)
            {
                if (actions.Length - currentIndex >= 18)
                    BattleActionX18Packet.BuildPacket(actorId, animationId, commandId, actions, ref currentIndex);
                else if (actions.Length - currentIndex > 1)
                    BattleActionX10Packet.BuildPacket(actorId, animationId, commandId, actions, ref currentIndex);
                else if (actions.Length - currentIndex == 1)
                {
                    BattleActionX01Packet.BuildPacket(actorId, animationId, commandId, actions[currentIndex]);
                    currentIndex++;
                }
                else
                    break;
                animationId = 0; //If more than one packet is sent out, only send the animation once to avoid double playing.
            }
        }

        public void DoBattleAction(ushort commandId, uint animationId, List<BattleAction> actions)
        {
            int currentIndex = 0;            

            while (true)
            {
                if (actions.Count - currentIndex >= 18)
                    BattleActionX18Packet.BuildPacket(actorId, animationId, commandId, actions, ref currentIndex);
                else if (actions.Count - currentIndex > 1)
                    BattleActionX10Packet.BuildPacket(actorId, animationId, commandId, actions, ref currentIndex);
                else if (actions.Count - currentIndex == 1)
                {
                    BattleActionX01Packet.BuildPacket(actorId, animationId, commandId, actions[currentIndex]);
                    currentIndex++;
                }
                else
                    break;
                animationId = 0; //If more than one packet is sent out, only send the animation once to avoid double playing.
            }            
        }

        #region ai stuff
        public void PathTo(float x, float y, float z, float stepSize = 0.70f, int maxPath = 40, float polyRadius = 0.0f)
        {
            aiContainer?.pathFind?.PreparePath(x, y, z, stepSize, maxPath, polyRadius);
        }

        public void FollowTarget(Actor target, float stepSize = 1.2f, int maxPath = 25, float radius = 0.0f)
        {
            var player = target as Player;

            if (player != null)
            {
                if (this.target != player)
                {
                    this.target = target;
                }
                // todo: move this to own function thing
                this.oldMoveState = this.moveState;
                this.moveState = 2;
                updateFlags |= ActorUpdateFlags.Position | ActorUpdateFlags.Speed;
                //this.moveSpeeds = player.moveSpeeds;

                PathTo(player.positionX, player.positionY, player.positionZ, stepSize, maxPath, radius);
            }
        }

        public Int64 GetMod(uint modifier)
        {
            Int64 res;
            if (modifiers.TryGetValue((Modifier)modifier, out res))
                return res;
            return 0;
        }

        public void SetMod(uint modifier, Int64 val)
        {
            if (modifiers.ContainsKey((Modifier)modifier))
                modifiers[(Modifier)modifier] = val;
            else
                modifiers.Add((Modifier)modifier, val);
        }

        public virtual void OnPath(Vector3 point)
        {
            lua.LuaEngine.CallLuaBattleAction(this, "onPath", this, point);

            updateFlags |= ActorUpdateFlags.Position;
            this.isAtSpawn = false;
        }

        public override void Update(DateTime tick)
        {

        }

        public override void PostUpdate(DateTime tick, List<SubPacket> packets = null)
        {
            if (updateFlags != ActorUpdateFlags.None)
            {
                packets = packets ?? new List<SubPacket>();

                if ((updateFlags & ActorUpdateFlags.Appearance) != 0)
                {
                    packets.Add(new SetActorAppearancePacket(modelId, appearanceIds).BuildPacket(actorId));
                }

                // todo: should probably add another flag for battleTemp since all this uses reflection
                if ((updateFlags & ActorUpdateFlags.HpTpMp) != 0)
                {
                    var propPacketUtil = new ActorPropertyPacketUtil("charaWork.parameterSave", this);
                    propPacketUtil.AddProperty("charaWork.parameterSave.mp");
                    propPacketUtil.AddProperty("charaWork.parameterSave.mpMax");
                    propPacketUtil.AddProperty("charaWork.parameterTemp.tp");
                    packets.AddRange(propPacketUtil.Done());
                }
                base.PostUpdate(tick, packets);
            }
        }

        public virtual bool IsValidTarget(Character target, ValidTarget validTarget)
        {
            return true;
        }

        public virtual bool CanAttack()
        {
            return true;
        }

        public virtual bool CanCast(Character target, BattleCommand spell)
        {
            return false;
        }

        public virtual bool CanWeaponSkill(Character target, BattleCommand skill)
        {
            return false;
        }

        public virtual bool CanUseAbility(Character target, BattleCommand ability)
        {
            return false;
        }

        public virtual uint GetAttackDelayMs()
        {
            return (uint)GetMod((uint)Modifier.AttackDelay);
        }

        public virtual uint GetAttackRange()
        {
            return (uint)GetMod((uint)Modifier.AttackRange);
        }

        public bool Engage(uint targid = 0)
        {
            // todo: attack the things
            if (targid == 0)
            {
                if (currentTarget != 0xC0000000)
                    targid = currentTarget;
                else if (currentLockedTarget != 0xC0000000)
                    targid = currentLockedTarget;
            }
            if (targid != 0)
            {
                aiContainer.Engage(Server.GetWorldManager().GetActorInWorld(targid) as Character);
            }
            return false;
        }

        public bool Disengage()
        {
            if (aiContainer != null)
            {
                aiContainer.Disengage();
                return true;
            }
            return false;
        }

        public void Cast(uint spellId, uint targetId = 0)
        {
            aiContainer.Cast(Server.GetWorldManager().GetActorInWorld(targetId == 0 ? currentTarget : targetId) as Character, spellId);
        }

        public void Ability(uint abilityId, uint targetId = 0)
        {
            aiContainer.Ability(Server.GetWorldManager().GetActorInWorld(targetId == 0 ? currentTarget : targetId) as Character, abilityId);
        }

        public void WeaponSkill(uint skillId)
        {
            aiContainer.WeaponSkill(Server.GetWorldManager().GetActorInWorld(currentTarget) as Character, skillId);
        }

        public virtual void Spawn(DateTime tick)
        {
            // todo: reset hp/mp/tp etc here
            ChangeState(SetActorStatePacket.MAIN_STATE_PASSIVE);
            charaWork.parameterSave.hp = charaWork.parameterSave.hpMax;
            charaWork.parameterSave.mp = charaWork.parameterSave.mpMax;
            RecalculateStats();
        }

        public virtual void Die(DateTime tick)
        {
            // todo: actual despawn timer
            aiContainer.InternalDie(tick, 10);
        }

        protected virtual void Despawn(DateTime tick)
        {

        }

        public bool IsDead()
        {
            return currentMainState == SetActorStatePacket.MAIN_STATE_DEAD || currentMainState == SetActorStatePacket.MAIN_STATE_DEAD2;
        }

        public bool IsAlive()
        {
            return !IsDead();
        }

        public short GetHP()
        {
            // todo: 
            return charaWork.parameterSave.hp[currentJob];
        }

        public short GetMaxHP()
        {
            return charaWork.parameterSave.hpMax[currentJob];
        }

        public short GetMP()
        {
            return charaWork.parameterSave.mp;
        }

        public ushort GetTP()
        {
            return tpBase;
        }

        public short GetMaxMP()
        {
            return charaWork.parameterSave.mpMax;
        }

        public byte GetMPP()
        {
            return (byte)((charaWork.parameterSave.mp / charaWork.parameterSave.mpMax) * 100);
        }

        public byte GetTPP()
        {
            return (byte)((tpBase / 3000) * 100);
        }

        public byte GetHPP()
        {
            return (byte)((charaWork.parameterSave.hp[0] / charaWork.parameterSave.hpMax[0]) * 100);
        }

        // todo: the following functions are virtuals since we want to check hidden item bonuses etc on player for certain conditions
        public virtual void AddHP(int hp)
        {
            // todo: +/- hp and die
            // todo: battlenpcs probably have way more hp?
            var addHp = charaWork.parameterSave.hp[currentJob] + hp;
            addHp = addHp.Clamp(ushort.MinValue, charaWork.parameterSave.hpMax[currentJob]);
            charaWork.parameterSave.hp[currentJob] = (short)addHp;

            if (charaWork.parameterSave.hp[currentJob] < 1)
                Die(Program.Tick);

            updateFlags |= ActorUpdateFlags.HpTpMp;
        }

        public void AddMP(int mp)
        {
            charaWork.parameterSave.mp = (short)(charaWork.parameterSave.mp + mp).Clamp(ushort.MinValue, charaWork.parameterSave.mpMax);

            // todo: check hidden effects and shit

            updateFlags |= ActorUpdateFlags.HpTpMp;
        }

        public void AddTP(int tp)
        {
            tpBase = (ushort)((tpBase + tp).Clamp(0, 3000));

            updateFlags |= ActorUpdateFlags.HpTpMp;
        }

        public void DelHP(int hp)
        {
            AddHP((short)-hp);
        }

        public void DelMP(int mp)
        {
            AddMP(-mp);
        }

        public void DelTP(int tp)
        {
            AddTP(-tp);
        }

        public void CalculateBaseStats()
        {
            // todo: apply mods and shit here, get race/level/job and shit
            // baseStats.generalParameter[ASIDHOASID] =  
        }
        // todo: should this include stats too?
        public void RecalculateStats()
        {
            if (GetMod((uint)Modifier.Hp) != 0)
            {
                
            }
            // todo: recalculate stats and crap
            updateFlags |= ActorUpdateFlags.HpTpMp;
        }

        public virtual float GetSpeed()
        {
            // todo: for battlenpc/player calculate speed
            return moveSpeeds[2] + GetMod((uint)Modifier.Speed);
        }

        public virtual void OnAttack(State state, BattleAction action, ref BattleAction error)
        {
            // todo: change animation based on equipped weapon
            action.effectId |= (uint)HitEffect.HitVisual1; // melee

            var target = state.GetTarget();
            // todo: get hitrate and shit, handle protect effect and whatever
            if (BattleUtils.TryAttack(this, target, action, ref error))
            {
                action.amount = BattleUtils.CalculateAttackDamage(this, target, action);
                //var packet = BattleActionX01Packet.BuildPacket(owner.actorId, owner.actorId, target.actorId, (uint)0x19001000, (uint)0x8000604, (ushort)0x765D, (ushort)BattleActionX01PacketCommand.Attack, (ushort)damage, (byte)0x1);
            }

            target.DelHP(action.amount);
        }

        public virtual void OnCast(State state, BattleAction action, ref SubPacket errorPacket)
        {

        }

        public virtual void OnWeaponSkill(State state, BattleAction action, ref SubPacket errorPacket)
        {

        }

        public virtual void OnAbility(State state, BattleAction action, ref SubPacket errorPacket)
        {

        }
        #endregion
    }

}
