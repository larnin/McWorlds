using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Starter
{
    class Program
    {
        static void Main(string[] args)
        {
            bool reessait = false;

            /*WebClient Client = new WebClient();
            try
            {
                Console.WriteLine("Demande d'autorisation ...");
                string allow = Client.DownloadString("https://dl.dropbox.com/u/77857161/lmp/allow.txt");
                if (allow == "permis") 
                {
                    Console.WriteLine("Ouverture autorise.");
                }
                else
                {
                    Console.WriteLine("Vous n'avez pas l'autorisation d'ouvrir ce serveur");
                    Console.WriteLine("Appuyez sur une touche pour me fermer ...");
                    Console.ReadLine();
                    Client.Dispose();
                    goto exit;
                }
            }
            catch 
            {
                Console.WriteLine("Erreur de connexion au serveur de verification");
                Console.WriteLine("Appuyez sur une touche pour me fermer ...");
                Console.ReadLine();
                Client.Dispose();
                goto exit;
            }

        retry:
            try
            {
                Console.WriteLine("Recherche de mise a jour...");
                string update = Client.DownloadString("https://dl.dropbox.com/u/77857161/lmp/update.txt");
                string[] uLignes = update.Split('\n');
                int version = int.Parse(uLignes[0].Split(' ')[1]);
                int verActuelle = 1;
                if (File.Exists("extra/version.txt"))
                { try { verActuelle = int.Parse(File.ReadAllLines("extra/version.txt")[0]); } catch { } }
                if (version > verActuelle || version == 0 || reessait)
                {
                    Console.WriteLine("Mise a jours du serveur ....");
                    string dll = uLignes[1].Split(' ')[1];
                    if (dll == "null") { Console.WriteLine("Mise a jour impossible"); }
                    else
                    {
                        string vers = uLignes[2].Split(' ')[1];
                        string rep = "";
                        if (!reessait)
                        {
                            Console.WriteLine("Une mise a jour est disponible");
                            Console.WriteLine("Voulez vous effectuer la mise a jour vers la version " + vers + "\n(o : oui / n : non)");
                            rep = Console.ReadLine().ToLower();
                        }
                        if (rep == "o" || rep == "oui" || reessait)
                        {
                            Console.WriteLine("Téléchargement en cours ...");
                            if (File.Exists("MCWorlds_.dll"))
                            {
                                if (File.Exists("MCWorlds_.dll.back")) { File.Delete("MCWorlds_.dll.back"); }
                                File.Move("MCWorlds_.dll", "MCWorlds_.dll.back");
                            }
                            Client.DownloadFile(dll, "MCWorlds_.dll");
                            if (uLignes.Length > 3)
                            {
                                string changelogs = uLignes[3].Split(' ')[1];
                                if (changelogs != "null")
                                {
                                    if (File.Exists("extra/Changelog.txt")) { File.Delete("extra/Changelog.txt"); }
                                    Client.DownloadFile(changelogs, "extra/Changelog.txt");
                                }
                            }
                            string[] v = new string[1]; v[0] = version.ToString();
                            File.WriteAllLines("extra/version.txt", v);
                            Console.WriteLine("Mise a jour terminée");
                            Console.WriteLine("Appuyez sur une touche pour continuer ...");
                            Console.ReadLine();
                        }
                        else { Console.WriteLine("Abandon ..."); }
                    }
                }
                else { Console.WriteLine("Serveur a jours"); }
            }
            catch
            {
                Console.WriteLine("Erreur de connexion au serveur de mise a jour");
            }
            Client.Dispose();*/
           
        start:
            if (File.Exists("MCWorlds_.dll"))
            {
                openServer(args);
            }
            else
            { 
                Console.WriteLine("Le fichier MCWorlds_.dll est introuvable");
                if(File.Exists("MCWorlds_.dll.back"))
                {
                    Console.WriteLine("Restoration de 'MCWorlds_.dll.back' ....");
                    File.Copy("MCWorlds_.dll.back", "MCWorlds_.dll");
                    goto start;
                }
                else if (!reessait)
                {
                    Console.WriteLine("Force update ...");
                    reessait = true;
                   // goto retry;
                }
                Console.WriteLine("Telechargez le manuellement et placez le dans mon dossier.");
                Console.WriteLine("Appuyez sur une touche pour me fermer ...");
                Console.ReadLine();
                goto exit;
            }

exit:   Console.WriteLine("Bye!");
        }

        static void openServer(string[] args)
        {
            MCWorlds_.Gui.Program.Main(args);
        }
    }
}
