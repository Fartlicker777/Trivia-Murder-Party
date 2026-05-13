using System.Collections;
using System.Linq;
using UnityEngine;

public class PixelArt : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public KMSelectable[] Buttons;
   public KMSelectable StatusLightButton;

   public GameObject[] ColorOfButton;

   public Material[] Colores;

   bool[] ColorAnswer = new bool[24];
   bool[] CurrentButtonColor = new bool[24];
   bool[] Active = { false, false, true };

   static int moduleIdCounter = 1;
   int moduleId;
   bool moduleSolved;

   void Awake () {
      moduleId = moduleIdCounter++;

      foreach (KMSelectable Button in Buttons) {
         Button.OnInteract += delegate () { PixelPress(Button); return false; };
      }
      StatusLightButton.OnInteract += delegate () { StatusPress(); return false; };
   }

   void PixelPress (KMSelectable Button) {
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Button.transform);
      if (!Active[1]) {
         return;
      }
      for (int i = 0; i < ColorAnswer.Count(); i++)
         if (Button == Buttons[i]) {
            CurrentButtonColor[i] = !CurrentButtonColor[i];
            if (CurrentButtonColor[i]) {
               ColorOfButton[i].GetComponent<MeshRenderer>().material = Colores[1];
            }
            else {
               ColorOfButton[i].GetComponent<MeshRenderer>().material = Colores[0];
            }
         }
   }

   void StatusPress () {
      if (Active[1]) {
         StartCoroutine(AnswerVerification());
      }
      else if (!Active[0]) {
         Active[0] = true;
         StartCoroutine(AnswerGenerator());
      }
   }

   IEnumerator AnswerGenerator () {
      Debug.LogFormat("[Pixel Art #{0}] In reading order the order is:", moduleId);
      for (int i = 0; i < ColorAnswer.Count(); i++) {
         if (UnityEngine.Random.Range(0, 2) == 1) {
            ColorAnswer[i] = true;
            ColorOfButton[i].GetComponent<MeshRenderer>().material = Colores[1];
            Debug.LogFormat("[Pixel Art #{0}] Red.", moduleId);
         }
         else {
            ColorOfButton[i].GetComponent<MeshRenderer>().material = Colores[0];
            Debug.LogFormat("[Pixel Art #{0}] White.", moduleId);
         }
      }
      yield return new WaitForSeconds(7f);
      for (int i = 0; i < ColorAnswer.Count(); i++) {
         ColorOfButton[i].GetComponent<MeshRenderer>().material = Colores[0];
      }
      Active[1] = true;
   }

   IEnumerator AnswerVerification () {
      Active[1] = false;
      for (int i = 0; i < ColorAnswer.Count(); i++) {
         if (!CurrentButtonColor[i]) {
            Debug.LogFormat("[Pixel Art #{0}] White", moduleId);
         }
         else {
            Debug.LogFormat("[Pixel Art #{0}] Red", moduleId);
         }
      }

      #region Lord forgive me for I must sin.

      if (ColorAnswer[19] == CurrentButtonColor[19]) {
         ColorOfButton[19].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[19].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[14] == CurrentButtonColor[14]) {
         ColorOfButton[14].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[14].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[9] == CurrentButtonColor[9]) {
         ColorOfButton[9].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[9].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[4] == CurrentButtonColor[4]) {
         ColorOfButton[4].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[4].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[0] == CurrentButtonColor[0]) {
         ColorOfButton[0].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[0].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[1] == CurrentButtonColor[1]) {
         ColorOfButton[1].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[1].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[5] == CurrentButtonColor[5]) {
         ColorOfButton[5].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[5].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[10] == CurrentButtonColor[10]) {
         ColorOfButton[10].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[10].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[15] == CurrentButtonColor[15]) {
         ColorOfButton[15].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[15].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[20] == CurrentButtonColor[20]) {
         ColorOfButton[20].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[20].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[21] == CurrentButtonColor[21]) {
         ColorOfButton[21].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[21].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[16] == CurrentButtonColor[16]) {
         ColorOfButton[16].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[16].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[11] == CurrentButtonColor[11]) {
         ColorOfButton[11].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[11].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[6] == CurrentButtonColor[6]) {
         ColorOfButton[6].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[6].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[2] == CurrentButtonColor[2]) {
         ColorOfButton[2].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[2].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[3] == CurrentButtonColor[3]) {
         ColorOfButton[3].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[3].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[7] == CurrentButtonColor[7]) {
         ColorOfButton[7].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[7].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[12] == CurrentButtonColor[12]) {
         ColorOfButton[12].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[12].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[17] == CurrentButtonColor[17]) {
         ColorOfButton[17].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[17].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[22] == CurrentButtonColor[22]) {
         ColorOfButton[22].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[22].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[23] == CurrentButtonColor[23]) {
         ColorOfButton[23].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[23].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[18] == CurrentButtonColor[18]) {
         ColorOfButton[18].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[18].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[13] == CurrentButtonColor[13]) {
         ColorOfButton[13].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[13].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);
      if (ColorAnswer[8] == CurrentButtonColor[8]) {
         ColorOfButton[8].GetComponent<MeshRenderer>().material = Colores[2];
      }
      else {
         ColorOfButton[8].GetComponent<MeshRenderer>().material = Colores[1];
         Active[2] = false;
      }
      yield return new WaitForSeconds(.1f);

      #endregion

      if (Active[2]) {
         GetComponent<KMBombModule>().HandlePass();
         moduleSolved = true;
      }
      else {
         GetComponent<KMBombModule>().HandleStrike();
         for (int i = 0; i < ColorAnswer.Count(); i++) {
            ColorAnswer[i] = false;
            CurrentButtonColor[i] = false;
            ColorOfButton[i].GetComponent<MeshRenderer>().material = Colores[3];
         }
         Active[0] = false;
         Active[2] = true;
      }
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} start to start. Use !{0} toggle A1 to toggle the corresponding square. Letters are columns and numbers are rows. The top of the E column is E2. Use !{0} submit to submit.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
      int Index = 0;
      Command = Command.Trim();
      string[] Parameters = Command.Split(' ');
      if (Parameters[0].ToString().ToLower() != "toggle" && Parameters[0].ToString().ToLower() != "submit" && Parameters[0].ToString().ToLower() != "start") {
         yield return "sendtochaterror Invalid command!";
         yield break;
      }
      if (Parameters[0].ToString().ToLower() == "start" || Parameters[0].ToString().ToLower() == "submit") {
         if (Active[1]) {
            bool weed = true;
            for (int i = 0; i < 24; i++) {
               if (ColorAnswer[i] != CurrentButtonColor[i]) {
                  weed = false;
               }
            }
            yield return weed ? "solve" : "strike";
         }
         StatusLightButton.OnInteract();
      }
      else if (Parameters[0].ToString().ToLower() == "toggle") {
         for (int i = 1; i < Parameters.Length; i++) {
            if (Parameters[i].Length != 2) {
               yield return "sendtochaterror Invalid command!";
               yield break;
            }
            if (Parameters[i].ToString().ToLower() == "e1") {
               yield return "sendtochaterror That does not exist!";
               yield break;
            }
            else if (Parameters[i][1].ToString() == "1") { Index += 0; } //I know this is useless, but it doesn't look right without that index += 0
            else if (Parameters[i][1].ToString() == "2") { Index += 4; }
            else if (Parameters[i][1].ToString() == "3") { Index += 9; }
            else if (Parameters[i][1].ToString() == "4") { Index += 14; }
            else if (Parameters[i][1].ToString() == "5") { Index += 19; }
            else {
               yield return "sendtochaterror Invalid command!";
               yield break;
            }
            if (Parameters[i][0].ToString().ToLower() == "a") { Index++; }
            else if (Parameters[i][0].ToString().ToLower() == "b") { Index += 2; }
            else if (Parameters[i][0].ToString().ToLower() == "c") { Index += 3; }
            else if (Parameters[i][0].ToString().ToLower() == "d") { Index += 4; }
            else if (Parameters[i][0].ToString().ToLower() == "e") { Index += 5; }
            else {
               yield return "sendtochaterror Invalid command!";
               yield break;
            }
            Index--;
            Buttons[Index].OnInteract();
            Index &= 0;
         }
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      if (!Active[0]) {
         StatusLightButton.OnInteract();
      }
      while (!Active[1]) {
         yield return true;
      }
      for (int i = 0; i < 24; i++) {
         while (CurrentButtonColor[i] != ColorAnswer[i]) {
            Buttons[i].OnInteract();
            yield return new WaitForSecondsRealtime(.1f);
         }
      }
      StatusLightButton.OnInteract();
      while (!moduleSolved) yield return true;
   }
}
