using System.Collections;
using System.Linq;
using UnityEngine;

public class Rules : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMSelectable[] RuleButtons;
   public KMSelectable StartNowIGuessFuckYouImTerryDavis;
   public TextMesh[] Options;
   public TextMesh TheRuleTM;
   public Font[] Fonts;
   public Material[] FontMats;

   int[] OddOrEvenNumbers = new int[4];
   int RuleIndex = 0;
   int DoOrDontRuleSet = 0;
   int Counter = 0;
   int Threshhold = 0;

   float InitialTimer = 0f;
   float Timer = 0f;

   readonly string[] WordList = {
"ACID","BUST","CODE","DAZE","ECHO","FILM","GOLF","HUNT","ITCH","JURY","KING","LIME","MONK","NUMB","ONLY","PREY","QUIT","RAVE","SIZE","TOWN","URGE","VERY","WAXY","XYLO","YARD","ZERO","ABORT","BLEND","CRYPT","DWARF","EQUIP","FANCY","GIZMO","HELIX","IMPLY","JOWLS","KNIFE","LEMON","MAJOR","NIGHT","OVERT","POWER","QUILT","RUSTY","STOMP","TRASH","UNTIL","VIRUS","WHISK","XERIC","YACHT","ZEBRA","ADVICE","BUTLER","CAVITY","DIGEST","ELBOWS","FIXURE","GOBLET","HANDLE","INDUCT","JOKING","KNEADS","LENGTH","MOVIES","NIMBLE","OBTAIN","PERSON","QUIVER","RACHET","SAILOR","TRANCE","UPHELD","VANISH","WALNUT","XYLOSE","YANKED","ZODIAC","ALREADY","BROWSED","CAPITOL","DESTROY","ERASING","FLASHED","GRIMACE","HIDEOUT","INFUSED","JOYRIDE","KETCHUP","LOCKING","MAILBOX","NUMBERS","OBSCURE","PHANTOM","QUIETLY","REFUSAL","SUBJECT","TRAGEDY","UNKEMPT","VENISON","WARSHIP","XANTHIC","YOUNGER","ZEPHYRS","ADVOCATE","BACKFLIP","CHIMNEYS","DISTANCE","EXPLOITS","FOCALIZE","GIFTWRAP","HOVERING","INVENTOR","JEALOUSY","KINSFOLK","LOCKABLE","MERCIFUL","NOTECARD","OVERCAST","PERILOUS","QUESTION","RAINCOAT","STEALING","TREASURY","UPDATING","VERTICAL","WISHBONE","XENOLITH","YEARLONG","ZEALOTRY","ABHORRENT","BACCARATS","CULTIVATE","DAMNINGLY","EFFLUXION","FUTURISTS","GYROSCOPE","HAZARDOUS","ILLOGICAL","JUXTAPOSE","KILOBYTES","LANTHANUM","MATERIALS","NIHILISTS","OBSCENITY","PAINFULLY","QUEERNESS","RESTROOMS","SABOTAGED","TYRANNOUS","UMPTEENTH","VEXILLATE","WAYLAYERS","XENOBLAST","YTTERBIUM","ZIGZAGGER"
};
   //string[] Introtexts = { "You will fail", "The timer is ticking", "It is inevitable", "Better flee", "Give up", "There is no hope" };
   string[] RuleList = { "Press a word\nwith the letter\n", "Press the\nshortest word", "Press the\nlongest word", "Press an\neven number", "Press an\nodd number", "Press the\n{0} button" };
   string[] DontRuleList = { "Don't press a\nword with the\nletter ", "Don't press\nthe shortest\nword", "Don't press\nthe longest\nword", "Don't press\nan even\nnumber", "Don't press\nan odd number", "Don't press\nthe {0}\nbutton" };
   string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
   string PossibilitiesForLetters = "";
   //string ForMrsKwan = "https://www.youtube.com/watch?v=7ExEXoTm4Dc";
   string ShortestOrLongest = "";

   char LuckyLetter = ' ';

   bool[] Validity = { false, false, false, false };
   bool Activate = false;

   Coroutine Stunned;
   bool IsStunned;

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

