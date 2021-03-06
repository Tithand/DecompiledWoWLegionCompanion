using System;
using System.Collections.Generic;
using UnityEngine;
using WowStaticData;

public class TextureAtlas
{
	private static TextureAtlas s_instance;

	private static bool s_initialized;

	private Dictionary<int, Dictionary<int, Sprite>> m_atlas;

	private Dictionary<string, int> m_atlasMemberIDCache;

	public static TextureAtlas instance
	{
		get
		{
			if (TextureAtlas.s_instance == null)
			{
				TextureAtlas.s_instance = new TextureAtlas();
				TextureAtlas.s_instance.InitAtlas();
			}
			return TextureAtlas.s_instance;
		}
	}

	public static Sprite GetSprite(int memberID)
	{
		return TextureAtlas.instance.GetAtlasSprite(memberID);
	}

	private void InitAtlas()
	{
		if (TextureAtlas.s_initialized)
		{
			Debug.Log("WARNING! ATTEMPTED TO INIT TEXTURE ATLAS, BUT IT IS ALREADY INITIALIZED!! IGNORING");
			return;
		}
		this.m_atlas = new Dictionary<int, Dictionary<int, Sprite>>();
		TextAsset textAsset = Resources.Load("TextureAtlas/AtlasDirectory") as TextAsset;
		string text = textAsset.ToString();
		int num = 0;
		int num2;
		string text2;
		while (true)
		{
			num2 = text.IndexOf('\n', num);
			if (num2 >= 0)
			{
				text2 = text.Substring(num, num2 - num + 1).Trim();
				string text3 = text2.Substring(text2.get_Length() - 10);
				int num3 = Convert.ToInt32(text3);
				Sprite[] array = Resources.LoadAll<Sprite>("TextureAtlas/" + text2);
				if (array.Length <= 0)
				{
					break;
				}
				Dictionary<int, Sprite> dictionary = new Dictionary<int, Sprite>();
				Sprite[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					Sprite sprite = array2[i];
					int num4 = Convert.ToInt32(sprite.get_name());
					dictionary.Add(num4, sprite);
				}
				this.m_atlas.Add(num3, dictionary);
				num = num2 + 1;
			}
			if (num2 <= 0)
			{
				goto Block_5;
			}
		}
		Debug.Log("Found no sprites in atlas " + text2);
		num = num2 + 1;
		throw new Exception("Atlas in Resources folder is missing or has no sprites: " + text2);
		Block_5:
		this.m_atlasMemberIDCache = new Dictionary<string, int>();
		TextureAtlas.s_initialized = true;
	}

	private int GetOverrideMemberID(int memberID)
	{
		switch (memberID)
		{
		case 6128:
			memberID = 6100;
			break;
		case 6129:
			memberID = 6127;
			break;
		case 6130:
			memberID = 6126;
			break;
		case 6131:
			memberID = 6095;
			break;
		case 6132:
			memberID = 6097;
			break;
		}
		return memberID;
	}

	public Sprite GetAtlasSprite(int memberID)
	{
		memberID = this.GetOverrideMemberID(memberID);
		UiTextureAtlasMemberRec record = StaticDB.uiTextureAtlasMemberDB.GetRecord(memberID);
		if (record == null)
		{
			Debug.LogWarning("GetAtlasSprite(): Unknown member ID " + memberID);
			return null;
		}
		Dictionary<int, Sprite> dictionary;
		this.m_atlas.TryGetValue((int)record.UiTextureAtlasID, ref dictionary);
		if (dictionary == null)
		{
			Debug.LogWarning("GetAtlasSprite(): Unknown atlas ID " + record.UiTextureAtlasID);
			return null;
		}
		Sprite result;
		dictionary.TryGetValue(record.ID, ref result);
		return result;
	}

	public static int GetUITextureAtlasMemberID(string atlasMemberName)
	{
		int textureAtlasMemberID = 0;
		TextureAtlas.instance.m_atlasMemberIDCache.TryGetValue(atlasMemberName, ref textureAtlasMemberID);
		if (textureAtlasMemberID > 0)
		{
			return textureAtlasMemberID;
		}
		StaticDB.uiTextureAtlasMemberDB.EnumRecords(delegate(UiTextureAtlasMemberRec memberRec)
		{
			if (memberRec.CommittedName != null && atlasMemberName != null && memberRec.CommittedName.ToLower() == atlasMemberName.ToLower())
			{
				textureAtlasMemberID = memberRec.ID;
				TextureAtlas.instance.m_atlasMemberIDCache.Add(atlasMemberName, memberRec.ID);
				return false;
			}
			return true;
		});
		return textureAtlasMemberID;
	}
}
