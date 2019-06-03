using System;
using System.IO;

namespace MCWorlds
{
    public class CmdBotSet : Command
    {
        public override string name { get { return "botset"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdBotSet() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }

            try
            {
                if (message.Split(' ').Length == 1)
                {
                    PlayerBot pB = PlayerBot.Find(message);
                    if (pB == null) { Player.SendMessage(p, "Ne trouve pas le bot"); return; }
                    try { pB.Waypoints.Clear(); }
                    catch { }
                    pB.kill = false;
                    pB.hunt = false;
                    pB.AIName = "";
                    Player.SendMessage(p, "L'AI de " + pB.color + pB.name + Server.DefaultColor + " a ete desactive.");
                    return;
                }

                if (message.Split(' ')[1] == "say")
                {
                    PlayerBot pB = PlayerBot.Find(message.Split(' ')[0]);
                    if (pB == null) { Player.SendMessage(p, "Ne trouve pas le bot"); return; }
                    
                    message = message.Substring(message.IndexOf("say") + 4).Trim();

                    pB.sayMessage = message;
                    pB.lastSpeek = DateTime.Now;

                    if (message == "") { Player.SendMessage(p, "Le bot " + pB.color + pB.name + Server.DefaultColor + " ne parle plus"); }
                    else { Player.SendMessage(p, "Le bot " + pB.color + pB.name + Server.DefaultColor + " sait maintenant parler"); }

                    return;
                }
                if (message.Split(' ')[1] == "color")
                {
                    PlayerBot pB = PlayerBot.Find(message.Split(' ')[0]);
                    if (pB == null) { Player.SendMessage(p, "Ne trouve pas le bot"); return; }

                    string color = c.Parse(message.Split(' ')[2]);
                    if (color == "") { Player.SendMessage(p, "Il n'existe pas de couleur \"" + message + "\"."); return; }
                    pB.color = color;
                    pB.GlobalDie();
                    pB.GlobalSpawn();

                    Player.SendMessage(p, "La couleur du bot " + pB.color + pB.name + Server.DefaultColor + " a changer en " + message.Split(' ')[2].ToLower());
                    return;
                }

                if (message.Split(' ').Length != 2)
                {
                    Help(p); return;
                }

                PlayerBot Pb = PlayerBot.Find(message.Split(' ')[0]);
                if (Pb == null) { Player.SendMessage(p, "Ne trouve pas le bot"); return; }
                string foundPath = message.Split(' ')[1].ToLower();

                if (foundPath == "hunt")
                {
                    Pb.hunt = !Pb.hunt;
                    try { Pb.Waypoints.Clear(); }
                    catch { }
                    Pb.AIName = "";
                    if (p != null) Player.GlobalChatLevel(p, Pb.color + Pb.name + Server.DefaultColor + " a trouve son instinct de suiveur: " + Pb.hunt, false);
                    return;
                }
                else if (foundPath == "kill")
                {
                    if (p.group.Permission < LevelPermission.Operator) { Player.SendMessage(p, "Seul un OP peut donner un instinct de tueur."); return; }
                    Pb.kill = !Pb.kill;
                    if (p != null) Player.GlobalChatLevel(p, Pb.color + Pb.name + Server.DefaultColor + " a trouve son instinct de tueur: " + Pb.kill, false);
                    return;
                }

                if (!File.Exists("bots/" + foundPath)) { Player.SendMessage(p, "Ne trouve pas l'IA."); return; }

                string[] foundWay = File.ReadAllLines("bots/" + foundPath);

                if (foundWay[0] != "#Version 2") { Player.SendMessage(p, "Version du fichier unvalide"); return; }

                PlayerBot.Pos newPos = new PlayerBot.Pos();
                try { Pb.Waypoints.Clear(); Pb.currentPoint = 0; Pb.countdown = 0; Pb.movementSpeed = 12; }
                catch { }

                try
                {
                    foreach (string s in foundWay)
                    {
                        if (s == "") { continue; }
                        if (s != "" && s[0] != '#')
                        {
                            bool skip = false;
                            newPos.type = s.Split(' ')[0];
                            switch (s.Split(' ')[0].ToLower())
                            {
                                case "walk":
                                case "teleport":
                                    newPos.x = Convert.ToUInt16(s.Split(' ')[1]);
                                    newPos.y = Convert.ToUInt16(s.Split(' ')[2]);
                                    newPos.z = Convert.ToUInt16(s.Split(' ')[3]);
                                    newPos.rotx = Convert.ToByte(s.Split(' ')[4]);
                                    newPos.roty = Convert.ToByte(s.Split(' ')[5]);
                                    break;
                                case "wait":
                                case "speed":
                                    newPos.seconds = Convert.ToInt16(s.Split(' ')[1]); break;
                                case "nod":
                                case "spin":
                                    newPos.seconds = Convert.ToInt16(s.Split(' ')[1]);
                                    newPos.rotspeed = Convert.ToInt16(s.Split(' ')[2]);
                                    break;
                                case "linkscript":
                                    newPos.newscript = s.Split(' ')[1]; break;
                                case "reset":
                                case "jump":
                                case "remove": break;
                                default: skip = true; break;
                            }
                            if (!skip) Pb.Waypoints.Add(newPos);
                        }
                    }
                }
                catch { Player.SendMessage(p, "Le fichier d'IA est corrompu."); return; }

                Pb.AIName = foundPath;
                if (p != null) Player.GlobalChatLevel(p,"l'IA de " + Pb.color + Pb.name + Server.DefaultColor + " est maintenant " + foundPath, false);
            }
            catch { Player.SendMessage(p, "Erreur"); return; }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/botset [bot] [script IA] - Donne au bot l'IA <script IA>");
            Player.SendMessage(p, "/botset [bot] say [message] - Fait parler le bot lorsque l'on se place devant lui");
            Player.SendMessage(p, "/botset [bot] color [couleur] - Change la couleur de pseudo du bot");
            Player.SendMessage(p, "Scripts speciaux : Kill et Hunt");
        }
    }
}