#pragma warning disable 0649
   bool TwitchPlaysActive;
#pragma warning restore 0649

   void Awake () {
      moduleId = moduleIdCounter++;
      foreach (KMSelectable RuleButton in RuleButtons) {
         RuleButton.OnInteract += delegate () { RuleButtonPress(RuleButton); return false; };
         RuleButton.OnHighlight += delegate () { RuleHover(RuleButton); };
         RuleButton.OnHighlightEnded += delegate () { RuleDehover(RuleButton); };
      }
      StartNowIGuessFuckYouImTerryDavis.OnInteract += delegate () { StartPress(); return false; };
   }

   void RuleHover (KMSelectable Button) {
      for (int i = 0; i < 4; i++) {
         if (Button == RuleButtons[i]) {
            Options[i].color = new Color32(255, 0, 0, 255);
         }
      }
   }

   void RuleDehover (KMSelectable Button) {
      for (int i = 0; i < 4; i++) {
         if (Button == RuleButtons[i]) {
            Options[i].color = new Color32(255, 255, 255, 255);
         }
      }
   }

   void Start () {
      IntroTextGenerator();
      InitialTimer = Bomb.GetTime();
   }

   void IntroTextGenerator () {
      /*Introtexts.Shuffle();
      for (int i = 0; i < 4; i++) {
         Options[i].text = Introtexts[i];
      }*/
      TheRuleTM.text = "Press here to start.";
   }

   void StartPress () {
      if (!Activate) {
         for (int i = 0; i < 4; i++) {
            Options[i].font = Fonts[1];
            Options[i].fontSize = 200;
            Options[i].GetComponent<Renderer>().material = FontMats[1];
         }
         Activate = true;
         if (Bomb.GetTime() <= 60 || InitialTimer == 0) {
            Threshhold = 1;
         }
         else if (Bomb.GetTime() / InitialTimer >= .81f) {
            Threshhold = 20;
         }
         else if (Bomb.GetTime() / InitialTimer >= .61f) {
            Threshhold = 16;
         }
         else if (Bomb.GetTime() / InitialTimer >= .41f) {
            Threshhold = 12;
         }
         else if (Bomb.GetTime() / InitialTimer >= .21f) {
            Threshhold = 8;
         }
         else if (Bomb.GetTime() / InitialTimer >= .01f) {
            Threshhold = 4;
         }
         else {
            Threshhold = 1;
         }
         if (Bomb.GetSolvableModuleNames().Count - Bomb.GetSolvedModuleNames().Count <= 1) {
            Threshhold = 1;
         }
         Debug.LogFormat("[Rules #{0}] {1} module(s) need to be solved.", moduleId, Threshhold);
         RulePicker();
      }
   }

   void RuleButtonPress (KMSelectable RuleButton) {
      if (!Activate || IsStunned) {
         return;
      }
      else {
         for (int i = 0; i < 4; i++) {
            if (RuleButton == RuleButtons[i]) {
               if (Validity[i]) {
                  Audio.PlaySoundAtTransform("BiggerDick", RuleButton.transform);
                  Counter++;
                  for (int j = 0; j < 4; j++) {
                     Validity[j] = false;
                  }
                  RulePicker();
               }
               else {
                  Stunned = StartCoroutine(Stun());
               }
            }
         }
      }
   }

   IEnumerator Stun () {
      IsStunned = true;
      for (int i = 0; i < 4; i++) {
         Options[i].text = "";
      }
      TheRuleTM.text = "You are stunned!";
      for (int i = 0; i < 4; i++) {
         Validity[i] = false;
      }
      yield return new WaitForSeconds(3f);
      IsStunned = false;
      RulePicker();
   }

   void RulePicker () {
      if (IsStunned) {
         return;
      }
      DoOrDontRuleSet = UnityEngine.Random.Range(0, 2);
      if (DoOrDontRuleSet == 0) {
         RuleIndex = UnityEngine.Random.Range(0, RuleList.Length);
         TheRuleTM.text = RuleList[RuleIndex];
      }
      else {
         RuleIndex = UnityEngine.Random.Range(0, DontRuleList.Length);
         TheRuleTM.text = DontRuleList[RuleIndex];
      }
      switch (RuleIndex) {
         case 0:
            Rule0Reset:
            for (int i = 0; i < 4; i++) {
               Validity[i] = false;
            }
            PossibilitiesForLetters = "";
            for (int i = 0; i < 4; i++) {
               Options[i].text = WordList[UnityEngine.Random.Range(0, WordList.Length)];
            }
            for (int i = 0; i < Alphabet.Length; i++) {
               int Rule0Temp = 0;
               for (int j = 0; j < 4; j++) {
                  if (Options[j].text.Contains(Alphabet[i])) {
                     Rule0Temp += 1;
                  }
               }
               if (Rule0Temp == 4) {
                  PossibilitiesForLetters += ".";
               }
               else if (Rule0Temp == 0) {
                  PossibilitiesForLetters += ".";
               }
               else {
                  PossibilitiesForLetters += Alphabet[i].ToString();
               }
            }
            if (PossibilitiesForLetters == "..........................") {
               goto Rule0Reset;
            }
            do {
               LuckyLetter = PossibilitiesForLetters[UnityEngine.Random.Range(0, PossibilitiesForLetters.Length)];
            } while (LuckyLetter == '.' || LuckyLetter == ' ');
            for (int i = 0; i < 4; i++) {
               if (Options[i].text.Contains(LuckyLetter)) {
                  Validity[i] = true;
               }
            }
            if ((!Validity[0] && !Validity[1] && !Validity[2] && !Validity[3]) || (Validity[0] && Validity[1] && Validity[2] && Validity[3])) {
               goto Rule0Reset;
            }
            if (DoOrDontRuleSet == 1) {
               Inverter();
            }
            TheRuleTM.text += LuckyLetter;
            PossibilitiesForLetters = "";
            break;
         case 1:
            Rule1Reset:
            for (int i = 0; i < 4; i++) {
               Validity[i] = false;
            }
            for (int i = 0; i < 4; i++) {
               Options[i].text = WordList[UnityEngine.Random.Range(0, WordList.Length)];
            }
            if ((Options[0].text.Length == Options[1].text.Length || Options[0].text.Length == Options[2].text.Length || Options[0].text.Length == Options[3].text.Length) || (Options[1].text.Length == Options[2].text.Length || Options[1].text.Length == Options[3].text.Length) || Options[2].text.Length == Options[3].text.Length) {
               goto Rule1Reset;
            }
            ShortestOrLongest = Options[0].text;
            for (int i = 1; i < 4; i++) {
               if (ShortestOrLongest.Length > Options[i].text.Length) {
                  ShortestOrLongest = Options[i].text;
               }
            }
            for (int i = 0; i < 4; i++) {
               if (Options[i].text == ShortestOrLongest) {
                  Validity[i] = true;
               }
            }
            if ((!Validity[0] && !Validity[1] && !Validity[2] && !Validity[3]) || (Validity[0] && Validity[1] && Validity[2] && Validity[3])) {
               goto Rule1Reset;
            }
            if (DoOrDontRuleSet == 1) {
               Inverter();
            }
            break;
         case 2:
            Rule2Reset:
            for (int i = 0; i < 4; i++) {
               Validity[i] = false;
            }
            for (int i = 0; i < 4; i++) {
               Options[i].text = WordList[UnityEngine.Random.Range(0, WordList.Length)];
            }
            if ((Options[0].text.Length == Options[1].text.Length || Options[0].text.Length == Options[2].text.Length || Options[0].text.Length == Options[3].text.Length) || (Options[1].text.Length == Options[2].text.Length || Options[1].text.Length == Options[3].text.Length) || Options[2].text.Length == Options[3].text.Length) {
               goto Rule2Reset;
            }
            ShortestOrLongest = Options[0].text;
            for (int i = 1; i < 4; i++) {
               if (ShortestOrLongest.Length < Options[i].text.Length) {
                  ShortestOrLongest = Options[i].text;
               }
            }
            for (int i = 0; i < 4; i++) {
               if (Options[i].text == ShortestOrLongest) {
                  Validity[i] = true;
               }
            }
            if ((!Validity[0] && !Validity[1] && !Validity[2] && !Validity[3]) || (Validity[0] && Validity[1] && Validity[2] && Validity[3])) {
               goto Rule2Reset;
            }
            if (DoOrDontRuleSet == 1) {
               Inverter();
            }
            break;
         case 3:
            Rule3Reset:
            int Rule3Temp = 0;
            for (int i = 0; i < 4; i++) {
               Validity[i] = false;
            }
            for (int i = 0; i < 4; i++) {
               OddOrEvenNumbers[i] = UnityEngine.Random.Range(1, 50);
               Options[i].text = OddOrEvenNumbers[i].ToString();
               if (OddOrEvenNumbers[i] % 2 == 0) {
                  Rule3Temp += 1;
                  Validity[i] = true;
               }
            }
            if (Rule3Temp == 4 || (!Validity[0] && !Validity[1] && !Validity[2] && !Validity[3]) || (Validity[0] && Validity[1] && Validity[2] && Validity[3])) {
               goto Rule3Reset;
            }
            if (DoOrDontRuleSet == 1) {
               Inverter();
            }
            break;
         case 4:
            Rule4Reset:
            int Rule4Temp = 0;
            for (int i = 0; i < 4; i++) {
               Validity[i] = false;
            }
            for (int i = 0; i < 4; i++) {
               OddOrEvenNumbers[i] = UnityEngine.Random.Range(1, 50);
               Options[i].text = OddOrEvenNumbers[i].ToString();
               if (OddOrEvenNumbers[i] % 2 == 1) {
                  Rule4Temp += 1;
                  Validity[i] = true;
               }
            }
            if (Rule4Temp == 4 || (!Validity[0] && !Validity[1] && !Validity[2] && !Validity[3]) || (Validity[0] && Validity[1] && Validity[2] && Validity[3])) {
               goto Rule4Reset;
            }
            if (DoOrDontRuleSet == 1) {
               Inverter();
            }
            break;
         case 5:
            if (DoOrDontRuleSet == 0) {
               for (int i = 0; i < 4; i++) {
                  Validity[i] = false;
                  Options[i].text = "Button";
               }
               TheRuleTM.text = "Press the\n";
               switch (UnityEngine.Random.Range(0, 4)) {
                  case 0:
                     TheRuleTM.text += "first button";
                     break;
                  case 1:
                     TheRuleTM.text += "second button";
                     break;
                  case 2:
                     TheRuleTM.text += "third button";
                     break;
                  case 3:
                     TheRuleTM.text += "fourth button";
                     break;
               }
               switch (TheRuleTM.text[11]) {
                  case 'i':
                     Validity[0] = true;
                     break;
                  case 'e':
                     Validity[1] = true;
                     break;
                  case 'h':
                     Validity[2] = true;
                     break;
                  case 'o':
                     Validity[3] = true;
                     break;
               }
            }
            else {
               for (int i = 0; i < 4; i++) {
                  Validity[i] = true;
                  Options[i].text = "Button";
               }
               TheRuleTM.text = "Don't press the\n";
               switch (UnityEngine.Random.Range(0, 4)) {
                  case 0:
                     TheRuleTM.text += "first button";
                     break;
                  case 1:
                     TheRuleTM.text += "second button";
                     break;
                  case 2:
                     TheRuleTM.text += "third button";
                     break;
                  case 3:
                     TheRuleTM.text += "fourth button";
                     break;
               }
               switch (TheRuleTM.text[17]) {
                  case 'i':
                     Validity[0] = false;
                     break;
                  case 'e':
                     Validity[1] = false;
                     break;
                  case 'h':
                     Validity[2] = false;
                     break;
                  case 'o':
                     Validity[3] = false;
                     break;
               }
            }
            break;
      }
      Debug.LogFormat("[Rules #{0}] The current rule is \"{1}\".", moduleId, TheRuleTM.text.Replace("\n", " "));
      if (RuleIndex != 5) {
         Debug.LogFormat("[Rules #{0}] The options are {1}, {2}, {3}, and {4}.", moduleId, Options[0].text, Options[1].text, Options[2].text, Options[3].text);
      }
   }

   void Inverter () {
      if (DoOrDontRuleSet == 1) {
         for (int i = 0; i < 4; i++) {
            Validity[i] = !Validity[i];
         }
      }
   }

   void Update () {
      if (Activate) {
         Timer += Time.deltaTime;
         if (Timer >= 30f && !TwitchPlaysActive) {
            StartCoroutine(Check());
         }
         else if (Timer >= 100f && TwitchPlaysActive) {
            StartCoroutine(Check());
         }
      }
   }

   IEnumerator Check () {
      for (int j = 0; j < 4; j++) {
         Options[j].font = Fonts[0];
         Options[j].fontSize = 144;
         Options[j].GetComponent<Renderer>().material = FontMats[0];
      }
      Timer = 0f;
      for (int i = 0; i < 4; i++) {
         Options[i].text = "";
      }
      TheRuleTM.text = Counter.ToString() + " out of " + Threshhold.ToString();
      yield return new WaitForSecondsRealtime(3f);
      if (Counter >= Threshhold) {
         GetComponent<KMBombModule>().HandlePass();
         moduleSolved = true;
         Debug.LogFormat("[Rules #{0}] You followed {1} rules correctly out of the required minimum of {2}. Module disarmed.", moduleId, Counter.ToString(), Threshhold.ToString());
      }
      else {
         Counter = 0;
         Threshhold = 0;
         Activate = false;
         GetComponent<KMBombModule>().HandleStrike();
         Debug.LogFormat("[Rules #{0}] You followed {1} rules correctly out of the required minimum of {2}. Strike!", moduleId, Counter.ToString(), Threshhold.ToString());
         IntroTextGenerator();
      }
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} start to start. Use !{0} 1/2/3/4 to press the corresponding button from top to bottom.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.Trim().ToUpper();
      yield return null;
      if (Command.ToString() == "START") {
         StartNowIGuessFuckYouImTerryDavis.OnInteract();
         yield return new WaitForSecondsRealtime(.1f);
      }
      else if (Command.ToString() == "1") {
         RuleButtons[0].OnInteract();
         yield return new WaitForSecondsRealtime(.1f);
      }
      else if (Command.ToString() == "2") {
         RuleButtons[1].OnInteract();
         yield return new WaitForSecondsRealtime(.1f);
      }
      else if (Command.ToString() == "3") {
         RuleButtons[2].OnInteract();
         yield return new WaitForSecondsRealtime(.1f);
      }
      else if (Command.ToString() == "4") {
         RuleButtons[3].OnInteract();
         yield return new WaitForSecondsRealtime(.1f);
      }
      else {
         yield return "sendtochaterror I don't understand!";
         yield break;
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      Timer = 70f;
      if (!Activate) {
         StartNowIGuessFuckYouImTerryDavis.OnInteract();
         yield return new WaitForSecondsRealtime(.1f);
      }
      while (IsStunned) {
         yield return null;
      }
      while (Counter != Threshhold) {
         for (int i = 0; i < 4; i++) {
            if (Validity[i]) {
               RuleButtons[i].OnInteract();
               yield return new WaitForSecondsRealtime(.1f);
               break;
            }
         }
      }
      while (!moduleSolved) {
         yield return true;
      }
   }
}
