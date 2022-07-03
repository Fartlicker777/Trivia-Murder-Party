using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Patterns : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public KMSelectable[] Cards;
   public Sprite[] Symbols;
   public SpriteRenderer[] CardSymbols;
   public Sprite[] WhitePeopleBeLike;
   public Material[] Colors;

   int[] GridColor = {
      0, 3, 5, 1, 2, 0, 6, 7, 0, 5,
      6, 3, 6, 7, 2, 5, 7, 4, 1, 4,
      1, 1, 2, 1, 5, 7, 1, 5, 3, 0,
      6, 1, 1, 2, 2, 3, 6, 1, 3, 7,
      5, 2, 4, 6, 3, 1, 6, 1, 2, 6,
      5, 0, 2, 4, 1, 2, 4, 7, 5, 3,
      3, 5, 2, 4, 3, 4, 5, 0, 5, 5,
      2, 5, 6, 0, 2, 4, 5, 3, 7, 5,
      4, 1, 5, 5, 2, 2, 1, 3, 5, 0,
      5, 5, 7, 6, 0, 5, 6, 5, 1, 7
   };
   int[] GridSymbols = {
      2, 2, 0, 0, 0, 1, 1, 0, 1, 2,
      2, 0, 0, 1, 2, 0, 1, 0, 1, 1,
      1, 2, 2, 0, 2, 1, 0, 0, 2, 2,
      1, 0, 2, 1, 0, 1, 1, 2, 0, 2,
      2, 2, 2, 0, 2, 1, 2, 1, 0, 2,
      0, 2, 2, 0, 0, 0, 2, 2, 0, 0,
      1, 2, 0, 2, 1, 1, 2, 0, 1, 2,
      2, 0, 2, 0, 1, 2, 1, 0, 1, 0,
      0, 2, 2, 1, 0, 1, 0, 1, 1, 2,
      2, 0, 1, 1, 1, 2, 0, 1, 0, 0
   };
   int[][] SelectedCards = {
      new int[2] { 0, 0},
      new int[2] { 0, 0},
      new int[2] { 0, 0},
      new int[2] { 0, 0},
      new int[2] { 0, 0}
   };
   int StartIndex;

   bool[] Pressed = new bool[5];

   enum Directions {
      Up,
      Right,
      Down,
      Left
   }
   List<Directions> ValidDirections = new List<Directions> { };
   int CorrectPresses;
   int DirectionProgression;

   float[] XScales = { 0.1068979f, 0.1021792f, 0.2589611f };
   float[] YScales = { 0.06551633f, 0.0626243f, 0.1587139f };

   //Logging
   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   void Awake () {
      moduleId = moduleIdCounter++;

      foreach (var Card in Cards) {
         Card.OnInteract += delegate () { CardPress(Array.IndexOf(Cards, Card)); return false; };
      }
   }

   void CardPress (int Index) {
      if (moduleSolved) {
         Audio.PlaySoundAtTransform("Flash", Cards[Index].transform);
         return;
      }
      if (SelectedCards[Index][0] == GridColor[StartIndex] && SelectedCards[Index][1] == GridSymbols[StartIndex] && !Pressed[Index]) {
         Audio.PlaySoundAtTransform(Rnd.Range(0, 100) != 99 ? "Flash" : "Squeak", Cards[Index].transform);
         StartIndex += DirectionProgression;
         CorrectPresses++;
         Pressed[Index] = true;
         Cards[Index].GetComponent<MeshRenderer>().material = Colors[0];
         CardSymbols[Index].GetComponent<SpriteRenderer>().sprite = null;
         if (CorrectPresses == 5) {
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
         }
      }
      else {
         GetComponent<KMBombModule>().HandleStrike();
         Reset();
      }
   }

   // Use this for initialization
   void Start () {
      Reset();
      //StartCoroutine(Spin());
   }

   void Reset () {
      CorrectPresses = 0;
      for (int i = 0; i < 5; i++) {
         Pressed[i] = false;
      }
      ChooseDirection();
      DisplayCards();
      Log();
   }

   void ChooseDirection () {
      ValidDirections.Clear();
      do {
         StartIndex = Rnd.Range(0, 100);
         /*if (StartIndex % 10 > 3) {
         ValidDirections.Add(Directions.Left);
         }*/
         if (StartIndex % 10 < 6) {
            ValidDirections.Add(Directions.Right);
         }
         /*if (StartIndex / 10 > 3) {
            ValidDirections.Add(Directions.Up);
         }*/
         if (StartIndex / 10 < 6) {
            ValidDirections.Add(Directions.Down);
         }
      } while (ValidDirections.Count() == 0);
      ValidDirections.Shuffle();
      switch (ValidDirections[0]) {
         /*case Directions.Up:
            for (int i = 0; i < 5; i++) {
               SelectedCards[i][0] = GridColor[StartIndex - i * 10];
               SelectedCards[i][1] = GridSymbols[StartIndex - i * 10];
               DirectionProgression = -10;
            }
            break;*/
         case Directions.Right:
            for (int i = 0; i < 5; i++) {
               SelectedCards[i][0] = GridColor[StartIndex + i];
               SelectedCards[i][1] = GridSymbols[StartIndex + i];
               DirectionProgression = 1;
            }
            break;
         case Directions.Down:
            for (int i = 0; i < 5; i++) {
               SelectedCards[i][0] = GridColor[StartIndex + i * 10];
               SelectedCards[i][1] = GridSymbols[StartIndex + i * 10];
               DirectionProgression = 10;
            }
            break;
         /*case Directions.Left:
            for (int i = 0; i < 5; i++) {
               SelectedCards[i][0] = GridColor[StartIndex - i];
               SelectedCards[i][1] = GridSymbols[StartIndex - i];
               DirectionProgression = -1;
            }
            break;*/
         default:
            break;
      }
      SelectedCards.Shuffle();
   }

   void DisplayCards () {
      for (int i = 0; i < 5; i++) {
         Cards[i].GetComponent<MeshRenderer>().material = Colors[SelectedCards[i][0]];
         CardSymbols[i].GetComponent<SpriteRenderer>().sprite = SelectedCards[i][0] == 0 ? WhitePeopleBeLike[SelectedCards[i][1]] : Symbols[SelectedCards[i][1]];
         CardSymbols[i].transform.localScale = new Vector3(XScales[SelectedCards[i][1]], YScales[SelectedCards[i][1]], 1f);
      }
   }

   void Log () {
      Debug.LogFormat("[Patterns #{0}] The starting coordinate is at {1}, going {2}.", moduleId, "ABCDEFGHJI"[StartIndex % 10].ToString() + (StartIndex / 10 + 1), ValidDirections[0]);
   }

   //Unused but was funny to see
   /*IEnumerator Spin () {
      for (int j = 0; j < 30; j++) {
         for (int i = 0; i < 5; i++) {
            Cards[i].transform.Rotate(0, -1f, 0);
         }
         yield return null;
      }
      while (true) {
         for (int j = 0; j < 60; j++) {
            for (int i = 0; i < 5; i++) {
               Cards[i].transform.Rotate(0, 1f, 0);
            }
            yield return null;
         }
         if (moduleSolved) {
            goto CCW;
         }
         yield return new WaitForSeconds(.05f);
         for (int j = 0; j < 60; j++) {
            for (int i = 0; i < 5; i++) {
               Cards[i].transform.Rotate(0, -1f, 0);
            }
            yield return null;
         }
         yield return new WaitForSeconds(.05f);
         if (moduleSolved) {
            goto CW;
         }
      }

      CCW:
      for (int j = 0; j < 30; j++) {
         for (int i = 0; i < 5; i++) {
            Cards[i].transform.Rotate(0, -1f, 0);
         }
         yield return null;
      }
      goto End;
      CW:
      for (int j = 0; j < 30; j++) {
         for (int i = 0; i < 5; i++) {
            Cards[i].transform.Rotate(0, -1f, 0);
         }
         yield return null;
      }
      End:
      int q = 0;
   }*/

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} # to press that card in reading order. You can chain by appending.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.ToUpper().Trim();
      yield return null;
      for (int i = 0; i < Command.Length; i++) {
         if (!"12345".Contains(Command[i].ToString())) {
            yield return "sendtochaterror I don't understand!";
            yield break;
         }
      }
      for (int i = 0; i < Command.Length; i++) {
         Cards[int.Parse(Command[i].ToString()) - 1].OnInteract();
         yield return new WaitForSeconds(.1f);
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      while (CorrectPresses != 5) {
         for (int i = 0; i < 5; i++) {
            if (SelectedCards[i][0] == GridColor[StartIndex] && SelectedCards[i][1] == GridSymbols[StartIndex] && !Pressed[i]) {
               yield return ProcessTwitchCommand((i + 1).ToString());
            }
         }
      }
   }
}