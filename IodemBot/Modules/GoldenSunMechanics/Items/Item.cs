﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using static IodemBot.Modules.GoldenSunMechanics.Psynergy;

namespace IodemBot.Modules.GoldenSunMechanics
{
    public enum ItemType
    {
        Collectible,
        LongSword, Axe, Staff, LightBlade, Mace, Bow, Claw,
        Shield, Bracelet, Glove,
        Helmet, Hat, Circlet, Crown,
        HeavyArmor, Robe, LightArmor,
        UnderWear,
        Boots, Greaves,
        Ring, Misc
    }

    public class Item : ICloneable
    {
        private static ItemType[] Weapons = { ItemType.LongSword, ItemType.Axe, ItemType.Staff, ItemType.LightBlade, ItemType.Mace, ItemType.Bow, ItemType.Claw };
        private static ItemType[] ArmWear = { ItemType.Shield, ItemType.Bracelet, ItemType.Glove };
        private static ItemType[] ChestWear = { ItemType.HeavyArmor, ItemType.Robe, ItemType.LightArmor };
        private static ItemType[] HeadWear = { ItemType.Helmet, ItemType.Hat, ItemType.Circlet, ItemType.Crown };
        private static ItemType[] UnderWear = { ItemType.UnderWear };
        private static ItemType[] Footwear = { ItemType.Boots, ItemType.Greaves };
        private static ItemType[] Accessoires = { ItemType.Ring, ItemType.Misc };

        public string Name { get; set; }
        public string Icon { get; set; }
        public uint Price { get; set; }

        [JsonIgnore]
        public uint sellValue { get { return Price / 2; } }

        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType ItemType { get; set; }

        public Element[] ExclusiveTo { get; set; }
        public bool IsStackable { get; set; }

        public bool IsUsable { get; set; } = false;
        public bool IsArtifact { get; set; } = false;

        public int increaseUnleashRate { get; set; }

        [DefaultValue(100)]
        public int ChanceToActivate { get; set; } = 80;

        public int ChanceToBreak { get; set; } = 100;

        public bool isBroken { get; set; }

        public bool IsUnleashable { get { return unleash != null; } }
        public Unleash unleash { get; set; }

        [DefaultValue(Element.none)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Element DamageAlignment { get; set; }

        public bool IsCursed { get; set; }
        public bool CuresCurse { get; set; }

        public bool IsEquippable { get; set; }
        public bool IsEquipped { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Stats AddStatsOnEquip { get; set; } = new Stats(0, 0, 0, 0, 0);

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Stats MultStatsOnEquip { get; set; } = new Stats(100, 100, 100, 100, 100);

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ElementalStats AddElStatsOnEquip { get; set; } = new ElementalStats(0, 0, 0, 0, 0, 0, 0, 0);

        public int HPRegen { get; set; }
        public int PPRegen { get; set; }

        [JsonIgnore]
        public bool IsWeapon { get { return Weapons.Contains(ItemType); } }

        [JsonIgnore]
        public bool IsHeadWear { get { return HeadWear.Contains(ItemType); } }

        [JsonIgnore]
        public bool IsChestWear { get { return ChestWear.Contains(ItemType); } }

        [JsonIgnore]
        public bool IsArmWear { get { return ArmWear.Contains(ItemType); } }

        [JsonIgnore]
        public bool IsUnderWear { get { return UnderWear.Contains(ItemType); } }

        [JsonIgnore]
        public bool IsFootWear { get { return Footwear.Contains(ItemType); } }

        [JsonIgnore]
        public bool IsAccessoire { get { return Accessoires.Contains(ItemType); } }

        public object Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Item>(serialized);
        }

        public override string ToString()
        {
            return Name;
        }

        public string Summary()
        {
            var s = new StringBuilder();
            if (AddStatsOnEquip.NonZerosToString() != "``")
            {
                s.Append(AddStatsOnEquip.NonZerosToString());
                s.Append("\n");
            }
            if (MultStatsOnEquip.MultipliersToString() != "``")
            {
                s.Append(MultStatsOnEquip.MultipliersToString());
                s.Append("\n");
            }
            if (AddElStatsOnEquip.NonZerosToSTring() != "")
            {
                s.Append(AddElStatsOnEquip.NonZerosToSTring());
                s.Append("\n");
            }

            if (HPRegen > 0)
            {
                s.Append($"HP Regen: {HPRegen} | ");
            }

            if (PPRegen > 0)
            {
                s.Append($"PP Regen: {PPRegen} | ");
            }

            if (increaseUnleashRate > 0)
            {
                s.Append($"Increases Unleashrate | ");
            }

            if (IsUnleashable)
            {
                s.Append($"{unleash.ToString()} | ");
            }

            if (CuresCurse)
            {
                s.Append($"Cures Curse");
            }

            return s.ToString();
        }
    }

    public class Unleash
    {
        public string UnleashName { get; set; }

        [DefaultValue(Element.none)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Element UnleashAlignment { get; set; }

        [DefaultValue(35)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int UnleashRate { get; set; }

        [JsonIgnore] public List<IEffect> effects { get; set; }
        public List<EffectImage> effectImages { get; set; }

        public Unleash(List<EffectImage> effectImages)
        {
            this.effects = new List<IEffect>();
            this.effectImages = effectImages;
            if (effectImages != null)
            {
                effectImages.ForEach(e => effects.Add(IEffect.EffectFactory(e.id, e.args)));
            }
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            if (UnleashName != null)
            {
                s.Append(UnleashName);
            }

            if (effects.Count > 0)
            {
                if (UnleashName != null)
                {
                    s.Append(" (");
                }
                s.Append(string.Join(" , ", effects.Select(e => $"{e.ToString()}")));
                if (UnleashName != null)
                {
                    s.Append(")");
                }
            }
            return s.ToString();
        }
    }
}