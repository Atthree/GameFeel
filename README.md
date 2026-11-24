# Game Feel

**Game feel / juice toolkit for Unity 6**: lightweight tweens, trauma-based camera shake, hit stop,
slow motion, hit flash, floating damage numbers, and a no-code **Feedback Player** that combines them.

[English](#english) · [Türkçe](#türkçe)

---

<a name="english"></a>
# English

"Juice" is the small feedback that makes an action feel good: the screen shakes, the game freezes
for a few hundredths of a second on a hit, the enemy flashes white, numbers fly out. Same mechanics,
very different feel.

- **Tweens** (no DOTween needed): move, scale, rotate, punch, shake, numbers, colors, timers, 12 eases,
  loops/yoyo, delays, callbacks, unscaled time.
- **Camera shake** (trauma-based, Perlin noise). Works on top of any camera script without drifting.
- **Hit stop** (freeze frame) and **slow motion**, safe with your pause menu.
- **Hit flash** without copying materials (URP and Built-in).
- **Floating text** for damage numbers, pooled, no TextMeshPro setup needed.
- **Feedback Player**: build "shake + hit stop + flash + punch + particles + sound" in the Inspector,
  play it with one call.
- **Accessibility switches** for shake, flash and time effects.

**Requirements:** Unity 6 (6000.0+). No other packages.

## Step-by-step setup

### 1. Add the package

**Window > Package Manager** → **+** → **Install package from git URL…** →

```
https://github.com/Atthree/GameFeel.git
```

### 2. Try the demo

1. **Tools > Game Feel > Create Demo Scene**, then **Play**.
2. Click the three targets. Each hit: camera shake, hit stop, white flash, squash, sparks, sound and a
   damage number (20% are critical).
3. Turn effects off one by one in the panel (or all at once with **JUICE**) and click again: same
   click, much weaker feeling. That's what juice does.
4. **Zıpla** (jump) shows squash & stretch made from a chain of tweens; **Büyük patlama** (big
   explosion) shows full shake + slow motion.
5. The **Sarsıntı şiddeti** slider is an example of an accessibility setting.

### 3. One-liners from code

```csharp
using Atthree.GameFeel;

Feel.Shake(0.4f);                          // camera shake, 0..1 (adds up)
Feel.HitStop(0.06f);                       // freeze for 60 ms
Feel.SlowMotion(0.3f, 0.5f);               // 30% speed for 0.5 s, then recover
Feel.Flash(enemy.gameObject);              // white flash
Feel.Squash(player.transform);             // landing squash
Feel.Stretch(player.transform);            // jump stretch
Feel.Text("120", hitPoint, Color.yellow);  // floating damage number
```

A typical melee hit: `Feel.HitStop(0.05f); Feel.Shake(0.25f); Feel.Flash(enemy); Feel.Text(...)`.

### 4. Tweens

```csharp
Tween.Move(door, door.position + Vector3.up * 3f, 1f).SetEase(Ease.InOutQuad);
Tween.Scale(coinIcon, Vector3.one * 1.3f, 0.15f).SetLoops(2, yoyo: true);
Tween.Rotate(chestLid, Quaternion.Euler(-110, 0, 0), 0.4f, local: true).SetEase(Ease.OutBack);
Tween.PunchScale(button, Vector3.one * 0.2f, 0.3f);
Tween.ShakePosition(lockedDoor, 0.1f, 0.3f);
Tween.Float(0f, 1f, 0.5f, v => canvasGroup.alpha = v).SetUnscaled();   // fades during pause
Tween.Delay(2f, () => SpawnWave());                                        // timer
Tween.Move(enemy, target, 1f).OnComplete(() => Attack());                  // chain
```

- Tweens stop by themselves when their object is destroyed. `Tween.Kill(transform)` stops them early.
- `.SetUnscaled()` keeps a tween running while `Time.timeScale` is 0 (pause menus, hit stop).
- A new punch or shake on the same object finishes the previous one first, so values never drift.

### 5. Feedback Player (no code)

1. Select the object that gets hit (an enemy, a crate, a button).
2. **Add Component > Game Feel > Feedback Player**.
3. **+ Efekt Ekle** (Add Effect): Camera Shake, Hit Stop, Slow Motion, Flash, Punch Scale, Punch
   Position, Object Shake, Particles, Sound, Floating Text, Event (UnityEvent).
4. Set the values and a **Delay** if an effect should come later. Reorder with ▲▼, disable with the
   checkbox.
5. Play it from code, or from any UnityEvent (e.g. a Button's **On Click**):

```csharp
[SerializeField] FeedbackPlayer hitFeedback;

void OnHit(Vector3 point) => hitFeedback.Play(point);   // effects happen at the hit point
```

In Play mode the Inspector has a **▶ Oynat** (Play) button to try the combination.

### 6. Accessibility

Some players get motion sick from shaking or are sensitive to flashing. Connect these to your
settings menu; every part of the package respects them:

```csharp
Feel.ShakeIntensity = 0.5f;      // 0 = off, 1 = normal
Feel.FlashEnabled = false;
Feel.TimeEffectsEnabled = false; // hit stop and slow motion
```

## Where to use it

| Game type | Moment | Juice |
|---|---|---|
| Action / melee | Hitting an enemy | `HitStop(0.05)` + `Shake(0.25)` + `Flash(enemy)` + damage number |
| | Critical hit / finishing blow | Longer hit stop (0.1), `SlowMotion(0.3, 0.4)`, bigger text |
| | Player takes damage | Red flash on the player, `Shake(0.4)` |
| Platformer | Jump / land | `Stretch` on take-off, `Squash` + small shake on landing |
| | Collecting a coin | `Tween.PunchScale` on the coin counter UI, "+1" floating text |
| Shooter | Firing | `PunchPosition` backwards on the weapon (recoil), tiny shake (0.05) |
| | Explosion | `Shake(0.8–1)`, slow motion for a moment |
| Racing / vehicles | Crash | `Shake` scaled by impact speed, `Squash` on the car body after a jump |
| Physics / party | Knockout, big fall | `HitStop(0.08)` + `Shake(0.6)` |
| Any game: UI | Buttons, popups, menus | `PunchScale` on click, `Tween.Scale` from 0 with `Ease.OutBack` for popups, `Tween.Move` for sliding panels (with `SetUnscaled()` in pause menus) |
| Any game: doors, chests, platforms | Opening, moving | `Tween.Rotate` / `Tween.Move` with `InOutQuad` instead of writing an Update loop |
| Any game: timers | "Do X in 2 seconds" | `Tween.Delay(2f, X)` instead of a coroutine |

## Good to know

- **Less is more.** Typical values: hit stop 0.03–0.08 s, shake 0.1–0.3 for normal hits and 0.6–1 for
  explosions, flash 0.08–0.15 s. Too much juice tires the player and hides what's happening.
- **Hit stop and your pause menu:** when a time effect starts, the current `Time.timeScale` is
  remembered and restored at the end. If your game changes `Time.timeScale` itself while an effect
  runs (opening the pause menu), the effect cancels itself and leaves your value alone.
- **Use unscaled time for things that must run during hit stop or pause:** UI animations (`SetUnscaled()`),
  Flash, floating texts and camera shake already use unscaled time.
- **Physics in slow motion:** `Time.fixedDeltaTime` is scaled with slow motion so physics stays smooth,
  and restored afterwards. If you change `fixedDeltaTime` yourself, do it outside slow motion.
- **Camera scripts:** the shake is added after every other `LateUpdate` and removed at the start of the
  next frame, so it works with follow cameras (including the ones in ActiveRagdoll and VehiclePhysics)
  and never moves your camera permanently. It should also work with Cinemachine for the same reason
  (not tested yet); Cinemachine also has its own Impulse system.
- **Only the camera tagged MainCamera is shaken** by `Feel.Shake`. For another camera, add a
  `CameraShaker` to it and call `shaker.AddTrauma(...)`.
- **Flash on white objects** is invisible (white → white). Use a red or bright color for those.
- **Floating text** uses Unity's built-in font so it works in any project. For a custom font or
  outline, replace it with TextMeshPro in your own game.

## Using it with your own models and objects

| Your object | Notes |
|---|---|
| Any mesh (FBX from Blender, Asset Store, Synty...) | Flash, punch and shake work. Flash colors every `MeshRenderer` and `SkinnedMeshRenderer` under the object |
| Materials with **URP Lit / Simple Lit / Unlit** or **Built-in Standard** | Flash works (`_BaseColor` / `_Color`) |
| **Custom shaders** (Shader Graph, toon shaders) | Flash needs a color property named `_BaseColor` or `_Color`. In Shader Graph, name the color property's *Reference* `_BaseColor` |
| **Animated characters** (Animator) | Punch/scale on the root fights the Animator if the animation also scales it. Put the punch on a parent object, or on a child that the animation doesn't touch |
| **Rigidbody objects** | Tween.Move on a non-kinematic Rigidbody fights physics. Use punch scale / flash on them, or make the Rigidbody kinematic while tweening |
| **UI (Canvas)** | Tweens work on RectTransforms (move, scale, punch). Flash and floating text are for 3D objects |
| **Particles** | Make a particle prefab with **Play On Awake** off and **Looping** off, put it in a Particles effect. A copy is spawned at the hit point each time and cleaned up |
| **Sounds** | The Sound effect plays a clip directly. If you use the AudioManager package, use an Event effect that calls `AudioManager.PlayAt` |

### With the other packages

```csharp
// VehiclePhysics: shake on hard crashes
void OnCollisionEnter(Collision c) { if (c.relativeVelocity.magnitude > 8f) Feel.Shake(0.5f); }

// ActiveRagdoll: hit stop when knocked out
ragdoll.Knocked += () => { Feel.HitStop(0.08f); Feel.Shake(0.6f); };

// SaveSystem: save the accessibility settings
SaveSystem.Preferences.Set("shake", Feel.ShakeIntensity);
```

## Troubleshooting

| Problem | Likely cause |
|---|---|
| No shake | No camera tagged **MainCamera**, `Feel.ShakeIntensity` is 0, or the camera's `CameraShaker` is disabled |
| The game stays frozen after hit stop | Something else set `Time.timeScale = 0` during it (then the effect leaves it alone on purpose) |
| Flash does nothing | The object is white already, the shader has no `_BaseColor`/`_Color`, or `Feel.FlashEnabled` is false |
| An object slowly grows / moves after many punches | It was animated by something else at the same time (Animator, your own script) |
| A tween doesn't run during pause | Add `.SetUnscaled()` |
| Particles appear pink | The particle material doesn't match your render pipeline: use a URP particle material in URP projects |

## How it works

- `TweenRunner` is an invisible `[Game Feel]` object (created on first use, survives scene loads) that
  updates all tweens and time effects every frame.
- `CameraShaker` keeps a *trauma* value (0..1) that decays over time. The shake strength is trauma²,
  so small hits stay subtle. Offsets come from Perlin noise, so the shake is smooth instead of jittery.
- `TimeEffects` remembers your time scale, applies the slowest active effect, and gives control back
  when everything is done or when your game changes the time scale.
- `HitFlash` uses `MaterialPropertyBlock`, so no materials are copied and other objects are not affected.

| File | Job |
|---|---|
| `Runtime/Feel.cs` | One-liners and accessibility switches |
| `Runtime/Tweening/Tween.cs`, `Ease.cs`, `TweenRunner.cs` | Tween engine |
| `Runtime/CameraShaker.cs`, `CameraShakeRestorer.cs` | Camera shake |
| `Runtime/TimeEffects.cs` | Hit stop, slow motion |
| `Runtime/HitFlash.cs` | Flash |
| `Runtime/FloatingText.cs`, `FloatingTextItem.cs` | Floating texts |
| `Runtime/Feedbacks/` | Feedback Player and its effects (add your own by deriving from `FeedbackItem`) |
| `Runtime/Demo/` | Demo panel and targets |
| `Editor/` | Feedback Player inspector, menus |
| `Tests/Runtime/GameFeelTests.cs` | 16 tests |

## Tests

16 PlayMode tests: eases, tween timing with delay and callbacks, yoyo loops, killing and destroyed
owners, unscaled tweens during pause, repeated punches without drift, object shake, camera shake
without drift (with and without a follow script), shake intensity 0, hit stop restore and extend,
hit stop vs. pause menu, slow motion with physics step, disabled time effects, flash during hit stop
and cleanup, floating text pooling, Feedback Player delays and switches.

```json
"testables": [ "com.atthree.game-feel" ]
```

Add this to `Packages/manifest.json`, then **Window > General > Test Runner > PlayMode**.

## License

[MIT](LICENSE.md)

---

<a name="türkçe"></a>
# Türkçe

"Juice", bir eylemi iyi hissettiren küçük geri bildirimlerdir: ekran sarsılır, vuruşta oyun saniyenin
birkaç yüzde biri kadar donar, düşman beyaz yanıp söner, sayılar uçuşur. Aynı mekanik, çok farklı his.

- **Tween'ler** (DOTween gerekmez): hareket, ölçek, rotasyon, punch, sarsıntı, sayılar, renkler,
  zamanlayıcılar, 12 yumuşatma eğrisi, tekrar/yoyo, gecikme, geri çağırma, ölçeksiz zaman.
- **Kamera sarsıntısı** (trauma tabanlı, Perlin gürültüsü). Her kamera script'inin üstünde kaymadan çalışır.
- Duraklatma menünle güvenli **hit stop** (kısa donma) ve **ağır çekim**.
- Materyal kopyalamadan **hit flash** (URP ve Built-in).
- Hasar sayıları için **uçuşan yazı**: havuzlu, TextMeshPro kurulumu gerekmez.
- **Feedback Player**: "sarsıntı + hit stop + flash + punch + parçacık + ses"i Inspector'da kur, tek
  çağrıyla oynat.
- Sarsıntı, flash ve zaman efektleri için **erişilebilirlik ayarları**.

**Gereksinimler:** Unity 6 (6000.0+). Başka paket gerekmez.

## Adım adım kurulum

### 1. Paketi ekle

**Window > Package Manager** → **+** → **Install package from git URL…** →

```
https://github.com/Atthree/GameFeel.git
```

### 2. Demoyu dene

1. **Tools > Game Feel > Create Demo Scene**, sonra **Play**.
2. Üç hedefe tıkla. Her vuruşta: kamera sarsıntısı, hit stop, beyaz flash, basılma, kıvılcım, ses ve
   bir hasar sayısı (%20'si kritik).
3. Paneldeki efektleri tek tek kapat (ya da **JUICE** ile hepsini birden) ve tekrar tıkla: aynı tıklama,
   çok daha zayıf his. Juice'un yaptığı iş bu.
4. **Zıpla** tween zinciriyle yapılmış basıl-uza'yı (squash & stretch), **Büyük patlama** tam sarsıntı +
   ağır çekimi gösterir.
5. **Sarsıntı şiddeti** kaydırıcısı bir erişilebilirlik ayarı örneğidir.

### 3. Koddan tek satır

```csharp
using Atthree.GameFeel;

Feel.Shake(0.4f);                          // kamera sarsıntısı, 0..1 (üst üste eklenir)
Feel.HitStop(0.06f);                       // 60 ms donma
Feel.SlowMotion(0.3f, 0.5f);               // 0.5 sn %30 hız, sonra toparlanma
Feel.Flash(enemy.gameObject);              // beyaz flash
Feel.Squash(player.transform);             // yere iniş basılması
Feel.Stretch(player.transform);            // zıplama uzaması
Feel.Text("120", hitPoint, Color.yellow);  // uçuşan hasar sayısı
```

Tipik bir yakın dövüş vuruşu: `Feel.HitStop(0.05f); Feel.Shake(0.25f); Feel.Flash(enemy); Feel.Text(...)`.

### 4. Tween'ler

```csharp
Tween.Move(door, door.position + Vector3.up * 3f, 1f).SetEase(Ease.InOutQuad);
Tween.Scale(coinIcon, Vector3.one * 1.3f, 0.15f).SetLoops(2, yoyo: true);
Tween.Rotate(chestLid, Quaternion.Euler(-110, 0, 0), 0.4f, local: true).SetEase(Ease.OutBack);
Tween.PunchScale(button, Vector3.one * 0.2f, 0.3f);
Tween.ShakePosition(lockedDoor, 0.1f, 0.3f);
Tween.Float(0f, 1f, 0.5f, v => canvasGroup.alpha = v).SetUnscaled();   // duraklatmada da çalışır
Tween.Delay(2f, () => SpawnWave());                                        // zamanlayıcı
Tween.Move(enemy, target, 1f).OnComplete(() => Attack());                  // zincir
```

- Tween'ler objeleri yok olunca kendiliğinden durur. `Tween.Kill(transform)` erken durdurur.
- `.SetUnscaled()`, `Time.timeScale` 0 iken de tween'i çalıştırır (duraklatma menüsü, hit stop).
- Aynı objede yeni bir punch ya da sarsıntı öncekini bitirerek başlar; değerler asla kaymaz.

### 5. Feedback Player (kodsuz)

1. Vurulan objeyi seç (düşman, kutu, buton).
2. **Add Component > Game Feel > Feedback Player**.
3. **+ Efekt Ekle**: Kamera Sarsıntısı, Hit Stop, Ağır Çekim, Flash, Punch Scale, Punch Position, Obje
   Sarsıntısı, Parçacık, Ses, Uçuşan Yazı, Olay (UnityEvent).
4. Değerleri ve bir efekt sonra gelecekse **Delay**'i ayarla. ▲▼ ile sırala, onay kutusuyla kapat.
5. Koddan ya da herhangi bir UnityEvent'ten (ör. Button'ın **On Click**'i) oynat:

```csharp
[SerializeField] FeedbackPlayer hitFeedback;

void OnHit(Vector3 point) => hitFeedback.Play(point);   // efektler vuruş noktasında olur
```

Play modunda Inspector'da kombinasyonu denemek için **▶ Oynat** düğmesi var.

### 6. Erişilebilirlik

Bazı oyuncular sarsıntıdan midesi bulanır ya da yanıp sönmeye duyarlıdır. Bunları ayarlar menüne
bağla; paketin her parçası bu değerlere uyar:

```csharp
Feel.ShakeIntensity = 0.5f;      // 0 = kapalı, 1 = normal
Feel.FlashEnabled = false;
Feel.TimeEffectsEnabled = false; // hit stop ve ağır çekim
```

## Nerede kullanılır?

| Oyun türü | An | Juice |
|---|---|---|
| Aksiyon / yakın dövüş | Düşmana vurmak | `HitStop(0.05)` + `Shake(0.25)` + `Flash(enemy)` + hasar sayısı |
| | Kritik vuruş / son darbe | Daha uzun hit stop (0.1), `SlowMotion(0.3, 0.4)`, büyük yazı |
| | Oyuncunun hasar alması | Oyuncuda kırmızı flash, `Shake(0.4)` |
| Platform | Zıplama / yere inme | Kalkışta `Stretch`, inişte `Squash` + hafif sarsıntı |
| | Altın toplama | Altın sayacı UI'ında `Tween.PunchScale`, "+1" uçuşan yazı |
| Nişancı | Ateş etme | Silahta geriye `PunchPosition` (tepme), çok hafif sarsıntı (0.05) |
| | Patlama | `Shake(0.8–1)`, bir anlık ağır çekim |
| Yarış / araç | Kaza | Darbe hızına göre `Shake`, zıplamadan sonra gövdede `Squash` |
| Fizik / party | Bayılma, büyük düşüş | `HitStop(0.08)` + `Shake(0.6)` |
| Her oyun: UI | Butonlar, açılır pencereler, menüler | Tıklamada `PunchScale`, açılır pencerede 0'dan `Ease.OutBack` ile `Tween.Scale`, kayan paneller için `Tween.Move` (pause menüsünde `SetUnscaled()` ile) |
| Her oyun: kapı, sandık, platform | Açılma, hareket | Update döngüsü yazmak yerine `InOutQuad` ile `Tween.Rotate` / `Tween.Move` |
| Her oyun: zamanlayıcılar | "2 saniye sonra X'i yap" | Coroutine yerine `Tween.Delay(2f, X)` |

## Dikkat edilecekler

- **Az, çoktur.** Tipik değerler: hit stop 0.03–0.08 sn, sarsıntı normal vuruşta 0.1–0.3, patlamada
  0.6–1, flash 0.08–0.15 sn. Fazla juice oyuncuyu yorar ve olanı gizler.
- **Hit stop ve duraklatma menün:** zaman efekti başlarken o anki `Time.timeScale` hatırlanır, bitince
  geri konur. Efekt sürerken oyunun kendisi `Time.timeScale`'i değiştirirse (pause menüsü açılırsa)
  efekt kendini iptal eder ve senin değerine dokunmaz.
- **Hit stop veya duraklatmada çalışması gerekenler için ölçeksiz zaman kullan:** UI animasyonları
  (`SetUnscaled()`). Flash, uçuşan yazılar ve kamera sarsıntısı zaten ölçeksiz zamanla çalışır.
- **Ağır çekimde fizik:** `Time.fixedDeltaTime` ağır çekimle birlikte ölçeklenir (fizik akıcı kalsın diye)
  ve sonra geri konur. `fixedDeltaTime`'ı kendin değiştiriyorsan ağır çekim dışında yap.
- **Kamera script'leri:** sarsıntı diğer tüm `LateUpdate`'lerden sonra eklenir, bir sonraki karenin
  başında geri alınır. Böylece takip kameralarıyla (ActiveRagdoll ve VehiclePhysics'teki dahil) çalışır,
  kamerayı asla kalıcı olarak kaydırmaz. Aynı sebeple Cinemachine ile de çalışması gerekir (henüz test
  edilmedi); Cinemachine'in kendi Impulse sistemi de var.
- `Feel.Shake` **sadece MainCamera etiketli kamerayı** sarsar. Başka bir kamera için ona `CameraShaker`
  ekle ve `shaker.AddTrauma(...)` çağır.
- **Beyaz objelerde flash** görünmez (beyaz → beyaz). Onlar için kırmızı ya da parlak bir renk kullan.
- **Uçuşan yazı** her projede çalışsın diye Unity'nin dahili fontunu kullanır. Özel font ya da kenar
  çizgisi istersen kendi oyununda TextMeshPro ile değiştir.

## Kendi modellerin ve objelerinle

| Objen | Notlar |
|---|---|
| Her mesh (Blender'dan FBX, Asset Store, Synty...) | Flash, punch ve sarsıntı çalışır. Flash objenin altındaki her `MeshRenderer` ve `SkinnedMeshRenderer`'ı boyar |
| **URP Lit / Simple Lit / Unlit** ya da **Built-in Standard** materyaller | Flash çalışır (`_BaseColor` / `_Color`) |
| **Özel shader'lar** (Shader Graph, toon shader) | Flash için `_BaseColor` ya da `_Color` adında bir renk özelliği gerekir. Shader Graph'te renk özelliğinin *Reference*'ını `_BaseColor` yap |
| **Animasyonlu karakterler** (Animator) | Animasyon da ölçeği değiştiriyorsa kökteki punch/ölçek Animator'la çakışır. Punch'ı bir üst objeye ya da animasyonun dokunmadığı bir alt objeye uygula |
| **Rigidbody'li objeler** | Kinematik olmayan Rigidbody'de Tween.Move fizikle çakışır. Onlarda punch scale / flash kullan ya da tween sırasında Rigidbody'yi kinematik yap |
| **UI (Canvas)** | Tween'ler RectTransform'larda çalışır (hareket, ölçek, punch). Flash ve uçuşan yazı 3D objeler içindir |
| **Parçacıklar** | **Play On Awake** ve **Looping** kapalı bir parçacık prefab'ı yap, Parçacık efektine koy. Her seferinde vuruş noktasında bir kopyası yaratılır ve temizlenir |
| **Sesler** | Ses efekti klibi doğrudan çalar. AudioManager paketini kullanıyorsan `AudioManager.PlayAt`'i çağıran bir Olay efekti kullan |

### Diğer paketlerle

```csharp
// VehiclePhysics: sert kazada sarsıntı
void OnCollisionEnter(Collision c) { if (c.relativeVelocity.magnitude > 8f) Feel.Shake(0.5f); }

// ActiveRagdoll: bayılınca hit stop
ragdoll.Knocked += () => { Feel.HitStop(0.08f); Feel.Shake(0.6f); };

// SaveSystem: erişilebilirlik ayarlarını kaydet
SaveSystem.Preferences.Set("shake", Feel.ShakeIntensity);
```

## Sorun giderme

| Sorun | Muhtemel sebep |
|---|---|
| Sarsıntı yok | **MainCamera** etiketli kamera yok, `Feel.ShakeIntensity` 0, ya da kameradaki `CameraShaker` kapalı |
| Hit stop'tan sonra oyun donuk kalıyor | Hit stop sırasında başka bir şey `Time.timeScale = 0` yaptı (o durumda efekt bilerek dokunmaz) |
| Flash bir şey yapmıyor | Obje zaten beyaz, shader'da `_BaseColor`/`_Color` yok, ya da `Feel.FlashEnabled` false |
| Çok punch'tan sonra obje yavaşça büyüyor / kayıyor | Aynı anda başka bir şey (Animator, kendi script'in) de onu oynatıyordu |
| Tween duraklatmada çalışmıyor | `.SetUnscaled()` ekle |
| Parçacıklar pembe görünüyor | Parçacık materyali render pipeline'ına uymuyor: URP projelerinde URP parçacık materyali kullan |

## Nasıl çalışıyor?

- `TweenRunner`, görünmez bir `[Game Feel]` objesidir (ilk kullanımda oluşur, sahne değişince yok
  olmaz); tüm tween'leri ve zaman efektlerini her kare ilerletir.
- `CameraShaker` zamanla azalan bir *trauma* değeri (0..1) tutar. Sarsıntı gücü trauma²'dir; küçük
  vuruşlar hafif kalır. Kaymalar Perlin gürültüsünden gelir; sarsıntı titrek değil akıcıdır.
- `TimeEffects` senin zaman ölçeğini hatırlar, aktif efektlerin en yavaşını uygular ve her şey bitince
  ya da oyunun zaman ölçeğini değiştirdiğinde kontrolü geri verir.
- `HitFlash`, `MaterialPropertyBlock` kullanır: materyal kopyalanmaz, diğer objeler etkilenmez.

Dosya listesi ve testler için İngilizce bölümdeki [How it works](#how-it-works) ve [Tests](#tests)
kısımlarına bak.

## Lisans

[MIT](LICENSE.md)
