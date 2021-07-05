using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class Chalices : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public KMSelectable[] Buttons;

   public SpriteRenderer[] ChaliceSpriteRenderer;
   public Sprite[] ChaliceSprite;

   public GameObject[] SelectablesGameObject;

   readonly static string[] InitialCupConfig = { "Party Cup", "Mug", "Tall Glass", "Skull Glass", "Exotic Cup", "Wine Glass", "Shot Glass", "Metal Chalice" };
   string[] Cups = { "Party Cup", "Mug", "Tall Glass", "Skull Glass", "Exotic Cup", "Wine Glass", "Shot Glass", "Metal Chalice" };
   string SerialNumber = "";

   bool[] AnswerToIsAmazing = new bool[8];
   bool[] BlackConditions = new bool[8];
   bool[] Conditions = new bool[20];
   bool Active = false;

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   void Awake () {
      moduleId = moduleIdCounter++;
      foreach (KMSelectable Button in Buttons) {
         Button.OnInteract += delegate () { ButtonPress(Button); return false; };
      }
   }

   void Start () {//party mug tall skull exotic wine shot metal
      SerialNumber = Bomb.GetSerialNumber();
      Cups.Shuffle();
      Debug.LogFormat("[Chalices #{0}] The order of the chalices are: {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}.", moduleId, Cups[0], Cups[1], Cups[2], Cups[3], Cups[4], Cups[5], Cups[6], Cups[7]);

      for (int i = 0; i < 8; i++) {
         for (int j = 0; j < 8; j++) {
            if (Cups[i] == InitialCupConfig[j]) {
               ChaliceSpriteRenderer[i].sprite = ChaliceSprite[j];
            }
         }
      }
      
      //a to h
      if (Array.IndexOf(Cups, "Skull Glass") < Array.IndexOf(Cups, "Wine Glass")) {//a
         Conditions[0] = true;
         Debug.LogFormat("[Chalices #{0}] A's red condition is true.", moduleId);
      }
      else if (Array.IndexOf(Cups, "Party Cup") > Array.IndexOf(Cups, "Skull Glass")) {
         BlackConditions[0] = true;
         Debug.LogFormat("[Chalices #{0}] A's black condition is true.", moduleId);
      }
      if ((Array.IndexOf(Cups, "Tall Glass") + 1) % 2 == 0) {//b
         Conditions[1] = true;
         Debug.LogFormat("[Chalices #{0}] B's red condition is true.", moduleId);
      }
      else if ((Array.IndexOf(Cups, "Metal Chalice") + 1) % 2 == 1) {
         BlackConditions[1] = true;
         Debug.LogFormat("[Chalices #{0}] B's black condition is true.", moduleId);
      }
      if (Array.IndexOf(Cups, "Exotic Cup") - 1 != Array.IndexOf(Cups, "Tall Glass") && Array.IndexOf(Cups, "Exotic Cup") + 1 != Array.IndexOf(Cups, "Tall Glass")) {//c
         Conditions[2] = true;
         Debug.LogFormat("[Chalices #{0}] C's red condition is true.", moduleId);
      }
      else if (Array.IndexOf(Cups, "Shot Glass") - 1 == Array.IndexOf(Cups, "Wine Glass") || Array.IndexOf(Cups, "Shot Glass") + 1 == Array.IndexOf(Cups, "Wine Glass")) {
         BlackConditions[2] = true;
         Debug.LogFormat("[Chalices #{0}] C's black condition is true.", moduleId);
      }
      if (Array.IndexOf(Cups, "Mug") == 1 || Array.IndexOf(Cups, "Mug") == 2 || Array.IndexOf(Cups, "Mug") == 4 || Array.IndexOf(Cups, "Mug") == 6) {//d
         Conditions[3] = true;
         Debug.LogFormat("[Chalices #{0}] D's red condition is true.", moduleId);
      }
      else if ((Array.IndexOf(Cups, "Skull Cup") + 1).ToString() == SerialNumber[5].ToString()) {
         BlackConditions[3] = true;
         Debug.LogFormat("[Chalices #{0}] D's black condition is true.", moduleId);
      }
      if (Array.IndexOf(Cups, "Party Cup") + 1 == Array.IndexOf(Cups, "Skull Glass") && Array.IndexOf(Cups, "Skull Glass") + 1 == Array.IndexOf(Cups, "Mug")) {//e
         Conditions[4] = true;
         Debug.LogFormat("[Chalices #{0}] E's red condition is true.", moduleId);
      }
      else if (Array.IndexOf(Cups, "Exotic Cup") + 1 == Array.IndexOf(Cups, "Wine Glass") && Array.IndexOf(Cups, "Wine Glass") + 1 == Array.IndexOf(Cups, "Tall Glass")) {
         BlackConditions[4] = true;
         Debug.LogFormat("[Chalices #{0}] E's black condition is true.", moduleId);
      }
      if (Array.IndexOf(Cups, "Metal Chalice") + 2 == Array.IndexOf(Cups, "Tall Glass") || Array.IndexOf(Cups, "Metal Chalice") - 2 == Array.IndexOf(Cups, "Tall Glass")) {//f
         Conditions[5] = true;
         Debug.LogFormat("[Chalices #{0}] F's red condition is true.", moduleId);
      }
      else if (Array.IndexOf(Cups, "Shot Glass") < 4) {
         BlackConditions[5] = true;
         Debug.LogFormat("[Chalices #{0}] F's black condition is true.", moduleId);
      }
      if (Array.IndexOf(Cups, "Skull Glass") > Array.IndexOf(Cups, "Metal Chalice")) {//g
         Conditions[6] = true;
         Debug.LogFormat("[Chalices #{0}] G's red condition is true.", moduleId);
      }
      else if (Array.IndexOf(Cups, "Tall Glass") > 3) {
         BlackConditions[6] = true;
         Debug.LogFormat("[Chalices #{0}] G's black condition is true.", moduleId);
      }
      if (Array.IndexOf(Cups, "Party Cup") == 0 || Array.IndexOf(Cups, "Party Cup") == 5 || Array.IndexOf(Cups, "Party Cup") == 6 || Array.IndexOf(Cups, "Party Cup") == 7) {//h
         Conditions[7] = true;
         Debug.LogFormat("[Chalices #{0}] H's red condition is true.", moduleId);
      }
      else if (Array.IndexOf(Cups, "Mug") == 1 || Array.IndexOf(Cups, "Mug") == 2 || Array.IndexOf(Cups, "Mug") == 3 || Array.IndexOf(Cups, "Mug") == 4) {
         BlackConditions[7] = true;
         Debug.LogFormat("[Chalices #{0}] H's black condition is true.", moduleId);
      }
      //i to p
      if (Bomb.IsIndicatorOn("BOB")) { //i
         Conditions[8] = true;
         Debug.LogFormat("[Chalices #{0}] I's condition is true.", moduleId);
      }
      if (Bomb.GetBatteryCount() == 5) { //j
         Conditions[9] = true;
         Debug.LogFormat("[Chalices #{0}] J's condition is true.", moduleId);
      }
      for (int i = 0; i < 6; i++) {//k
         if (SerialNumber[i].ToString() == "A" || SerialNumber[i].ToString() == "E" || SerialNumber[i].ToString() == "I" || SerialNumber[i].ToString() == "O" || SerialNumber[i].ToString() == "U") {
            Conditions[10] = true;
            Debug.LogFormat("[Chalices #{0}] K's condition is true.", moduleId);
         }
      }
      if (Bomb.GetBatteryHolderCount() % 2 == 0) { //l
         Conditions[11] = true;
         Debug.LogFormat("[Chalices #{0}] L's condition is true.", moduleId);
      }
      for (int i = 0; i < 6; i++) {//m
         if (SerialNumber[i].ToString() == "T" || SerialNumber[i].ToString() == "M" || SerialNumber[i].ToString() == "P") {
            Conditions[12] = true;
            Debug.LogFormat("[Chalices #{0}] M's condition is true.", moduleId);
         }
      }
      if (Bomb.IsIndicatorPresent("CAR") || Bomb.IsIndicatorPresent("CLR") || Bomb.IsIndicatorPresent("IND") || Bomb.IsIndicatorPresent("MSA") || Bomb.IsIndicatorPresent("NSA") || Bomb.IsIndicatorPresent("SIG") || Bomb.IsIndicatorPresent("SND")) { //n
         Conditions[13] = true;
         Debug.LogFormat("[Chalices #{0}] N's condition is true.", moduleId);
      }
      if (Bomb.IsPortPresent(Port.Parallel) && Bomb.IsPortPresent(Port.Serial)) { //o
         Conditions[14] = true;
         Debug.LogFormat("[Chalices #{0}] O's condition is true.", moduleId);
      }
      foreach (object[] plate in Bomb.GetPortPlates()) {//p
         if (plate.Length == 0) {
            Conditions[15] = true;
            Debug.LogFormat("[Chalices #{0}] P's condition is true.", moduleId);
         }
      }
      //w to z
      if (!(Array.IndexOf(Cups, "Mug") + 1 == Array.IndexOf(Cups, "Skull Glass") && Array.IndexOf(Cups, "Skull Glass") + 1 == Array.IndexOf(Cups, "Metal Chalice"))) { //w
         Conditions[16] = true;
         Debug.LogFormat("[Chalices #{0}] W's condition is true.", moduleId);
      }
      if (Array.IndexOf(Cups, "Party Cup") + 1 == Array.IndexOf(Cups, "Tall Glass") && Array.IndexOf(Cups, "Tall Glass") + 1 == Array.IndexOf(Cups, "Exotic Cup")) { //x
         Conditions[17] = true;
         Debug.LogFormat("[Chalices #{0}] X's condition is true.", moduleId);
      }
      if ((Array.IndexOf(Cups, "Wine Glasses") == 0 || Array.IndexOf(Cups, "Wine Glasses") == 7) && (Array.IndexOf(Cups, "Shot Glass") == 0 || Array.IndexOf(Cups, "Shot Glass") == 7)) { //y
         Conditions[18] = true;
         Debug.LogFormat("[Chalices #{0}] Y's condition is true.", moduleId);
      }
      if (Array.IndexOf(Cups, "Exotic Cup") == 0 && Array.IndexOf(Cups, "Metal Chalice") == 1 && Array.IndexOf(Cups, "Mug") == 2 && Array.IndexOf(Cups, "Party Cup") == 3 && Array.IndexOf(Cups, "Shot Glass") == 4 && Array.IndexOf(Cups, "Skull Glass") == 5 && Array.IndexOf(Cups, "Tall Glass") == 6 && Array.IndexOf(Cups, "Wine Glass") == 7) {//z
         Conditions[19] = true;
         Debug.LogFormat("[Chalices #{0}] Z's condition is true.", moduleId);
      }
      if (Conditions[0] && Conditions[1] && Conditions[2] && Conditions[3] && Conditions[4] && Conditions[5] && Conditions[6] && Conditions[7] && Conditions[8] && Conditions[9]) {
         AnswerToIsAmazing[0] = true;
         Debug.LogFormat("[Chalices #{0}] There has been a gridlock, select the first cup.", moduleId);
         return;
      }
      switch (Bomb.GetPortCount() % 8) {
         case 0:
            Debug.LogFormat("[Chalices #{0}] 8n + 0 ports goes to inner C.", moduleId);
            InnerC();
            break;
         case 1:
            Debug.LogFormat("[Chalices #{0}] 8n + 1 ports goes to inner H.", moduleId);
            InnerH();
            break;
         case 2:
            Debug.LogFormat("[Chalices #{0}] 8n + 2 ports goes to inner E.", moduleId);
            InnerE();
            break;
         case 3:
            Debug.LogFormat("[Chalices #{0}] 8n + 3 ports goes to inner F.", moduleId);
            InnerF();
            break;
         case 4:
            Debug.LogFormat("[Chalices #{0}] 8n + 4 ports goes to inner d.", moduleId);
            InnerD();
            break;
         case 5:
            Debug.LogFormat("[Chalices #{0}] 8n + 5 ports goes to inner G.", moduleId);
            InnerG();
            break;
         case 6:
            Debug.LogFormat("[Chalices #{0}] 8n + 6 ports goes to inner B.", moduleId);
            InnerB();
            break;
         case 7:
            Debug.LogFormat("[Chalices #{0}] 8n + 7 ports goes to inner A.", moduleId);
            InnerA();
            break;
      }
   }

   void ButtonPress (KMSelectable Button) {
      if (Active)
         return;
      for (int i = 0; i < 8; i++) {
         if (Button == Buttons[i]) {
            if (AnswerToIsAmazing[i]) {
               Active = true;
               StartCoroutine(Good());
               StartCoroutine(JustLikeDad(i));
            }
            else {
               Active = true;
               StartCoroutine(Bad());
               Debug.LogFormat("[Chalices #{0}] You selected chalice {1}. Strike...................................................................................................................................................................................................................................................................................................................................................................................................................................................", moduleId, i + 1);
            }
         }
      }
   }

   IEnumerator Good () {
      Audio.PlaySoundAtTransform("Coffee", transform);
      yield return new WaitForSeconds(4f);
      Audio.PlaySoundAtTransform("jnoneeofnw", transform);
      GetComponent<KMBombModule>().HandlePass();
      Debug.LogFormat("[Chalices #{0}] You selected the right chalice again.", moduleId);
   }

   IEnumerator Bad () {
      Audio.PlaySoundAtTransform("Coffee", transform);
      yield return new WaitForSeconds(4f);
      Audio.PlaySoundAtTransform("OH MY GOD!!! WAAAAAAH!!!!", transform);
      GetComponent<KMBombModule>().HandleStrike();
      Active = false;
   }

   IEnumerator JustLikeDad (int Index) {
      yield return new WaitForSeconds(4f);
      SelectablesGameObject[Index].gameObject.SetActive(false);
      ChaliceSpriteRenderer[Index].gameObject.SetActive(false);
   }

   void InnerA () {
      if (Conditions[0]) {
         Debug.LogFormat("[Chalices #{0}] Going to Inner B.", moduleId);
         InnerB();
      }
      else if (BlackConditions[0]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer A.", moduleId);
         OuterA();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer E.", moduleId);
         OuterE();
      }
   }

   void InnerB () {
      if (Conditions[1]) {
         Debug.LogFormat("[Chalices #{0}] Going to Inner G.", moduleId);
         InnerG();
      }
      else if (BlackConditions[1]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer B.", moduleId);
         OuterB();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer A.", moduleId);
         OuterA();
      }
   }

   void InnerC () {
      if (Conditions[2]) {
         Debug.LogFormat("[Chalices #{0}] Going to Inner A.", moduleId);
         InnerA();
      }
      else if (BlackConditions[2]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer E.", moduleId);
         OuterE();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer C.", moduleId);
         OuterC();
      }
   }

   void InnerD () {
      if (Conditions[3]) {
         Debug.LogFormat("[Chalices #{0}] Going to Inner F.", moduleId);
         InnerF();
      }
      else if (BlackConditions[3]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer D.", moduleId);
         OuterD();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer G.", moduleId);
         OuterG();
      }
   }

   void InnerE () {
      if (Conditions[4]) {
         Debug.LogFormat("[Chalices #{0}] Going to Inner H.", moduleId);
         InnerH();
      }
      else if (BlackConditions[4]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer H.", moduleId);
         OuterH();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer F.", moduleId);
         OuterF();
      }
   }

   void InnerF () {
      if (Conditions[5]) {
         Debug.LogFormat("[Chalices #{0}] Going to Inner E.", moduleId);
         InnerE();
      }
      else if (BlackConditions[5]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer F.", moduleId);
         OuterF();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer D.", moduleId);
         OuterD();
      }
   }

   void InnerG () {
      if (Conditions[6]) {
         Debug.LogFormat("[Chalices #{0}] Going to Inner D.", moduleId);
         InnerD();
      }
      else if (BlackConditions[6]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer G.", moduleId);
         OuterG();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer B.", moduleId);
         OuterB();
      }
   }

   void InnerH () {
      if (Conditions[7]) {
         Debug.LogFormat("[Chalices #{0}] Going to Inner C.", moduleId);
         InnerC();
      }
      else if (BlackConditions[7]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer C.", moduleId);
         OuterC();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer H.", moduleId);
         OuterH();
      }
   }

   void OuterA () {
      if (Conditions[0]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer B.", moduleId);
         OuterB();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to X.", moduleId);
         LetterX();
      }
   }

   void OuterB () {
      if (Conditions[1]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer G.", moduleId);
         OuterG();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Outer X.", moduleId);
         LetterX();
      }
   }

   void OuterC () {
      if (Conditions[2]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer E.", moduleId);
         OuterE();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to W.", moduleId);
         LetterW();
      }
   }

   void OuterD () {
      if (Conditions[3]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer F.", moduleId);
         OuterF();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Y.", moduleId);
         LetterY();
      }
   }

   void OuterE () {
      if (Conditions[4]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer A.", moduleId);
         OuterA();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to W.", moduleId);
         LetterW();
      }
   }

   void OuterF () {
      if (Conditions[5]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer H.", moduleId);
         OuterH();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Z.", moduleId);
         LetterZ();
      }
   }

   void OuterG () {
      if (Conditions[6]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer D.", moduleId);
         OuterD();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Y.", moduleId);
         LetterY();
      }
   }

   void OuterH () {
      if (Conditions[7]) {
         Debug.LogFormat("[Chalices #{0}] Going to Outer C.", moduleId);
         OuterC();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to Z.", moduleId);
         LetterZ();
      }
   }

   void LetterW () {
      if (Conditions[16]) {
         Debug.LogFormat("[Chalices #{0}] Going to N.", moduleId);
         LetterN();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to I.", moduleId);
         LetterI();
      }
   }

   void LetterX () {
      if (Conditions[17]) {
         Debug.LogFormat("[Chalices #{0}] Going to P.", moduleId);
         LetterP();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to J.", moduleId);
         LetterJ();
      }
   }

   void LetterY () {
      if (Conditions[18]) {
         Debug.LogFormat("[Chalices #{0}] Going to M.", moduleId);
         LetterM();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to L.", moduleId);
         LetterL();
      }
   }

   void LetterZ () {
      if (Conditions[19]) {
         Debug.LogFormat("[Chalices #{0}] Going to K.", moduleId);
         LetterK();
      }
      else {
         Debug.LogFormat("[Chalices #{0}] Going to O.", moduleId);
         LetterO();
      }
   }

   void LetterI () {
      if (Conditions[8]) {
         AnswerToIsAmazing[2] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 3.", moduleId);
      }
      else {
         AnswerToIsAmazing[1] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 2.", moduleId);
      }
   }

   void LetterJ () {
      if (Conditions[9]) {
         AnswerToIsAmazing[0] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 1.", moduleId);
      }
      else {
         AnswerToIsAmazing[7] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 8.", moduleId);
      }
   }

   void LetterK () {
      if (Conditions[10]) {
         AnswerToIsAmazing[3] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 4.", moduleId);
      }
      else {
         AnswerToIsAmazing[2] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 3.", moduleId);
      }
   }

   void LetterL () {
      if (Conditions[11]) {
         AnswerToIsAmazing[6] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 7.", moduleId);
      }
      else {
         AnswerToIsAmazing[5] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 6.", moduleId);
      }
   }

   void LetterM () {
      if (Conditions[12]) {
         AnswerToIsAmazing[5] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 6.", moduleId);
      }
      else {
         AnswerToIsAmazing[4] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 5.", moduleId);
      }
   }

   void LetterN () {
      if (Conditions[13]) {
         AnswerToIsAmazing[1] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 2.", moduleId);
      }
      else {
         AnswerToIsAmazing[0] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 1.", moduleId);
      }
   }

   void LetterO () {
      if (Conditions[14]) {
         AnswerToIsAmazing[4] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 5.", moduleId);
      }
      else {
         AnswerToIsAmazing[3] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 4.", moduleId);
      }
   }

   void LetterP () {
      if (Conditions[14]) {
         AnswerToIsAmazing[7] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 8.", moduleId);
      }
      else {
         AnswerToIsAmazing[6] = true;
         Debug.LogFormat("[Chalices #{0}] You should drink from chalice 7.", moduleId);
      }
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} # to select the corresponding chalice.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
      Command = Command.Trim();
      for (int i = 1; i < 9; i++) {
         if (Command == i.ToString()) {
            Buttons[i - 1].OnInteract();
            yield break;
         }
      }
      yield return "sendtochaterror Invalid Command!";
   }

   IEnumerator TwitchHandleForcedSolve () {
      for (int i = 0; i < 8; i++)
         if (AnswerToIsAmazing[i])
            Buttons[i].OnInteract();
      yield return null;
   }
}
