using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class TheArena : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public KMSelectable[] EventButtons;
   public GameObject AtkObjects;
   public GameObject DefObjects;
   public GameObject GrbObjects;
   public GameObject[] EventButtonObjects;

   public TextMesh AtkDisplay;
   public TextMesh AtkNum;
   public KMSelectable[] AtkButtons; //Left, Right, Use

   public TextMesh DefEnemies;
   public TextMesh DefTurn;
   public KMSelectable[] DefButtons;

   public TextMesh[] GrbNums;
   public KMSelectable[] GrbButtons;

   private string Order;
   private int currentEvent = 0;
   private List<bool> alreadyLogged = new List<bool> { false, false, false };
   private bool inEvent = false;

   private List<string> atkWeapons = new List<string> { };
   private List<string> atkAnswers = new List<string> { };
   private List<int> atkEx = new List<int> { };
   //private int atkMax = -1;
   private int atkShownWeapon = 0;
   private string atkSubmission = "";
   private bool atkGood = false;

   private string defSeq = "";
   private int defNum = 0;

   private List<int> grbDisp = new List<int> { };
   private List<int> grbFinal = new List<int> { };
   private int grbTimes = 0;
   private List<int> grbSorted = new List<int> { };
   private int grbPresses = 0;
   private List<bool> grbPressed = new List<bool> { false, false, false, false, false, false, false, false, false };

   string TPModeClarifier = "";
   int TPStrikes;

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   void Awake () {
      moduleId = moduleIdCounter++;

      foreach (KMSelectable evB in EventButtons) {
         evB.OnInteract += delegate () { EventButtonPress(evB); return false; };
      }
      foreach (KMSelectable atB in AtkButtons) {
         atB.OnInteract += delegate () { AtkButtonPress(atB); return false; };
      }
      foreach (KMSelectable dfB in DefButtons) {
         dfB.OnInteract += delegate () { DefButtonPress(dfB); return false; };
      }
      foreach (KMSelectable gbB in GrbButtons) {
         gbB.OnInteract += delegate () { GrbButtonPress(gbB); return false; };
      }
   }

   void Start () {
      AtkObjects.SetActive(false);
      DefObjects.SetActive(false);
      GrbObjects.SetActive(false);

      Order = DeduceOrder();
      Debug.LogFormat("[The Arena #{0}] The order of the events is: {1}", moduleId, Order);
      for (int e = 0; e < 3; e++) {
         switch (Order[e]) {
            case 'A': ExecuteAtk(); break;
            case 'D': ExecuteDef(); break;
            case 'G': ExecuteGrb(); break;
         }
      }
   }

   string DeduceOrder () {
      int lit = Bomb.GetOnIndicators().Count();
      int unlit = Bomb.GetOffIndicators().Count();
      int batt = Bomb.GetBatteryCount() / 2;
      int ports = Bomb.GetPortCount() / 2;
      string serial = Bomb.GetSerialNumber();
      int first, second;
      int col = -1; //For some reason I need to give a value to col and row even though I'm pretty certain you do not need to do that. If you know how to fix this, please do let me know.
      int row = -1;
      List<string> table = new List<string> { "ADG", "GAD", "DGA", "DAG", "GAD", "AGD", "ADG", "DGA", "DAG", "GDA", "GAD", "AGD", "DGA", "DAG", "AGD", "GDA" };

      if (lit > 3) { lit = 3; }
      if (unlit > 3) { unlit = 3; }
      if (batt > 3) { batt = 3; }
      if (ports > 3) { ports = 3; }

      if ("0123456789".Contains(serial[0])) { first = 0; }
      else if ("ACEGIKMOQSUWY".Contains(serial[0])) { first = 1; }
      else { first = 2; }

      if ("0123456789".Contains(serial[1])) { second = 0; }
      else if ("ACEGIKMOQSUWY".Contains(serial[1])) { second = 1; }
      else { second = 2; }

      switch (second) {
         case 0:
            switch (first) {
               case 0: col = batt; row = lit; break;
               case 1: col = ports; row = lit; break;
               case 2: col = batt; row = unlit; break;
            }
            break;
         case 1:
            switch (first) {
               case 0: col = ports; row = unlit; break;
               case 1: col = batt; row = unlit; break;
               case 2: col = batt; row = lit; break;
            }
            break;
         case 2:
            switch (first) {
               case 0: col = ports; row = lit; break;
               case 1: col = batt; row = unlit; break;
               case 2: col = ports; row = unlit; break;
            }
            break;
      }

      return table[row * 4 + col];

   }

   void ExecuteAtk () {
      int attempts = 1;
      List<string> weapons = new List<string> { "Katanas", "Sais", "Bo-staff", "Nunchucks", "Battle Axe", "Mace", "Dagger", "Sabre", "Shortsword", "Lance", "Bow", "Ballista", "Kunais", "Catapult", "Trebuchet", "Bombard", "Cannon", "Battering Ram" };
      List<int> letters = new List<int> { 7, 4, 7, 9, 9, 4, 6, 5, 10, 5, 3, 8, 6, 8, 9, 7, 6, 12 };
      List<int> indexes = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
      List<string> chosens = new List<string> { };
      List<string> perms = new List<string> { "12345", "12354", "12435", "12453", "12534", "12543", "13245", "13254", "13425", "13452", "13524", "13542", "14235", "14253", "14325", "14352", "14523", "14532", "15234", "15243", "15324", "15342", "15423", "15432", "21345", "21354", "21435", "21453", "21534", "21543", "23145", "23154", "23415", "23451", "23514", "23541", "24135", "24153", "24315", "24351", "24513", "24531", "25134", "25143", "25314", "25341", "25413", "25431", "31245", "31254", "31425", "31452", "31524", "31542", "32145", "32154", "32415", "32451", "32514", "32541", "34125", "34152", "34215", "34251", "34512", "34521", "35124", "35142", "35214", "35241", "35412", "35421", "41235", "41253", "41325", "41352", "41523", "41532", "42135", "42153", "42315", "42351", "42513", "42531", "43125", "43152", "43215", "43251", "43512", "43521", "45123", "45132", "45213", "45231", "45312", "45321", "51234", "51243", "51324", "51342", "51423", "51432", "52134", "52143", "52314", "52341", "52413", "52431", "53124", "53142", "53214", "53241", "53412", "53421", "54123", "54132", "54213", "54231", "54312", "54321" };
      int maxDmg;
      int nonparadoxes;

      List<string> validAnswers = new List<string> { };

      tryAgain:
      indexes.Shuffle();
      chosens.Clear();
      maxDmg = 0;
      nonparadoxes = 0;

      for (int w = 0; w < 5; w++) {
         chosens.Add(weapons[indexes[w]]);
      }
      for (int p = 0; p < 120; p++) {
         int currentDmg = 0;
         List<int> damage = new List<int> { 0, 0, 0, 0, 0 };
         List<bool> valid = new List<bool> { false, false, false, false, false };
         List<string> actualChosens = new List<string> { };

         actualChosens.Clear();

         for (int x = 0; x < 5; x++) {
            switch (perms[p][x]) {
               case '1': actualChosens.Add(chosens[0]); break;
               case '2': actualChosens.Add(chosens[1]); break;
               case '3': actualChosens.Add(chosens[2]); break;
               case '4': actualChosens.Add(chosens[3]); break;
               case '5': actualChosens.Add(chosens[4]); break;
            }
            damage[x] = 0;
            valid[x] = false;
         }

         for (int c = 0; c < 30; c++) {
            if (!valid[c % 5]) {
               switch (actualChosens[c % 5]) {
                  case "Katanas": //turn
                     damage[c % 5] = (c % 5) + 1;
                     valid[c % 5] = true;
                     break;
                  case "Sais": //letters in prev. weap
                     if (c % 5 == 0) {
                        valid[c % 5] = true;
                     }
                     else {
                        //damage[c%5] = letters[indexes[(c%5)-1]];
                        damage[c % 5] = letters[actualIndexes((c % 5) - 1, perms[p], indexes[0], indexes[1], indexes[2], indexes[3], indexes[4])];
                        valid[c % 5] = true;
                     }
                     break;
                  case "Bo-staff": //prev. weap
                     if (c % 5 == 0) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) - 1]) {
                           damage[c % 5] = damage[(c % 5) - 1];
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Nunchucks": //2 on odd, 5 on even
                     if ((c % 5) % 2 == 0) {
                        damage[c % 5] = 2;
                     }
                     else {
                        damage[c % 5] = 5;
                     }
                     valid[c % 5] = true;
                     break;
                  case "Battle Axe": //sum of all dmg prior
                     switch (c % 5) {
                        case 0:
                           damage[c % 5] = 0;
                           valid[c % 5] = true;
                           break;
                        case 1:
                           if (valid[0]) {
                              damage[c % 5] = damage[0];
                              valid[c % 5] = true;
                           }
                           break;
                        case 2:
                           if (valid[0] && valid[1]) {
                              damage[c % 5] = damage[0] + damage[1];
                              valid[c % 5] = true;
                           }
                           break;
                        case 3:
                           if (valid[0] && valid[1] && valid[2]) {
                              damage[c % 5] = damage[0] + damage[1] + damage[2];
                              valid[c % 5] = true;
                           }
                           break;
                        case 4:
                           if (valid[0] && valid[1] && valid[2] && valid[3]) {
                              damage[c % 5] = damage[0] + damage[1] + damage[2] + damage[3];
                              valid[c % 5] = true;
                           }
                           break;
                     }
                     break;
                  case "Mace": //sum of previous 2 weap
                     if (c % 5 == 0 || c % 5 == 1) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) - 1] && valid[(c % 5) - 2]) {
                           damage[c % 5] = damage[(c % 5) - 1] + damage[(c % 5) - 2];
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Dagger": //prev weap - turn
                     if (c % 5 == 0) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) - 1]) {
                           damage[c % 5] = (damage[(c % 5) - 1]) - ((c % 5) + 1);
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Sabre": //prev weap * -1
                     if (c % 5 == 0) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) - 1]) {
                           damage[c % 5] = (damage[(c % 5) - 1]) * -1;
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Shortsword": //next weap + turn
                     if (c % 5 == 4) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) + 1]) {
                           damage[c % 5] = (damage[(c % 5) + 1]) + ((c % 5) + 1);
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Lance": //prev weap + turn
                     if (c % 5 == 0) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) - 1]) {
                           damage[c % 5] = (damage[(c % 5) - 1]) + ((c % 5) + 1);
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Bow": //next weap * -1
                     if (c % 5 == 4) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) + 1]) {
                           damage[c % 5] = (damage[(c % 5) + 1]) * -1;
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Ballista": //next weap - turn
                     if (c % 5 == 4) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) + 1]) {
                           damage[c % 5] = (damage[(c % 5) + 1]) - ((c % 5) + 1);
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Kunais": //sum of next 2 weap
                     if (c % 5 == 3 || c % 5 == 4) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) + 1] && valid[(c % 5) + 2]) {
                           damage[c % 5] = damage[(c % 5) + 1] + damage[(c % 5) + 2];
                           valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Catapult": //sum of all dmg after
                     switch (c % 5) {
                        case 4:
                           damage[c % 5] = 0;
                           valid[c % 5] = true;
                           break;
                        case 3:
                           if (valid[4]) {
                              damage[c % 5] = damage[4];
                              valid[c % 5] = true;
                           }
                           break;
                        case 2:
                           if (valid[3] && valid[4]) {
                              damage[c % 5] = damage[3] + damage[4];
                              valid[c % 5] = true;
                           }
                           break;
                        case 1:
                           if (valid[2] && valid[3] && valid[4]) {
                              damage[c % 5] = damage[2] + damage[3] + damage[4];
                              valid[c % 5] = true;
                           }
                           break;
                        case 0:
                           if (valid[1] && valid[2] && valid[3] && valid[4]) {
                              damage[c % 5] = damage[1] + damage[2] + damage[3] + damage[4];
                              valid[c % 5] = true;
                           }
                           break;
                     }
                     break;
                  case "Trebuchet": //5 on odd, 2 on even
                     if ((c % 5) % 2 == 0) {
                        damage[c % 5] = 5;
                     }
                     else {
                        damage[c % 5] = 2;
                     }
                     valid[c % 5] = true;
                     break;
                  case "Bombard": //next weap
                     if (c % 5 == 4) {
                        valid[c % 5] = true;
                     }
                     else {
                        if (valid[(c % 5) + 1]) {
                           damage[c % 5] = damage[(c % 5) + 1]; valid[c % 5] = true;
                        }
                     }
                     break;
                  case "Cannon": //letters in next weap
                     if (c % 5 == 4) {
                        valid[c % 5] = true;
                     }
                     else {
                        //damage[c%5] = letters[indexes[(c%5)+1]];
                        damage[c % 5] = letters[actualIndexes((c % 5) + 1, perms[p], indexes[0], indexes[1], indexes[2], indexes[3], indexes[4])];
                        valid[c % 5] = true;
                     }
                     break;
                  case "Battering Ram": //6 - turn
                     damage[c % 5] = 6 - ((c % 5) + 1);
                     valid[c % 5] = true;
                     break;
               }
            }
         }
         if (valid[0] && valid[1] && valid[2] && valid[3] && valid[4]) {
            currentDmg = damage[0] + damage[1] + damage[2] + damage[3] + damage[4];
            if (currentDmg > maxDmg) {
               maxDmg = currentDmg;
               validAnswers.Clear();
               validAnswers.Add(perms[p]);
               atkEx.Clear();
               for (int d = 0; d < 5; d++) {
                  atkEx.Add(damage[d]);
               }
            }
            else if (currentDmg == maxDmg) {
               validAnswers.Add(perms[p]);
            }
            nonparadoxes++;
         }
      }

      if (nonparadoxes == 0 || maxDmg < 10) {
         attempts++;
         goto tryAgain;
      }
      else {
         Debug.LogFormat("<The Arena #{0}> (Attack) Attempts: {1}, Number of solutions: {2}", moduleId, attempts, validAnswers.Count());
         Debug.LogFormat("[The Arena #{0}] (Attack) Weapons are: {1}, {2}, {3}, {4}, {5}", moduleId, chosens[0], chosens[1], chosens[2], chosens[3], chosens[4]);
         Debug.LogFormat("[The Arena #{0}] (Attack) The maximum amount of damage you can achieve is: {1}", moduleId, maxDmg);
         Debug.LogFormat("[The Arena #{0}] (Attack) Example solution: {1}", moduleId, validAnswers[0]);
         Debug.LogFormat("<The Arena #{0}> (Attack) Damage dealt: {1}, {2}, {3}, {4}, {5}", moduleId, atkEx[0], atkEx[1], atkEx[2], atkEx[3], atkEx[4]);
         for (int q = 0; q < 5; q++) {
            atkWeapons.Add(chosens[q]);
         }
         for (int y = 0; y < validAnswers.Count(); y++) {
            atkAnswers.Add(validAnswers[y]);
         }
         AtkDisplay.text = atkWeapons[0];
         AtkNum.text = "[" + maxDmg.ToString() + "]";
      }

   }

   int actualIndexes (int a, string p, int v, int w, int x, int y, int z) {
      char c = p[a];
      switch (c) {
         case '1': return v;
         case '2': return w;
         case '3': return x;
         case '4': return y;
         default: return z;
      }
   }

   void ExecuteDef () {
      int practicallyInfinite = int.MaxValue;
      List<string> enemies = new List<string> { "Bat", "Snake", "Spider", "Cobra", "Scorpion", "Mole", "Creeper", "Goblin", "Golem", "Robo-Mouse", "Skeleton", "Undead Guard", "The Reaper", "The Mole’s Dad" };
      List<string> patterns = new List<string> { "AIAIAIAIAI", "AIIAAIIAAI", "IIAAIIIAAI", "IAAIIAAIIA", "AAIAAIAAIA", "IIIIIIIIII", "IIIAIIIAII", "AAIAIIIAAI", "AAAAAIIIII", "AAAIAIAAAI", "IAIIAIAAAI", "AAIAIIAAIA", "IIIIIIIIIA", "AAAAAAAAAA" };
      int[][] stats = new int[][] {
            new int[] { 3, 3, 0, 1, 9 },
            new int[] { 4, 4, 1, 4, 1 },
            new int[] { 2, 2, 3, 8, 10 },
            new int[] { 5, 6, 4, 9, 2 },
            new int[] { 6, 5, 2, 7, 3 },
            new int[] { 1, practicallyInfinite, 5, 2, 11 },
            new int[] { 13, 7, 6, 5, 4 },
            new int[] { 8, 9, 7, 3, 5 },
            new int[] { 7, 11, 8, 10, 6 },
            new int[] { 10, 1, -1, 6, 13 },
            new int[] { 9, 10, 9, 13, 12 },
            new int[] { 12, 8, 10, 11, 14 },
            new int[] { 0, 12, 11, 0, 7 },
            new int[] { 11, 0, 12, 12, 8 }
        };
      List<int> indexes = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
      List<string> chosens = new List<string> { };
      int[][] chosenStats = new int[][] {
            new int[] { 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0, 0 }
        };
      int first, second, third, fourth, fifth;
      string seq;

      indexes.Shuffle();
      for (int m = 0; m < 5; m++) {
         chosens.Add(enemies[indexes[m]]);
         chosenStats[m][0] = stats[indexes[m]][0];
         chosenStats[m][1] = stats[indexes[m]][1];
         chosenStats[m][2] = stats[indexes[m]][2];
         chosenStats[m][3] = stats[indexes[m]][3];
         chosenStats[m][4] = stats[indexes[m]][4];
      }
      DefEnemies.text = chosens[0] + "\n" + chosens[1] + "\n" + chosens[2] + "\n" + chosens[3] + "\n" + chosens[4];

      first = Biggest(chosenStats[0][0], chosenStats[1][0], chosenStats[2][0], chosenStats[3][0], chosenStats[4][0]);
      second = Biggest(chosenStats[0][1], chosenStats[1][1], chosenStats[2][1], chosenStats[3][1], chosenStats[4][1]);
      third = Biggest(chosenStats[0][2], chosenStats[1][2], chosenStats[2][2], chosenStats[3][2], chosenStats[4][2]);
      fourth = Biggest(chosenStats[0][3], chosenStats[1][3], chosenStats[2][3], chosenStats[3][3], chosenStats[4][3]);
      fifth = Biggest(chosenStats[0][4], chosenStats[1][4], chosenStats[2][4], chosenStats[3][4], chosenStats[4][4]);

      seq = patterns[indexes[first]][0].ToString() + patterns[indexes[second]][1] + patterns[indexes[third]][2] + patterns[indexes[fourth]][3] + patterns[indexes[fifth]][4] + patterns[indexes[first]][5] + patterns[indexes[second]][6] + patterns[indexes[third]][7] + patterns[indexes[fourth]][8] + patterns[indexes[fifth]][9];

      Debug.LogFormat("[The Arena #{0}] (Defend) Enemies are: {1}, {2}, {3}, {4}, {5}", moduleId, chosens[0], chosens[1], chosens[2], chosens[3], chosens[4]);
      Debug.LogFormat("[The Arena #{0}] (Defend) The enemy with the largest first stat is {1}", moduleId, enemies[indexes[first]]);
      Debug.LogFormat("[The Arena #{0}] (Defend) The enemy with the largest second stat is {1}", moduleId, enemies[indexes[second]]);
      Debug.LogFormat("[The Arena #{0}] (Defend) The enemy with the largest third stat is {1}", moduleId, enemies[indexes[third]]);
      Debug.LogFormat("[The Arena #{0}] (Defend) The enemy with the largest fourth stat is {1}", moduleId, enemies[indexes[fourth]]);
      Debug.LogFormat("[The Arena #{0}] (Defend) The enemy with the largest fifth stat is {1}", moduleId, enemies[indexes[fifth]]);
      Debug.LogFormat("[The Arena #{0}] (Defend) The final sequence is {1}", moduleId, seq);

      defSeq = seq;
   }

   int Biggest (int a, int b, int c, int d, int e) {
      if (a > b && a > c && a > d && a > e) {
         return 0;
      }
      else if (b > a && b > c && b > d && b > e) {
         return 1;
      }
      else if (c > a && c > b && c > d && c > e) {
         return 2;
      }
      else if (d > a && d > b && d > c && d > e) {
         return 3;
      }
      else {
         return 4;
      }
   }

   void ExecuteGrb () {
      List<int> nums = new List<int> { };
      for (int n = 0; n < 9; n++) {
         nums.Add(UnityEngine.Random.Range(0, 90) + 10); //10-99
         GrbNums[n].text = nums[n].ToString();
         grbDisp.Add(nums[n]);
      }
      string serial = Bomb.GetSerialNumber();
      int op = 0;
      int look = 5;

      Debug.LogFormat("[The Arena #{0}] (Grab) Numbers are: {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}", moduleId, nums[0], nums[1], nums[2], nums[3], nums[4], nums[5], nums[6], nums[7], nums[8]);

      while (op == 0) {
         switch (serial[look]) {
            case '1': op = 1; break;
            case '2': op = 2; break;
            case '3': op = 3; break;
            case '4': op = 4; break;
            case '5': op = 5; break;
            case '6': op = 6; break;
            case '7': op = 7; break;
            case '8': op = 8; break;
            case '9': op = 9; break;
            default:
               if (look == 0) {
                  op = 9;
               }
               else {
                  look--;
               }
               break;
         }
      }

      switch (op) {
         case 1:
            AddAll(nums[6] + nums[0], nums[0] + nums[2], nums[1] + nums[5], nums[8] + nums[8], nums[4] + nums[6], nums[3] + nums[1], nums[7] + nums[4], nums[2] + nums[7], nums[5] + nums[3]);
            break;
         case 2:
            AddAll(nums[5] - nums[7], nums[3] - nums[6], nums[8] - nums[3], nums[7] - nums[0], nums[2] - nums[2], nums[6] - nums[4], nums[0] - nums[5], nums[4] - nums[1], nums[1] - nums[8]);
            break;
         case 3:
            AddAll(nums[4] * nums[8], nums[7] * nums[1], nums[2] * nums[4], nums[0] * nums[7], nums[5] * nums[5], nums[1] * nums[3], nums[8] * nums[6], nums[3] * nums[0], nums[6] * nums[2]);
            break;
         case 4: //5,2   1,0     0,4     3,3     6,5     7,8     4,7     8,1     2,6
            AddAll(big(nums[5], nums[2]) / smol(nums[5], nums[2]), big(nums[1], nums[0]) / smol(nums[1], nums[0]), big(nums[0], nums[4]) / smol(nums[0], nums[4]),
            big(nums[3], nums[3]) / smol(nums[3], nums[3]), big(nums[6], nums[5]) / smol(nums[6], nums[5]), big(nums[7], nums[8]) / smol(nums[7], nums[8]),
            big(nums[4], nums[7]) / smol(nums[4], nums[7]), big(nums[8], nums[1]) / smol(nums[8], nums[1]), big(nums[2], nums[6]) / smol(nums[2], nums[6]));
            break;
         case 5: //4,3   8,5     3,7     2,1     0,0     5,6     6,8     1,4     7,2
            AddAll(big(nums[4], nums[3]) % smol(nums[4], nums[3]), big(nums[8], nums[5]) % smol(nums[8], nums[5]), big(nums[3], nums[7]) % smol(nums[3], nums[7]),
            big(nums[2], nums[1]) % smol(nums[2], nums[1]), big(nums[0], nums[0]) % smol(nums[0], nums[0]), big(nums[5], nums[6]) % smol(nums[5], nums[6]),
            big(nums[6], nums[8]) % smol(nums[6], nums[8]), big(nums[1], nums[4]) % smol(nums[1], nums[4]), big(nums[7], nums[2]) % smol(nums[7], nums[2]));
            break;
         case 6: //6,1   2,8     7,6     1,2     4,4     8,7     5,0     0,3     3,5
            AddAll(nums[6] * 100 + nums[1], nums[2] * 100 + nums[8], nums[7] * 100 + nums[6],
            nums[1] * 100 + nums[2], nums[4] * 100 + nums[4], nums[8] * 100 + nums[7],
            nums[5] * 100 + nums[0], nums[0] * 100 + nums[3], nums[3] * 100 + nums[5]);
            break;
         case 7: //0,6   3,8     4,2     2,5     7,3     6,7     1,1     5,4     8,0
            AddAll(DR(nums[0]) * DR(nums[6]), DR(nums[3]) * DR(nums[8]), DR(nums[4]) * DR(nums[2]),
            DR(nums[2]) * DR(nums[5]), DR(nums[7]) * DR(nums[3]), DR(nums[6]) * DR(nums[7]),
            DR(nums[1]) * DR(nums[1]), DR(nums[5]) * DR(nums[4]), DR(nums[8]) * DR(nums[0]));
            break;
         case 8: //8,4   6,3     2,0     1,6     5,8     0,1     3,2     7,7     4,5
            AddAll(lunA(nums[8], nums[4]), lunA(nums[6], nums[3]), lunA(nums[2], nums[0]),
            lunA(nums[1], nums[6]), lunA(nums[5], nums[8]), lunA(nums[0], nums[1]),
            lunA(nums[3], nums[2]), lunA(nums[7], nums[7]), lunA(nums[4], nums[5]));
            break;
         case 9: //7,5   1,7     5,1     3,4     8,2     4,0     2,3     6,6     0,8
            AddAll(lunM(nums[7], nums[5]), lunM(nums[1], nums[7]), lunM(nums[5], nums[1]),
            lunM(nums[3], nums[4]), lunM(nums[8], nums[2]), lunM(nums[4], nums[0]),
            lunM(nums[2], nums[3]), lunM(nums[6], nums[6]), lunM(nums[0], nums[8]));
            break;
      }
      Debug.LogFormat("[The Arena #{0}] (Grab) Money amounts are: {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}", moduleId, grbFinal[0], grbFinal[1], grbFinal[2], grbFinal[3], grbFinal[4], grbFinal[5], grbFinal[6], grbFinal[7], grbFinal[8]);

      grbTimes = UnityEngine.Random.Range(0, 7) + 3;
      for (int w = 0; w < 9; w++) {
         grbSorted.Add(grbFinal[w]);
      }
      grbSorted.Sort();

      Debug.LogFormat("[The Arena #{0}] (Grab) Amounts from highest to lowest are: {9}, {8}, {7}, {6}, {5}, {4}, {3}, {2}, {1}", moduleId, grbSorted[0], grbSorted[1], grbSorted[2], grbSorted[3], grbSorted[4], grbSorted[5], grbSorted[6], grbSorted[7], grbSorted[8]);
   }

   void AddAll (int a, int b, int c, int d, int e, int f, int g, int h, int i) {
      grbFinal.Add(a);
      grbFinal.Add(b);
      grbFinal.Add(c);
      grbFinal.Add(d);
      grbFinal.Add(e);
      grbFinal.Add(f);
      grbFinal.Add(g);
      grbFinal.Add(h);
      grbFinal.Add(i);
   }

   int big (int x, int y) {
      if (x >= y) {
         return x;
      }
      else {
         return y;
      }
   }

   int smol (int x, int y) {
      if (x >= y) {
         return y;
      }
      else {
         return x;
      }
   }

   int DR (int q) {
      return ((q - 1) % 9) + 1;
   }

   // <Danny's Lunar Arithmetic functions>

   int lunA (int first, int second) {
      int maxLength = Math.Max(first.ToString().Length, second.ToString().Length);
      string A = first.ToString().PadLeft(maxLength, '0');
      string B = second.ToString().PadLeft(maxLength, '0');
      int result = 0;
      for (int i = 0; i < maxLength; i++) {
         result *= 10;
         result += Math.Max(A[i], B[i]) - '0';
      }
      return result;
   }
   int lunM (int first, int second) {
      int maxLength = Math.Max(first.ToString().Length, second.ToString().Length);
      string A = first.ToString().PadLeft(maxLength, '0');
      string B = second.ToString().PadLeft(maxLength, '0');
      int append = 0;
      Stack<int> adders = new Stack<int>();
      for (int i = A.Length - 1; i >= 0; i--) {
         string result = "";
         for (int j = B.Length - 1; j >= 0; j--)
            result = Math.Min(A[j], B[i]) - '0' + result;
         for (int k = 0; k < append; k++)
            result += '0';
         append++;
         adders.Push(int.Parse(result));
      }
      Debug.Log(adders.Join());
      while (adders.Count > 1)
         adders.Push(lunA(adders.Pop(), adders.Pop()));
      return adders.First();
   }

   // </Danny's Lunar Arithmetic functions>

   void EventButtonPress (KMSelectable evB) {
      evB.AddInteractionPunch();
      GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
      for (int e = 0; e < 3; e++) {
         if (evB == EventButtons[e] && !inEvent) {
            switch (e) {
               case 0:
                  if (Order[currentEvent] == 'A') {
                     if (!alreadyLogged[0]) {
                        TPModeClarifier = "ATT";
                        AtkObjects.SetActive(true);
                        Debug.LogFormat("[The Arena #{0}] You went to the Attack event, this is correct.", moduleId);
                        inEvent = true;
                        alreadyLogged[0] = true;
                     }
                  }
                  else {
                     GetComponent<KMBombModule>().HandleStrike();
                     Debug.LogFormat("[The Arena #{0}] You went to the Attack event, this is incorrect. You should go to {1} instead. Strike!", moduleId, (Order[currentEvent] == 'D') ? "Defend" : "Grab");
                  }
                  break;
               case 1:
                  if (Order[currentEvent] == 'D') {
                     if (!alreadyLogged[1]) {
                        TPModeClarifier = "DEF";
                        DefObjects.SetActive(true);
                        Debug.LogFormat("[The Arena #{0}] You went to the Defend event, this is correct.", moduleId);
                        inEvent = true;
                        alreadyLogged[1] = true;
                     }
                  }
                  else {
                     GetComponent<KMBombModule>().HandleStrike();
                     Debug.LogFormat("[The Arena #{0}] You went to the Defend event, this is incorrect. You should go to {1} instead. Strike!", moduleId, (Order[currentEvent] == 'A') ? "Attack" : "Grab");
                  }
                  break;
               case 2:
                  if (Order[currentEvent] == 'G') {
                     if (!alreadyLogged[2]) {
                        TPModeClarifier = "GRA";
                        GrbObjects.SetActive(true);
                        Debug.LogFormat("[The Arena #{0}] You went to the Grab event, this is correct.", moduleId);
                        inEvent = true;
                        alreadyLogged[2] = true;
                     }
                  }
                  else {
                     GetComponent<KMBombModule>().HandleStrike();
                     Debug.LogFormat("[The Arena #{0}] You went to the Grab event, this is incorrect. You should go to {1} instead. Strike!", moduleId, (Order[currentEvent] == 'A') ? "Attack" : "Defend");
                  }
                  break;
            }
         }
      }
   }

   void AtkButtonPress (KMSelectable atB) {
      atB.AddInteractionPunch();
      GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
      for (int a = 0; a < 3; a++) {
         if (atB == AtkButtons[a]) {
            switch (a) {
               case 0:
                  atkShownWeapon = (atkShownWeapon + 4) % 5;
                  AtkDisplay.text = atkWeapons[atkShownWeapon];
                  break;
               case 1:
                  atkShownWeapon = (atkShownWeapon + 1) % 5;
                  AtkDisplay.text = atkWeapons[atkShownWeapon];
                  break;
               case 2:
                  atkSubmission = atkSubmission + "12345"[atkShownWeapon];
                  if (atkSubmission.Length == 5) {
                     for (int o = 0; o < atkAnswers.Count(); o++) {
                        if (atkSubmission == atkAnswers[o]) {
                           atkGood = true;
                        }
                     }
                     if (!atkGood) {
                        Debug.LogFormat("[The Arena #{0}] You submitted {1}, this is incorrect. Strike!", moduleId, atkSubmission);
                        GetComponent<KMBombModule>().HandleStrike();
                        atkSubmission = "";
                     }
                     else {
                        Debug.LogFormat("[The Arena #{0}] You submitted {1}, this is correct.", moduleId, atkSubmission);
                        EventButtonObjects[0].SetActive(false);
                        AtkObjects.SetActive(false);
                        currentEvent++;
                        inEvent = false;
                        TPModeClarifier = "";
                        if (currentEvent == 3) {
                           Debug.LogFormat("[The Arena #{0}] All events complete, module solved.", moduleId);
                           GetComponent<KMBombModule>().HandlePass();
                           moduleSolved = true;
                        }
                     }
                  }
                  break;
            }
         }
      }
   }

   void DefButtonPress (KMSelectable dfB) {
      dfB.AddInteractionPunch();
      GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
      if (dfB == DefButtons[0]) {
         if (defSeq[defNum] == 'I') {
            Debug.LogFormat("[The Arena #{0}] You pressed Sword for turn {1}, this is correct.", moduleId, defNum + 1);
            defNum++;
            DefTurn.text = "#" + (defNum + 1);
            if (defNum == 10) {
               EventButtonObjects[1].SetActive(false);
               DefObjects.SetActive(false);
               currentEvent++;
               inEvent = false;
               TPModeClarifier = "";
               if (currentEvent == 3) {
                  Debug.LogFormat("[The Arena #{0}] All events complete, module solved.", moduleId);
                  GetComponent<KMBombModule>().HandlePass();
                  moduleSolved = true;
               }
            }
         }
         else {
            Debug.LogFormat("[The Arena #{0}] You pressed Sword for turn {1}, this is incorrect. Strike!", moduleId, defNum + 1);
            TPStrikes++;
            GetComponent<KMBombModule>().HandleStrike();
         }
      }
      else {
         if (defSeq[defNum] == 'A') {
            Debug.LogFormat("[The Arena #{0}] You pressed Shield for turn {1}, this is correct.", moduleId, defNum + 1);
            defNum++;
            DefTurn.text = "#" + (defNum + 1);
            if (defNum == 10) {
               EventButtonObjects[1].SetActive(false);
               DefObjects.SetActive(false);
               currentEvent++;
               inEvent = false;
               TPModeClarifier = "";
               if (currentEvent == 3) {
                  Debug.LogFormat("[The Arena #{0}] All events complete, module solved.", moduleId);
                  GetComponent<KMBombModule>().HandlePass();
                  moduleSolved = true;
               }
            }
         }
         else {
            Debug.LogFormat("[The Arena #{0}] You pressed Shield for turn {1}, this is incorrect. Strike!", moduleId, defNum + 1);
            TPStrikes++;
            GetComponent<KMBombModule>().HandleStrike();
         }
      }
   }

   void GrbButtonPress (KMSelectable gbB) {
      gbB.AddInteractionPunch();
      GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
      for (int g = 0; g < 9; g++) {
         if (gbB == GrbButtons[g]) {
            if (grbPressed[g]) {
               return;
            }
            if (grbSorted[8 - grbPresses] == grbFinal[g]) {
               Debug.LogFormat("[The Arena #{0}] You pressed {1} (the button that says \"{2}\"), this is correct.", moduleId, Pos(g), grbDisp[g]);
               grbPresses++;
               grbPressed[g] = true;
               if (grbPresses == grbTimes) {
                  EventButtonObjects[2].SetActive(false);
                  GrbObjects.SetActive(false);
                  currentEvent++;
                  inEvent = false;
                  TPModeClarifier = "";
                  if (currentEvent == 3) {
                     Debug.LogFormat("[The Arena #{0}] All events complete, module solved.", moduleId);
                     GetComponent<KMBombModule>().HandlePass();
                     moduleSolved = true;
                  }
               }
            }
            else {
               Debug.LogFormat("[The Arena #{0}] You pressed {1} (the button that says \"{2}\"), this is incorrect. Strike!", moduleId, Pos(g), grbDisp[g]);
               GetComponent<KMBombModule>().HandleStrike();
            }
         }
      }
   }

   string Pos (int k) {
      switch (k) {
         case 0: return "top left";
         case 1: return "top middle";
         case 2: return "top right";
         case 3: return "middle left";
         case 4: return "middle center";
         case 5: return "middle right";
         case 6: return "bottom left";
         case 7: return "bottom middle";
         case 8: return "bottom right";
         default: throw new ArgumentOutOfRangeException("k");
      }
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} A/D/G to press Attack/Defend/Grab. Attack: Use !{0} cycle to view all weapons. Use !{0} <Weapon> to use that specific weapon. This is not chainable. Defend: Use !{0} Hit/Block/H/B to press that button, chain with single letters such as HBHBHBHB. Grab: Use !{0} ABC123 to hit that specific button. This is not chainable.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      Command = Command.Trim().ToUpper();
      yield return null;
      switch (TPModeClarifier) {
         case "ATT":
            if (Command == "CYCLE") {
               for (int i = 0; i < 5; i++) {
                  AtkButtons[1].OnInteract();
                  yield return new WaitForSecondsRealtime(1f);
               }
            }
            else {
               for (int i = 0; i < 5; i++) {
                  AtkButtons[1].OnInteract();
                  if (Command == AtkDisplay.text.ToUpper()) {
                     AtkButtons[2].OnInteract();
                     yield break;
                  }
                  yield return new WaitForSecondsRealtime(.1f);
               }
               yield return "sendtochaterror I don't understand!";
            }
            break;
         case "DEF":
            int Temp = TPStrikes;
            if (Command == "HIT" || Command == "H") {
               DefButtons[0].OnInteract();
               yield return new WaitForSecondsRealtime(.1f);
            }
            else if (Command == "BLOCK" || Command == "B") {
               DefButtons[1].OnInteract();
               yield return new WaitForSecondsRealtime(.1f);
            }
            else {
               for (int i = 0; i < Command.Length; i++) {
                  if (Command[i] != 'H' && Command[i] != 'B') {
                     yield return "sendtochaterror I don't understand!";
                     yield break;
                  }
               }
               for (int i = 0; i < Command.Length; i++) {
                  if (Command[i] == 'H') {
                     DefButtons[0].OnInteract();
                  }
                  else {
                     DefButtons[1].OnInteract();
                  }
                  yield return new WaitForSeconds(.1f);
                  if (TPStrikes > Temp) {
                     yield return "sendtochaterror A strike has been detecting. Ending operation.";
                     yield break;
                  }
               }
            }
            break;
         case "GRA":
            if (Command.Length != 2) {
               yield return "sendtochaterror I don't understand!";
               yield break;
            }
            int IndexOfTPButton = 0;
            switch (Command[0]) {
               case 'A':
                  //IndexOfTPButton++;
                  break;
               case 'B':
                  IndexOfTPButton++;
                  break;
               case 'C':
                  IndexOfTPButton += 2;
                  break;
               default:
                  yield return "sendtochaterror I don't understand!";
                  break;
            }
            switch (Command[1]) {
               case '1':
                  //IndexOfTPButton++;
                  break;
               case '2':
                  IndexOfTPButton += 3;
                  break;
               case '3':
                  IndexOfTPButton += 6;
                  break;
               default:
                  yield return "sendtochaterror I don't understand!";
                  break;
            }
            if (grbPressed[IndexOfTPButton]) {
               yield return "sendtochaterror You already pressed that button!";
               break;
            }
            GrbButtons[IndexOfTPButton].OnInteract();
            yield return new WaitForSecondsRealtime(.1f);
            break;
         default:
            switch (Command) {
               case "A":
                  EventButtons[0].OnInteract();
                  yield return new WaitForSecondsRealtime(.1f);
                  break;
               case "D":
                  EventButtons[1].OnInteract();
                  yield return new WaitForSecondsRealtime(.1f);
                  break;
               case "G":
                  EventButtons[2].OnInteract();
                  yield return new WaitForSecondsRealtime(.1f);
                  break;
            }
            break;
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      while (!moduleSolved) {
         switch (TPModeClarifier) {
            case "ATT":
               for (int i = 0; i < 5; i++) {
                  while (atkShownWeapon != int.Parse(atkAnswers[0][i].ToString()) - 1) {
                     AtkButtons[1].OnInteract();
                     yield return new WaitForSecondsRealtime(.1f);
                  }
                  AtkButtons[2].OnInteract();
                  yield return new WaitForSecondsRealtime(.1f);
               }
               break;
            case "DEF":
               while (defNum != 10) {
                  if (defSeq[defNum] == 'A') {
                     DefButtons[1].OnInteract();
                     yield return new WaitForSecondsRealtime(.1f);
                  }
                  else {
                     DefButtons[0].OnInteract();
                     yield return new WaitForSecondsRealtime(.1f);
                  }
               }
               break;
            case "GRA":
               for (int i = 0; i < grbTimes; i++) {
                  for (int j = 0; j < 9; j++) {
                     if (TPModeClarifier == "") {
                        break;
                     }
                     if (grbSorted[8 - grbPresses] == grbFinal[j]) {
                        GrbButtons[j].OnInteract();
                        yield return new WaitForSecondsRealtime(.1f);
                     }
                  }
               }
               break;
            default:
               switch (Order[currentEvent]) {
                  case 'A':
                     EventButtons[0].OnInteract();
                     yield return new WaitForSecondsRealtime(.1f);
                     break;
                  case 'D':
                     EventButtons[1].OnInteract();
                     yield return new WaitForSecondsRealtime(.1f);
                     break;
                  case 'G':
                     EventButtons[2].OnInteract();
                     yield return new WaitForSecondsRealtime(.1f);
                     break;
               }
               break;
         }
      }
   }
}
