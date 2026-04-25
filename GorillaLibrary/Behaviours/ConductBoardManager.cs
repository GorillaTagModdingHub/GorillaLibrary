using GorillaLibrary.Extensions;
using GorillaLibrary.Utilities;
using GorillaTag;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Random = System.Random;

namespace GorillaLibrary.Behaviours;

internal class ConductBoardManager : MonoBehaviour
{
    private int PageCount => boardContent.Count;

    private readonly List<Section> boardContent = [];

    private ModeSelectButton buttonTemplate;

    private GameObject stumpRootObject;

    private Transform conductTransform;

    private TextMeshPro baseHeaderText, baseBodyText, headerText, bodyText, footerText;

    private int currentPage = 0;

    private Dictionary<string, Supporter[]> _supporters;

    private Dictionary<string, Color> _sponsorColours = new()
    {
        { "Basic", new Color32(255, 123, 121, 255) },
        { "Dweller", new Color32(255, 145, 144, 255) },
        { "Prestige", new Color32(248, 166, 165, 255) }
    };

    public void Start()
    {
        buttonTemplate = FindFirstObjectByType<GameModeSelectorButtonLayout>().GetField<ModeSelectButton>("pf_button");

        stumpRootObject = Array.Find(ZoneUtility.Objects, gameObject => gameObject.name == "TreeRoom");
        conductTransform = stumpRootObject.transform.FindChildRecursive("code of conduct");

        baseHeaderText = stumpRootObject.transform.FindChildRecursive("CodeOfConductHeadingText")?.GetComponent<TextMeshPro>();
        if (baseHeaderText == null)
        {
            // Logging.Warning("COC (Code of Conduct) header text is missing");
            return;
        }

        GameObject headingTextObject = Instantiate(baseHeaderText.gameObject);
        headingTextObject.transform.position = baseHeaderText.transform.position;
        headingTextObject.transform.rotation = baseHeaderText.transform.rotation;
        headingTextObject.transform.localScale = baseHeaderText.transform.localScale;
        SanitizeTextObject(headingTextObject);

        headerText = headingTextObject.GetComponent<TextMeshPro>();
        headerText.fontSizeMax = baseHeaderText.fontSize;
        headerText.enableAutoSizing = true;
        headerText.textWrappingMode = TextWrappingModes.NoWrap;

        GameObject footerTextObject = Instantiate(baseHeaderText.gameObject);
        footerTextObject.transform.position = baseHeaderText.transform.position;
        footerTextObject.transform.rotation = baseHeaderText.transform.rotation;
        footerTextObject.transform.localScale = baseHeaderText.transform.localScale;
        SanitizeTextObject(footerTextObject);

        MelonInfoAttribute info = Melon<Mod>.Instance.Info;
        footerText = footerTextObject.GetComponent<TextMeshPro>();
        footerText.text = $"{info.Name} {info.Version}".ToUpper();
        footerText.enableAutoSizing = false;
        footerText.fontSize = 45;
        footerText.margin = new Vector4(0f, 110f, 0f, 0f);
        footerText.characterSpacing = -11.5f;
        footerText.enabled = true;
        footerText.renderer.enabled = true;

        baseBodyText = stumpRootObject.transform.FindChildRecursive("COCBodyText")?.GetComponent<TextMeshPro>();
        if (baseBodyText == null)
        {
            // Logging.Warning("COC (Code of Conduct) body text is missing");
            return;
        }

        GameObject bodyTextObject = Instantiate(baseBodyText.gameObject);
        bodyTextObject.transform.position = baseBodyText.transform.position;
        bodyTextObject.transform.rotation = baseBodyText.transform.rotation;
        bodyTextObject.transform.localScale = baseBodyText.transform.localScale;
        SanitizeTextObject(bodyTextObject);

        bodyText = bodyTextObject.GetComponent<TextMeshPro>();
        bodyText.fontSizeMax = baseBodyText.fontSize;
        bodyText.fontSizeMin = 0f;
        bodyText.enableAutoSizing = true;
        bodyText.margin = new Vector4(0f, 0f, 0f, 36f);
        bodyText.richText = true;

        boardContent.Insert(0, new());

        CreateButton(-1f, "-->", NextPage);
        CreateButton(1f, "<--", PrevPage);

        ShowPage();
        CheckVersion();
        CreateEntries();
    }

    private void NextPage()
    {
        currentPage = (currentPage + 1) % PageCount;
        ShowPage();
    }

    private void PrevPage()
    {
        currentPage = (currentPage <= 0) ? PageCount - 1 : currentPage - 1;
        ShowPage();
    }

    private void ShowPage()
    {
        if (baseHeaderText == null || baseBodyText == null) return;

        Section content = boardContent.ElementAtOrDefault(Mathf.Max(0, Mathf.Min(currentPage, boardContent.Count - 1)));

        if (content.UseBaseText)
        {
            baseHeaderText.renderer?.forceRenderingOff = false;
            headerText.enabled = false;

            baseBodyText.renderer?.forceRenderingOff = false;
            bodyText.enabled = false;
        }
        else
        {
            baseHeaderText.renderer?.forceRenderingOff = true;
            headerText.enabled = true;
            headerText.text = content.Title;

            baseBodyText.renderer?.forceRenderingOff = true;
            bodyText.enabled = true;
            bodyText.text = content.Body;
        }
    }

