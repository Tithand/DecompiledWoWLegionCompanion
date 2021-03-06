using System;
using UnityEngine;
using UnityEngine.UI;
using WowJamMessages;
using WowStatConstants;
using WowStaticData;

public class FollowerExperienceDisplay : MonoBehaviour
{
	[Header("Portrait")]
	public Image m_portraitBG;

	public Image m_followerPortrait;

	public Image m_qualityBorder;

	public Image m_levelBorder;

	public Text m_followerNameText;

	public Text m_iLevelText;

	public Text m_XPLabel;

	public Image m_classIcon;

	public Text m_classText;

	[Header("Troop Specific")]
	public GameObject m_troopHeartContainerEmpty;

	public GameObject m_troopHeartContainerFull;

	public GameObject m_troopHeartPrefab;

	public GameObject m_troopEmptyHeartPrefab;

	public GameObject m_expiredPortraitX;

	[Header("XP Bar")]
	public GameObject m_progressBarObj;

	public FancyNumberDisplay m_fancyNumberDisplay;

	public Image m_progressBarFillImage;

	public Text m_xpAmountText;

	public Text m_toNextLevelOrUpgradeText;

	private int m_followerID;

	private bool m_showedLevelUpEffect;

	private uint m_currentCap;

	private bool m_newCapIsQuality;

	private uint m_newCap;

	private bool m_newFollowerIsMaxLevelAndMaxQuality;

	private string m_iLvlString;

	private void Start()
	{
		this.m_followerNameText.set_font(GeneralHelpers.LoadStandardFont());
		this.m_iLevelText.set_font(GeneralHelpers.LoadStandardFont());
		this.m_classText.set_font(GeneralHelpers.LoadStandardFont());
		this.m_xpAmountText.set_font(GeneralHelpers.LoadStandardFont());
		this.m_toNextLevelOrUpgradeText.set_font(GeneralHelpers.LoadStandardFont());
		this.m_XPLabel.set_font(GeneralHelpers.LoadStandardFont());
		this.m_XPLabel.set_text(StaticDB.GetString("XP2", null));
	}

	private void OnEnable()
	{
		FancyNumberDisplay expr_06 = this.m_fancyNumberDisplay;
		expr_06.TimerUpdateAction = (Action<int>)Delegate.Combine(expr_06.TimerUpdateAction, new Action<int>(this.SetFillValue));
	}

	private void OnDisable()
	{
		FancyNumberDisplay expr_06 = this.m_fancyNumberDisplay;
		expr_06.TimerUpdateAction = (Action<int>)Delegate.Remove(expr_06.TimerUpdateAction, new Action<int>(this.SetFillValue));
	}

