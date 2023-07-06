using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Translations
{
    private static Translations m_Instance;
    private TextAsset m_TextAsset;
    private JObject m_AllCompanyNodeInfo;

    // Singleton
    public static Translations Instance
    {
        get
        {
            if (null == m_Instance)
                m_Instance = new Translations();

            return m_Instance;
        }
    }

    private Dictionary<string, Dictionary<string, string>> m_AllTranslations;
    private string m_CurrentLang;

    public Translations()
    {
        // Load company info and node info from file
        m_TextAsset = Resources.Load<TextAsset>("ITO_TextData");
        string companyNodeInfoJSON = m_TextAsset.text;
        m_AllCompanyNodeInfo = JObject.Parse(companyNodeInfoJSON);

        m_AllTranslations = new Dictionary<string, Dictionary<string, string>>();

        // "Menu"
        Dictionary<string, string> menu = new Dictionary<string, string>();
        menu.Add("en", "Menu");
        m_AllTranslations.Add("Menu", menu);

        // "Upgrade"

        // "Current Cubit: "
        Dictionary<string, string> currentCubit = new Dictionary<string, string>();
        currentCubit.Add("en", "Current Cubit: ");
        m_AllTranslations.Add("Current Cubit: ", currentCubit);

        // "Cache Bonus: "
        Dictionary<string, string> cacheBonus = new Dictionary<string, string>();
        cacheBonus.Add("en", "Cache Bonus: ");
        m_AllTranslations.Add("Cache Bonus: ", cacheBonus);

        // "Current Level: "
        Dictionary<string, string> currentLevel = new Dictionary<string, string>();
        currentLevel.Add("en", "Current Level: ");
        m_AllTranslations.Add("Current Level: ", currentLevel);

        // "Previous Time Machine Activation Level: "
        Dictionary<string, string> previousTMActivition = new Dictionary<string, string>();
        previousTMActivition.Add("en", "Previous Time Machine Activation Level: ");
        m_AllTranslations.Add("Previous Time Machine Activation Level: ", previousTMActivition);

        // "Activate and collect: "
        Dictionary<string, string> activateAndCollect = new Dictionary<string, string>();
        activateAndCollect.Add("en", "Activate and collect: ");
        m_AllTranslations.Add("Activate and collect: ", activateAndCollect);

        // "Cubit"
        Dictionary<string, string> cubit = new Dictionary<string, string>();
        cubit.Add("en", "Cubit");
        m_AllTranslations.Add("Cubit", cubit);

        // "Cubits"
        Dictionary<string, string> cubits = new Dictionary<string, string>();
        cubits.Add("en", "Cubits");
        m_AllTranslations.Add("Cubits", cubits);

        // "Activate"
        Dictionary<string, string> activate = new Dictionary<string, string>();
        activate.Add("en", "Activate");
        m_AllTranslations.Add("Activate", activate);

        // "Thank you for your time coming to visit us. You have decent knowledge and positive altitude. Unfortunately"
        Dictionary<string, string> unfortunately = new Dictionary<string, string>();
        unfortunately.Add("en", "Pass level 1 to activate time machine");
        m_AllTranslations.Add("Unable to activate time machine", unfortunately);
    }

    public void SelectLang(string lang)
    {
        if (!m_AllTranslations["Activate"].ContainsKey(lang))
        {
            throw new System.Exception("Wrong language specified for translation: " + lang);
        }
        else
        {
            m_CurrentLang = lang;
        }
    }

    public string GetText(string text)
    {
        if (m_AllTranslations.ContainsKey(text))
            return (m_AllTranslations[text][m_CurrentLang]);
        else if (m_AllCompanyNodeInfo[m_CurrentLang][text] != null)
            return m_AllCompanyNodeInfo[m_CurrentLang][text].Value<string>();
        else
        {
            UnityEngine.Debug.LogWarning("Undefined translation for: " + text);
            return text;
        }
    }
}
