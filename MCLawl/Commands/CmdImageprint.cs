using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;

namespace MCWorlds
{
    public class CmdImageprint : Command
    {
        public override string name { get { return "imageprint"; } }
        public override string shortcut { get { return "i"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdImageprint() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (!Directory.Exists("extra/images/")) { Directory.CreateDirectory("extra/images/"); }
            layer = false;
            popType = 1;
            if (message == "") { Help(p); return; }
            if (message.IndexOf(' ') != -1)     //Yay parameters
            {
                string[] parameters = message.Split(' ');

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] == "layer" || parameters[i] == "l") layer = true;
                    else if (parameters[i] == "1" || parameters[i] == "2color") popType = 1;
                    else if (parameters[i] == "2" || parameters[i] == "1color") popType = 2;
                    else if (parameters[i] == "3" || parameters[i] == "2gray") popType = 3;
                    else if (parameters[i] == "4" || parameters[i] == "1gray") popType = 4;
                    else if (parameters[i] == "5" || parameters[i] == "bw") popType = 5;
                    else if (parameters[i] == "6" || parameters[i] == "gray") popType = 6;
                }

                message = parameters[parameters.Length - 1];
            }
            if (message.IndexOf('/') == -1 && message.IndexOf('.') != -1)
            {
                try
                {
                    WebClient web = new WebClient();
                    Player.SendMessage(p, "Telechargement de l'imahe a partire de: &fhttp://www.imgur.com/" + message);
                    web.DownloadFile("http://www.imgur.com/" + message, "extra/images/tempImage_" + p.name + ".bmp");
                    web.Dispose();
                    Player.SendMessage(p, "Telechargement reussi.");
                    bitmaplocation = "tempImage_" + p.name;
                    message = bitmaplocation;   
                }
                catch { }
            }
            else if (message.IndexOf('.') != -1)
            {
                try
                {
                    WebClient web = new WebClient();
                    if (message.Substring(0, 4) != "http")
                    {
                        message = "http://" + message;
                    }
                    Player.SendMessage(p, "Telechargement de l'image : &f" + message + Server.DefaultColor + ".");
                    web.DownloadFile(message, "extra/images/tempImage_" + p.name + ".bmp");
                    web.Dispose();
                    Player.SendMessage(p, "Telechargement reussi.");
                    bitmaplocation = "tempImage_" + p.name;
                }
                catch { }
            }
            else
            {
                bitmaplocation = message;
            }

            if (!File.Exists("extra/images/" + bitmaplocation + ".bmp")) { Player.SendMessage(p, "The URL entered was invalid!"); return; }

