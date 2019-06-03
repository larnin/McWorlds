using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MCWorlds
{
    public class script
    {
        struct etageIf
        {
            public int niveauBoucle;
            public bool findElse;
        }
        public Thread threadPrincipal;

        public Level lvl;
        public List<baseElements> listeAction;
        public List<variable> listVar;
        public int elementActuel;
        public Player pl;
        public bool uniquePl;

        public script() // création du script
        { 
            pl = null;
            elementActuel = 0;

        }

        public string load(string fichier, Level level)
        {
            if (!File.Exists("extra/script/" + fichier + ".mcwscr"))
            { return "Fichier '" + fichier + "' introuvable"; }

            string[] allLines = File.ReadAllLines("extra/script/" + fichier + ".mcwscr");

    #region == TEST_IF_BOUCLE == 
            //test si les boucles et les if sont bien construit

            List<int> eBList = new List<int>();
            List<etageIf> eIList = new List<etageIf>();
            int eBNivIf;
            etageIf eI;

            char carVar;

            int etageIfActuel = 0, etageBoucleActuel = 0;
            
            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i] == "") { continue; }
                switch (allLines[i].Split(' ')[0])
                {
                    case "if":
                        etageIfActuel++;
                        eI.niveauBoucle = etageBoucleActuel;
                        eI.findElse = false;
                        eIList.Add(eI);
                        break;
                    case "else":
                        if (eIList[eIList.Count - 1].findElse == true)
                        { return "Ligne " + i + " : Multiples else";}
                        eI = eIList[eIList.Count - 1];
                        eIList.RemoveAt(eIList.Count - 1);
                        eI.findElse = true;
                        eIList.Add(eI);
                        break;
                    case "endif":
                        etageIfActuel--;
                        if (etageIfActuel < 0) { return "Ligne " + i + " : endif sans if de commence";}
                        eI = eIList[eIList.Count - 1];
                        if (eI.niveauBoucle != etageBoucleActuel) { return "Ligne " + i + " : endif non associe a un if ou n'est pas situe dans la meme boucle"; }
                        eIList.RemoveAt(eIList.Count - 1);
                        break;
                    case "boucle":
                        etageBoucleActuel++;
                        eBNivIf = etageIfActuel;
                        eBList.Add(eBNivIf);
                        break;
                    case "endboucle":
                        etageBoucleActuel--;
                        if (etageBoucleActuel < 0) { return "Line " + i + " : endboucle sans boucle de commence"; }
                        eBNivIf = eBList[eBList.Count - 1];
                        if (eBNivIf != etageIfActuel) { return "Ligne" + i + " : endboucle non associe a un boucle ou n'est pas situe dans le meme if"; }
                        eBList.RemoveAt(eBList.Count - 1);
                        break;
                    case "var":
                        carVar = allLines[i].Split(' ')[1][0];
                        if (carVar == 'p') { return "Ligne" + i + " : Impossible de creer une variable commencant par 'p'"; }
                        if (carVar == 'l') { return "Ligne" + i + " : Impossible de creer une variable commencant par 'l'"; }
                        break;
                }
            }
            if (etageIfActuel != 0) { return "Line" + allLines.Length + " : pas de fin de if"; }
            if (etageBoucleActuel != 0) { return "Line" + allLines.Length + " : pas de fin de boucle"; }
    #endregion

    #region == Creation variables ==
            listVar.Clear();
            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i] == "") { continue; }
                string[] parametres = allLines[i].Split(' ');
                if (parametres[0] == "var")
                {
                    if (parametres.Length == 1) { return "Line " + i + " : aucune variable definie"; }

                    variable varFind = listVar.Find(v => v.name == parametres[1]);
                    if (varFind == null)
                    {
                        varFind = new variable(parametres[0]);
                        listVar.Add(varFind);
                    }
                }
            }
    #endregion

    #region == Creation fonctions ==
            
            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i] == "") { continue; }
                string[] parametres = allLines[i].Split(' ');

                switch (parametres[0])
                {
                    case "if":
                        if (parametres.Length != 4) { return "Line " + i + " : condition if incomplette"; }
                        elementIf eIf = new elementIf(parametres[1], parametres[3], parametres[2]);
                        listeAction.Add(eIf);
                        break;
                    case "else":
                        if (parametres.Length != 1) { return "Line " + i + " : else doit etre seul sur une ligne"; }
                        elementElse eElse = new elementElse();
                        listeAction.Add(eElse);
                        break;
                    case "endif":
                        if (parametres.Length != 1) { return "Line " + i + " : endif doit etre seul sur une ligne"; }
                        elementEndIf eEndIf = new elementEndIf();
                        listeAction.Add(eEndIf);
                        break;
                    case "boucle":
                        if (parametres.Length != 4) { return "Line " + i + " : condition boucle incomplette"; }
                        elementBoucle eBoucle = new elementBoucle(parametres[1], parametres[3], parametres[2]);
                        listeAction.Add(eBoucle);
                        break;
                    case "endboucle":
                        if (parametres.Length != 1) { return "Line " + i + " : endboucle doit etre seul sur une ligne"; }
                        elementEndBoucle eEndBoucle = new elementEndBoucle();
                        listeAction.Add(eEndBoucle);
                        break;
                    case "changebloc":

                        break;
                    case "move":

                        break;
                    case "var":

                        break;
                    case "commande":

                        break;
                    case "message":

                        break;
                    case "wait":

                        break;
                    case "stop":

                        break;


                }

            }

    #endregion

            return "";
        }

        public void exec()
        {
            threadPrincipal = new Thread(new ThreadStart(boucle));
            threadPrincipal.Start();
        }

        public void stop()
        {
            threadPrincipal.Abort();
            threadPrincipal.Join();
        }

        public void boucle()
        {

        }

    }
}
