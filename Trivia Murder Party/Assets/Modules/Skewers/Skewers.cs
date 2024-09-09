using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class Skewers : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMSelectable[] Gems;
   public Material[] Colors;
   public GameObject[] GemsGO;
   public GameObject Top;
   public GameObject[] Swords;
   public GameObject TopSwords;
   public GameObject BottomSwords;
   public GameObject LeftSwords;
   public GameObject RightSwords;
   public KMSelectable Module;
   public VoltageMeterReader VMR;

   List<int> GemColors = new List<int> { };
   List<int> ValidSpots = new List<int> { };
   int[] SwordPositions = new int[5];
   static int[] StabSpots = { 10, 9, 4, 12, 0, 7, 15, 11, 13, 8, 5, 1, 3, 14, 6, 2 };
   static int[] DefaultRGBGems = {
      2, 4, 1, 4,
      2, 1, 4, 2,
      1, 2, 1, 4,
      4, 1, 4, 2
   };
   int tempFifthSN;

   string[] ColorNamesForLog = { "black", "red", "green", "yellow", "blue", "magenta", "cyan", "white"};

   bool Red; //Need to declare these here for some reason.
   bool Green;
   bool Blue;
   bool willStrike;
   bool Animating;
   bool Ran;
   bool autosolved;

   //Logging
   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   void Awake () {
      moduleId = moduleIdCounter++;

      foreach (KMSelectable Gem in Gems) {
         Gem.OnInteract += delegate () { GemPress(Gem); return false; };
      }

      Module.OnHighlight += delegate () { HighlightSomething(); };
   }

   void HighlightSomething () {
      if (Ran) {
         return;
      }
      Ran = true;
      StartCoroutine(InitialOpen());
   }

   IEnumerator InitialOpen () {
      for (int i = 0; i < 25; i++) {
         Top.transform.Rotate(-3.6f, 0, 0);
         yield return new WaitForSeconds(.01f);
      }
   }

   // Use this for initialization
   void Start () {
      for (int i = 0; i < 25; i++) {
         Top.transform.Rotate(3.6f, 0, 0);
      }
      SwordPositions[0] = Bomb.GetSerialNumberNumbers().Last();
      SwordPositions[0] = StabSpots[Modulo(Array.IndexOf(StabSpots, SwordPositions[0]) - Bomb.GetSerialNumberNumbers().First(), 16)];
      Generate();
   }

   void GemPress (KMSelectable Gem) {
      if (Animating) {
         return;
      }
      if (!autosolved)
      {
         Calculate();
         PermissibleGems();
      }
      Animating = true;
      StartCoroutine(CloseOrOpen("close"));
      for (int i = 0; i < 16; i++) {
         if (Gem == Gems[i]) {
            Debug.LogFormat("[Skewers #{0}] You submitted {1} at {2} solves.", moduleId, "ABCD"[i % 4].ToString() + "1234"[i / 4].ToString(), Bomb.GetSolvedModuleNames().Count());
            if (ValidSpots.Contains(i)) {
               willStrike = false;
               //GetComponent<KMBombModule>().HandlePass();
            }
            else {
               willStrike = true;
               /*GetComponent<KMBombModule>().HandleStrike();
               ValidSpots.Clear();
               for (int j = 0; j < 16; j++) {
                  ValidSpots.Add(j);
               }*/
            }
         }
      }
   }

   void Generate () {
      
      GemColors.Clear();
      ValidSpots.Clear();
      int Match = UnityEngine.Random.Range(0, 16);
      for (int i = 0; i < 16; i++) {
         if (i != Match) {
            GemColors.Add(Random.Range(0, 8));
         }
         else {
            GemColors.Add(DefaultRGBGems[i]);
         }
         GemsGO[i].GetComponent<MeshRenderer>().material = Colors[GemColors[i]];
         ValidSpots.Add(i);
         Swords[i].gameObject.SetActive(false);
         
      }

      Debug.LogFormat("[Skewers #{0}] The colors are:", moduleId);
      Debug.LogFormat("[Skewers #{0}] {1} {2} {3} {4}", moduleId, ColorNamesForLog[GemColors[0]], ColorNamesForLog[GemColors[1]], ColorNamesForLog[GemColors[2]], ColorNamesForLog[GemColors[3]]);
      Debug.LogFormat("[Skewers #{0}] {1} {2} {3} {4}", moduleId, ColorNamesForLog[GemColors[4]], ColorNamesForLog[GemColors[5]], ColorNamesForLog[GemColors[6]], ColorNamesForLog[GemColors[7]]);
      Debug.LogFormat("[Skewers #{0}] {1} {2} {3} {4}", moduleId, ColorNamesForLog[GemColors[8]], ColorNamesForLog[GemColors[9]], ColorNamesForLog[GemColors[10]], ColorNamesForLog[GemColors[11]]);
      Debug.LogFormat("[Skewers #{0}] {1} {2} {3} {4}", moduleId, ColorNamesForLog[GemColors[12]], ColorNamesForLog[GemColors[13]], ColorNamesForLog[GemColors[14]], ColorNamesForLog[GemColors[15]]);
      Debug.LogFormat("[Skewers #{0}] The first sword will be always be at position {1}.", moduleId, "0123456789ABCDEF"[SwordPositions[0]]);
   }

   IEnumerator CloseOrOpen (string type) {
      if (type == "close") {
         for (int i = 0; i < 25; i++) {
            Top.transform.Rotate(3.6f, 0, 0);
            yield return new WaitForSeconds(.01f);
         }
         Audio.PlaySoundAtTransform("Close", transform);
         yield return new WaitForSeconds(1f);
         StartCoroutine(Penetration());
      }
      else {
         for (int i = 0; i < 25; i++) {
            Top.transform.Rotate(-3.6f, 0, 0);
            yield return new WaitForSeconds(.01f);
         }
         yield return new WaitForSeconds(1f);
         if (!willStrike) {
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            for (int i = 0; i < 25; i++) {
               Top.transform.Rotate(3.6f, 0, 0);
               yield return new WaitForSeconds(.01f);
            }
            Audio.PlaySoundAtTransform("Close", transform);
         }
         else {
            GetComponent<KMBombModule>().HandleStrike();
            for (int i = 0; i < 16; i++) {
               Swords[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < 30; i++) {
               TopSwords.transform.localPosition -= new Vector3(0, 0, .011f); //No idea why this one is different.
               BottomSwords.transform.localPosition += new Vector3(0, 0, .01f);
               LeftSwords.transform.localPosition += new Vector3(.01f, 0, 0);
               RightSwords.transform.localPosition -= new Vector3(.01f, 0, 0);
            }
            for (int i = 0; i < 25; i++) {
               TopSwords.transform.localPosition += new Vector3(0, 0, .005f);
               BottomSwords.transform.localPosition -= new Vector3(0, 0, .005f);
               LeftSwords.transform.localPosition -= new Vector3(.005f, 0, 0);
               RightSwords.transform.localPosition += new Vector3(.005f, 0, 0);
            }
            for (int i = 0; i < 25; i++) {
               Top.transform.Rotate(3.6f, 0, 0);
               yield return new WaitForSeconds(.01f);
            }
            Audio.PlaySoundAtTransform("Close", transform);

            Generate();

            yield return new WaitForSeconds(.5f);
            for (int i = 0; i < 25; i++) {
               Top.transform.Rotate(-3.6f, 0, 0);
               yield return new WaitForSeconds(.01f);
            }
            Animating = false;
         }
      }
   }

   IEnumerator Penetration () {
      for (int i = 0; i < 5; i++) {
         Swords[SwordPositions[i]].gameObject.SetActive(true);
      }
      yield return new WaitForSeconds(.5f);
      for (int i = 0; i < 25; i++) {
         TopSwords.transform.localPosition -= new Vector3(0, 0, .005f);
         BottomSwords.transform.localPosition += new Vector3(0, 0, .005f);
         LeftSwords.transform.localPosition += new Vector3(.005f, 0, 0);
         RightSwords.transform.localPosition -= new Vector3(.005f, 0, 0);
         yield return new WaitForSeconds(.01f);
      }
      yield return new WaitForSeconds(.2f);
      for (int i = 0; i < 30; i++) {
         TopSwords.transform.localPosition += new Vector3(0, 0, .011f); //No idea why this one is different.
         BottomSwords.transform.localPosition -= new Vector3(0, 0, .01f);
         LeftSwords.transform.localPosition -= new Vector3(.01f, 0, 0);
         RightSwords.transform.localPosition += new Vector3(.01f, 0, 0);
         if (i == 2) {
            Audio.PlaySoundAtTransform("Tearing", transform);
         }
         yield return new WaitForSeconds(.01f);
      }
      yield return new WaitForSeconds(.2f);
      StartCoroutine(CloseOrOpen("open"));
   }

   void Calculate () {
      
      int temp = ((VMR.GetVoltageMeterInt() == -1 ? 0 : VMR.GetVoltageMeterInt()) + Bomb.GetBatteryCount() + Bomb.GetIndicators().Count()) % 16;
      SwordPositions[1] = temp;
      if (SwordPositions[1] == SwordPositions[0]) {
         SwordPositions[1] = StabSpots[Modulo(Array.IndexOf(StabSpots, SwordPositions[1]) + 1, 16)];
      }
      else {
         SwordPositions[1] = temp;
      }
      int tempSameAmount = 0;
      temp = 0; //Reuse a temp variable
      for (int i = 0; i < 16; i++) {
         temp = GemColors[i];
         if (temp >= 4) {
            temp -= 4;
            Blue = true;
         }
         if (temp >= 2) {
            temp -= 2;
            Green = true;
         }
         if (temp >= 1) {
            temp--;
            Red = true;
         }
         switch (DefaultRGBGems[i]) {
            case 1:
               if (Red) {
                  tempSameAmount++;
               }
               break;
            case 2:
               if (Green) {
                  tempSameAmount++;
               }
               break;
            case 4:
               if (Blue) {
                  tempSameAmount++;
               }
               break;
         }
         Red = false;
         Green = false;
         Blue = false;
      }
      SwordPositions[2] = tempSameAmount;
      while (SwordPositions[2] == SwordPositions[0] || SwordPositions[2] == SwordPositions[1]) {
         SwordPositions[2] = StabSpots[Modulo(Array.IndexOf(StabSpots, SwordPositions[2]) + 1, 16)];
      }
      int differentColors = 16;
      for (int i = 0; i < 16; i++) {
         if (GemColors[i] == DefaultRGBGems[i]) {
            differentColors--;
         }
      }
      SwordPositions[3] = Modulo(differentColors, 16);
      while (SwordPositions[3] == SwordPositions[0] || SwordPositions[3] == SwordPositions[1] || SwordPositions[3] == SwordPositions[2]) {
         SwordPositions[3] = StabSpots[Modulo(Array.IndexOf(StabSpots, SwordPositions[3]) + 1, 16)];
      }
      //                 012345678901234567890123456
      string alphabet = ".ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      string fourthSN = Bomb.GetSerialNumber()[3].ToString();
      string fifthSN = Bomb.GetSerialNumber()[4].ToString();

      for (int i = 1; i < 27; i++) {
         if (alphabet[i].ToString() == fifthSN) {
            tempFifthSN = i;
         }
      }
      for (int i = 1; i < 27; i++) {
         if (alphabet[i].ToString() == fourthSN) {
            temp = i;
            while (temp >= 7) {
               temp -= 6;
            }
            break;
         }
      }
      //Debug.Log(tempFifthSN);
      //Debug.Log(temp);
      temp += 9;
      temp = StabSpots[Modulo(Array.IndexOf(StabSpots, temp) - (tempFifthSN + 9), 16)];
      SwordPositions[4] = temp;
      while (SwordPositions[4] == SwordPositions[0] || SwordPositions[4] == SwordPositions[1] || SwordPositions[4] == SwordPositions[2] || SwordPositions[4] == SwordPositions[3]) {
         SwordPositions[4] = StabSpots[Modulo(Array.IndexOf(StabSpots, SwordPositions[4]) + 1, 16)];
      }
      Debug.LogFormat("[Skewers #{0}] The swords will be at: {1}{2}{3}{4}", moduleId, "0123456789ABCDEF"[SwordPositions[1]], "0123456789ABCDEF"[SwordPositions[2]], "0123456789ABCDEF"[SwordPositions[3]], "0123456789ABCDEF"[SwordPositions[4]]);
   }

   void PermissibleGems () {
      for (int i = 0; i < 5; i++) {
         switch (SwordPositions[i]) {
            case 0:
               ValidSpots.Remove(1);
               ValidSpots.Remove(2);
               ValidSpots.Remove(3); //Don't know if this stacks. Also can't think of a more efficient way to do this.
               break;
            case 1:
               ValidSpots.Remove(4);
               ValidSpots.Remove(8);
               ValidSpots.Remove(12);
               break;
            case 2:
               ValidSpots.Remove(0);
               ValidSpots.Remove(1);
               ValidSpots.Remove(2);
               break;
            case 3:
               ValidSpots.Remove(12);
               ValidSpots.Remove(13);
               ValidSpots.Remove(14);
               break;
            case 4:
               ValidSpots.Remove(2);
               ValidSpots.Remove(6);
               ValidSpots.Remove(10);
               break;
            case 5:
               ValidSpots.Remove(5);
               ValidSpots.Remove(9);
               ValidSpots.Remove(13);
               break;
            case 6:
               ValidSpots.Remove(4);
               ValidSpots.Remove(5);
               ValidSpots.Remove(6);
               break;
            case 7:
               ValidSpots.Remove(5);
               ValidSpots.Remove(6);
               ValidSpots.Remove(7);
               break;
            case 8:
               ValidSpots.Remove(6);
               ValidSpots.Remove(10);
               ValidSpots.Remove(14);
               break;
            case 9:
               ValidSpots.Remove(1);
               ValidSpots.Remove(5);
               ValidSpots.Remove(9);
               break;
            case 10:
               ValidSpots.Remove(0);
               ValidSpots.Remove(4);
               ValidSpots.Remove(8);
               break;
            case 11:
               ValidSpots.Remove(13);
               ValidSpots.Remove(14);
               ValidSpots.Remove(15);
               break;
            case 12:
               ValidSpots.Remove(3);
               ValidSpots.Remove(7);
               ValidSpots.Remove(11);
               break;
            case 13:
               ValidSpots.Remove(7);
               ValidSpots.Remove(11);
               ValidSpots.Remove(15);
               break;
            case 14:
               ValidSpots.Remove(8);
               ValidSpots.Remove(9);
               ValidSpots.Remove(10);
               break;
            case 15:
               ValidSpots.Remove(9);
               ValidSpots.Remove(10);
               ValidSpots.Remove(11);
               break;
         }
      }
   }

   int Modulo (int input, int By) {
      return (((input % By) + By) % By);
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} X# to submit that coordinate. Use !{0} anything to open the briefcase.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.ToUpper().Trim();
      int Index = 0;
      yield return null;
      if (!Ran) {
         Module.OnHighlight();
         yield break;
      }
      if (Command.Length != 2) {
         yield return "sendtochaterror I don't understand!";
      }
      else if (!"ABCD".Contains(Command[0]) || !"1234".Contains(Command[1])) {
         yield return "sendtochaterror I don't understand!";
      }
      else {
         for (int i = 0; i < 4; i++) {
            if (Command[0] == "ABCD"[i]) {
               Index = i;
            }
         }
         for (int i = 0; i < 4; i++) {
            if (Command[1] == "1234"[i]) {
               Index += i * 4;
            }
         }
         Gems[Index].OnInteract();
         if (ValidSpots.Contains(Index)) {
            yield return "solve";
         }
         else {
            yield return "strike";
         }
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      autosolved = true;
      if (!Ran) {
         Module.OnHighlight();
         yield return new WaitForSeconds(2.5f);
      }
      Calculate();
      PermissibleGems();
      Gems[ValidSpots[Random.Range(0, ValidSpots.Count())]].OnInteract();
      while (!moduleSolved) yield return true;
   }
}