	private void SetFollowerAppearance(JamGarrisonFollower follower, bool nextCapIsForQuality, bool isMaxLevelAndMaxQuality, bool isTroop, float initialEntranceDelay)
	{
		GarrFollowerRec record = StaticDB.garrFollowerDB.GetRecord(follower.GarrFollowerID);
		this.m_troopHeartContainerEmpty.SetActive(isTroop);
		this.m_troopHeartContainerFull.SetActive(isTroop);
		this.m_expiredPortraitX.SetActive(false);
		if (isTroop)
		{
			this.m_levelBorder.get_gameObject().SetActive(false);
			this.m_progressBarObj.SetActive(false);
			this.m_portraitBG.get_gameObject().SetActive(false);
			this.m_troopHeartContainerEmpty.SetActive(true);
			this.m_troopHeartContainerFull.SetActive(true);
			Transform[] componentsInChildren = this.m_troopHeartContainerEmpty.GetComponentsInChildren<Transform>();
			Transform[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform = array[i];
				if (transform != this.m_troopHeartContainerEmpty.get_transform())
				{
					Object.DestroyImmediate(transform.get_gameObject());
				}
			}
			Transform[] componentsInChildren2 = this.m_troopHeartContainerFull.GetComponentsInChildren<Transform>();
			Transform[] array2 = componentsInChildren2;
			for (int j = 0; j < array2.Length; j++)
			{
				Transform transform2 = array2[j];
				if (transform2 != this.m_troopHeartContainerFull.get_transform())
				{
					Object.DestroyImmediate(transform2.get_gameObject());
				}
			}
			float num = 0.15f;
			JamGarrisonFollower jamGarrisonFollower = PersistentFollowerData.preMissionFollowerDictionary.get_Item(follower.GarrFollowerID);
			for (int k = 0; k < jamGarrisonFollower.Durability; k++)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.m_troopHeartPrefab);
				gameObject.get_transform().SetParent(this.m_troopHeartContainerFull.get_transform(), false);
				if (k >= follower.Durability)
				{
					float num2 = initialEntranceDelay + (float)(jamGarrisonFollower.Durability - (k - follower.Durability)) * num;
					float num3 = 2f;
					iTween.ValueTo(gameObject, iTween.Hash(new object[]
					{
						"name",
						"fade",
						"from",
						0f,
						"to",
						1f,
						"time",
						num3,
						"easetype",
						iTween.EaseType.easeOutCubic,
						"delay",
						num2,
						"onupdatetarget",
						gameObject,
						"onupdate",
						"SetHeartEffectProgress",
						"oncomplete",
						"FinishHeartEffect"
					}));
				}
			}
			for (int l = 0; l < record.Vitality; l++)
			{
				GameObject gameObject2 = Object.Instantiate<GameObject>(this.m_troopEmptyHeartPrefab);
				gameObject2.get_transform().SetParent(this.m_troopHeartContainerEmpty.get_transform(), false);
			}
			if (follower.Durability <= 0)
			{
				DelayedUIAnim delayedUIAnim = base.get_gameObject().AddComponent<DelayedUIAnim>();
				float num4 = initialEntranceDelay + (float)(jamGarrisonFollower.Durability - follower.Durability) * num + 1f;
				delayedUIAnim.Init(num4, "RedFailX", "SFX/UI_Mission_Fail_Red_X", this.m_followerPortrait.get_transform(), 1.5f);
				DelayedObjectEnable delayedObjectEnable = base.get_gameObject().AddComponent<DelayedObjectEnable>();
				delayedObjectEnable.Init(num4 + 0.25f, this.m_expiredPortraitX);
			}
		}
		int iconFileDataID = (GarrisonStatus.Faction() != PVP_FACTION.HORDE) ? record.AllianceIconFileDataID : record.HordeIconFileDataID;
		Sprite sprite = GeneralHelpers.LoadIconAsset(AssetBundleType.PortraitIcons, iconFileDataID);
		if (sprite != null)
		{
			this.m_followerPortrait.set_sprite(sprite);
		}
		if (isTroop)
		{
			this.m_qualityBorder.get_gameObject().SetActive(false);
			this.m_levelBorder.get_gameObject().SetActive(false);
			this.m_followerNameText.set_color(Color.get_white());
		}
		else
		{
			Color qualityColor = GeneralHelpers.GetQualityColor(follower.Quality);
			this.m_qualityBorder.set_color(qualityColor);
			this.m_levelBorder.set_color(qualityColor);
			this.m_followerNameText.set_color(qualityColor);
		}
		CreatureRec record2 = StaticDB.creatureDB.GetRecord((GarrisonStatus.Faction() != PVP_FACTION.HORDE) ? record.AllianceCreatureID : record.HordeCreatureID);
		this.m_followerNameText.set_text(record2.Name);
		int num5 = (follower.ItemLevelWeapon + follower.ItemLevelArmor) / 2;
		if (this.m_iLvlString == null)
		{
			this.m_iLvlString = StaticDB.GetString("ITEM_LEVEL_ABBREVIATION", null);
		}
		this.m_iLevelText.set_text(this.m_iLvlString + " " + num5);
		GarrClassSpecRec record3 = StaticDB.garrClassSpecDB.GetRecord((int)((GarrisonStatus.Faction() != PVP_FACTION.HORDE) ? record.AllianceGarrClassSpecID : record.HordeGarrClassSpecID));
		this.m_classText.set_text(record3.ClassSpec);
		Sprite atlasSprite = TextureAtlas.instance.GetAtlasSprite((int)record3.UiTextureAtlasMemberID);
		if (atlasSprite != null)
		{
			this.m_classIcon.set_sprite(atlasSprite);
		}
		if (!isTroop)
		{
			if (isMaxLevelAndMaxQuality)
			{
				this.m_progressBarObj.SetActive(false);
				this.m_toNextLevelOrUpgradeText.set_text(string.Empty);
			}
			else if (nextCapIsForQuality)
			{
				this.m_progressBarObj.SetActive(true);
				this.m_toNextLevelOrUpgradeText.set_text(StaticDB.GetString("TO_NEXT_UPGRADE", string.Empty));
			}
			else
			{
				this.m_progressBarObj.SetActive(true);
				this.m_toNextLevelOrUpgradeText.set_text(StaticDB.GetString("TO_NEXT_LEVEL", string.Empty));
			}
		}
	}

	public void SetFollower(JamGarrisonFollower oldFollower, JamGarrisonFollower newFollower, float initialEffectDelay)
	{
		this.m_followerID = oldFollower.GarrFollowerID;
		bool flag = (oldFollower.Flags & 8) != 0;
		if (flag)
		{
			JamGarrisonFollower jamGarrisonFollower = newFollower;
			if (jamGarrisonFollower == null)
			{
				jamGarrisonFollower = new JamGarrisonFollower();
				jamGarrisonFollower.GarrFollowerID = oldFollower.GarrFollowerID;
				jamGarrisonFollower.Quality = oldFollower.Quality;
				jamGarrisonFollower.Durability = 0;
			}
			this.SetFollowerAppearance(jamGarrisonFollower, false, false, true, initialEffectDelay);
			return;
		}
		this.m_showedLevelUpEffect = false;
		bool isMaxLevelAndMaxQuality = false;
		bool nextCapIsForQuality = false;
		GeneralHelpers.GetXpCapInfo(oldFollower.FollowerLevel, oldFollower.Quality, out this.m_currentCap, out nextCapIsForQuality, out isMaxLevelAndMaxQuality);
		this.SetFollowerAppearance(oldFollower, nextCapIsForQuality, isMaxLevelAndMaxQuality, false, initialEffectDelay);
		GeneralHelpers.GetXpCapInfo(newFollower.FollowerLevel, newFollower.Quality, out this.m_newCap, out this.m_newCapIsQuality, out this.m_newFollowerIsMaxLevelAndMaxQuality);
		this.m_fancyNumberDisplay.SetValue((int)(this.m_currentCap - (uint)oldFollower.Xp), true, 0f);
		if (oldFollower.FollowerLevel != newFollower.FollowerLevel || oldFollower.Quality != newFollower.Quality)
		{
			this.m_fancyNumberDisplay.SetValue(0, initialEffectDelay);
		}
		else
		{
			this.m_fancyNumberDisplay.SetValue((int)(this.m_currentCap - (uint)newFollower.Xp), initialEffectDelay);
		}
	}

	private void SetFillValue(int newXPRemainingUntilNextLevel)
	{
		if (newXPRemainingUntilNextLevel == 0 && this.m_currentCap != this.m_newCap)
		{
			if (!this.m_showedLevelUpEffect)
			{
				Main.instance.m_UISound.Play_ChampionLevelUp();
				UiAnimMgr.instance.PlayAnim("FlameGlowPulse", this.m_followerPortrait.get_transform(), Vector3.get_zero(), 2f, 0f);
				UiAnimMgr.instance.PlayAnim("MinimapPulseAnim", this.m_followerPortrait.get_transform(), Vector3.get_zero(), 2f, 0f);
				this.m_showedLevelUpEffect = true;
			}
			JamGarrisonFollower jamGarrisonFollower = PersistentFollowerData.followerDictionary.get_Item(this.m_followerID);
			this.SetFollowerAppearance(jamGarrisonFollower, this.m_newCapIsQuality, this.m_newFollowerIsMaxLevelAndMaxQuality, false, 0f);
			this.m_currentCap = this.m_newCap;
			this.m_fancyNumberDisplay.SetValue((int)this.m_newCap, true, 0f);
			this.m_fancyNumberDisplay.SetValue((int)(this.m_newCap - (uint)jamGarrisonFollower.Xp), 0f);
		}
		this.m_xpAmountText.set_text(string.Concat(new object[]
		{
			string.Empty,
			(long)((ulong)this.m_currentCap - (ulong)((long)newXPRemainingUntilNextLevel)),
			"\\",
			this.m_currentCap
		}));
		float fillAmount = Mathf.Clamp01((float)((ulong)this.m_currentCap - (ulong)((long)newXPRemainingUntilNextLevel)) / this.m_currentCap);
		this.m_progressBarFillImage.set_fillAmount(fillAmount);
	}

	public int GetFollowerID()
	{
		return this.m_followerID;
	}
}