            CatchPos cpos;

            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;
            Player.SendMessage(p, "Placez deux blocs pour determiner la direction.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos bp = (CatchPos)p.blockchangeObject;
            bp.x = x; bp.y = y; bp.z = z; p.blockchangeObject = bp;
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange2);
        }
        public void Blockchange2(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);

            Bitmap myBitmap = new Bitmap("extra/images/" + bitmaplocation + ".bmp"); 

            myBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            
            CatchPos cpos = (CatchPos)p.blockchangeObject;

            if (x == cpos.x && z == cpos.z) { Player.SendMessage(p, "Aucune direction choisie"); return; }

            int direction;
            if (Math.Abs(cpos.x - x) > Math.Abs(cpos.z - z))
            {
                direction = 0;
                if (x <= cpos.x)
                {
                    direction = 1;
                }
            }
            else
            {
                direction = 2;
                if (z <= cpos.z)
                {
                    direction = 3;
                }
            }
            if (layer)
            {
                if (popType == 1) popType = 2;
                if (popType == 3) popType = 4;
            }
            List<FindReference.ColorBlock> refCol = FindReference.popRefCol(popType);
            FindReference.ColorBlock colblock;
            p.SendMessage("" + direction);
            Thread printThread = new Thread(new ThreadStart(delegate
            {
                double[] distance = new double[refCol.Count]; // Array of distances between color pulled from image to the referance colors.

                int position; // This is the block selector for when we find which distance is the shortest.

                for (int k = 0; k < myBitmap.Width; k++)
                {

                    for (int i = 0; i < myBitmap.Height; i++)
                    {
                        if (layer)
                        {
                            colblock.y = cpos.y;
                            if (direction <= 1)
                            {
                                if (direction == 0) { colblock.x = (ushort)(cpos.x + k); colblock.z = (ushort)(cpos.z - i); }
                                else { colblock.x = (ushort)(cpos.x - k); colblock.z = (ushort)(cpos.z + i); }
                                //colblock.z = (ushort)(cpos.z - i);
                            }
                            else
                            {
                                if (direction == 2) { colblock.z = (ushort)(cpos.z + k); colblock.x = (ushort)(cpos.x + i); }
                                else { colblock.z = (ushort)(cpos.z - k); colblock.x = (ushort)(cpos.x - i); }
                                //colblock.x = (ushort)(cpos.x - i);
                            }
                        }
                        else
                        {
                            colblock.y = (ushort)(cpos.y + i);
                            if (direction <= 1)
                            {

                                if (direction == 0) colblock.x = (ushort)(cpos.x + k);
                                else colblock.x = (ushort)(cpos.x - k);
                                colblock.z = cpos.z;
                            }
                            else
                            {
                                if (direction == 2) colblock.z = (ushort)(cpos.z + k);
                                else colblock.z = (ushort)(cpos.z - k);
                                colblock.x = cpos.x;
                            }
                        }


                        colblock.r = myBitmap.GetPixel(k, i).R;
                        colblock.g = myBitmap.GetPixel(k, i).G;
                        colblock.b = myBitmap.GetPixel(k, i).B;
                        colblock.a = myBitmap.GetPixel(k, i).A;

                        if (popType == 6)
                        {
                            if ((colblock.r + colblock.g + colblock.b) / 3 < (256 / 4))
                            {
                                colblock.type = Block.obsidian;
                            }
                            else if (((colblock.r + colblock.g + colblock.b) / 3) >= (256 / 4) && ((colblock.r + colblock.g + colblock.b) / 3) < (256 / 4) * 2)
                            {
                                colblock.type = Block.darkgrey;
                            }
                            else if (((colblock.r + colblock.g + colblock.b) / 3) >= (256 / 4) * 2 && ((colblock.r + colblock.g + colblock.b) / 3) < (256 / 4) * 3)
                            {
                                colblock.type = Block.lightgrey;
                            }
                            else
                            {
                                colblock.type = Block.white;
                            }
                        }
                        else
                        {
                            for (int j = 0; j < distance.Length; j++) // Calculate distances between the colors in the image and the set referance colors, and store them.
                            {
                                distance[j] = Math.Sqrt(Math.Pow((colblock.r - refCol[j].r), 2) + Math.Pow((colblock.b - refCol[j].b), 2) + Math.Pow((colblock.g - refCol[j].g), 2));
                            }

                            position = 0;
                            double minimum = distance[0];
                            for (int h = 1; h < distance.Length; h++) // Find the smallest distance in the array of distances.
                            {
                                if (distance[h] < minimum)
                                {
                                    minimum = distance[h];
                                    position = h;
                                }
                            }


                            colblock.type = refCol[position].type; // Set the block we found closest to the image to the block we are placing.

                            if (popType == 1)
                            {
                                if (position <= 20)
                                {
                                    if (direction == 0)
                                    {
                                        colblock.z = (ushort)(colblock.z + 1);
                                    }
                                    else if (direction == 2)
                                    {
                                        colblock.x = (ushort)(colblock.x - 1);
                                    }
                                    else if (direction == 1)
                                    {
                                        colblock.z = (ushort)(colblock.z - 1);
                                    }
                                    else if (direction == 3)
                                    {
                                        colblock.x = (ushort)(colblock.x + 1);
                                    }
                                }
                            }
                            else if (popType == 3)
                            {
                                if (position <= 3)
                                {
                                    if (direction == 0)
                                    {
                                        colblock.z = (ushort)(colblock.z + 1);
                                    }
                                    else if (direction == 2)
                                    {
                                        colblock.x = (ushort)(colblock.x - 1);
                                    }
                                    else if (direction == 1)
                                    {
                                        colblock.z = (ushort)(colblock.z - 1);
                                    }
                                    else if (direction == 3)
                                    {
                                        colblock.x = (ushort)(colblock.x + 1);
                                    }
                                }
                            }
                        }

                        //ALPHA HANDLING (REAL HARD STUFF, YO)
                        if (colblock.a < 20) colblock.type = Block.air;

                        FindReference.placeBlock(p.level, p, colblock.x, colblock.y, colblock.z, colblock.type);
                    }
                }
                if (bitmaplocation == "tempImage_" + p.name) File.Delete("extra/images/tempImage_" + p.name + ".bmp");

                string printType;
                switch (popType)
                {
                    case 1: printType = "2-layer color"; break;
                    case 2: printType = "1-layer color"; break;
                    case 3: printType = "2-layer grayscale"; break;
                    case 4: printType = "1-layer grayscale"; break;
                    case 5: printType = "Black and White"; break;
                    case 6: printType = "Mathematical grayscale"; break;
                    default: printType = "Something unknown"; break;
                }

                Player.SendMessage(p, "Dessin de l'image avec le type " + printType +  "termine." );
            })); printThread.Start();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/imageprint <type> <image> - Dessine l'image situee dans le dossier extra/images/. Elle doit etre du type .bmp, ne pas ecrire l'extention de l'image.");
            Player.SendMessage(p, "/imageprint <type> <image.extension> - Dessine l'image enregistree sur le site www.imgur.com. Exemple: /i piCTm.gif dessine l'image qui a l'adresse www.imgur.com/piCTm.gif.");
            Player.SendMessage(p, "/imageprint <type> <adresseweb> - Dessine l'image d'adresse domain.com/dossier/image.jpg. Pas besoin de http:// ou www.");
            Player.SendMessage(p, "Types valides: (&f1" + Server.DefaultColor + ") 2-Layer Color image, (&f2" + Server.DefaultColor + ") 1-Layer Color Image, (&f3" + Server.DefaultColor + ") 2-Layer Grayscale, (&f4" + Server.DefaultColor + ") 1-Layer Grayscale, (%f5" + Server.DefaultColor + ") Black and White, (&f6" + Server.DefaultColor + ") Mathematical Grayscale");
            Player.SendMessage(p, "Types de fichiers local: .bmp. Types de fichiers a distance: .gif .png .bmp .jpg. ... Les PNG et GIF peuvent utiliser la transparence");
            Player.SendMessage(p, "Utilise le type (&flayer" + Server.DefaultColor + ") ou (&fl" + Server.DefaultColor + ") pour dessiner horizontalement.");
        }

        public struct CatchPos { public ushort x, y, z; }

        string bitmaplocation;
        bool layer = false;
        byte popType = 1;
    }
}

