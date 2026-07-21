using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";

    public enum TutorialStep
    {
        None,
        OpenGacha,
        Pull5x,
        RevealAll,
        OpenCollection,
        OpenDeckEditor,
        AutoEquip,
        Completed
    }

    [Header("Targets")]
    public Button skipButton;
    public Button gachaNavButton;
    public Button gacha5xButton;
    public TextMeshProUGUI flipAllLabel;
    public Button collectionNavButton;
    public Button deckEditorButton;
    public Button autoEquipButton;

    [Header("UI Feedback")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI messageText;

    public GameObject overlayPanel;

    [Header("Cursor")]
    public GameObject tutorialCursor;


    public TutorialStep currentStep = TutorialStep.None;
    private Button highlightedButton;
    private Image overlayImage;
    private RectTransform overlayRect;
    private RectTransform highlightedTargetRect;
    private RectTransform cursorRect;
    private Image cursorImage;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        

        EnsureCursor();

        if (PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 0)
        {
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        if (currentStep != TutorialStep.None && currentStep != TutorialStep.Completed)
            return;

        currentStep = TutorialStep.OpenGacha;
        ShowCurrentStep();
        skipButton?.onClick.AddListener(EndTutorial);
    }


    public void EndTutorial()
    {
        currentStep = TutorialStep.Completed;
        PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
        PlayerPrefs.Save();

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        ClearHighlight();
        if (overlayPanel != null)
            overlayPanel.SetActive(false);
        DialogManager.Instance.Hide();
    }

    public TutorialStep GetCurrentStep()
    {
        return currentStep;
    }

    public void NotifyGachaScreenOpened()
    {
        TryAdvanceStep(TutorialStep.OpenGacha);
        
    }

    public void NotifyGacha5xClicked()
    {
        TryAdvanceStep(TutorialStep.Pull5x);
        
    }

    public void NotifyRevealAllClicked()
    {
        TryAdvanceStep(TutorialStep.RevealAll);
        
    }

    public void NotifyCollectionScreenOpened()
    {
        TryAdvanceStep(TutorialStep.OpenCollection);
        
    }

    public void NotifyDeckEditorOpened()
    {
        
        TryAdvanceStep(TutorialStep.OpenDeckEditor);
    }

    public void NotifyAutoEquipClicked()
    {
       
        TryAdvanceStep(TutorialStep.AutoEquip);
    }

    private void TryAdvanceStep(TutorialStep expectedStep)
    {
       

        if (currentStep != expectedStep || currentStep == TutorialStep.Completed)
        {
            
            return;
        }

        var nextStep = (TutorialStep)((int)currentStep + 1);
        Debug.Log($"[Tutorial] Avançando de {currentStep} para {nextStep}");
        currentStep = nextStep;
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        

        if (currentStep == TutorialStep.Completed)
        {
            DialogManager.Instance.ShowPersistent("Excellent! Now you can participate in your first battle.",KrakenExpression.Proud);
            EndTutorial();
            return;
        }

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        switch (currentStep)
        {
            case TutorialStep.OpenGacha:
                DialogManager.Instance.ShowPersistent("First, open the gacha screen to summon cards.",KrakenExpression.Mischievous);
                HighlightTarget(gachaNavButton);
                break;
            case TutorialStep.Pull5x:
                DialogManager.Instance.ShowPersistent("Now use the 5x gacha to summon five cards at once.",KrakenExpression.Explaining);
                HighlightTarget(gacha5xButton);
                break;
            case TutorialStep.RevealAll:
                DialogManager.Instance.ShowPersistent("Reveal all the cards you just summoned.",KrakenExpression.Cute);
                HighlightTarget(flipAllLabel);
                break;
            case TutorialStep.OpenCollection:
                DialogManager.Instance.ShowPersistent("Great! Now go to your collection to build your battle deck.",KrakenExpression.Happy);
                HighlightTarget(collectionNavButton);
                break;
            case TutorialStep.OpenDeckEditor:
                DialogManager.Instance.ShowPersistent("Open the deck editor to organize your cards.",KrakenExpression.Explaining);
                HighlightTarget(deckEditorButton);
                break;
            case TutorialStep.AutoEquip:
                DialogManager.Instance.ShowPersistent("Use Auto-Equip to quickly assemble a deck with the best cards.",KrakenExpression.Explaining);
                HighlightTarget(autoEquipButton);
                break;
            default:
                ClearHighlight();
                break;
        }
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
        else
        {
            DialogManager.Instance.ShowPersistent(message,KrakenExpression.Explaining);
        }
    }

    private void HighlightTarget(Component target)
    {
        ClearHighlight();
        if (target == null)
            return;

        highlightedButton = target as Button;
        if (highlightedButton == null && target is TextMeshProUGUI label)
        {
            highlightedButton = label.GetComponent<Button>();
        }

        highlightedTargetRect = target as RectTransform;
        if (highlightedTargetRect == null && highlightedButton != null)
        {
            highlightedTargetRect = highlightedButton.GetComponent<RectTransform>();
        }

        if (highlightedButton == null || highlightedTargetRect == null)
        {
            return;
        }

        if (overlayPanel != null)
        {
            overlayPanel.SetActive(true);
            BuildOverlayCutout(highlightedTargetRect);
        }

        EnsureCursor();
        PositionCursorToTarget(highlightedTargetRect);
    }

    private void EnsureCursor()
    {
        if (tutorialCursor == null)
        {
            tutorialCursor = new GameObject("TutorialCursor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        }

        cursorRect = tutorialCursor.GetComponent<RectTransform>();
        cursorImage = tutorialCursor.GetComponent<Image>();

        if (cursorRect != null)
        {
            cursorRect.anchorMin = new Vector2(0.5f, 0.5f);
            cursorRect.anchorMax = new Vector2(0.5f, 0.5f);
            cursorRect.pivot = new Vector2(0.5f, 0.5f);
            cursorRect.localScale = Vector3.one;
            cursorRect.localRotation = Quaternion.Euler(0f, 0f, -20f);
            cursorRect.gameObject.SetActive(false);
        }

        if (cursorImage != null)
        {
            cursorImage.raycastTarget = false;
        }
    }

    private void PositionCursorToTarget(RectTransform targetRect)
    {
        if (cursorRect == null || targetRect == null)
            return;

        Vector3 targetWorldPosition = targetRect.TransformPoint(new Vector3(18f, -80f, 0f));
        Vector3 localPoint;

        if (cursorRect.parent != null)
        {
            localPoint = cursorRect.parent.InverseTransformPoint(targetWorldPosition);
        }
        else
        {
            localPoint = targetWorldPosition;
        }

        cursorRect.localPosition = localPoint;
        cursorRect.localScale = Vector3.one;
        cursorRect.localRotation = Quaternion.Euler(0f, 0f, -20f);
        cursorRect.gameObject.SetActive(true);
    }

    private void BuildOverlayCutout(RectTransform targetRect)
    {
        if (overlayPanel == null || targetRect == null)
            return;

        overlayRect = overlayPanel.GetComponent<RectTransform>();
        if (overlayRect == null)
            return;

        overlayImage = overlayPanel.GetComponent<Image>();
        if (overlayImage == null)
            overlayImage = overlayPanel.AddComponent<Image>();

        overlayImage.type = Image.Type.Simple;
        overlayImage.preserveAspect = false;
        overlayImage.color = new Color(0f, 0f, 0f, 0.75f);
        overlayImage.raycastTarget = true;
        overlayImage.sprite = null;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(targetRect);

        var eventTrigger = overlayPanel.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = overlayPanel.AddComponent<EventTrigger>();

        eventTrigger.triggers.Clear();
        var pointerClickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        pointerClickEntry.callback.AddListener((BaseEventData eventData) =>
        {
            
            if (TryForwardTutorialClick((PointerEventData)eventData, targetRect))
            {
                Debug.Log($"[Tutorial] Clique aceito para o alvo atual: {highlightedButton?.name ?? "null"}");
                return;
            }else{
                Debug.Log($"[Tutorial] Clique ignorado. O clique não estava dentro do alvo: {highlightedButton?.name ?? "null"}");
            }

            
        });

        eventTrigger.triggers.Add(pointerClickEntry);
    }

    private bool TryForwardTutorialClick(PointerEventData data, RectTransform targetRect)
    {
        if (highlightedButton == null || data == null || targetRect == null)
            return false;

        var camera = data.pressEventCamera ?? Camera.main;
        if (camera == null)
            return false;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRect,
            data.position,
            camera,
            out localPoint
        );

        bool insideTarget =
            localPoint.x >= -targetRect.rect.width / 2f &&
            localPoint.x <= targetRect.rect.width / 2f &&
            localPoint.y >= -targetRect.rect.height / 2f &&
            localPoint.y <= targetRect.rect.height / 2f;

        if (!insideTarget)
            return false;

        highlightedButton.onClick?.Invoke();
        return true;
    }

    private void ClearHighlight()
    {
        if (highlightedButton != null)
        {
            highlightedButton = null;
        }

        highlightedTargetRect = null;

        if (cursorRect != null)
            cursorRect.gameObject.SetActive(false);

        if (overlayPanel != null)
            overlayPanel.SetActive(false);
    }
}
