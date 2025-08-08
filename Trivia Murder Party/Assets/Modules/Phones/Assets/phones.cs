using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class phones : MonoBehaviour {
   public new KMAudio audio;
   private KMAudio.KMAudioRef audioRef;
   public KMBombInfo bomb;
   public KMBombModule module;

   public KMSelectable[] digits;
   public KMSelectable centerButton;
   public Transform dial;
   public TextMesh screenText;

   private List<Player> playersPresent = new List<Player>();
   private int[] generatedNumbers = new int[7];
   private bool[] selected = new bool[7];
   private string solution;
   private string input;

   private int startingTime;
   private bool cantPress;
   private bool sequenceStarted;
   private bool easterEggSeen;
   private bool easterEggPlaying;
   private static int moduleIdCounter = 1;
   private int moduleId;
   private bool moduleSolved;

   private void Awake () {
      moduleId = moduleIdCounter++;
      foreach (KMSelectable digit in digits)
         digit.OnInteract += delegate () { PressDigit(digit); return false; };
      centerButton.OnInteract += delegate () { PressCenter(); return false; };
   }

   private void Start () {
      screenText.text = "";
      startingTime = (int) bomb.GetTime();
      SetUpPlayers();
      Debug.LogFormat("[Phones #{0}] Serial number: {1}", moduleId, bomb.GetSerialNumber());
      Debug.LogFormat("[Phones #{0}] Players present: {1}", moduleId, playersPresent.Select(x => x.name).Join(", "));
      Debug.LogFormat("[Phones #{0}] Initial edgework numbers: {1}", moduleId, playersPresent.Select(x => x.baseNumber).Join(", "));
      Debug.LogFormat("[Phones #{0}] Calculated numbers: {1}", moduleId, playersPresent.Select(x => x.factor).Join(", "));
      Debug.LogFormat("[Phones #{0}] Relevant modules present: {1}", moduleId, playersPresent.Select(x => bomb.GetModuleNames().Contains(x.module) ? "yes" : "no").Join(", "));

      var playerChoices = new List<int>();
      tryAgain:
      for (int i = 0; i < 7; i++) {
         generatedNumbers[i] = 0;
         selected[i] = false;
      }
      playerChoices.Clear();
      generatedNumbers = new int[7].Select(x => x = rnd.Range(24, 1000)).ToArray();
      if (generatedNumbers.Distinct().Count() != 7)
         goto tryAgain;

      for (int i = 0; i < 6; i++) {
         var currentPlayer = playersPresent[i];
         if (generatedNumbers.Where((x, ix) => x % currentPlayer.factor == 0 && !selected[ix]).Count() == 0)
            goto tryAgain;

         var validNumbers = generatedNumbers.Where((x, ix) => x % currentPlayer.factor == 0 && !selected[ix]);
         var selectedNumber = bomb.GetModuleNames().Contains(currentPlayer.module) ? validNumbers.Last() : validNumbers.First();
         selected[Array.IndexOf(generatedNumbers, selectedNumber)] = true;
         playerChoices.Add(selectedNumber);
      }
      solution = generatedNumbers[Array.IndexOf(selected, false)].ToString("000");
      if (solution == "666")
         goto tryAgain;

      Debug.LogFormat("[Phones #{0}] Numbers present in the cycle: {1}", moduleId, generatedNumbers.Select(x => x.ToString("000")).Join(", "));
      for (int i = 0; i < 6; i++)
         Debug.LogFormat("[Phones #{0}] {1} chose {2}.", moduleId, playersPresent[i].name, playerChoices[i].ToString("000"));
      Debug.LogFormat("[Phones #{0}] The only unchosen number left is {1}.", moduleId, solution);
   }

   private void PressDigit (KMSelectable digit) {
      if (moduleSolved || cantPress || easterEggPlaying)
         return;
      var ix = Array.IndexOf(digits, digit);
      input += "1234567890"[ix];
      StartCoroutine(RotateDial(ix));
   }

   private IEnumerator RotateDial (int ix) {
      cantPress = true;
      var count = 300 * (ix + 1);
      var target = 180f + (.1f * count);
      var elapsed = 0f;
      var duration = 0f;
      if (ix == 0 || ix == 1 || ix == 2)
         duration = .5f;
      else if (ix == 3 || ix == 4 || ix == 5)
         duration = .75f;
      else if (ix == 6 || ix == 7 || ix == 8)
         duration = 1f;
      else
         duration = 1.25f;
      audio.PlaySoundAtTransform("dial", transform);
      while (elapsed < duration) {
         dial.localEulerAngles = new Vector3(0f, Mathf.Lerp(180f, target, elapsed / duration), 0f);
         yield return null;
         elapsed += Time.deltaTime;
      }
      dial.localEulerAngles = new Vector3(0f, target, 0f);
      yield return new WaitForSeconds(.25f);
      elapsed = 0f;
      audioRef = audio.PlaySoundAtTransformWithRef("rewind", transform);
      while (elapsed < duration) {
         dial.localEulerAngles = new Vector3(0f, Mathf.Lerp(target, 180f, elapsed / duration), 0f);
         yield return null;
         elapsed += Time.deltaTime;
      }
      audioRef.StopSound();
      audioRef = null;
      dial.localEulerAngles = new Vector3(0f, 180f, 0f);
      cantPress = false;

      if (input.Length == 3)
         CheckAnswer();
   }

   private void CheckAnswer () {
      Debug.LogFormat("[Phones #{0}] You have dialed {1}.", moduleId, input);
      if (input == solution) {
         Debug.LogFormat("[Phones #{0}] That was correct. Module solved!", moduleId);
         module.HandlePass();
         audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
         moduleSolved = true;
      }
      else if (!easterEggSeen && input == "666" && DateTime.Now.Hour == 3)
         StartCoroutine(EasterEgg());
      else {
         Debug.LogFormat("[Phones #{0}] That was incorrect. Strike!", moduleId);
         module.HandleStrike();
         input = string.Empty;
         StartCoroutine(CycleNumbers());
      }
   }

   private void PressCenter () {
      centerButton.AddInteractionPunch(.4f);
      audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, centerButton.transform);
      if (moduleSolved || sequenceStarted)
         return;
      sequenceStarted = true;
      StartCoroutine(CycleNumbers());
   }

   private IEnumerator CycleNumbers () {
      var startingColor = screenText.color;
      cantPress = true;
      foreach (string number in generatedNumbers.Select(x => x.ToString("000"))) {
         screenText.text = number;
         screenText.color = startingColor;
         var elapsed = 0f;
         var duration = .75f;
         while (elapsed < duration) {
            screenText.color = Color.Lerp(startingColor, Color.clear, elapsed / duration);
            yield return null;
            elapsed += Time.deltaTime;
         }
         screenText.color = Color.clear;
         yield return new WaitForSeconds(.5f);
      }
      cantPress = false;
      screenText.text = string.Empty;
      screenText.color = startingColor;
   }

   private class Player {
      public char snChar { get; set; }
      public int position { get; set; }
      public int baseNumber { get; set; }
      public int factor { get; set; }
      public string name { get; set; }
      public string module { get; set; }

      public Player (char s, int p, int b, int f, string n, string m) {
         snChar = s;
         position = p;
         baseNumber = b;
         factor = f;
         name = n;
         module = m;
      }
   }

   private void SetUpPlayers () {
      var batteries = bomb.GetBatteryCount();
      var holders = bomb.GetBatteryHolderCount();
      var aa = bomb.GetBatteryCount(Battery.AA);
      var d = bomb.GetBatteryCount(Battery.D);
      var indicators = bomb.GetIndicators().Count();
      var lits = bomb.GetOnIndicators().Count();
      var unlits = bomb.GetOffIndicators().Count();
      var ports = bomb.GetPortCount();
      var plates = bomb.GetPortPlateCount();
      var modules = bomb.GetSolvableModuleNames().Count();
      var time = startingTime / 60;
      var month = DateTime.Now.Month;

      var standardCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
      var standardNames = new string[] { "Arthur", "Brooke", "Chevon", "Dante", "Ethelgard", "Florence", "Gregory", "Hester", "Isala", "Javier", "Kevin", "Lexi", "Meghan", "Niamh", "Oliver", "Patrick", "Quentin", "Riley", "Sabrina", "Tambry", "Ulysses", "Via", "Wynter", "Xavier", "Yaretzi", "Zander", "Wanda", "Malachy", "Trey", "Tobias", "Quinn", "Sia", "Seven", "Octavius", "Nina", "Dex" };
      var standardBaseNumbers = new int[] { ports, aa, unlits, holders, lits, plates, d, indicators, batteries, month, modules, time, month, unlits, aa, batteries, holders, time, lits, modules, ports, plates, d, indicators, batteries, holders, aa, d, indicators, lits, unlits, ports, plates, time, modules, month };
      var standardModules = new string[] { "Big Circle", "Gridlock", "Black Hole", "Yellow Arrows", "Radiator", "Pathfinder", "Triamonds", "Regular Hexpressions", "Simon Shrieks", "hexOrbits", "Etterna", "1D Maze", "Scavenger Hunt", "Chinese Counting", "Binary Puzzle", "Cooking", "Laundry", "Forget This", "Bamboozling Button", "Brush Strokes", "Tennis", "The Assorted Arrangement", "Simon Screams", "Morsematics", "Bone Apple Tea", "Crazy Talk", "Round Keypad", "Widdershins", "Yahtzee", "The Cube", "Snooker", "IKEA Plushies", "Indigo Cipher", "Hold Ups", "Navinums", "Name Codes" };

      var tmpModules = new string[] { "Phones", "Mirror", "Scratch-Off", "Words", "Skewers", "Rules", "Pixel Art", "Chalices", "Dumb Waiters", "Jailbreak", "Dictation", "The Arena", "Mental Math", "Mind Meld", "Patterns" };
      var repeatNames = new string[] { "Kanye", "Gorg", "Candy", "Steyganfries", "Lord Honkingshire III" };
      var repeatBaseNumbers = new int[]
     {
        bomb.GetModuleNames().Count(x => tmpModules.Contains(x)),
        DateTime.Now.Hour,
        bomb.GetModuleNames().Count(i => "0123456789".Any(j => i.Contains(j))),
        bomb.GetTwoFactorCounts(),
        bomb.GetIndicators().Count(x => x == "NLL")
     };
      var repeatModules = new string[] { "Negativity", "Spangled Stars", "Indentation", "The Stopwatch", "Duck, Duck, Goose" };
      var repeatCount = 0;

      var sn = bomb.GetSerialNumber();
      for (int i = 0; i < 6; i++) {
         var currentChar = sn[i];
         var index = standardCharacters.IndexOf(currentChar);
         if (playersPresent.Any(x => x.snChar == currentChar)) {
            var calculated = repeatBaseNumbers[repeatCount] * (i + 11);
            calculated %= 100;
            if (calculated == 0)
               calculated = 1;
            playersPresent.Add(new Player(currentChar, i, repeatBaseNumbers[repeatCount], calculated, repeatNames[repeatCount], repeatModules[repeatCount]));
            repeatCount++;
         }
         else {
            var calculated = standardBaseNumbers[index] * (i + 11);
            calculated %= 100;
            if (calculated == 0)
               calculated = 1;
            playersPresent.Add(new Player(currentChar, i, standardBaseNumbers[index], calculated, standardNames[index], standardModules[index]));
         }
      }
   }

   private IEnumerator EasterEgg () {
      easterEggSeen = true;
      easterEggPlaying = true;
      Debug.LogFormat("[Phones #{0}] That was a mistake. Someone might answer.", moduleId);
      audio.PlaySoundAtTransform("easter egg", transform);
      yield return new WaitForSeconds(46.5f);
      easterEggPlaying = false;
      input = string.Empty;
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = "!{0} start [Presses the button in the center.] !{0} <123> [Dials that 3 digit number.]";
#pragma warning restore 414

   private IEnumerator ProcessTwitchCommand (string command) {
      var numbers = "1234567890";
      command = command.ToLowerInvariant().Trim();
      if (command == "start") {
         if (sequenceStarted) {
            yield return "sendtochaterror The red button has already been pressed.";
            yield break;
         }
         yield return null;
         centerButton.OnInteract();
      }
      else if (!command.Any(x => numbers.Contains(x)) || command.Length != 3) {
         yield return "sendtochaterror You have not sent a valid 3 digit number.";
      }
      else {
         for (int i = 0; i < 3; i++) {
            if (command[i] == '0') {
               digits[9].OnInteract();
            }
            else {
               digits[int.Parse(command[i].ToString()) - 1].OnInteract();
            }
            while (cantPress) {
               yield return true;
            }
         }
      }
   }

   private IEnumerator TwitchHandleForcedSolve () {
      yield return ProcessTwitchCommand(solution);
   }
}
