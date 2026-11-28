using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static OndaS.Gameplay.UI.OndaSMenuUi;

namespace OndaS.Gameplay.UI
{
    public sealed class OndaSMainMenu : MonoBehaviour
    {
        [SerializeField] private Texture2D background;
        internal const string ScenePath = "Assets/OndaS/Art/Scenes/OndaS_MenuPrincipal.unity";
        private const string GameScene = "Assets/OndaS/Art/Scenes/OndaS_RetroEdificio.unity";
        private RectTransform canvas;
        private GameObject mainPanel, settingsPanel;
        private Button playButton, backButton;
        private Texture2D shadeTexture;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            AudioListener.volume = OndaSGameSettings.MasterVolume;
            BuildMenu();
        }

        private void Update()
        {
            if (settingsPanel != null && settingsPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ShowMain();
        }

        private void BuildMenu()
        {
            var cameraObject = new GameObject("Cámara del menú", typeof(Camera));
            var cam = cameraObject.GetComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black; cam.cullingMask = 0;
            var eventObject = new GameObject("Eventos del menú", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var root = new GameObject("Menú principal", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = root.GetComponent<RectTransform>();
            var rootCanvas = root.GetComponent<Canvas>(); rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var raw = Raw(canvas, "Edificio de noche", background);
            var aspect = raw.gameObject.AddComponent<AspectRatioFitter>(); aspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; aspect.aspectRatio = background != null ? (float)background.width / background.height : 16f/9f;
            shadeTexture = CreateShade();
            Raw(canvas, "Sombra del título", shadeTexture);
            // A subtle line grid gives the still background a low-resolution CRT texture.
            for (int i = 0; i < 90; i++)
            {
                var line = Image(canvas, "Línea CRT", new Vector2(0, -i * 12f), new Vector2(1920, 2), new Color(0,0,0,.10f));
                line.raycastTarget = false;
            }
            mainPanel = Panel("Opciones principales");
            var main = mainPanel.GetComponent<RectTransform>();
            Text(main, "Título sombra", "ONDAS", new Vector2(80,-68), new Vector2(950,230), 220, new Color32(16,12,12,220));
            Text(main, "Título", "ONDAS", new Vector2(74,-60), new Vector2(950,230), 220, Paper);
            Image(main,"Acento rojo",new Vector2(82,-284),new Vector2(92,5),Red);
            Text(main, "Subtítulo", "UNA NOCHE. UN SISMO.", new Vector2(190,-265), new Vector2(600,45), 25, new Color32(188,180,166,255));
            playButton = MenuButton(main, "01", "JUGAR", -410, StartGame);
            MenuButton(main, "02", "CONFIGURACIÓN", -540, ShowSettings);
            MenuButton(main, "03", "SALIR", -670, Quit);
            Text(main, "Pie", "MANTÉN LA CALMA. BUSCA REFUGIO.", new Vector2(82,-1005), new Vector2(820,34), 20, new Color32(157,151,138,210));
            BuildSettings();
            settingsPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        private void BuildSettings()
        {
            settingsPanel = Panel("Configuración"); var panel = settingsPanel.GetComponent<RectTransform>();
            Text(panel, "Título", "CONFIGURACIÓN", new Vector2(75,-80), new Vector2(900,110), 76, Paper);
            Image(panel,"Acento rojo",new Vector2(82,-222),new Vector2(92,5),Red);
            Text(panel, "Volumen título", "VOLUMEN GENERAL", new Vector2(83,-300), new Vector2(650,60), 39, Paper);
            AddVolumeSlider(panel, new Vector2(84, -390));
            Text(panel, "Ratón título", "SENSIBILIDAD DEL RATÓN", new Vector2(83,-500), new Vector2(740,60), 39, Paper);
            AddSensitivitySlider(panel, new Vector2(84, -590));
            backButton = MenuButton(panel, "‹", "VOLVER", -770, ShowMain);
            Text(panel, "Pista", "ESC PARA VOLVER", new Vector2(82,-1005), new Vector2(650,34), 20, new Color32(157,151,138,210));
        }
        private GameObject Panel(string name)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>(); rect.SetParent(canvas,false);
            rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero; return go;
        }
        private Button MenuButton(RectTransform parent, string index, string label, float y, UnityEngine.Events.UnityAction action)
        {
            var image = Image(parent, label, new Vector2(80,y), new Vector2(660,104), new Color(0.035f,.032f,.032f,.78f));
            var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image; var colors=button.colors;
            colors.normalColor=Color.white; colors.highlightedColor=new Color(1f,.78f,.73f,1); colors.selectedColor=colors.highlightedColor; colors.pressedColor=new Color(.65f,.30f,.27f,1); button.colors=colors;
            Image(image.rectTransform,"Raya",new Vector2(0,0),new Vector2(5,104),Red).raycastTarget=false;
            Text(image.rectTransform,"Número",index,new Vector2(32,-18),new Vector2(70,68),34,Red);
            Text(image.rectTransform,"Etiqueta",label,new Vector2(120,-11),new Vector2(520,82),55,Paper);
            button.onClick.AddListener(action); return button;
        }
        private static RawImage Raw(RectTransform parent, string name, Texture texture)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<RawImage>(); image.texture = texture; image.raycastTarget = false; return image;
        }
        private static Texture2D CreateShade()
        {
            var tex=new Texture2D(64,2,TextureFormat.RGBA32,false); tex.name="Degradado menú";
            for(int x=0;x<64;x++) for(int y=0;y<2;y++) { float t=x/63f; float a=Mathf.Lerp(.90f,.08f,Mathf.SmoothStep(.05f,.8f,t)); tex.SetPixel(x,y,new Color(.015f,.012f,.014f,a)); }
            tex.wrapMode=TextureWrapMode.Clamp; tex.filterMode=FilterMode.Bilinear; tex.Apply(); return tex;
        }
        private void ShowSettings() { mainPanel.SetActive(false); settingsPanel.SetActive(true); EventSystem.current.SetSelectedGameObject(backButton.gameObject); }
        private void ShowMain() { settingsPanel.SetActive(false); mainPanel.SetActive(true); EventSystem.current.SetSelectedGameObject(playButton.gameObject); }
        private static void StartGame() { SceneManager.LoadScene(GameScene); }
        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void OnDestroy() { if (shadeTexture != null) Destroy(shadeTexture); }
    }
}
