using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MentalMath : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public KMSelectable[] Buttons;
   public KMSelectable Crank;

   public GameObject MobilePartOfCrank;

   public TextMesh[] AnswerChoiceText;
   public TextMesh Equation;
   public TextMesh OutOfText;

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   float SolvableMods;
   float SolvedMods;
   float Timer;

   private List<int> OriginalButtonOrder = new List<int> { 0, 1, 2, 3 };
   int FirstNumber;
   int QuestionsAnswered;
   int SecondNumber;
   int Threshold;

   bool Activated;

   Coroutine Stunned;
   bool IsStunned;

#pragma warning disable 0649
   bool TwitchPlaysActive;
#pragma warning restore 0649
   bool suckmyMrsQuanAsshole = false;

   void Awake () {
      moduleId = moduleIdCounter++;

      foreach (KMSelectable Button in Buttons) {
         Button.OnInteract += delegate () { ButtonPress(Button); return false; };
         Button.OnHighlight += delegate () { RuleHover(Button); };
         Button.OnHighlightEnded += delegate () { RuleDehover(Button); };
      }

      Crank.OnInteract += delegate () { CrankPress(); return false; };

   }

   void RuleHover (KMSelectable Button) {
      for (int i = 0; i < 4; i++) {
         if (Button == Buttons[i]) {
            AnswerChoiceText[i].color = new Color32(255, 0, 0, 255);
         }
      }
   }

   void RuleDehover (KMSelectable Button) {
      for (int i = 0; i < 4; i++) {
         if (Button == Buttons[i]) {
            AnswerChoiceText[i].color = new Color32(255, 255, 255, 255);
         }
      }
   }

   void CrankPress () {
      if (moduleSolved || Activated) {
         return;
      }
      if (TwitchPlaysActive) {
         Audio.PlaySoundAtTransform("deaf_is_an_asshole", transform);
      }
      else {
         Audio.PlaySoundAtTransform("Trivia Murder Party Math Weasel Timer", transform);
      }
      ThresholdCalculator();
      StartCoroutine(CrankTurnAnimation());
   }

   void ButtonPress (KMSelectable Button) {
      if (!Activated || IsStunned) {
         return;
      }
      for (int i = 0; i < 4; i++) {
         if (Button == Buttons[i]) {
            if (i == OriginalButtonOrder[0] && Activated) {
               Audio.PlaySoundAtTransform("BiggerDick", Button.transform);
               QuestionsAnswered++;
               EquationGeneration();
            }
            else {
               Stunned = StartCoroutine(Stun());
               Debug.LogFormat("[Mental Math #{0}] You pressed the wrong number, you are now stunned!.", moduleId);
            }
         }
      }
   }

   IEnumerator Stun () {
      IsStunned = true;
      Equation.transform.localPosition = new Vector3(0, Equation.transform.localPosition.y, 0);
      for (int i = 0; i < 4; i++) {
         AnswerChoiceText[i].text = "";
      }
      Equation.text = "You are\nstunned!";
      yield return new WaitForSeconds(3f);
      Equation.transform.localPosition = new Vector3(-0.0148f, Equation.transform.localPosition.y, .0686f);
      IsStunned = false;
      EquationGeneration();
   }

   void ThresholdCalculator () {
      if (Activated)
         return;
      if (SolvableMods - SolvedMods == 1)
         Threshold = 1;
      else if (SolvedMods / SolvableMods >= .81f)
         Threshold = 5;
      else if (SolvedMods / SolvableMods >= .61f)
         Threshold = 10;
      else if (SolvedMods / SolvableMods >= .41f)
         Threshold = 15;
      else if (SolvedMods / SolvableMods >= .21f)
         Threshold = 20;
      else if (SolvedMods / SolvableMods >= .01f)
         Threshold = 25;
      else if (SolvedMods / SolvableMods == 0f)
         Threshold = 30;
      else
         Threshold = 1;
      Debug.LogFormat("[Mental Math #{0}] {1} module(s) need to be solved.", moduleId, Threshold);
      EquationGeneration();
   }

   void EquationGeneration () {
      if (IsStunned) {
         return;
      }
      Activated = true;
      FirstNumber = UnityEngine.Random.Range(0, 16);
      SecondNumber = UnityEngine.Random.Range(0, 16);
      if (UnityEngine.Random.Range(0, 2) == 1) {
         Equation.text = FirstNumber.ToString() + " - " + SecondNumber.ToString();
         Debug.LogFormat("[Mental Math #{0}] It shows {1} - {2}, the answer is {3}.", moduleId, FirstNumber, SecondNumber, FirstNumber - SecondNumber);
         FirstNumber -= SecondNumber;
      }
      else {
         Equation.text = FirstNumber.ToString() + " + " + SecondNumber.ToString();
         Debug.LogFormat("[Mental Math #{0}] It shows {1} + {2}, the answer is {3}.", moduleId, FirstNumber, SecondNumber, FirstNumber + SecondNumber);
         FirstNumber += SecondNumber;
      }
      OriginalButtonOrder.Shuffle();
      var x = UnityEngine.Random.Range(-5, 6);
      while (x == 0) {
         x = UnityEngine.Random.Range(-5, 6);
      }
      var y = UnityEngine.Random.Range(-5, 6);
      while (y == 0 || y == x) {
         y = UnityEngine.Random.Range(-5, 6);
      }
      var z = UnityEngine.Random.Range(-5, 6);
      while (z == 0 || z == x || z == y) {
         z = UnityEngine.Random.Range(-5, 6);
      }
      AnswerChoiceText[OriginalButtonOrder[0]].text = FirstNumber.ToString();
      AnswerChoiceText[OriginalButtonOrder[1]].text = (FirstNumber + x).ToString();
      AnswerChoiceText[OriginalButtonOrder[2]].text = (FirstNumber + y).ToString();
      AnswerChoiceText[OriginalButtonOrder[3]].text = (FirstNumber + z).ToString();
   }

   void Update () {
      SolvedMods = Bomb.GetSolvedModuleNames().Count;
      SolvableMods = Bomb.GetSolvableModuleNames().Count;
      if (Activated) {
         Timer += Time.deltaTime;
         if ((Timer >= 30f && !TwitchPlaysActive) || (Timer >= 100f && TwitchPlaysActive)) {
            Activated = false;
            StopAllCoroutines();
            MobilePartOfCrank.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            StartCoroutine(Check());
         }
      }
   }

   IEnumerator Check () {
      for (int i = 0; i < 4; i++) {
         AnswerChoiceText[i].text = "";
         Equation.text = "";
      }
      OutOfText.text = QuestionsAnswered.ToString() + " out of " + Threshold.ToString();
      yield return new WaitForSeconds(5f);
      if (QuestionsAnswered >= Threshold) {
         GetComponent<KMBombModule>().HandlePass();
         moduleSolved = true;
         Debug.LogFormat("[Mental Math #{0}] You answered {1} out of the required {2}. Module disarmed.", moduleId, QuestionsAnswered, Threshold);
      }
      else {
         GetComponent<KMBombModule>().HandleStrike();
         Debug.LogFormat("[Mental Math #{0}] You answered {1} out of the required {2}. Strike.", moduleId, QuestionsAnswered, Threshold);
         Timer = 0f;
         suckmyMrsQuanAsshole = false;
         OutOfText.text = "";
         Equation.text = "This is";
         AnswerChoiceText[0].text = "Your";
         AnswerChoiceText[1].text = "Wakeup Call";
         AnswerChoiceText[2].text = "Prepare";
         AnswerChoiceText[3].text = "To die!";
         QuestionsAnswered = 0;
      }
   }

   IEnumerator CrankTurnAnimation () {
      MobilePartOfCrank.transform.Rotate(5.0f, 0.0f, 0.0f, Space.Self);
      yield return new WaitForSeconds(.00833333f);
      if (TwitchPlaysActive && !suckmyMrsQuanAsshole && Timer >= 50f) {
         Audio.PlaySoundAtTransform("deaf_is_an_asshole", transform);
         suckmyMrsQuanAsshole = true; //hahahahahahahahahahahahahahahahahahahahahahahahahahahahahahaha
      }
      StartCoroutine(CrankTurnAnimation());
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} crank to start. Use !{0} # to press the label that matches your command.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.Trim().ToUpper();
      yield return null;
      if (Command == "CRANK") {
         Crank.OnInteract();
         yield break;
      }
      else if (!Activated) {
         yield return "sendtochaterror You haven't started!";
         yield break;
      }
      for (int i = 0; i < AnswerChoiceText.Length; i++) {
         if (Command == AnswerChoiceText[i].text) {
            Buttons[i].OnInteract();
            yield break;
         }
      }
      yield return "sendtochaterror I don't understand!";
   }

   IEnumerator TwitchHandleForcedSolve () {
      Timer += 50f;
      if (!Activated) {
         Crank.OnInteract();
         yield return new WaitForSecondsRealtime(.1f);
      }
      while (QuestionsAnswered != Threshold) {
         Buttons[OriginalButtonOrder[0]].OnInteract();
         yield return new WaitForSecondsRealtime(.1f);
      }
      while (!moduleSolved) {
         yield return true;
      }
   }
}
