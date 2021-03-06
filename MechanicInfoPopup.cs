using System;
using UnityEngine;
using UnityEngine.UI;
using WowStatConstants;

public class MechanicInfoPopup : MonoBehaviour
{
	public Image m_mechanicIcon;

	public Text m_mechanicName;

	public Text m_mechanicDescription;

	private void OnEnable()
	{
		Main.instance.m_backButtonManager.PushBackAction(BackAction.hideAllPopups, null);
	}

	private void OnDisable()
	{
		Main.instance.m_backButtonManager.PopBackAction();
	}
}
