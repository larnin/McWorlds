using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdUndo : Command
    {
        public override string name { get { return "undo"; } }
        public override string shortcut { get { return "u"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdUndo() { }

        public override void Use(Player p, string message)
        {
            byte b; Int64 seconds; Player who; Player.UndoPos Pos; int CurrentPos = 0;
            if (p != null) p.RedoBuffer.Clear();

            if (message == "") message = p.name + " 30";

            if (message.Split(' ').Length == 2)
            {
                if (message.Split(' ')[1].ToLower() == "all" && p.group.Permission > LevelPermission.Operator)
                {
                    seconds = 500000;
                }
                else
                {
                    try
                    {
                        seconds = Int64.Parse(message.Split(' ')[1]);
                    }
                    catch
                    {
                        Player.SendMessage(p, "Secondes invalides.");
                        return;
                    }
                }
            }
            else
            {
                try
                {
                    seconds = int.Parse(message);
                    if (p != null) message = p.name + " " + message;
                }
                catch
                {
                    seconds = 30;
                    message = message + " 30";
                }
            }

            //if (message.Split(' ').Length == 1) if (char.IsDigit(message, 0)) { message = p.name + " " + message; } else { message = message + " 30"; }

            //try { seconds = Convert.ToInt16(message.Split(' ')[1]); } catch { seconds = 2; }
            if (seconds == 0) seconds = 5400;

            who = Player.Find(message.Split(' ')[0]);
            if (who != null)
            {
                if (p != null)
                {
                    if (who.group.Permission > p.group.Permission && who != p) { Player.SendMessage(p, "Impossible d'utiliser undo sur un rang superieur au votre"); return; }
                    if (who != p && p.group.Permission < LevelPermission.Operator) { Player.SendMessage(p, "Annuler les actions d'un autre joueur est reserve aux OP+"); return; }

                    if (p.group.Permission < LevelPermission.Builder && seconds > 120) { Player.SendMessage(p, "Les guests peuvent annuler 2 min au maximum."); return; }
                    else if (p.group.Permission < LevelPermission.Operator && seconds > 1200) { Player.SendMessage(p, "Les constructeurs peuvent annuler 10 min maximum."); return; }
                    else if (p.group.Permission == LevelPermission.Operator && seconds > 5400) { Player.SendMessage(p, "Les OP peuvent annuler 90 min au maximum."); return; }
                }

                for (CurrentPos = who.UndoBuffer.Count - 1; CurrentPos >= 0; --CurrentPos)
                {
                    try
                    {
                        Pos = who.UndoBuffer[CurrentPos];
                        Level foundLevel = Level.FindExact(Pos.mapName, Pos.worldName);
                        b = foundLevel.GetTile(Pos.x, Pos.y, Pos.z);
                        if (Pos.timePlaced.AddSeconds(seconds) >= DateTime.Now)
                        {
                            if (b == Pos.newtype || Block.Convert(b) == Block.water || Block.Convert(b) == Block.lava)
                            {
                                foundLevel.Blockchange(Pos.x, Pos.y, Pos.z, Pos.type, true);

                                Pos.newtype = Pos.type; Pos.type = b;
                                if (p != null) p.RedoBuffer.Add(Pos);
                                who.UndoBuffer.RemoveAt(CurrentPos);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch { }
                }

                if (p != who) Player.GlobalChat(p, "Les actions de " + who.color + who.Name() + Server.DefaultColor + " ont ete annule sur &b" + seconds + " secondes.", false);
                else Player.SendMessage(p, "Vous avez annule vos actions sur &b" + seconds + Server.DefaultColor + " secondes.");
                return;
            }
            else if (message.Split(' ')[0].ToLower() == "physics")
            {
                if (p.group.Permission < LevelPermission.Operator && p.level.world != p.name.ToLower()) { Player.SendMessage(p, "Vous ne pouvez pas undo les physics sur ce monde"); return; }
                if (p.group.Permission < LevelPermission.Operator && seconds > 600) { Player.SendMessage(p, "Les Constructeurs peuvent annuler 10 min maximum."); return; }
                else if (p.group.Permission == LevelPermission.Operator && seconds > 5400) { Player.SendMessage(p, "Les OP peuvent annuler 90 min maximum."); return; }

                Command.all.Find("pause").Use(p, "120");
                Level.UndoPos uP;
                ushort x, y, z;

                if (p.level.UndoBuffer.Count != Server.physUndo)
                {
                    for (CurrentPos = p.level.currentUndo; CurrentPos >= 0; CurrentPos--)
                    {
                        try
                        {
                            uP = p.level.UndoBuffer[CurrentPos];
                            b = p.level.GetTile(uP.location);
                            if (uP.timePerformed.AddSeconds(seconds) >= DateTime.Now)
                            {
                                if (b == uP.newType || Block.Convert(b) == Block.water || Block.Convert(b) == Block.lava)
                                {
                                    p.level.IntToPos(uP.location, out x, out y, out z);
                                    p.level.Blockchange(p, x, y, z, uP.oldType, true);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    for (CurrentPos = p.level.currentUndo; CurrentPos != p.level.currentUndo + 1; CurrentPos--)
                    {
                        try
                        {
                            if (CurrentPos < 0) CurrentPos = p.level.UndoBuffer.Count - 1;
                            uP = p.level.UndoBuffer[CurrentPos];
                            b = p.level.GetTile(uP.location);
                            if (uP.timePerformed.AddSeconds(seconds) >= DateTime.Now)
                            {
                                if (b == uP.newType || Block.Convert(b) == Block.water || Block.Convert(b) == Block.lava)
                                {
                                    p.level.IntToPos(uP.location, out x, out y, out z);
                                    p.level.Blockchange(p, x, y, z, uP.oldType, true);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch { }
                    }
                }

                Command.all.Find("pause").Use(p, "");
                Player.GlobalMessageLevel(p.level, "Les physics ont ete annule de &b" + seconds + Server.DefaultColor + " secondes");
            }
            else
            {
                if (p != null)
                {
                    if (p.group.Permission < LevelPermission.Operator) { Player.SendMessage(p, "Reserve aux OP+"); return; }
                    if (seconds > 5400 && p.group.Permission == LevelPermission.Operator) { Player.SendMessage(p, "Seulement les superop peuvent undo plus de 90 min."); return; }
                }

                bool FoundUser = false;

                try
                {
                    DirectoryInfo di;
                    string[] fileContent;

                    p.RedoBuffer.Clear();

                    if (Directory.Exists("extra/undo/" + message.Split(' ')[0]))
                    {
                        di = new DirectoryInfo("extra/undo/" + message.Split(' ')[0]);

                        for (int i = di.GetFiles("*.undo").Length - 1; i >= 0; i--)
                        {
                            fileContent = File.ReadAllText("extra/undo/" + message.Split(' ')[0] + "/" + i + ".undo").Split(' ');
                            if (!undoBlah(fileContent, seconds, p)) break;
                        }
                        FoundUser = true;
                    }

                    if (Directory.Exists("extra/undoPrevious/" + message.Split(' ')[0]))
                    {
                        di = new DirectoryInfo("extra/undoPrevious/" + message.Split(' ')[0]);

                        for (int i = di.GetFiles("*.undo").Length - 1; i >= 0; i--)
                        {
                            fileContent = File.ReadAllText("extra/undoPrevious/" + message.Split(' ')[0] + "/" + i + ".undo").Split(' ');
                            if (!undoBlah(fileContent, seconds, p)) break;
                        }
                        FoundUser = true;
                    }
                    
                    if (FoundUser) Player.GlobalChat(p, Server.FindColor("Les actions de " + message.Split(' ')[0]) + message.Split(' ')[0] + Server.DefaultColor + " ont ete anule sur " + seconds + Server.DefaultColor + " secondes.", false);
                    else Player.SendMessage(p, "Could not find player specified.");
                }
                catch (Exception e)
                {
                    Server.ErrorLog(e);
                }
            }
        }

        public bool undoBlah(string[] fileContent, Int64 seconds, Player p)
        {

            //fileContents += uP.map.name + " " + uP.x + " " + uP.y + " " + uP.z + " ";
            //fileContents += uP.timePlaced + " " + uP.type + " " + uP.newtype + " ";

            //Maps = 0, 7, 14, 21, 28, 35...
            //X = 1, 8, 15...
            //newtype = 6, 13, 20, 27...

            Player.UndoPos Pos;

            for (int i = fileContent.Length / 7; i >= 0; i--)
            {
                try
                {
                    if (Convert.ToDateTime(fileContent[(i * 8) + 4].Replace('&', ' ')).AddSeconds(seconds) >= DateTime.Now)
                    {
                        Level foundLevel = Level.FindExact(fileContent[i * 8], fileContent[i * 8 + 1]);
                        if (foundLevel != null)
                        {
                            Pos.mapName = foundLevel.name;
                            Pos.worldName = foundLevel.world;
                            Pos.x = Convert.ToUInt16(fileContent[(i * 8) + 2]);
                            Pos.y = Convert.ToUInt16(fileContent[(i * 8) + 3]);
                            Pos.z = Convert.ToUInt16(fileContent[(i * 8) + 4]);

                            Pos.type = foundLevel.GetTile(Pos.x, Pos.y, Pos.z);

                            if (Pos.type == Convert.ToByte(fileContent[(i * 8) + 7]) || Block.Convert(Pos.type) == Block.water || Block.Convert(Pos.type) == Block.lava || Pos.type == Block.grass)
                            {
                                Pos.newtype = Convert.ToByte(fileContent[(i * 8) + 6]);
                                Pos.timePlaced = DateTime.Now;

                                foundLevel.Blockchange(Pos.x, Pos.y, Pos.z, Pos.newtype, true);
                                if (p != null) p.RedoBuffer.Add(Pos);
                            }
                        }
                    }
                    else return false;
                }
                catch { }
            }

            return true;
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/undo [joueur] - Annule les changement de blocs sur 30 secondes");
            Player.SendMessage(p, "/undo [joueur] <secondes> - Annule les changements de blocs du joueur");
            Player.SendMessage(p, "/undo [joueur] all - &cAnnule les actions de [player] sur les 138h precedentes <Abmin+>");
            Player.SendMessage(p, "/undo [joueur] 0 - &cAnnule les actions des 30 min precedentes <Modo+>");
            Player.SendMessage(p, "/undo physics <secondes> - Annule les physics de la map ou vous etes.");
        }
    }
}