    private void CreateButton(float horizontalPosition, string text, Action onButtonPressed = null)
    {
        GameObject buttonObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buttonObject.transform.parent = conductTransform;
        buttonObject.transform.localPosition = new Vector3(horizontalPosition, 0.52f, 0.13f);
        buttonObject.transform.localRotation = Quaternion.Euler(353.5f, 0f, 0f);
        buttonObject.transform.localScale = new Vector3(0.1427168f, 0.1427168f, 0.1f);
        buttonObject.GetComponent<Renderer>().material = buttonTemplate.unpressedMaterial;
        buttonObject.GetComponent<Collider>().isTrigger = true;
        buttonObject.SetLayer(UnityLayer.GorillaInteractable);

        GameObject textObject = new();
        textObject.transform.parent = buttonObject.transform;
        textObject.transform.localPosition = Vector3.forward * 0.525f;
        textObject.transform.localRotation = Quaternion.AngleAxis(180f, Vector3.up);
        textObject.transform.localScale = Vector3.one;

        TextMeshPro textMeshPro = textObject.AddComponent<TextMeshPro>();
        textMeshPro.font = buttonTemplate?.GetComponentInChildren<TMP_Text>()?.font ?? stumpRootObject.GetComponentInChildren<GorillaComputerTerminal>()?.myScreenText?.font;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.characterSpacing = -10f;
        textMeshPro.overflowMode = TextOverflowModes.Overflow;
        textMeshPro.fontSize = 3f;
        textMeshPro.color = new Color(0.1960784f, 0.1960784f, 0.1960784f);
        textMeshPro.text = text;

        GorillaPressableButton pressableButton = buttonObject.AddComponent<GorillaPressableButton>();
        pressableButton.buttonRenderer = buttonObject.GetComponent<MeshRenderer>();
        pressableButton.unpressedMaterial = buttonTemplate.unpressedMaterial;
        pressableButton.pressedMaterial = buttonTemplate.pressedMaterial;

        UnityEvent onPressEvent = new();
        onPressEvent.AddListener(new UnityAction(() =>
        {
            pressableButton.StartCoroutine(ButtonColourUpdate(pressableButton));
        }));
        onPressEvent.AddListener(new UnityAction(onButtonPressed));
        pressableButton.onPressButton = onPressEvent;
    }

    public async void CheckVersion()
    {
        string endpoint = @"https://api.github.com/repos/GorillaTagModdingHub/GorillaLibrary/releases/latest";
        UnityWebRequest webRequest = UnityWebRequest.Get(endpoint);
        UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();
        await asyncOperation;

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Melon<Mod>.Logger.Error($"Version could not be accessed from {webRequest.url}: {webRequest.downloadHandler.error}");
            return;
        }

        string version = JsonConvert.DeserializeObject<Dictionary<string, string>>(webRequest.downloadHandler.text)["tag_name"];
        Melon<Mod>.Logger.Msg($"GorillaLibrary is on Version {version}");

