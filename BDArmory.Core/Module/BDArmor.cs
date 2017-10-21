﻿using System;
using KSPDev.ConfigUtils;

namespace BDArmory.Core.Module
{
    public class BDArmor : PartModule
    {
        static BDArmor instance;
        public static BDArmor Instance => instance;
        public ArmorUtils.ExplodeMode _explodeMode { get; private set; } = ArmorUtils.ExplodeMode.Never;

        #region KSP Fields
        [KSPField(isPersistant = true)]
        public float ArmorThickness = 0f;

        //[KSPField(guiActive = true, guiActiveEditor = true, isPersistant = false, guiName = "Part Area")]
        //public float PartArea = 0;

        [KSPField(guiActive = true, guiActiveEditor = true, isPersistant = false, guiName = "Max Damage")]
        public float maxDamage = 0;

        [KSPField(guiActive = true, guiActiveEditor = true, isPersistant = false, guiName = "Part Volume")]
        public float PartVolume = 0;

        [KSPField(guiActive = true, guiActiveEditor = true, isPersistant = false, guiName = "Part Volume with Armor")]
        public float PartVolume2 = 0;

        [KSPField(guiActive = true, guiActiveEditor = true, isPersistant = false, guiName = "ArmorMass")]
        public float ArmorMass = 0;

        [KSPField(guiActive = true, guiActiveEditor = true, isPersistant = false, guiName = "Current Mass")]
        public float currMass = 0;

        [KSPField(guiActive = true, guiActiveEditor = true, isPersistant = false, guiName = "Rescale Factor")]
        public float rescaleFactor = 0;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Calc Part")]
        public void doCalc()
        {
            SetPartMassByArmor();
        }


        [KSPField]
        public string explModelPath = "BDArmory/Models/explosion/explosionLarge";

        [KSPField]
        public string explSoundPath = "BDArmory/Sounds/explode1";

        [KSPField]
        public string explodeMode = "Never";

        #endregion
          
        public override void OnStart(StartState state)
        {
            base.OnAwake();
            part.force_activate();
            doCalc();

            switch (explodeMode)
            {
                case "Always":
                    _explodeMode = ArmorUtils.ExplodeMode.Always;
                    break;
                case "Dynamic":
                    _explodeMode = ArmorUtils.ExplodeMode.Dynamic;
                    break;
                case "Never":
                    _explodeMode = ArmorUtils.ExplodeMode.Never;
                    break;
            }
        }

        public void SetPartMassByArmor()
        {
            ArmorThickness = part.FindModuleImplementing<DamageTracker>().Armor;
            maxDamage = part.FindModuleImplementing<DamageTracker>().GetMaxPartDamage();
            PartVolume = (float) Math.Round(GetPartVolume(part.partInfo, part),2);
            PartVolume2 = (float)Math.Round(GetPartVolume_withArmor(part.partInfo,part),2);
            ArmorMass = (float)Math.Round(8.05f * (PartVolume2 - PartVolume)/1000f,2);
            currMass = part.mass;
        }

        public float GetPartVolume(AvailablePart partInfo,Part part)
        {
            var p = partInfo.partPrefab;
            float volume;

            var boundsSize = PartGeometryUtil.MergeBounds(p.GetRendererBounds(), p.transform).size;
            volume = boundsSize.x * boundsSize.y * boundsSize.z * 10f;            

            // Apply cube of the scale modifier since volume involves all 3 axis.
            return (float)(volume * Math.Pow(GetPartExternalScaleModifier(part), 3));
        }

        public float GetPartVolume_withArmor(AvailablePart partInfo,Part p)
        {
            float volume;
            var boundsSize = PartGeometryUtil.MergeBounds(p.GetRendererBounds(), p.transform).size;
            volume = (boundsSize.x + (ArmorThickness/1000)) * boundsSize.y * boundsSize.z;
            volume *= 10f;

            // Apply cube of the scale modifier since volume involves all 3 axis.
            return (float)(volume * Math.Pow(GetPartExternalScaleModifier(part), 3));
        }

        public static float GetPartArea(AvailablePart partInfo)
        {
            var p = partInfo.partPrefab;
            float area;

            var boundsSize = PartGeometryUtil.MergeBounds(p.GetRendererBounds(), p.transform).size;
            area = 2 * (boundsSize.x * boundsSize.y) + 2 * (boundsSize.y * boundsSize.z) + 2 * (boundsSize.x * boundsSize.z);
                       
            return area;
        }        

        public float GetPartExternalScaleModifier(Part part)
        {
            double defaultScale = 1.0f;
            double currentScale = 1.0f;

            if (part.Modules.Contains("TweakScale"))
            {
                PartModule pM = part.Modules["TweakScale"];
                if (pM.Fields.GetValue("currentScale") != null)
                {
                   try
                    {
                        defaultScale = pM.Fields.GetValue<float>("defaultScale");
                        currentScale = pM.Fields.GetValue<float>("currentScale");
                    }
                    catch
                    {
                        
                    }
                    rescaleFactor = (float)(currentScale / defaultScale);
                    return (float)(currentScale / defaultScale);
                }
            }
            return 1.0f;
        }

    }
}