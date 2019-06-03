/*
	Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl) Licensed under the
	Educational Community License, Version 2.0 (the "License"); you may
	not use this file except in compliance with the License. You may
	obtain a copy of the License at
	
	http://www.osedu.org/licenses/ECL-2.0
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the License is distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the License for the specific language governing
	permissions and limitations under the License.
*/
using System;

namespace MCWorlds
{
    public class CmdBlocks : Command
    {
        public override string name { get { return "blocks"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdBlocks() { }

        public override void Use(Player p, string message)
        {
            try
            {
                if (message == "")
                {
                    Player.SendMessage(p, "Blocs basic: ");
                    for (byte i = 0; i < 50; i++)
                    {
                        message += ", " + Block.Name(i);
                    }
                    Player.SendMessage(p, message.Remove(0, 2));
                    Player.SendMessage(p, "&d/blocks all <0/1/2/3/4> " + Server.DefaultColor + "pour tous les voir.");
                }
                else if (message.ToLower() == "all")
                {
                    Player.SendMessage(p, "Blocs complexe: ");
                    for (byte i = 50; i < 255; i++)
                    {
                        if (Block.Name(i).ToLower() != "unknown") message += ", " + Block.Name(i);
                    }
                    Player.SendMessage(p, message.Remove(0, 2));
                    Player.SendMessage(p, "utilise &d/blocks all <0/1/2/3/4> " + Server.DefaultColor + "pour une liste lisible.");
                }
                else if (message.ToLower().IndexOf(' ') != -1 && message.Split(' ')[0] == "all")
                {
                    int foundRange = 0;
                    try { foundRange = int.Parse(message.Split(' ')[1]); }
                    catch { Player.SendMessage(p, "Syntaxe incorrecte"); return; }

                    if (foundRange >= 5 || foundRange < 0) { Player.SendMessage(p, "Le nombre doit etre entre 0 et 4"); return; }

                    message = "";
                    Player.SendMessage(p, "Blocs entre " + foundRange * 51 + " et " + (foundRange + 1) * 51);
                    for (byte i = (byte)(foundRange * 51); i < (byte)((foundRange + 1) * 51); i++)
                    {
                        if (Block.Name(i).ToLower() != "unknown") message += ", " + Block.Name(i);
                    }
                    Player.SendMessage(p, message.Remove(0, 2));
                }
                else
                {
                    string printMessage = ">>>&b";

                    if (Block.Byte(message) != Block.Zero)
                    {
                        byte b = Block.Byte(message);
                        if (b < 51)
                        {
                            for (byte i = 51; i < 255; i++)
                            {
                                if (Block.Convert(i) == b)
                                    printMessage += Block.Name(i) + ", ";
                            }

                            if (printMessage != ">>>&b")
                            {
                                Player.SendMessage(p, "Les blocs qui ressemblent a \"" + message + "\":");
                                Player.SendMessage(p, printMessage.Remove(printMessage.Length - 2));
                            }
                            else Player.SendMessage(p, "Pas de blocs complexe qui ressemble a \"" + message + "\"");
                        }
                        else
                        {
                            Player.SendMessage(p, "&bInformation pour le bloc complexe \"" + message + "\":");
                            Player.SendMessage(p, "&cCe bloc ressemble a \"" + Block.Name(Block.Convert(b)) + "\" block");

                            if (Block.LightPass(b)) Player.SendMessage(p, "La lumiere passe a travert le bloc");
                            if (Block.Physics(b)) Player.SendMessage(p, "La physics agis sur lui");
                            else Player.SendMessage(p, "La physics n'a aucuns effets sur le bloc");
                            if (Block.NeedRestart(b)) Player.SendMessage(p, "La physics du bloc redemarre automatiquement");

                            if (Block.OPBlocks(b)) Player.SendMessage(p, "Bloc non affecte par les explosions");

                            if (Block.AllowBreak(b)) Player.SendMessage(p, "Le bloc peut etre activer en le touchant");
                            if (Block.Walkthrough(b)) Player.SendMessage(p, "On peut marcher sur le bloc");
                            if (Block.Death(b)) Player.SendMessage(p, "Marcher sur le bloc est mortel");

                            if (Block.DoorAirs(b) != (byte)0) Player.SendMessage(p, "Le bloc est une door simple");
                            if (Block.tDoor(b)) Player.SendMessage(p, "Le bloc est une tdoor, elle s'ouvre grace a d'autres blocs");
                            if (Block.odoor(b) != Block.Zero) Player.SendMessage(p, "Le bloc est une odoor , elle bascule (GLITCH)");

                            if (Block.Mover(b)) Player.SendMessage(p, "Le bloc peut etre activer en marchant dessus");
                        }
                    }
                    else if (Group.Find(message) != null)
                    {
                        LevelPermission Perm = Group.Find(message).Permission;
                        foreach (Block.Blocks bL in Block.BlockList)
                        {
                            if (Block.canPlace(Perm, bL.type) && Block.Name(bL.type).ToLower() != "unknown") printMessage += Block.Name(bL.type) + ", ";
                        }

                        if (printMessage != ">>>&b")
                        {
                            Player.SendMessage(p, "les blocs que peut placer le rang " + Group.Find(message).color + Group.Find(message).name + Server.DefaultColor + " : ");
                            Player.SendMessage(p, printMessage.Remove(printMessage.Length - 2));
                        }
                        else Player.SendMessage(p, "Pas de blocs specifiques au rang");
                    }
                    else if (message.IndexOf(' ') == -1)
                    {
                        if (message.ToLower() == "count") Player.SendMessage(p, "Le bloc est dans cet map : " + p.level.blocks.Length);
                        else Help(p);
                    }
                    else if (message.Split(' ')[0].ToLower() == "count")
                    {
                        int foundNum = 0; byte foundBlock = Block.Byte(message.Split(' ')[1]);
                        if (foundBlock == Block.Zero) { Player.SendMessage(p, "Ne trouve pas le bloc"); return; }

                        for (int i = 0; i < p.level.blocks.Length; i++)
                        {
                            if (foundBlock == p.level.blocks[i]) foundNum++;
                        }

                        if (foundNum == 0) Player.SendMessage(p, "Pas de blocs de ce type \"" + message.Split(' ')[1] + "\"");
                        else if (foundNum == 1) Player.SendMessage(p, "1 bloc de ce type \"" + message.Split(' ')[1] + "\"");
                        else Player.SendMessage(p, foundNum.ToString() + " blocs de ce type \"" + message.Split(' ')[1] + "\"");
                    }
                    else
                    {
                        Player.SendMessage(p, "Impossible de trouver le bloc ou le rang");
                    }
                }
            }
            catch (Exception e) { Server.ErrorLog(e); Help(p); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/blocks - Liste tous les blocs basiques");
            Player.SendMessage(p, "/blocks all - liste tous les blocs complexe");
            Player.SendMessage(p, "/blocks [bloc basic] - Liste tous les bloc ressemblant a [bloc basic]");
            Player.SendMessage(p, "/blocks [bloc complexe] - Donne des informations sur le bloc");
            Player.SendMessage(p, "/blocks [rang] - Liste tous les bloc que le <rang> peut utiliser");
            Player.SendMessage(p, ">> " + Group.concatList());
            Player.SendMessage(p, "/blocks count [bloc] - Trouve le nombre de fois qu'il y a <bloc> dans la map");
        }
    }
}