# Card-Clash AI Coding Guidelines

## Architecture Overview
Card-Clash is a Unity-based card battle game inspired by Triple Triad mechanics. Core components:
- **BoardManager**: Singleton handling 3x3 game board logic, card placement, and capture rules
- **CardData**: ScriptableObject defining cards with 4 directional values (top/right/bottom/left), rarity, and artwork
- **Screen System**: UI screens (e.g., HomeScreen) managed via Unity's scene system with fade transitions
- **Managers**: Singleton pattern for AudioManager, BattleSetupManager, PlayerDeckManager, etc.

Data flows from CardData assets → DeckManagers → BoardManager for gameplay, with UI updates via events/callbacks.

## Key Patterns & Conventions
- **Singleton Managers**: Use `Instance` property for global access (e.g., `BoardManager.Instance.CheckCaptures(index)`)
- **Asset Loading**: Use `Resources.Load<Sprite>()` for dynamic assets, stored in `Assets/Resources/Art/Artworks/`
- **Persistence**: PlayerPrefs for simple data like character selection; avoid for complex game state
- **Special Rules**: Implement in BoardManager with inspector toggles (ruleSame, rulePlus); capture logic extends standard higher-value rule
- **UI Fading**: Coroutines for smooth transitions (e.g., FadeInImage in HomeScreen)
- **Portuguese Naming**: UI elements and comments use Portuguese (e.g., "txtTickets", "Personagem")

## Development Workflow
- **Build**: Unity Editor → File → Build Settings; target platforms via URP pipeline
- **Debug**: Attach Visual Studio debugger; use Debug.Log for gameplay events
- **Testing**: Manual playtesting in Unity Editor; no automated tests currently
- **Dependencies**: Managed via Unity Package Manager; key packages: Input System, URP, Ads, Purchasing

## Code Examples
- Card capture: Check adjacents in BoardManager.cs lines 35-85, handling directional comparisons
- UI screen: HomeScreen.cs OnEnable() loads character via PlayerPrefs and Resources
- Special rule: CheckRuleSame() in BoardManager.cs for "MESMO" logic (equal values capture)

Reference: [BoardManager.cs](Assets/Resources/Scripts/BoardManager.cs) for core gameplay, [CardData.cs](Assets/Resources/Scripts/CardData.cs) for data structure.</content>
<parameter name="filePath">c:\Users\Mike\Projetos\Card-Clash\.github\copilot-instructions.md