        MelonInfoAttribute info = Melon<Mod>.Instance.Info;
        if (Version.TryParse(info.Version, out Version installedVersion) && Version.TryParse(version, out Version latestVersion) && latestVersion > installedVersion)
        {
            footerText.color = Color.red;
            footerText.fontSize *= 0.85f;
            footerText.text = $"{info.Name} {info.Version}: Out of Date".ToUpper();
        }
    }

    private async void CreateEntries()
    {
        await DownloadEntries();

        /*
        foreach (var info in Chainloader.PluginInfos)
        {
            if (info.Value is null) continue;

            BaseUnityPlugin plugin = info.Value.Instance;
            if (plugin is null) continue;

            Type type = plugin.GetType();
            IEnumerable<ModdedBoardTextAttribute> attributes = type.GetCustomAttributes<ModdedBoardTextAttribute>();

            if (attributes is not null)
            {
                var assembly = type.Assembly;
                var names = assembly.GetManifestResourceNames();

                foreach (ModdedBoardTextAttribute attribute in attributes)
                {
                    if (string.IsNullOrEmpty(attribute.Title) || string.IsNullOrWhiteSpace(attribute.Title) || string.IsNullOrEmpty(attribute.Text) || string.IsNullOrWhiteSpace(attribute.Text)) continue;

                    if (names.SingleOrDefault(resourceName => resourceName == attribute.Text) is string resourceName)
                    {
                        using Stream stream = assembly.GetManifestResourceStream(resourceName);
                        using StreamReader reader = new(stream);
                        string resourceText = reader.ReadToEnd();
                        boardContent.Add(new(attribute.Title, resourceText));
                        continue;
                    }

                    boardContent.Add(new(attribute.Title, attribute.Text));
                }
            }
        }
        */

        Random random = new(Mathf.FloorToInt(DateTime.UtcNow.DayOfYear + DateTime.UtcNow.Year));

        using UnityWebRequest request = UnityWebRequest.Get("https://gworkers.soweli.uk/supporters");
        await request.SendWebRequest().AsAwaitable();
        _supporters = request.result == UnityWebRequest.Result.Success ? JsonConvert.DeserializeObject<Dictionary<string, Supporter[]>>(request.downloadHandler.text) : null;

        Melon<Mod>.Logger.Msg("going thru tiers now");

        foreach (var (tier, supportersInTier) in _supporters)
        {
            Melon<Mod>.Logger.Msg(tier);

            string header;

            StringBuilder stringBuilder = new();

            if (tier == "Prestige")
            {
                header = "Featured Supporters";

                stringBuilder.AppendLine($"<align=center>Entirety of <color=#{ColorUtility.ToHtmlStringRGB(_sponsorColours[tier])}>Prestige Tier</color></align>").AppendLine();

                for (int i = 0; i < supportersInTier.Length; i++)
                {
                    Supporter supporter = supportersInTier[i];
                    stringBuilder.Append(i + 1).Append(". ").Append(supporter.DisplayName).Append(" - ").Append(supporter.Username).AppendLine();
                }

                goto AddContent;
            }

            header = $"Featured Supporters";
            List<int> supporterIndicies = [.. Enumerable.Range(0, supportersInTier.Length)];

            stringBuilder.AppendLine($"<align=center>Selection from <color=#{ColorUtility.ToHtmlStringRGB(_sponsorColours[tier])}>{tier} Tier</color></align>").AppendLine();
            stringBuilder.Append("<size=140%>");

            for (int i = 0; i < 5; i++)
            {
                int realIndex = random.Next() % supporterIndicies.Count;
                int index = supporterIndicies[realIndex];
                Supporter supporter = supportersInTier[index];
                supporterIndicies.RemoveAt(realIndex);
                stringBuilder.Append(i + 1).Append(". ").Append(supporter.DisplayName).Append(" - ").Append(supporter.Username).AppendLine().AppendLine();
            }

            stringBuilder.Append("</size>");

        AddContent:

            stringBuilder.Append("</align>");

            boardContent.Add(new(header.ToUpper(), stringBuilder.ToString()));

            Melon<Mod>.Logger.Msg("added");
        }
    }

    public async Task DownloadEntries()
    {
        /*
        using UnityWebRequest webRequest = UnityWebRequest.Get(string.Join('/', Constants.InfoRepositoryURL, "ConductBoard", "Entries.json"));
        UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();
        await asyncOperation;

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            foreach (JObject item in JArray.Parse(webRequest.downloadHandler.text).Cast<JObject>())
            {
                Logging.Message(item.ToString(Formatting.Indented));

                using UnityWebRequest webRequest2 = UnityWebRequest.Get(string.Join('/', Constants.InfoRepositoryURL, (string)item.Property("body").Value));
                asyncOperation = webRequest2.SendWebRequest();
                await asyncOperation;

                if (webRequest2.result != UnityWebRequest.Result.Success)
                {
                    Logging.Fatal($"Body text could not be accessed from {webRequest2.url}");
                    Logging.Error(webRequest.downloadHandler.error);
                    continue;
                }

                boardContent.Add(new((string)item.Property("title").Value, webRequest2.downloadHandler.text));
            }
        }
        else
        {
            // Logging.Fatal($"ModData could not be accessed from {webRequest.url}");
            // Logging.Info(webRequest.downloadHandler.error);
        }
        */

        
    }

    private IEnumerator ButtonColourUpdate(GorillaPressableButton pressableButton)
    {
        pressableButton.isOn = true;
        pressableButton.UpdateColor();

        yield return new WaitForSeconds(pressableButton.debounceTime);
        if ((pressableButton.touchTime + pressableButton.debounceTime) < Time.time)
        {
            pressableButton.isOn = false;
            pressableButton.UpdateColor();
        }

        yield break;
    }

    private void SanitizeTextObject(GameObject gameObject)
    {
        Type[] typesToRemove = [typeof(PlayFabTitleDataTextDisplay), typeof(LocalizedText), typeof(StaticLodGroup)];
        Component[] components = gameObject.GetComponents<Component>();

        for (int i = 0; i < components.Length; i++)
        {
            Type type = components[i].GetType();
            if (typesToRemove.Contains(type)) Destroy(components[i]);
        }
    }

    private struct Section
    {
        public bool UseBaseText;

        [TextArea(1, 1)]
        public string Title;

        [TextArea(12, 32)]
        public string Body;

        public Section()
        {
            UseBaseText = true;
        }

        public Section(string title, string body)
        {
            UseBaseText = false;
            Title = title;
            Body = body;
        }
    }

    [Serializable]
    private class Supporter
    {
        [JsonProperty("display")]
        public string DisplayName { get; set; }

        [JsonProperty("user")]
        public string Username { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("origin")]
        public string Platform { get; set; }
    }
}
