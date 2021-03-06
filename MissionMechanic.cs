using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WowJamMessages;
using WowStatConstants;
using WowStaticData;

public class MissionMechanic : MonoBehaviour
{
	public int m_missionMechanicTypeID;

	public Image m_missionMechanicIcon;

	public Image m_missionMechanicIconBorder;

	public Image m_counteredIcon;

	public Image m_canCounterButBusyIcon;

	public Shader m_grayscaleShader;

	private bool m_isCountered;

	private int m_garrAbilityID;

	private int m_counterWithThisAbilityID;

	public void SetCountered(bool isCountered, bool counteringFollowerIsBusy = false, bool playCounteredEffect = true)
	{
		if (counteringFollowerIsBusy)
		{
			if (!this.m_counteredIcon.get_gameObject().get_activeSelf())
			{
				this.m_canCounterButBusyIcon.get_gameObject().SetActive(true);
			}
			return;
		}
		this.m_isCountered = isCountered;
		this.m_counteredIcon.get_gameObject().SetActive(isCountered);
		if (this.m_isCountered && this.m_canCounterButBusyIcon != null)
		{
			this.m_canCounterButBusyIcon.get_gameObject().SetActive(false);
		}
		this.m_missionMechanicIcon.get_material().SetFloat("_GrayscaleAmount", (!this.m_isCountered) ? 1f : 0f);
		if (this.m_isCountered && playCounteredEffect)
		{
			UiAnimMgr.instance.PlayAnim("GreenCheck", base.get_transform(), Vector3.get_zero(), 1.8f, 0f);
			Main.instance.m_UISound.Play_GreenCheck();
		}
	}

	public bool IsCountered()
	{
		return this.m_isCountered;
	}

	public void SetMechanicTypeWithMechanicID(int missionMechanicID, bool hideBorder = false)
	{
		GarrMechanicRec record = StaticDB.garrMechanicDB.GetRecord(missionMechanicID);
		if (record == null)
		{
			Debug.LogWarning("Invalid MissionMechanicID " + missionMechanicID);
			return;
		}
		this.SetMechanicType((int)record.GarrMechanicTypeID, record.GarrAbilityID, hideBorder);
	}

	public static List<int> GetUsefulBuffAbilitiesForFollower(int garrFollowerID)
	{
		List<int> usefulBuffAbilityIDs = new List<int>();
		JamGarrisonFollower jamGarrisonFollower = PersistentFollowerData.followerDictionary.get_Item(garrFollowerID);
		int[] abilityID2 = jamGarrisonFollower.AbilityID;
		int abilityID;
		for (int i = 0; i < abilityID2.Length; i++)
		{
			abilityID = abilityID2[i];
			if (abilityID != 414 && abilityID != 415)
			{
				StaticDB.garrAbilityEffectDB.EnumRecordsByParentID(abilityID, delegate(GarrAbilityEffectRec garrAbilityEffectRec)
				{
					if (garrAbilityEffectRec.AbilityAction == 0u)
					{
						return true;
					}
					<GetUsefulBuffAbilitiesForFollower>c__AnonStorey.usefulBuffAbilityIDs.Add(abilityID);
					return true;
				});
			}
		}
		return usefulBuffAbilityIDs;
	}

	public static int GetAbilityToCounterMechanicType(int garrMechanicTypeID)
	{
		int counterWithThisAbilityID = 0;
		StaticDB.garrFollowerDB.EnumRecords(delegate(GarrFollowerRec garrFollowerRec)
		{
			if (garrFollowerRec.GarrFollowerTypeID != 4u)
			{
				return true;
			}
			if (garrFollowerRec.ChrClassID != GarrisonStatus.CharacterClassID())
			{
				return true;
			}
			StaticDB.garrFollowerXAbilityDB.EnumRecordsByParentID(garrFollowerRec.ID, delegate(GarrFollowerXAbilityRec xAbilityRec)
			{
				StaticDB.garrAbilityEffectDB.EnumRecords(delegate(GarrAbilityEffectRec garrAbilityEffectRec)
				{
					if (garrAbilityEffectRec.GarrMechanicTypeID == 0u)
					{
						return true;
					}
					if (garrAbilityEffectRec.AbilityAction != 0u)
					{
						return true;
					}
					if ((ulong)garrAbilityEffectRec.GarrAbilityID == (ulong)((long)xAbilityRec.GarrAbilityID) && (ulong)garrAbilityEffectRec.GarrMechanicTypeID == (ulong)((long)garrMechanicTypeID))
					{
						counterWithThisAbilityID = xAbilityRec.GarrAbilityID;
						return false;
					}
					return true;
				});
				return counterWithThisAbilityID == 0;
			});
			return counterWithThisAbilityID == 0;
		});
		return counterWithThisAbilityID;
	}

	public int AbilityID()
	{
		return this.m_garrAbilityID;
	}

	public void SetMechanicType(int missionMechanicTypeID, int mechanicAbilityID, bool hideBorder = false)
	{
		this.m_garrAbilityID = mechanicAbilityID;
		GarrMechanicTypeRec record = StaticDB.garrMechanicTypeDB.GetRecord(missionMechanicTypeID);
		if (record != null)
		{
			this.m_missionMechanicIcon.get_gameObject().SetActive(true);
			this.m_missionMechanicTypeID = record.ID;
			this.m_counterWithThisAbilityID = MissionMechanic.GetAbilityToCounterMechanicType(missionMechanicTypeID);
			if (this.m_counterWithThisAbilityID != 0)
			{
				GarrAbilityRec record2 = StaticDB.garrAbilityDB.GetRecord(this.m_counterWithThisAbilityID);
				if (record2 != null)
				{
					Sprite sprite = GeneralHelpers.LoadIconAsset(AssetBundleType.Icons, record2.IconFileDataID);
					if (sprite != null)
					{
						this.m_missionMechanicIcon.set_sprite(sprite);
						if (this.m_grayscaleShader != null)
						{
							Material material = new Material(this.m_grayscaleShader);
							this.m_missionMechanicIcon.set_material(material);
						}
					}
				}
			}
		}
		else
		{
			this.m_missionMechanicIcon.get_gameObject().SetActive(false);
		}
		this.SetCountered(false, false, true);
		this.m_missionMechanicIconBorder.get_gameObject().SetActive(!hideBorder);
	}

	public void ShowTooltip()
	{
		Main.instance.allPopups.ShowAbilityInfoPopup(this.m_counterWithThisAbilityID);
	}
}
