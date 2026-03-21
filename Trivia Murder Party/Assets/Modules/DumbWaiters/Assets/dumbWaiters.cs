using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class dumbWaiters : MonoBehaviour {
   public new KMAudio audio;
   public KMBombInfo bomb;
   public KMBombModule module;

   public KMSelectable[] buttons;
   public Renderer[] bases;
   public Color[] baseColors;
   public TextMesh[] moduleNames;
   public TextMesh[] leftNames;
   public TextMesh[] rightNames;
   public Color[] resultColors;

   private int[] presentPlayers = new int[7];
   private int[] orderedPlayers = new int[7];
   private List<int> chosen = new List<int>();
   private bool[] choseLeft = new bool[7];
   private bool leftGreen;
   private bool rightRed;
   private int leftCount;
   private int rightCount;
   private int solution;

   private static readonly string[] names = new string[24] { "TasThiluna", "Deaf", "Blananas2", "Fish", "Usernam3", "EpicToast", "Makebao", "KavinKul", "Crazycaleb", "tandyCake", "Fang", "Vinco", "Arceus", "Xmaster", "FredV", "Kaito", "SillyPuppy", "Edan", "Mythers", "Procyon", "eXish", "RedPenguin", "MCD573", "Mr. Peanut" };
   private static readonly int[] parenthesesNumbers = new int[24] { 17, 24, 6, 9, 15, 7, 22, 19, 8, 11, 3, 21, 18, 20, 16, 14, 4, 13, 2, 10, 12, 1, 5, 23 };
   private static readonly int[] startsWithVowel = new int[5] { 4, 5, 12, 17, 20 };
   private static readonly int[] containsNumber = new int[2] { 4, 22 };
   private bool animating = true;

   private static int moduleIdCounter = 1;
   private int moduleId;
   private bool moduleSolved;

   void Awake () {
      moduleId = moduleIdCounter++;
      leftGreen = rnd.Range(0, 2) == 0;
      rightRed = rnd.Range(0, 2) == 0;
      bases[0].material.color = leftGreen ? baseColors[0] : baseColors[1];
      bases[1].material.color = rightRed ? baseColors[2] : baseColors[3];
      foreach (KMSelectable button in buttons)
         button.OnInteract += delegate () { PressButton(button); return false; };
      module.OnActivate += delegate () { StartCoroutine(ShowNames()); };
      foreach (TextMesh t in moduleNames)
         t.text = "";
      for (int i = 0; i < 8; i++) {
         leftNames[i].text = "";
         rightNames[i].text = "";
      }
   }

   void Start () {
      for (int i = 0; i < 8; i++) {
         leftNames[i].color = Color.white;
         rightNames[i].color = Color.white;
      }
      presentPlayers = Enumerable.Range(0, 24).ToList().Shuffle().Take(7).ToArray();
      var tempPlayers = presentPlayers.ToArray();
      Array.Sort(tempPlayers);
      orderedPlayers = leftGreen ? tempPlayers.ToArray() : presentPlayers.OrderBy(x => parenthesesNumbers[Array.IndexOf(Enumerable.Range(0, 24).ToArray(), x)]).ToArray();
      if (!rightRed)
         orderedPlayers = orderedPlayers.Reverse().ToArray();
      MakeChoices();
      leftCount = choseLeft.Count(b => b);
      rightCount = choseLeft.Count(b => !b);
      if (leftCount == 7 || rightCount == 7)
         solution = leftCount == 7 ? 0 : 1;
      else
         solution = leftCount > rightCount ? 1 : 0;
      Debug.LogFormat("[Dumb Waiters #{0}] The players present, other than yourself, are {1}, and {2}.", moduleId, presentPlayers.Take(6).Select(x => names[x]).Join(", "), names[presentPlayers[6]]);
      Debug.LogFormat("[Dumb Waiters #{0}] The left elevator is {1}, so order players by their {2}. The right elevator is {3}, so {4}.", moduleId, leftGreen ? "green" : "blue", leftGreen ? "position in the table" : "number in parentheses", rightRed ? "red" : "yellow", rightRed ? "leave this order as is" : "reverse this order");
      for (int i = 0; i < 7; i++)
         Debug.LogFormat("[Dumb Waiters #{0}] {1} chose the {2} elevator.", moduleId, names[orderedPlayers[i]], choseLeft[i] ? "left" : "right");
      Debug.LogFormat("[Dumb Waiters #{0}] The correct elevator to pick is the {1} elevator.", moduleId, solution == 0 ? "left" : "right");
   }

   void PressButton (KMSelectable button) {
      button.AddInteractionPunch(.5f);
      if (animating || moduleSolved)
         return;
      var ix = Array.IndexOf(buttons, button);
      audio.PlaySoundAtTransform("sting", transform);
      Debug.LogFormat("[Dumb Waiters #{0}] You chose the {1} elevator.", moduleId, ix == 0 ? "left" : "right");
      StartCoroutine(SubmitAnimation(ix));
   }

   IEnumerator SubmitAnimation (int ix) {
      animating = true;
      var leftNamesCount = 0;
      var rightNamesCount = 0;
      yield return new WaitForSeconds(.75f);
      for (int i = 0; i < 7; i++) {
         moduleNames.First(t => t.text == names[orderedPlayers[i]]).text = "";
         if (choseLeft[i]) {
            leftNames[leftNamesCount].text = names[orderedPlayers[i]];
            leftNamesCount++;
         }
         else {
            rightNames[rightNamesCount].text = names[orderedPlayers[i]];
            rightNamesCount++;
         }
         audio.PlaySoundAtTransform("swoosh", transform);
         yield return new WaitForSeconds(.5f);
      }
      if (ix == 0)
         leftNames[leftNamesCount].text = "You";
      else
         rightNames[rightNamesCount].text = "You";
      audio.PlaySoundAtTransform("swoosh", transform);
      yield return new WaitForSeconds(.75f);
      if (ix != solution) {
         foreach (TextMesh t in (ix == 0) ? leftNames : rightNames)
            t.color = resultColors[0];
         foreach (TextMesh t in (ix == 0) ? rightNames : leftNames)
            t.color = resultColors[1];
      }
      else {
         if ((leftCount == 7 || rightCount == 7) || ((leftCount == 3 && rightCount == 4) || (leftCount == 4 && rightCount == 3))) {
            foreach (TextMesh t in leftNames)
               t.color = resultColors[1];
            foreach (TextMesh t in rightNames)
               t.color = resultColors[1];
         }
         else {
            if (leftCount > rightCount) {
               foreach (TextMesh t in leftNames)
                  t.color = resultColors[0];
               foreach (TextMesh t in rightNames)
                  t.color = resultColors[1];
            }
            else {
               foreach (TextMesh t in rightNames)
                  t.color = resultColors[0];
               foreach (TextMesh t in leftNames)
                  t.color = resultColors[1];
            }
         }
      }
      yield return new WaitForSeconds(.5f);
      if (ix != solution) {
         Debug.LogFormat("[Dumb Waiters #{0}] Unfortunately, you have died. Strike! Resetting...", moduleId);
         chosen.Clear();
         module.HandleStrike();
         StartCoroutine(HideElevatorNames(true));
      }
      else {
         Debug.LogFormat("[Dumb Waiters #{0}] You survived! Module solved!", moduleId);
         module.HandlePass();
         audio.PlaySoundAtTransform("solve", transform);
         moduleSolved = true;
         StartCoroutine(HideElevatorNames(false));
      }
   }

   IEnumerator ShowNames () {
      for (int i = 0; i < 7; i++) {
         moduleNames[i].text = names[presentPlayers[i]];
         audio.PlaySoundAtTransform("tap", transform);
         yield return new WaitForSeconds(.25f);
      }
      animating = false;
   }

   IEnumerator HideElevatorNames (bool becauseStrike) {
      var count = leftCount > rightCount ? leftCount : rightCount;
      if ((becauseStrike && count != 7) || (count == 7 && !becauseStrike)) // this is the easiest way to do an XOR gate in C#
         count++;
      for (int i = 0; i < count; i++) {
         leftNames[i].text = "";
         rightNames[i].text = "";
         audio.PlaySoundAtTransform("beep", transform);
         yield return new WaitForSeconds(.25f);
      }
      if (becauseStrike) {
         yield return new WaitForSeconds(1f);
         Start();
         yield return new WaitForSeconds(.1f);
         StartCoroutine(ShowNames());
      }
   }

   void MakeChoices () {
      for (int i = 0; i < 7; i++) {
         var left = false;
         switch (names[orderedPlayers[i]]) {
            case "TasThiluna":
               left = chosen.Count() % 2 == 0;
               break;
            case "Deaf":
               left = presentPlayers.Any(x => startsWithVowel.Contains(x) && !chosen.Contains(x));
               break;
            case "Blananas2":
               left = presentPlayers.Any(x => containsNumber.Contains(x) && !chosen.Contains(x));
               break;
            case "Fish":
               left = presentPlayers.Any(x => x / 6 == 1 && chosen.Contains(x));
               break;
            case "Usernam3":
               left = presentPlayers.Any(x => x / 6 == 3 && !chosen.Contains(x));
               break;
            case "EpicToast":
               left = presentPlayers.Any(x => x % 6 == 0 && chosen.Contains(x));
               break;
            case "Makebao":
               left = !presentPlayers.Any(x => x / 6 == 2 && !chosen.Contains(x));
               break;
            case "KavinKul":
               left = presentPlayers.Any(x => x / 6 == 3 && chosen.Contains(x));
               break;
            case "Crazycaleb":
               left = bomb.GetModuleNames().Any(x => x == "The Jewel Vault");
               break;
            case "tandyCake":
               left = presentPlayers.Any(x => x / 6 == 0 && !chosen.Contains(x));
               break;
            case "Fang":
               left = presentPlayers.Any(x => x % 6 == 5 && chosen.Contains(x));
               break;
            case "Vinco":
               left = !presentPlayers.Any(x => x / 6 == 0 && !chosen.Contains(x));
               break;
            case "Arceus":
               left = presentPlayers.Any(x => x % 6 == 2 && chosen.Contains(x));
               break;
            case "Xmaster":
               left = presentPlayers.Any(x => x / 6 == 2 && chosen.Contains(x));
               break;
            case "FredV":
               left = presentPlayers.Any(x => x / 6 == 1 && !chosen.Contains(x));
               break;
            case "Kaito":
               left = presentPlayers.Any(x => x % 6 == 1 && chosen.Contains(x));
               break;
            case "SillyPuppy":
               left = !presentPlayers.Any(x => x / 6 == 3 && !chosen.Contains(x));
               break;
            case "Edan":
               left = chosen.Count() % 2 == 1;
               break;
            case "Mythers":
               left = presentPlayers.Any(x => x % 6 == 3 && chosen.Contains(x));
               break;
            case "Procyon":
               left = presentPlayers.Any(x => x / 6 == 0 && chosen.Contains(x));
               break;
            case "eXish":
               left = presentPlayers.Any(x => x % 6 == 4 && chosen.Contains(x));
               break;
            case "RedPenguin":
               left = !presentPlayers.Any(x => x / 6 == 1 && !chosen.Contains(x));
               break;
            case "MCD573":
               left = presentPlayers.Any(x => new int[] { 0, 15 }.Contains(x) && chosen.Contains(x));
               break;
            case "Mr. Peanut":
               left = presentPlayers.Any(x => x / 6 == 2 && !chosen.Contains(x));
               break;
            default:
               throw new System.ArgumentException("A name that shouldn't be present is being used.");
         }
         choseLeft[i] = left;
         chosen.Add(orderedPlayers[i]);
      }
   }

   // Twitch Plays
#pragma warning disable 414
   private readonly string TwitchHelpMessage = "!{0} left/right [Picks that elevator.]";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string input) {
      var cmd = input.ToLowerInvariant();
      if (cmd == "left") {
         yield return null;
         yield return "strike";
         yield return "solve";
         buttons[0].OnInteract();
      }
      else if (cmd == "right") {
         yield return null;
         yield return "strike";
         yield return "solve";
         buttons[1].OnInteract();
      }
      yield break;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
      while (animating)
         yield return null;
      buttons[solution].OnInteract();
      while (!moduleSolved) {
         yield return true;
      }
   }
}
