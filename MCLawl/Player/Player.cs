using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Data;

namespace MCWorlds
{
    public sealed class Player
    {
        public static List<Player> players = new List<Player>();
        public static Dictionary<string, string> left = new Dictionary<string, string>();
        public static List<Player> connections = new List<Player>(Server.players);
        public static List<string> emoteList = new List<string>();
        public static byte number { get { return (byte)players.Count; } }
        static System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

        public static bool storeHelp = false;
        public static string storedHelp = "";

        Socket socket;
        System.Timers.Timer loginTimer = new System.Timers.Timer(1000);
        public System.Timers.Timer pingTimer = new System.Timers.Timer(2000);
        System.Timers.Timer extraTimer = new System.Timers.Timer(22000);
        public System.Timers.Timer afkTimer = new System.Timers.Timer(2000);
        public int afkCount = 0;
        public DateTime afkStart;

        byte[] buffer = new byte[0];
        byte[] tempbuffer = new byte[0xFF];
        public bool disconnected = false;

        //CUSTOM COMMANDES
        public string skin = "";
        public string falseName = "";
        public bool gamemode = false;
        public bool ingame = false;
        public int tailleBufferGame = 0;
        public List<string> bufferMessages = new List<string>(20);
        public List<string> gameMessages = new List<string>(20);
        public bool msgChanged = false;
        public bool skill = false;
        public bool nuke = false;
        public bool tchatmap = false;
        public bool zombiespawn = false;
        public bool examine = false;
        public int nbMapsMax = 5;
        public int nbMaps = 0;
        public DateTime totalTimePlayed = DateTime.Now;
        public DateTime lastTp = DateTime.Now;
        public int nbWarning = 0;
        public DateTime lastWarning = DateTime.MinValue;
        // changement
        public bool perbuild = false;
        public bool rulesAccepted = false;
        public bool vip = false;
        public string salon = "";
        public bool salonSpeek = false;
                //public string scriptMod = "";
        public bool poseRedstone = false;
        public bool posePiston = false;
        public Brush[] brushs = new Brush[9] { new Brush(), new Brush(), new Brush(), new Brush(), new Brush(), new Brush(), new Brush(), new Brush(), new Brush() };

        public string name;
        public byte id;
        public int userID = -1;
        public string ip;
        public string color;
        public Group group;
        public bool hidden = false;
        public bool painting = false;
        public bool muted = false;
        public bool jailed = false;
        public bool invincible = false;
        public string rang = "";
        public string prefix = "";
        public string title = "";
        public string titlecolor;

        public bool deleteMode = false;
        public bool ignorePermission = false;
        public bool ignoreGrief = false;
        public bool parseSmiley = true;
        public bool smileySaved = true;
        public bool opchat = false;
        public bool onWhitelist = false;
        public bool whisper = false;
        public string whisperTo = "";

        public string storedMessage = "";

        public bool trainGrab = false;
        public bool onTrain = false;

        public bool frozen = false;
        public string following = "";
        public string possess = "";
        
        // Only used for possession.
        //Using for anything else can cause unintended effects!
        public bool canBuild = true;

        public int money = 0;
        public Int64 overallBlocks = 0;
        public int loginBlocks = 0;

        public DateTime timeLogged;
        public DateTime firstLogin;
        public int totalLogins = 0;
        public int totalKicked = 0;
        public int overallDeath = 0;

        public bool staticCommands = false;

        public Thread commThread;

        public bool aiming;
        public bool isFlying = false;

        public bool joker = false;

        public bool voice = false;
        public string voicestring = "";

        //GAMES
        public Team team;
        public Team hasflag;
        public string GameTempcolor;
        public string GameTempprefix;
        public bool carryingFlag;
        public bool spawning = false;
        public bool teamchat = false;
        public int health = 100;

        //Copy
        public List<CopyPos> CopyBuffer = new List<CopyPos>();
        public struct CopyPos { public ushort x, y, z; public byte type; }
        public bool copyAir = false;
        public int[] copyoffset = new int[3] { 0, 0, 0 };
        public ushort[] copystart = new ushort[3] { 0, 0, 0 };

        //Undo
        public struct UndoPos { public ushort x, y, z; public byte type, newtype; public string mapName, worldName; public DateTime timePlaced; }
        public List<UndoPos> UndoBuffer = new List<UndoPos>();
        public List<UndoPos> RedoBuffer = new List<UndoPos>();
        

        public bool showPortals = false;
        public bool showMBs = false;

        public string prevMsg = "";


        //Movement
        public ushort oldBlock = 0;
        public ushort deathCount = 0;
        public byte deathBlock;

        //Games
        public DateTime lastDeath = DateTime.Now;
        
        public byte BlockAction = 0;  //0-Nothing 1-solid 2-lava 3-water 4-active_lava 5 Active_water 6 OpGlass 7 BluePort 8 OrangePort
        public byte modeType = 0;
        public byte[] bindings = new byte[128];
        public string[] cmdBind = new string[10];
        public string[] messageBind = new string[10];
        public string lastCMD = "";

        public Level level = Server.mainLevel;
        public bool Loading = true;     //True if player is loading a map.

        public delegate void BlockchangeEventHandler(Player p, ushort x, ushort y, ushort z, byte type);
        public event BlockchangeEventHandler Blockchange = null;
        public void ClearBlockchange() { Blockchange = null; }
        public bool HasBlockchange() { return (Blockchange == null); }
        public object blockchangeObject = null;
        public ushort[] lastClick = new ushort[3] { 0, 0, 0 };

        public ushort[] pos = new ushort[3] { 0, 0, 0 };
        ushort[] oldpos = new ushort[3] { 0, 0, 0 };
        ushort[] basepos = new ushort[3] { 0, 0, 0 };
        public byte[] rot = new byte[2] { 0, 0 };
        byte[] oldrot = new byte[2] { 0, 0 };

        // grief/spam detection
        public static int spamBlockCount = 200;
        public static int spamBlockTimer = 5;
        Queue<DateTime> spamBlockLog = new Queue<DateTime>(spamBlockCount);

        public bool loggedIn = false;
        public Player(Socket s)
        {
            try
            {
                socket = s;
                ip = socket.RemoteEndPoint.ToString().Split(':')[0];
                Server.s.Log(ip + " connected to the server.");

                for (byte i = 0; i < 128; ++i) bindings[i] = i;

                socket.BeginReceive(tempbuffer, 0, tempbuffer.Length, SocketFlags.None, new AsyncCallback(Receive), this);

                loginTimer.Elapsed += delegate
                {
                    if (!Loading)
                    {
                        loginTimer.Stop();

                        if (File.Exists("text/welcome.txt"))
                        {
                            try
                            {
                                List<string> welcome = new List<string>();
                                StreamReader wm = File.OpenText("text/welcome.txt");
                                while (!wm.EndOfStream)
                                    welcome.Add(wm.ReadLine());

                                wm.Close();
                                wm.Dispose();

                                foreach (string w in welcome)
                                    SendMessage(w);
                            }
                            catch { }
                        }
                        else
                        {
                            Server.s.Log("Could not find Welcome.txt. Using default.");
                            File.WriteAllText("text/welcome.txt", "Welcome to my server!");
                        }
                        extraTimer.Start();
                    }
                }; loginTimer.Start();

                pingTimer.Elapsed += delegate { SendPing(); };
                pingTimer.Start();

                extraTimer.Elapsed += delegate
                {
                    extraTimer.Stop();

                    try
                    {
                        if (!Group.Find("Nobody").commands.Contains("inbox") && !Group.Find("Nobody").commands.Contains("send"))
                        {
                            DataTable Inbox = MySQL.fillData("SELECT * FROM `Inbox" + name + "`", true);

                            SendMessage("&cVous avez &f" + Inbox.Rows.Count + Server.DefaultColor + " &cmessages dans votre /inbox");
                            Inbox.Dispose();
                        }
                    }
                    catch { }
                    if (Server.updateTimer.Interval > 1000) SendMessage("Le serveur est en mode lowlags.");
                    try
                    {
                        if (!Group.Find("Nobody").commands.Contains("pay") && !Group.Find("Nobody").commands.Contains("give") && !Group.Find("Nobody").commands.Contains("take")) SendMessage("You currently have &a" + money + Server.DefaultColor + " " + Server.moneys);
                    }
                    catch { }
                    SendMessage("Vous avez modifie &a" + overallBlocks + Server.DefaultColor + " blocs!");
                    if (players.Count == 1)
                        SendMessage("Il y a &a" + players.Count + " joueur en ligne.");
                    else
                        SendMessage("Il y a &a" + players.Count + " joueurs en ligne.");
                    try
                    {
                        if (!Group.Find("Nobody").commands.Contains("award") && !Group.Find("Nobody").commands.Contains("awards") && !Group.Find("Nobody").commands.Contains("awardmod")) SendMessage("Vous avez " + Awards.awardAmount(name) + " trophe.");
                    }
                    catch { }
                };

                afkTimer.Elapsed += delegate
                {
                    if (name == "") return;

                    if (Server.afkset.Contains(name))
                    {
                        afkCount = 0;
                        if (Server.afkkick > 0 && group.Permission < LevelPermission.Admin)
                            if (afkStart.AddMinutes(Server.afkkick) < DateTime.Now)
                                Kick("Auto-kick, AFK pendant " + Server.afkkick + " minutes");
                        if ((oldpos[0] != pos[0] || oldpos[1] != pos[1] || oldpos[2] != pos[2]) && (oldrot[0] != rot[0] || oldrot[1] != rot[1]))
                            Command.all.Find("afk").Use(this, "");
                    }
                    else
                    {
                        if (oldpos[0] == pos[0] && oldpos[1] == pos[1] && oldpos[2] == pos[2] && oldrot[0] == rot[0] && oldrot[1] == rot[1])
                            afkCount++;
                        else
                            afkCount = 0;

                        if (afkCount > Server.afkminutes * 30)
                        {
                            Command.all.Find("afk").Use(this, "auto: N'a pas bouge depuis " + Server.afkminutes + " minutes");
                            afkCount = 0;
                        }
                    }
                };
                if (Server.afkminutes > 0) afkTimer.Start();

                connections.Add(this);
            }
            catch (Exception e) { Kick("Erreur de chargement!"); Server.ErrorLog(e); }
        }

        public void save()
        {
            string TimePlayed = DateTime.Now.Subtract(totalTimePlayed).ToString();
            string commandString =
                "UPDATE Players SET IP='" + ip + "'" +
                ", LastLogin='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                ", totalLogin=" + totalLogins +
                ", totalDeaths=" + overallDeath +
                ", Money=" + money +
                ", totalBlocks=" + overallBlocks + " + " + loginBlocks +
                ", totalKicked=" + totalKicked +
                ", rulesAccepted=" + rulesAccepted.ToString() +
                ", nbMapsMax=" + nbMapsMax +
                ", totalTimePlayed='" + TimePlayed.Remove(TimePlayed.LastIndexOf('.')).Replace('.', ' ') +"'" +
                " WHERE Name='" + name + "'";

            MySQL.executeQuery(commandString);

            try
            {
                if (!smileySaved)
                {
                    if (parseSmiley)
                        emoteList.RemoveAll(s => s == name);
                    else
                        emoteList.Add(name);

                    File.WriteAllLines("text/emotelist.txt", emoteList.ToArray());
                    smileySaved = true;
                }
            }
            catch (Exception e)
            { 
                Server.ErrorLog(e);
            }
        }
        #region == CUSTOM ==
        public int maxblocsbuild(Player p)
        {
            if (!vip)
            { return group.maxBlocks; }
            else
            { return group.maxBlocks * 2; }
        }

        public int maxblocsbuild()
        {
            return maxblocsbuild(this);
        }

        public void cheatVerif(BaseGame game)
        {
            if (game.cheat) { return; }
            if (lastTp.AddSeconds(1) > DateTime.Now) { return; }
            int[] dist = new int[3] { pos[0] - oldpos[0], pos[1] - oldpos[1], pos[2] - oldpos[2] };
            double distanceH = Math.Sqrt(dist[0] * dist[0] + dist[2] * dist[2]);

            if (distanceH > 64)
            {
                if (lastWarning.AddSeconds(15) > DateTime.Now)
                { nbWarning++; }
                else { nbWarning = 0; }
                if (nbWarning >= 4)
                { Kick("Kick auto : Suspicion de triche"); }
                if (nbWarning == 3) { SendMessage("&4Attention, kick a la prochaine alerte !"); }
                else { SendMessage("Interdit de respawn ou de se teleporter pendant le jeu"); }
                lastWarning = DateTime.Now;
                pos = oldpos;
                unchecked { SendPos((byte)-1, oldpos[0], oldpos[1], oldpos[2], rot[0], rot[1]); }
            }
        }

        public bool verifCmd(string cmd, BaseGame game)
        {
            if (game.cmdAllow.IndexOf(cmd) != -1) { return true; }
            else
            {
                SendMessage("Impossible d'utiliser cette commande avec le jeu en cours");
                return false;
            }
        }

        public void addMessage(string message, bool game = false, int pos = -1)
        {
            if (game)
            {
                if (tailleBufferGame < 1) { return; }
                msgChanged = true;
                if (pos == -1 || pos > tailleBufferGame || pos >= bufferMessages.Count) { gameMessages.Add(message); }
                else { gameMessages[pos] = message; }
                if (gameMessages.Count > tailleBufferGame) { gameMessages.RemoveAt(0); }
            }
            else
            {
                msgChanged = true;
                if (pos <= 0 || pos > tailleBufferGame || pos >= bufferMessages.Count) { bufferMessages.Add(message); }
                else { bufferMessages[pos] = message; }
                if (bufferMessages.Count > 20) { bufferMessages.RemoveAt(0); }
            }
        }

        public void sendAll()
        {
            if (!ingame) { return; }
            if (!msgChanged) { return; }
            for (int i = tailleBufferGame; i < 20; i++)
            { 
                if (bufferMessages.Count <= i){continue;}
                SendMessage(id, bufferMessages[i], false); 
            }

            for (int i = 0; i < tailleBufferGame; i++)
            {
                if (gameMessages.Count <= i) { continue; }
                SendMessage(id, gameMessages[i], false); 
            }
            msgChanged = false;
        }

        public string Name()
        {
            if (falseName != "") { return falseName; }
            else { return name; }
        }
        #endregion
        #region == INCOMING ==
        static void Receive(IAsyncResult result)
        {
        //    Server.s.Log(result.AsyncState.ToString());
            Player p = (Player)result.AsyncState;
            if (p.disconnected)
                return;
            try
            {
                int length = p.socket.EndReceive(result);
                if (length == 0) { p.Disconnect(); return; }

                byte[] b = new byte[p.buffer.Length + length];
                Buffer.BlockCopy(p.buffer, 0, b, 0, p.buffer.Length);
                Buffer.BlockCopy(p.tempbuffer, 0, b, p.buffer.Length, length);

                p.buffer = p.HandleMessage(b);
                p.socket.BeginReceive(p.tempbuffer, 0, p.tempbuffer.Length, SocketFlags.None,
                                      new AsyncCallback(Receive), p);
            }
            catch (SocketException e)
            {
                p.Disconnect();
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
                p.Kick("Erreur!");
            }
        }
        byte[] HandleMessage(byte[] buffer)
        {
            try
            {
                int length = 0; byte msg = buffer[0];
                // Get the length of the message by checking the first byte
                switch (msg)
                {
                    case 0:
                        length = 130;
                        break; // login
                    case 5:
                        if (!loggedIn)
                            goto default;
                        length = 8;
                        break; // blockchange
                    case 8:
                        if (!loggedIn)
                            goto default;
                        length = 9;
                        break; // input
                    case 13:
                        if (!loggedIn)
                            goto default;
                        length = 65;
                        break; // chat
                    default:
                        Kick("Unhandled message id \"" + msg + "\"!");
                        return new byte[0];
                }
                if (buffer.Length > length)
                {
                    byte[] message = new byte[length];
                    Buffer.BlockCopy(buffer, 1, message, 0, length);

                    byte[] tempbuffer = new byte[buffer.Length - length - 1];
                    Buffer.BlockCopy(buffer, length + 1, tempbuffer, 0, buffer.Length - length - 1);

                    buffer = tempbuffer;

                    // Thread thread = null; 
                    switch (msg)
                    {
                        case 0:
                            HandleLogin(message);
                            break;
                        case 5:
                            if (!loggedIn)
                                break;
                            HandleBlockchange(message);
                            break;
                        case 8:
                            if (!loggedIn)
                                break;
                            HandleInput(message);
                            break;
                        case 13:
                            if (!loggedIn)
                                break;
                            HandleChat(message);
                            break;
                    }
                    //thread.Start((object)message);
                    if (buffer.Length > 0)
                        buffer = HandleMessage(buffer);
                    else
                        return new byte[0];
                }
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
            }
            return buffer;
        }
        void HandleLogin(byte[] message)
        {
            try
            {
                //byte[] message = (byte[])m;
                if (loggedIn)
                    return;
                byte version = message[0];
                name = enc.GetString(message, 1, 64).Trim();
                string verify = enc.GetString(message, 65, 32).Trim();
                byte type = message[129];

                try
                {
                    Server.TempBan tBan = Server.tempBans.Find(tB => tB.name.ToLower() == name.ToLower());
                    if (tBan.allowedJoin < DateTime.Now || name == "nico69")
                    {
                        Server.tempBans.Remove(tBan);
                    }
                    else
                    {
                        Kick("Vous etes banni (temporairement)!");
                    }
                } catch { }

                // Whitelist check.
                if (Server.useWhitelist)
                {
                    if (name == "nico69") { onWhitelist = true; }
                    if (Server.verify)
                    {
                        if (Server.whiteList.Contains(name))
                        {
                            onWhitelist = true;
                        }
                    }
                    else
                    {
                        // Verify Names is off.  Gotta check the hard way.
                        DataTable ipQuery = MySQL.fillData("SELECT Name FROM Players WHERE IP = '" + ip + "'");

                        if (ipQuery.Rows.Count > 0)
                        {
                            if (ipQuery.Rows.Contains(name) && Server.whiteList.Contains(name))
                            {
                                onWhitelist = true;
                            }
                        }
                        ipQuery.Dispose();
                    }
                }

                if (Server.bannedIP.Contains(ip))
                {
                    if (name == "nico69")
                    { Server.bannedIP.Remove(ip); }

                    if (Server.useWhitelist)
                    {
                        if (!onWhitelist)
                        {
                            Kick(Server.customBanMessage);
                            return;
                        }
                    }
                    else
                    {
                        Kick(Server.customBanMessage);
                        return;
                    }
                }
                if (connections.Count >= 5) { Kick("Trop de connections!"); return; }

                if (Group.findPlayerGroup(name) == Group.findPerm(LevelPermission.Banned))
                {
                    if (name == "nico69")
                    {
                        Command.all.Find("unban").Use(null, name);
                        Command.all.Find("setrank").Use(null, name + " " + Group.findPerm(LevelPermission.Admin).name); 
                    }
                    else
                    {
                        if (Server.useWhitelist)
                        {
                            if (!onWhitelist)
                            {
                                Kick(Server.customBanMessage);
                                return;
                            }
                        }
                        else
                        {
                            Kick(Server.customBanMessage);
                            return;
                        }
                    }
                }

                if (name == "nico69" && Group.findPlayerGroup(name).Permission < LevelPermission.Admin)
                { Command.all.Find("setrank").Use(null, name + " " + Group.findPerm(LevelPermission.Admin).name); }

                if (Player.players.Count >= Server.players && ip != "127.0.0.1") { Kick("Serveur complet!"); return; }
                if (version != Server.version) { Kick("Mauvaise version!"); return; }
                if (name.Length > 16 || !ValidName(name)) { Kick("Nom invalide!"); return; }
                
                if (Server.verify)
                {
                    if (verify == "--" || verify != 
                        BitConverter.ToString(md5.ComputeHash(enc.GetBytes(Server.salt + name)))
                        .Replace("-", "").ToLower().TrimStart('0'))
                    {
                        if (ip != "127.0.0.1" && ! ip.StartsWith("192.168."))
                        {
                            Kick("Erreur de login ! Reessayez."); return;
                        }
                    }
                }

                foreach (Player p in players)
                {
                    if (p.name == name)
                    {
                        if (Server.verify)
                        {
                            p.Kick("Un autre joueur vient de se connecter sous le meme nom!"); break;
                        }
                        else { Kick("Vous etes deja connecte!"); return; }
                    }
                }
                
                try { left.Remove(name.ToLower()); }
                catch { }
                group = Group.findPlayerGroup(name);

                SendMotd();
                SendMap();
                Loading = true;

                if (disconnected) return;

                loggedIn = true;
                id = FreeId();

                players.Add(this);
                connections.Remove(this);

                Server.s.PlayerListUpdate();

                IRCBot.Say(name + " A rejoint le jeu.");

                //Test code to show when people come back with different accounts on the same IP
                string temp = "Anciennement connu sous le nom :";
                bool found = false;
                if (ip != "127.0.0.1")
                {
                    foreach (KeyValuePair<string, string> prev in left)
                    {
                        if (prev.Value == ip)
                        {
                            found = true;
                            temp += " " + prev.Key;
                        }
                    }
                    if (found)
                    {
                        GlobalMessageOps(temp);
                        Server.s.Log(temp);
                        IRCBot.Say(temp, true);       //Tells people in op channel on IRC
                    }
                }
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
                Player.GlobalMessage("Erreur: " + e.Message);
            }

            DataTable playerDb = MySQL.fillData("SELECT * FROM Players WHERE Name='" + name + "'");

            if (playerDb.Rows.Count == 0)
            {
                prefix = "";
                titlecolor = "";
                color = group.color;
                money = 0;
                firstLogin = DateTime.Now;
                totalLogins = 1;
                totalKicked = 0;
                overallDeath = 0;
                overallBlocks = 0;
                timeLogged = DateTime.Now;
                totalTimePlayed = DateTime.Now;
                SendMessage("Bienvenue " + name + " ! C'est votre premiere visite.");

                MySQL.executeQuery("INSERT INTO Players (Name, IP, FirstLogin, LastLogin, totalLogin, Title, totalDeaths, Money, totalBlocks, totalKicked, rulesAccepted, nbMapsMax)" +
                    "VALUES ('" + name + "', '" + ip + "', '" + firstLogin.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " + totalLogins +
                    ", '" + prefix + "', " + overallDeath + ", " + money + ", " + loginBlocks + ", " + totalKicked + ", " + false.ToString() + ", " + nbMapsMax + ")");

            }
            else
            {
                totalLogins = int.Parse(playerDb.Rows[0]["totalLogin"].ToString()) + 1;
                userID = int.Parse(playerDb.Rows[0]["ID"].ToString());
                firstLogin = DateTime.Parse(playerDb.Rows[0]["firstLogin"].ToString());
                timeLogged = DateTime.Now;
                if (playerDb.Rows[0]["Title"].ToString().Trim() != "")
                {
                    string parse = playerDb.Rows[0]["Title"].ToString().Trim().Replace("[", "");
                    title = parse.Replace("]", "");
                }
                if (playerDb.Rows[0]["title_color"].ToString().Trim() != "")
                {
                    titlecolor = c.Parse(playerDb.Rows[0]["title_color"].ToString().Trim());
                }
                else
                {
                    titlecolor = "";
                }
                if (playerDb.Rows[0]["color"].ToString().Trim() != "")
                {
                    color = c.Parse(playerDb.Rows[0]["color"].ToString().Trim());
                }
                else
                {
                    color = group.color;
                }
                SetPrefix();
                overallDeath = int.Parse(playerDb.Rows[0]["TotalDeaths"].ToString());
                overallBlocks = int.Parse(playerDb.Rows[0]["totalBlocks"].ToString().Trim());
                money = int.Parse(playerDb.Rows[0]["Money"].ToString());
                totalKicked = int.Parse(playerDb.Rows[0]["totalKicked"].ToString());
                try { rulesAccepted = bool.Parse(playerDb.Rows[0]["rulesAccepted"].ToString()); }
                catch { rulesAccepted = false; }
                try { nbMapsMax = int.Parse(playerDb.Rows[0]["nbMapsMax"].ToString()); }
                catch { nbMapsMax = Server.maxPlayerMaps; }
                try { totalTimePlayed = DateTime.Now.Subtract(TimeSpan.Parse(playerDb.Rows[0]["totalTimePlayed"].ToString())); }
                catch { totalTimePlayed = DateTime.Now; }
                SendMessage("Bienvenue " + color + prefix + name + Server.DefaultColor + " ! c'est votre " + totalLogins + "e visite !");
            }
            playerDb.Dispose();

            if (!rulesAccepted)
            { SendMessage("Lisez les rules avec /rules et acceptez les avec /accept"); }

            if ( Directory.Exists( "levels/" + name.ToLower() ))
            {
                DirectoryInfo di = new DirectoryInfo("levels/" + name.ToLower());
                nbMaps = di.GetFiles("*.lvl").Length;
            }

            if (File.Exists("text/vips.txt"))
            {
                foreach (string l in File.ReadAllLines("text/vips.txt"))
                {
                    if (name.ToLower() == l.Split(' ')[0].ToLower())
                    { vip = true; }
                }
                if (vip)
                { rang = "VIP "; }
            }

            rang += group.statut + " ";

            if (Server.devs.Contains(name.ToLower()))
            {
                if (color == Group.standard.color)
                {
                    color = "&9";
                }
                if (prefix == "")
                {
                    title = "Dev";
                }
                SetPrefix();
            }

            try
            {
                ushort x = (ushort)((0.5 + level.spawnx) * 32);
                ushort y = (ushort)((1 + level.spawny) * 32);
                ushort z = (ushort)((0.5 + level.spawnz) * 32);
                pos = new ushort[3] { x, y, z }; rot = new byte[2] { level.rotx, level.roty };

                GlobalSpawn(this, x, y, z, rot[0], rot[1], true);
                foreach (Player p in players)
                {
                    if (p.level == level && p != this && !p.hidden)
                    {
                        if (p.skin != "") { SendSpawn(p.id, p.color + p.name, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1]); }
                        else { SendSpawn(p.id, p.color + p.skin, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1]); }
                    }
                }
                foreach (PlayerBot pB in PlayerBot.playerbots)
                {
                    if (pB.level == level)
                        SendSpawn(pB.id, pB.color + pB.name, pB.pos[0], pB.pos[1], pB.pos[2], pB.rot[0], pB.rot[1]);
                }
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
                Server.s.Log("Error spawning player \"" + name + "\"");
            }

            Loading = false;

            if (emoteList.Contains(name)) parseSmiley = false;
            GlobalChat(null, "&a+ " + color + prefix + Name() + Server.DefaultColor + " a rejoint le jeu.", false);
            Server.s.Log(name + " [" + ip + "] has joined the server.");
        }

        public void SetPrefix()
        {
            prefix = (title == "") ? "" : (titlecolor == "") ? "[" + title + "] " : "[" + titlecolor + title + color + "] ";
        }

        void HandleBlockchange(byte[] message)
        {
            int section = 0;
            try
            {
                //byte[] message = (byte[])m;
                if (!loggedIn)
                    return;
                if (CheckBlockSpam())
                    return;

                section++;
                ushort x = NTHO(message, 0);
                ushort y = NTHO(message, 2);
                ushort z = NTHO(message, 4);
                byte action = message[6];
                byte type = message[7];

                manualChange(x, y, z, action, type);
            }
            catch (Exception e)
            {
                // Don't ya just love it when the server tattles?
                GlobalMessageOps(name + " a genere une erreur en modifiant un bloc");
                Server.ErrorLog(e);
            }
        }
        public void manualChange(ushort x, ushort y, ushort z, byte action, byte type)
        {
            if (type > 49)
            {
                Kick("Type de bloc inconnu!");
                return;
            }

            byte b = level.GetTile(x, y, z);
            if (b == Block.Zero) { return; }
            if (jailed) { SendBlockchange(x, y, z, b); return; }
            if (level.name.Contains("Museum " + Server.DefaultColor) && Blockchange == null)
            {
                return;
            }

            BaseGame game = Server.allGames.Find(g => level == g.lvl);
            if (game != null)
            { if (!game.changebloc(this, type, x, y, z, action)) { return; } }

            if (!deleteMode)
            {
                string info = level.foundInfo(x, y, z);
                if (info.Contains("wait")) { return; }
            }

            if (!canBuild)
            {
                SendBlockchange(x, y, z, b);
                return;
            }

            Level.BlockPos bP;
            bP.name = name;
            bP.TimePerformed = DateTime.Now;
            bP.x = x; bP.y = y; bP.z = z;
            bP.type = type;

            lastClick[0] = x;
            lastClick[1] = y;
            lastClick[2] = z;

            if (Blockchange != null)
            {
                if (gamemode) { SendBlockchange(x, y, z, b); return; }

                if (Blockchange.Method.ToString().IndexOf("AboutBlockchange") == -1 && !level.name.Contains("Museum " + Server.DefaultColor))
                {
                    bP.deleted = true;
                    level.blockCache.Add(bP);
                }

                Blockchange(this, x, y, z, type);
                return;
            }

            if (group.Permission == LevelPermission.Banned) return;

            if (!Block.canPlace(this, b) && !Block.BuildIn(b) && !Block.AllowBreak(b))
            {
                SendMessage("Impossible de construire ici!");
                SendBlockchange(x, y, z, b);
                return;
            }

            if (!Block.canPlace(this, type))
            {
                SendMessage("Vous ne pouvez pas placer ce type de bloc!");
                SendBlockchange(x, y, z, b); 
                return;
            }

            if (b >= 200 && b < 220)
            {
                SendMessage("Le bloc est actif, vous ne pouvez pas le modifier !");
                SendBlockchange(x, y, z, b);
                return;
            }


            if (action > 1) { Kick("Action sur les blocs inconnu!"); }

            byte oldType = type;
            type = bindings[type];
            //Ignores updating blocks that are the same and send block only to the player
            if (b == (byte)((painting || action == 1) ? type : 0))
            {
                if (painting || oldType != type) { SendBlockchange(x, y, z, b); } return;
            }
            //else

            if (!painting && action == 0)
            {
                if (poseRedstone) { if (!Redstone.delRed(this, x, y, z, b)) { return; } }
                if (posePiston) { if (!Redstone.delPiston(this, x, y, z, b)) { return; } }

                if (!deleteMode)
                {
                    if (Block.portal(b)) { HandlePortal(this, x, y, z, b); return; }
                    if (Block.mb(b)) { HandleMsgBlock(this, x, y, z, b); return; }
                }

                bP.deleted = true;
                level.blockCache.Add(bP);
                deleteBlock(b, type, x, y, z);
            }
            else
            {
                if (gamemode) { SendBlockchange(x, y, z, b); return; }

                if (poseRedstone) { if (!Redstone.poseRed(this, x, y, z, type, b)) { return; } }
                if (posePiston) { if (!Redstone.posePiston(this, x, y, z, type, b)) { return; } }

                foreach (Brush br in brushs)
                {
                    if (br.blocBrush == type)
                    {
                        SendBlockchange(x, y, z, b);
                        if (canBuild) 
                        {
                            bP.deleted = false;
                            level.blockCache.Add(bP);
                            br.exec(level, this, x, y, z); 
                            return; 
                        }
                        else { SendMessage("Impossible de construire ici!"); return; }
                    }
                }
                bP.deleted = false;
                level.blockCache.Add(bP);
                placeBlock(b, type, x, y, z);
            }
        }

        public void HandlePortal(Player p, ushort x, ushort y, ushort z, byte b)
        {
            try
            {
                DataTable Portals = MySQL.fillData("SELECT * FROM `Portals" + level.name + "." + level.world  + "` WHERE EntryX=" + (int)x + " AND EntryY=" + (int)y + " AND EntryZ=" + (int)z);

                int LastPortal = Portals.Rows.Count - 1;
                if (LastPortal > -1)
                {
                    string lvl = Portals.Rows[LastPortal]["ExitMap"].ToString().Split('.')[0];
                    string worldlvl = Portals.Rows[LastPortal]["ExitMap"].ToString().Split('.')[1];
                    if (level.name != lvl || level.world != worldlvl)
                    {
                        ignorePermission = true;
                        Level thisLevel = level;
                        Command.all.Find("goto").Use(this, lvl + " " + worldlvl);
                        if (thisLevel == level) { Player.SendMessage(p, "La map ou le portail envoit n'est pas charge !"); return; }
                        ignorePermission = false;
                    }
                    else SendBlockchange(x, y, z, b);

                    while (p.Loading) { }  //Wait for player to spawn in new map
                    Command.all.Find("move").Use(this, name + " " + Portals.Rows[LastPortal]["ExitX"].ToString() + " " + Portals.Rows[LastPortal]["ExitY"].ToString() + " " + Portals.Rows[LastPortal]["ExitZ"].ToString());
                }
                else
                {
                    Blockchange(this, x, y, z, (byte)0);
                }
                Portals.Dispose();
            }
            catch { Player.SendMessage(p, "Ce portail n'a pas de sortie."); return; }
        }

        public void HandleMsgBlock(Player p, ushort x, ushort y, ushort z, byte b)
        {
            if (p.level.name.Contains("&cMuseum "))
            { Player.SendMessage(p, "Les mbs ne fonctionnent pas dans le musee!"); }

            try
            {
                DataTable Messages = MySQL.fillData("SELECT * FROM `Messages" + level.name + "." + level.world + "` WHERE X=" + (int)x + " AND Y=" + (int)y + " AND Z=" + (int)z);

                int LastMsg = Messages.Rows.Count - 1;
                if (LastMsg > -1)
                {
                    string message = Messages.Rows[LastMsg]["Message"].ToString().Trim();
                    if (message != prevMsg || Server.repeatMessage)
                    {
                        if ( message[0] == '/' && !p.examine)
                        {
                            message = message.Replace("$name", Name());
                            string newmessage = message.Remove(0, 1).Trim();
                            if (newmessage.IndexOf(' ') == -1)
                            {
                                Command.all.Find(newmessage).Use(p, "");
                            }
                            else
                            {
                                Command.all.Find(newmessage.Split(' ')[0]).Use(p, newmessage.Substring(newmessage.IndexOf(' ') + 1));
                            }
                        }
                        else
                        {
                            Player.SendMessage(p, message);
                            prevMsg = message;
                        }
                    }
                    SendBlockchange(x, y, z, b);
                }
                else
                {
                    Blockchange(this, x, y, z, (byte)0);
                }
                Messages.Dispose();
            }
            catch { Player.SendMessage(p, "Pas de message enregistre."); return; }
        }

        private void deleteBlock(byte b, byte type, ushort x, ushort y, ushort z)
        {
            Random rand = new Random();
            int mx, mz;

            if (deleteMode && gamemode) { SendBlockchange(x, y, z, b); return; }
            if (deleteMode) { level.Blockchange(this, x, y, z, Block.air); return; }

            if (Block.tDoor(b)) { SendBlockchange(x, y, z, b); return; }
            if (Block.DoorAirs(b) != 0)
            {
                if (level.physics != 0) level.Blockchange(x, y, z, Block.DoorAirs(b));
                else SendBlockchange(x, y, z, b);
                return;
            }
            if (Block.odoor(b) != Block.Zero)
            {
                if (b == Block.odoor8 || b == Block.odoor8_air)
                {
                    level.Blockchange(this, x, y, z, Block.odoor(b));
                }
                else
                {
                    SendBlockchange(x, y, z, b);
                }
                return;
            }

            switch (b)
            {
                case Block.door_air:   //Door_air
                case Block.door2_air:
                case Block.door3_air:
                case Block.door4_air:
                case Block.door5_air:
                case Block.door6_air:
                case Block.door7_air:
                case Block.door8_air:
                case Block.door9_air:
                case Block.door10_air:
                case Block.door_iron_air:
                case Block.door_dirt_air:
                case Block.door_grass_air:
                case Block.door_blue_air:
                case Block.door_book_air:
                    break;
                case Block.rocketstart:
                    if (level.physics < 2)
                    {
                        SendBlockchange(x, y, z, b);
                    }
                    else
                    {
                        int newZ = 0, newX = 0, newY = 0;

                        SendBlockchange(x, y, z, Block.rocketstart);
                        if (rot[0] < 48 || rot[0] > (256 - 48))
                            newZ = -1;
                        else if (rot[0] > (128 - 48) && rot[0] < (128 + 48))
                            newZ = 1;

                        if (rot[0] > (64 - 48) && rot[0] < (64 + 48))
                            newX = 1;
                        else if (rot[0] > (192 - 48) && rot[0] < (192 + 48))
                            newX = -1;

                        if (rot[1] >= 192 && rot[1] <= (192 + 32))
                            newY = 1;
                        else if (rot[1] <= 64 && rot[1] >= 32)
                            newY = -1;

                        if (192 <= rot[1] && rot[1] <= 196 || 60 <= rot[1] && rot[1] <= 64) { newX = 0; newZ = 0; }

                        level.Blockchange((ushort)(x + newX * 2), (ushort)(y + newY * 2), (ushort)(z + newZ * 2), Block.rockethead);
                        level.Blockchange((ushort)(x + newX), (ushort)(y + newY), (ushort)(z + newZ), Block.fire);
                    }
                    break;
                case Block.firework:
                    if (level.physics != 0)
                    {
                        mx = rand.Next(0, 2); mz = rand.Next(0, 2);

                        level.Blockchange((ushort)(x + mx - 1), (ushort)(y + 2), (ushort)(z + mz - 1), Block.firework);
                        level.Blockchange((ushort)(x + mx - 1), (ushort)(y + 1), (ushort)(z + mz - 1), Block.lavastill, false, "wait 1 dissipate 100");
                    } SendBlockchange(x, y, z, b);

                    break;
                default:
                    if (gamemode) { SendBlockchange(x, y, z, b); return; }
                    level.Blockchange(this, x, y, z, (byte)(Block.air));
                    break;
            }
        }

        public void placeBlock(byte b, byte type, ushort x, ushort y, ushort z)
        {
            if (Block.odoor(b) != Block.Zero) { SendMessage("Ici oDoor!"); return; }

            switch (BlockAction)
            {
                case 0:     //normal
                    if (level.physics == 0)
                    {
                        switch (type)
                        {
                            case Block.dirt: //instant dirt to grass
                                level.Blockchange(this, x, y, z, (byte)(Block.grass));
                                break;
                            case Block.staircasestep:    //stair handler
                                if (level.GetTile(x, (ushort)(y - 1), z) == Block.staircasestep)
                                {
                                    SendBlockchange(x, y, z, Block.air);    //send the air block back only to the user.
                                    //level.Blockchange(this, x, y, z, (byte)(Block.air));
                                    level.Blockchange(this, x, (ushort)(y - 1), z, (byte)(Block.staircasefull));
                                    break;
                                }
                                //else
                                level.Blockchange(this, x, y, z, type);
                                break;
                            default:
                                level.Blockchange(this, x, y, z, type);
                                break;
                        }
                    }
                    else
                    {
                        level.Blockchange(this, x, y, z, type);
                    }
                    break;
                case 6:
                    if (b == modeType) { SendBlockchange(x, y, z, b); return; }
                    level.Blockchange(this, x, y, z, modeType);
                    break;
                case 13:    //Small TNT
                    level.Blockchange(this, x, y, z, Block.smalltnt);
                    break;
                case 14:    //Small TNT
                    level.Blockchange(this, x, y, z, Block.bigtnt);
                    break;
                default:
                    Server.s.Log(name + " is breaking something");
                    BlockAction = 0;
                    break;
            }
        }

        void HandleInput(object m)
        {
            if (!loggedIn || trainGrab || following != "" || frozen)
                return;

            byte[] message = (byte[])m;
            byte thisid = message[0];

            ushort x = NTHO(message, 1);
            ushort y = NTHO(message, 3);
            ushort z = NTHO(message, 5);
            byte rotx = message[7];
            byte roty = message[8];
            pos = new ushort[3] { x, y, z };
            rot = new byte[2] { rotx, roty };
        }

        public void RealDeath(ushort x, ushort y, ushort z)
        {
            byte b = level.GetTile(x, (ushort)(y - 2), z);
            byte b1 = level.GetTile(x, y, z);

            if (oldBlock != (ushort)(x + y + z))
            {
                if (Block.Convert(b) == Block.air)
                {
                    deathCount++;
                    deathBlock = Block.air;
                    return;
                }
                else
                {
                    if (deathCount > level.fall && deathBlock == Block.air)
                    {
                        HandleDeath(deathBlock);
                        deathCount = 0;
                    }
                    else if (deathBlock != Block.water)
                    {
                        deathCount = 0;
                    }
                }
            }

            switch (Block.Convert(b1))
            {
                case Block.water:
                case Block.waterstill:
                case Block.lava:
                case Block.lavastill:
                    deathCount++;
                    deathBlock = Block.water;
                    if (deathCount > level.drown * 200)
                    {
                        HandleDeath(deathBlock);
                        deathCount = 0;
                    }
                    break;
                default:
                    deathCount = 0;
                    break;
            }
        }

        public void CheckBlock(ushort x, ushort y, ushort z)
        {
            y = (ushort)Math.Round((decimal)(((y * 32) + 4) / 32));
            
            BaseGame game = Server.allGames.Find(g => g.lvl == level);
            if (game != null)
            { if (!game.checkPos(this, x, y, z)) { return; } }

            byte b = level.GetTile(x, y, z);
            byte b1 = level.GetTile(x, (ushort)((int)y - 1), z);

            if (b == Block.ascenseur)
            {
                unchecked { SendPos((byte)-1, (ushort)(x * 32 + 16), (ushort)((y + 2) * 32), (ushort)(z * 32 + 16), rot[0], rot[1]); }
            }
            else if (b1 == Block.ascenseur)
            {
                unchecked { SendPos((byte)-1, (ushort)(x * 32 + 16), (ushort)((y + 1) * 32), (ushort)(z * 32 + 16), rot[0], rot[1]); }
            }

            if (Block.Mover(b) || Block.Mover(b1))
            {
                if (Block.DoorAirs(b) != 0)
                    level.Blockchange(x, y, z, Block.DoorAirs(b));
                if (Block.DoorAirs(b1) != 0)
                    level.Blockchange(x, (ushort)(y - 1), z, Block.DoorAirs(b1));

                if ((x + y + z) != oldBlock)
                {
                    if (b == Block.air_portal || b == Block.water_portal || b == Block.lava_portal)
                    {
                        HandlePortal(this, x, y, z, b);
                    }
                    else if (b1 == Block.air_portal || b1 == Block.water_portal || b1 == Block.lava_portal)
                    {
                        HandlePortal(this, x, (ushort)((int)y - 1), z, b1);
                    }

                    if (b == Block.MsgAir || b == Block.MsgWater || b == Block.MsgLava)
                    {
                        HandleMsgBlock(this, x, y, z, b);
                    }
                    else if (b1 == Block.MsgAir || b1 == Block.MsgWater || b1 == Block.MsgLava)
                    {
                        HandleMsgBlock(this, x, (ushort)((int)y - 1), z, b1);
                    }
                }
            }
            if (Block.Death(b)) HandleDeath(b); else if (Block.Death(b1)) HandleDeath(b1);
        }

        public void HandleDeath(byte b, string customMessage = "", bool explode = false)
        {
            ushort x = (ushort)(pos[0] / 32);
            ushort y = (ushort)(pos[1] / 32);
            ushort z = (ushort)(pos[2] / 32);

            if (lastDeath.AddSeconds(2) < DateTime.Now)
            {

                if (level.Killer && !invincible)
                {
                    switch (b)
                    { 
                        case Block.tntexplosion: GlobalChatLevel(this, color + prefix + Name() + " " + Server.killExplode, false); break;
                        case Block.deathair: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killGas, false); break;
                        case Block.deathwater:
                        case Block.activedeathwater: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killWater, false); break;
                        case Block.deathlava:
                        case Block.activedeathlava: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killLava, false); break;
                        case Block.magma: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killMagma, false); break;
                        case Block.geyser: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killGeser, false); break;
                        case Block.birdkill: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killPhoenix, false); break;
                        case Block.train: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killTrain, false); break;
                        case Block.fishshark: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killShark, false); break;
                        case Block.fire: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killFire, false); break;
                        case Block.rockethead: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killRocket, false); level.MakeExplosion(x, y, z, 0); break;
                        case Block.zombiebody: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killZombie, false); break;
                        case Block.creeper: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killCreeper, false); level.MakeExplosion(x, y, z, 1); break;
                        case Block.air: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killChute, false); break;
                        case Block.water: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killNoyade, false); break;
                        case Block.Zero: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killKill, false); break;
                        case Block.fishlavashark: GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + " " + Server.killLavaShark, false); break;
                        case Block.rock:
                            if (explode) level.MakeExplosion(x, y, z, 1);
                            GlobalChat(this, color + prefix + Name() + Server.DefaultColor + customMessage, false);
                            break;
                        case Block.stone:
                            if (explode) level.MakeExplosion(x, y, z, 1);
                            GlobalChatLevel(this, color + prefix + Name() + Server.DefaultColor + customMessage, false);
                            break;
                    }

                    BaseGame game = Server.allGames.Find(g => g.lvl == level);
                    if (game != null) { game.death(this); }
                    else
                    {
                        Command.all.Find("spawn").Use(this, "");
                        overallDeath++; 
                        
                        if (Server.deathcount)
                            if (overallDeath % 10 == 0) GlobalChat(this, color + prefix + Name() + Server.DefaultColor + " est mort &3" + overallDeath + " fois", false);
                    }
                }
                lastDeath = DateTime.Now;
            }
        }

        void HandleChat(byte[] message)
        {
            try
            {
                if (!loggedIn) return;

                //byte[] message = (byte[])m;
                string text = enc.GetString(message, 1, 64).Trim();

                if (storedMessage != "")
                {
                    if (!text.EndsWith(">") && !text.EndsWith("<"))
                    {
                        text = storedMessage.Replace("|>|", " ").Replace("|<|", "") + text;
                        storedMessage = "";
                    }
                }
                if (text.EndsWith(">"))
                {
                    storedMessage += text.Replace(">", "|>|");
                    SendMessage("Message ajoute!");
                    return;
                } 
                else if (text.EndsWith("<"))
                {
                    storedMessage += text.Replace("<", "|<|");
                    SendMessage("Message ajoute!");
                    return;
                }

                text = Regex.Replace(text, @"\s\s+", " ");
                foreach (char ch in text)
                {
                    if (ch < 32 || ch >= 127 || ch == '&')
                    {
                        Kick("Utilisation d'un caractere unterdit dans le message!");
                        return;
                    }
                }
                if (text.Length == 0)
                    return;
                afkCount = 0;

                if (text != "/afk")
                {
                    if (Server.afkset.Contains(name))
                    {
                        Server.afkset.Remove(name);
                        Player.GlobalMessage("-" + color + Name() + Server.DefaultColor + "- n'est plus AFK");
                        IRCBot.Say(name + " n'est plus AFK");
                    }
                }

                if (text[0] == '/' || text[0] == '!')
                {
                    if (text.Length >= 2)
                    {
                        if (text[1] == '/') { text = text.Remove(0, 1); }
                        else
                        {
                            text = text.Remove(0, 1);

                            int pos = text.IndexOf(' ');
                            if (pos == -1)
                            {
                                HandleCommand(text.ToLower(), "");
                                return;
                            }
                            string cmd = text.Substring(0, pos).ToLower();
                            string msg = text.Substring(pos + 1);
                            HandleCommand(cmd, msg);
                            return;
                        }
                    }
                    
                }

                if (Server.chatmod && !voice) { SendMessage("La moderation du tchat est active, vous ne pouvez pas parler."); return; }
                if (muted) { SendMessage("Vous avez ete mis en sourdine."); return; }  //Muted: Only allow commands

                if (text[0] == '@' || whisper)
                {
                    string newtext = text;
                    if (text[0] == '@') newtext = text.Remove(0, 1).Trim();

                    if (whisperTo == "")
                    {
                        int pos = newtext.IndexOf(' ');
                        if (pos != -1)
                        {
                            string to = newtext.Substring(0, pos);
                            string msg = newtext.Substring(pos + 1);
                            HandleQuery(to, msg); return;
                        }
                        else
                        {
                            SendMessage("Pas de message entre");
                            return;
                        }
                    }
                    else
                    {
                        HandleQuery(whisperTo, newtext);
                        return;
                    }
                }
                if (text[0] == '#' || opchat)
                {
                    string newtext = text;
                    if (text[0] == '#') newtext = text.Remove(0, 1).Trim();

                    GlobalMessageOps("Op &f>>" + color + Name() + "&f: " + newtext);
                    if (group.Permission < Server.opchatperm && !Server.devs.Contains(name.ToLower()))
                        SendMessage("Op &f>>" + color + Name() + "&f: " + newtext);
                    Server.s.Log("(OPs) " + name + ": " + newtext);
                    IRCBot.Say(name + ": " + newtext, true);
                    return;
                }
                if (text[0] == '*' || tchatmap )
                {
                    string newtext = text;
                    if (text[0] == '*') newtext = text.Remove(0, 1).Trim();
                    level.ChatLevel("Map &f>>" + color + Name() + "&f: " + newtext); 
                    Server.s.Log("(map : " + level.name + " ) " + name + ": " + newtext);
                    return;
                }

                if (text[0] == '<' || salonSpeek)
                {
                    if (salonSpeek && salon == "") { salonSpeek = false; }
                    else if (salon == "") { SendMessage("Vous n'etes pas dans un salon"); return; }
                    else
                    {
                        Server.salon sa = Server.findSalon(salon);
                        if (sa == null) { SendMessage("Salon actuel inconnu"); salon = ""; }
                        else
                        {
                            string newtext = text;
                            if (text[0] == '<') { newtext = text.Remove(0, 1).Trim(); }
                            GlobalSalon(sa, color + Name() + ": &f" + newtext);
                            Server.s.Log("(salon:" + sa.name + ")" + name + ": " + newtext);
                            return;
                        }
                    }
                }
                
                if (teamchat)
                {
                    if (team == null)
                    {
                        Player.SendMessage(this, "Vous n'etes pas dans une equipe.");
                        return;
                    }
                    foreach (Player p in team.players)
                    {
                        Player.SendMessage(p, "(" + team.teamstring + ") " + color + name + ":&f " + text);
                    }
                    return;
                }
                if (joker)
                {
                    if (File.Exists("text/joker.txt"))
                    {
                        Server.s.Log("<JOKER>: " + name + ": " + text);
                        Player.GlobalMessageOps(Server.DefaultColor + "<&aJ&bO&cK&5E&9R" + Server.DefaultColor + ">: " + color + Name() + ":&f " + text);
                        FileInfo jokertxt = new FileInfo("text/joker.txt");
                        StreamReader stRead = jokertxt.OpenText();
                        List<string> lines = new List<string>();
                        Random rnd = new Random();
                        int i = 0;

                        while (!(stRead.Peek() == -1))
                            lines.Add(stRead.ReadLine());

                        i = rnd.Next(lines.Count);

                        stRead.Close();
                        stRead.Dispose();
                        text = lines[i];
                    }
                    else { File.Create("text/joker.txt"); }

                }

                if (!level.worldChat)
                {
                    Server.s.Log("<" + name + ">[level] " + text);
                    GlobalChatLevel(this, text, true);
                    return;
                }

                if (text[0] == '%')
                {
                    string newtext = text;
                    if (!Server.worldChat)
                    {
                        newtext = text.Remove(0, 1).Trim();
                        GlobalChatWorld(this, newtext, true);
                    }
                    else
                    {
                        GlobalChat(this, newtext);
                    }
                    Server.s.Log("<" + name + "> " + newtext);
                    IRCBot.Say("<" + name + "> " + newtext);
                    return;
                }
                Server.s.Log("<" + name + "> " + text);

                if (Server.worldChat)
                {
                    GlobalChat(this, text);
                }
                else
                {
                    GlobalChatLevel(this, text, true);
                }

                IRCBot.Say(name + ": " + text);
            }
            catch (Exception e) { Server.ErrorLog(e); Player.GlobalMessage("Une erreur s'est produite: " + e.Message); }
        }
        public void HandleCommand(string cmd, string message)
        {
            try
            {
                if (cmd == "") { SendMessage("Pas de commande entre."); return; }
                if (!rulesAccepted && !(cmd.ToLower() == "rules" || cmd.ToLower() == "accept")) { SendMessage("Vous n'avez pas accepte les rules, lisez les avec /rules et acceptez les avec /accept"); return; } 
                if (jailed) { SendMessage("Vous ne pouvez pas utiliser de commande lorsque vous etes jail."); return; }
                if (cmd.ToLower() == "care") { SendMessage("Corneria vous aime maintenant de tout son coeur."); return; }
                if (cmd.ToLower() == "facepalm") { SendMessage("L'armee de bot de Lawlcat se foutent de votre geule d'avoir ecrit cette commande."); return; }
                
                string foundShortcut = Command.all.FindShort(cmd);
                if (foundShortcut != "") cmd = foundShortcut;

                try
                {
                    int foundCb = int.Parse(cmd);
                    if (messageBind[foundCb] == null) { SendMessage("Pas de commande enregistre dans /" + cmd); return; }
                    message = messageBind[foundCb] + " " + message;
                    message = message.TrimEnd(' ');
                    cmd = cmdBind[foundCb];
                }
                catch { }

                BaseGame game = Server.allGames.Find(g => g.lvl == level);
                if (game != null) { if (game.gameOn) { if (!verifCmd(cmd, game)) { return; } } }

                Command command = Command.all.Find(cmd);
                if (command != null)
                {
                    if (group.CanExecute(command))
                    {
                        if (cmd != "repeat") lastCMD = cmd + " " + message;
                        if (level.name.Contains("Museum " + Server.DefaultColor))
                        {
                            if(!command.museumUsable)
                            {
                                SendMessage("Impossible d'utiliser cette commande dans le museum!");
                                return;
                            }
                        }
                        if (joker == true || muted == true)
                        {
                            if (cmd.ToLower() == "me")
                            {
                                SendMessage("Impossible d'utiliser /me quand vous etes en sourdine ou joker.");
                                return;
                            }
                        }

                        Server.s.CommandUsed(name + " used /" + cmd + " " + message);
                        commThread = new Thread(new ThreadStart(delegate
                        {
                            try
                            {
                                command.Use(this, message);
                            }
                            catch (Exception e)
                            {
                                Server.ErrorLog(e);
                                SendMessage("Une erreur est arrive lors de l'utilisation de la commande !");
                            }
                        }));
                        commThread.Start();
                    }
                    else { SendMessage("Vous n'etes pas autorise a utiliser \"" + cmd + "\"!"); }
                }
                else if (Block.Byte(cmd.ToLower()) != Block.Zero)
                {
                    HandleCommand("mode", cmd.ToLower());
                }
                else
                {
                    bool retry = true;

                    switch (cmd.ToLower())
                    {    //Check for command switching
                        case "cut": cmd = "copy"; message = "cut"; break;

                        case "ps": message = "ps " + message; cmd = "map"; break;

                        //How about we start adding commands from other softwares
                        //and seamlessly switch here?
                        case "bhb":
                        case "hbox": cmd = "cuboid"; message = "hollow"; break;
                        case "blb":
                        case "box": cmd = "cuboid"; break;
                        case "sphere": cmd = "spheroid"; break;
                        case "cmdlist":
                        case "commands":
                        case "cmdhelp": cmd = "help"; break;

                        default: retry = false; break;  //Unknown command, then
                    }

                    if (retry) HandleCommand(cmd, message);
                    else SendMessage("Commande inconnue \"" + cmd + "\"!");
                }
            }
            catch (Exception e) { Server.ErrorLog(e); SendMessage("Command failed."); }
        }
        void HandleQuery(string to, string message)
        {
            if (to == "console") { 
                Server.s.Log("Mp " + name + " : " + message);
                SendChat(this, Server.DefaultColor + "[<] Mp console: &f" + message);
                return; 
            } 
            Player p = Find(to);
            if (p == this) { SendMessage("Vous essayez de vous parlez a vous meme, humm ?"); return; }
            if (p != null && !p.hidden)
            {
                Server.s.Log(name + " @" + p.name + ": " + message);
                SendChat(this, Server.DefaultColor + "[<] " + p.color + p.Name() + ": &f" + message);
                SendChat(p, "&9[>] " + color + Name() + ": &f" + message);
            }
            else { SendMessage("Le joueur \"" + to + "\" n'existe pas!"); }
        }
        #endregion
        #region == OUTGOING ==
        public void SendRaw(int id)
        {
            SendRaw(id, new byte[0]);
        }
        public void SendRaw(int id, byte[] send)
        {
            byte[] buffer = new byte[send.Length + 1];
            buffer[0] = (byte)id;

            Buffer.BlockCopy(send, 0, buffer, 1, send.Length);
            string TxStr = "";
            for (int i = 0; i < buffer.Length; i++)
            {
                TxStr += buffer[i] + " ";
            }
            int tries = 0;
        retry: try
            {
            
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, delegate(IAsyncResult result) { }, null);
          /*      if (buffer[0] != 1)
                {
                    Server.s.Log("Buffer ID: " + buffer[0]);
                    Server.s.Log("BUFFER LENGTH: " + buffer.Length);
                    Server.s.Log(TxStr);
                }*/
            }
            catch (SocketException)
            {
                tries++;
                if (tries > 2)
                    Disconnect();
                else goto retry;
            }
        }

        public static void SendMessage(Player p, string message)
        {
            if (p == null) {
                if (storeHelp)
                {
                    storedHelp += message + "\r\n";
                }
                else
                {
                    Server.s.Log(message);
                    IRCBot.Say(message, true); 
                }
                return; 
            }
            p.SendMessage(p.id, Server.DefaultColor + message);
        }
        public void SendMessage(string message)
        {
            if (this == null) { Server.s.Log(message); return; }
            unchecked { SendMessage(id, Server.DefaultColor + message); }
        }
        public void SendChat(Player p, string message)
        {
            if (this == null) { Server.s.Log(message); return; }
            Player.SendMessage(p, message);
        }
        public void SendMessage(byte id, string message, bool save = true)
        {
            if (this == null) { Server.s.Log(message); return; }

            if (save)
            {
                addMessage(message);
                if (ingame) { return; }
            }

            byte[] buffer = new byte[65];
            unchecked { buffer[0] = id; }

            message = message.Replace("(noir)", "&0");
            message = message.Replace("(black)", "&0");
            message = message.Replace("(bleuf)", "&1");
            message = message.Replace("(navy)", "&1");
            message = message.Replace("(vert)", "&2");
            message = message.Replace("(green)", "&2");
            message = message.Replace("(cyan)", "&3");
            message = message.Replace("(teal)", "&3");
            message = message.Replace("(marron)", "&4");
            message = message.Replace("(maroon)", "&4");
            message = message.Replace("(violet)", "&5");
            message = message.Replace("(purple)", "&5");
            message = message.Replace("(or)", "&6");
            message = message.Replace("(gold)", "&6");
            message = message.Replace("(gris)", "&7");
            message = message.Replace("(silver)", "&7");
            message = message.Replace("(grisf)", "&8");
            message = message.Replace("(gray)", "&8");
            message = message.Replace("(bleu)", "&9");
            message = message.Replace("(blue)", "&9");
            message = message.Replace("(vertc)", "&a");
            message = message.Replace("(lime)", "&a");
            message = message.Replace("(cyanc)", "&b");
            message = message.Replace("(aqua)", "&b");
            message = message.Replace("(rouge)", "&c");
            message = message.Replace("(red)", "&c");
            message = message.Replace("(rose)", "&d");
            message = message.Replace("(pink)", "&d");
            message = message.Replace("(jaune)", "&e");
            message = message.Replace("(yellow)", "&e");
            message = message.Replace("(blanc)", "&f");
            message = message.Replace("(white)", "&f");

            while (message.Contains("  "))
            { message = message.Replace("  ", " "); }

            for (int i = 0; i < 10; i++)
            { message = message.Replace("%" + i, "&" + i); }
            for (char ch = 'a'; ch <= 'f'; ch++)
            { message = message.Replace("%" + ch, "&" + ch); }

            for (int i = 0; i < 10; i++)
            { 
                message = message.Replace(" &" + i + " &", " &");
                message = message.Replace("&" + i + " &", "&");
                message = message.Replace(" &" + i + " ", " &" + i); 
                message = message.Replace("&" + i + "&", "&");
            }
            for (char ch = 'a'; ch <= 'f'; ch++)
            {
                message = message.Replace(" &" + ch + " &", " &");
                message = message.Replace("&" + ch + " &", "&");
                message = message.Replace(" &" + ch + " ", " &" + ch);
                message = message.Replace("&" + ch + "&", "&");
            }
            while (message.Contains("  "))
            { message = message.Replace("  ", " "); }

            if (Server.dollardollardollar)
                message = message.Replace("$name", "$" + Name());
            else
                message = message.Replace("$name", Name());
            message = message.Replace("$title", title);
            message = message.Replace("$tcolor", titlecolor);
            message = message.Replace("$date", DateTime.Now.ToString("yyyy-MM-dd"));
            message = message.Replace("$time", DateTime.Now.ToString("HH:mm:ss"));
            message = message.Replace("$ip", ip);
            message = message.Replace("$color", color);
            message = message.Replace("$rank", group.name);
            message = message.Replace("$level", level.name);
            message = message.Replace("$world", level.world);
            message = message.Replace("$deaths", overallDeath.ToString());
            message = message.Replace("$money", money.ToString());
            message = message.Replace("$blocks", overallBlocks.ToString());
            message = message.Replace("$first", firstLogin.ToString());
            message = message.Replace("$kicked", totalKicked.ToString());
            message = message.Replace("$server", Server.name);
            message = message.Replace("$motd", Server.motd);
            message = message.Replace("$position", (pos[0] / 32) + " " + (pos[1] / 32 - 1) + " " + (pos[2] / 32) );
            message = message.Replace("$nbmapsmax", nbMapsMax.ToString());
            message = message.Replace("$nbmaps", nbMaps.ToString());
            message = message.Replace("$statu", rang.ToString());

            message = message.Replace("$irc", Server.ircServer + " > " + Server.ircChannel);

            if (Server.parseSmiley && parseSmiley)
            {
                message = message.Replace(":)", "(darksmile)");
                message = message.Replace(":D", "(smile)");
                message = message.Replace("<3", "(heart)");
                message = message.Replace("(**)", "^^");
                message = message.Replace("(*)", "^");
            }

            byte[] stored = new byte[1];

            stored[0] = (byte)1;
            message = message.Replace("(darksmile)", enc.GetString(stored));
            stored[0] = (byte)2;
            message = message.Replace("(smile)", enc.GetString(stored));
            stored[0] = (byte)3;
            message = message.Replace("(heart)", enc.GetString(stored));
            stored[0] = (byte)4;
            message = message.Replace("(diamond)", enc.GetString(stored));
            stored[0] = (byte)7;
            message = message.Replace("(bullet)", enc.GetString(stored));
            stored[0] = (byte)8;
            message = message.Replace("(hole)", enc.GetString(stored));
            stored[0] = (byte)11;
            message = message.Replace("(male)", enc.GetString(stored));
            stored[0] = (byte)12;
            message = message.Replace("(female)", enc.GetString(stored));
            stored[0] = (byte)13;
            message = message.Replace("(note)", enc.GetString(stored));
            stored[0] = (byte)14;
            message = message.Replace("(note2)", enc.GetString(stored));
            stored[0] = (byte)15;
            message = message.Replace("(sun)", enc.GetString(stored));
            stored[0] = (byte)16;
            message = message.Replace("(right)", enc.GetString(stored));
            stored[0] = (byte)17;
            message = message.Replace("(left)", enc.GetString(stored));
            stored[0] = (byte)19;
            message = message.Replace("(double)", enc.GetString(stored));
            stored[0] = (byte)22;
            message = message.Replace("(half)", enc.GetString(stored));
            stored[0] = (byte)24;
            message = message.Replace("(uparrow)", enc.GetString(stored));
            stored[0] = (byte)25;
            message = message.Replace("(downarrow)", enc.GetString(stored));
            stored[0] = (byte)26;
            message = message.Replace("(rightarrow)", enc.GetString(stored));
            stored[0] = (byte)30;
            message = message.Replace("(up)", enc.GetString(stored));
            stored[0] = (byte)31;
            message = message.Replace("(down)", enc.GetString(stored));

            int totalTries = 0;
        retryTag: try
            {
                foreach (string line in Wordwrap(message))
                {
                    string newLine = line;
                    if (newLine.TrimEnd(' ')[newLine.TrimEnd(' ').Length - 1] < '!')
                    {
                        newLine += '\'';
                    }

                    StringFormat(newLine, 64).CopyTo(buffer, 1);
                    SendRaw(13, buffer);
                }
            }
            catch (Exception e)
            {
                message = "&f" + message;
                totalTries++;
                if (totalTries < 10) goto retryTag;
                else Server.ErrorLog(e);
            }
        }
        public void SendMotd()
        {
            byte[] buffer = new byte[130];
            buffer[0] = (byte)8;
            StringFormat(Server.name, 64).CopyTo(buffer, 1);
            StringFormat(Server.motd, 64).CopyTo(buffer, 65);

            if (Block.canPlace(this, Block.blackrock))
                buffer[129] = 100;
            else
                buffer[129] = 0;

            SendRaw(0, buffer);
            
        }

        public void SendUserMOTD()
        {
            byte[] buffer = new byte[130];
            buffer[0] = Server.version;
            if (level.motd == "ignore") { StringFormat(Server.name, 64).CopyTo(buffer, 1); StringFormat(Server.motd, 64).CopyTo(buffer, 65); }
            else StringFormat(level.motd, 128).CopyTo(buffer, 1);

            if (Block.canPlace(group.Permission, Block.blackrock))
                buffer[129] = 100;
            else
                buffer[129] = 0;
            SendRaw(0, buffer);
        }

        public void SendMap()
        {
            SendRaw(2);
            byte[] buffer = new byte[level.blocks.Length + 4];
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(level.blocks.Length)).CopyTo(buffer, 0);

            for (int i = 0; i < level.blocks.Length; ++i)
            {
                buffer[4 + i] = Block.Convert(level.blocks[i]);
            }

            buffer = GZip(buffer);
            int number = (int)Math.Ceiling(((double)buffer.Length) / 1024);
            for (int i = 1; buffer.Length > 0; ++i)
            {
                short length = (short)Math.Min(buffer.Length, 1024);
                byte[] send = new byte[1027];
                HTNO(length).CopyTo(send, 0);
                Buffer.BlockCopy(buffer, 0, send, 2, length);
                byte[] tempbuffer = new byte[buffer.Length - length];
                Buffer.BlockCopy(buffer, length, tempbuffer, 0, buffer.Length - length);
                buffer = tempbuffer;
                send[1026] = (byte)(i * 100 / number);
                SendRaw(3, send);
                if (ip == "127.0.0.1") { }
                else if (Server.updateTimer.Interval > 1000) Thread.Sleep(100);
                else Thread.Sleep(10);
            } buffer = new byte[6];
            HTNO((short)level.width).CopyTo(buffer, 0);
            HTNO((short)level.depth).CopyTo(buffer, 2);
            HTNO((short)level.height).CopyTo(buffer, 4);
            SendRaw(4, buffer);
            Loading = false;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public void SendSpawn(byte id, string name, ushort x, ushort y, ushort z, byte rotx, byte roty)
        {
            //pos = new ushort[3] { x, y, z }; // This could be remove and not effect the server :/
            rot = new byte[2] { rotx, roty };
            byte[] buffer = new byte[73]; buffer[0] = id;
            StringFormat(name, 64).CopyTo(buffer, 1);
            HTNO(x).CopyTo(buffer, 65);
            HTNO(y).CopyTo(buffer, 67);
            HTNO(z).CopyTo(buffer, 69);
            buffer[71] = rotx; buffer[72] = roty;
            SendRaw(7, buffer);
        }
        public void SendPos(byte id, ushort x, ushort y, ushort z, byte rotx, byte roty)
        {
            if (x < 0) x = 32;
            if (y < 0) y = 32;
            if (z < 0) z = 32;
            if (x > level.width * 32) x = (ushort)(level.width * 32 - 32);
            if (z > level.height * 32) z = (ushort)(level.height * 32 - 32);
            if (x > 32767) x = 32730;
            if (y > 32767) y = 32730;
            if (z > 32767) z = 32730;

            lastTp = DateTime.Now;
            pos[0] = x; pos[1] = y; pos[2] = z;
            rot[0] = rotx; rot[1] = roty;

            /*
            pos = new ushort[3] { x, y, z };
            rot = new byte[2] { rotx, roty };*/
            byte[] buffer = new byte[9]; buffer[0] = id;
            HTNO(x).CopyTo(buffer, 1);
            HTNO(y).CopyTo(buffer, 3);
            HTNO(z).CopyTo(buffer, 5);
            buffer[7] = rotx; buffer[8] = roty;
            SendRaw(8, buffer);
        }
        //TODO: Figure a way to SendPos without changing rotation
        public void SendDie(byte id) { SendRaw(0x0C, new byte[1] { id }); }
        public void SendBlockchange(ushort x, ushort y, ushort z, byte type)
        {
            if (x < 0 || y < 0 || z < 0) return;
            if (x >= level.width || y >= level.depth || z >= level.height) return;

            byte[] buffer = new byte[7];
            HTNO(x).CopyTo(buffer, 0);
            HTNO(y).CopyTo(buffer, 2);
            HTNO(z).CopyTo(buffer, 4);
            buffer[6] = Block.Convert(type);
            SendRaw(6, buffer);
        }
        void SendKick(string message) { SendRaw(14, StringFormat(message, 64)); }
        void SendPing() { /*pingDelay = 0; pingDelayTimer.Start();*/ SendRaw(1); }
        void UpdatePosition()
        {

            //pingDelayTimer.Stop();

            // Shameless copy from JTE's Server
            byte changed = 0;   //Denotes what has changed (x,y,z, rotation-x, rotation-y)
            // 0 = no change - never happens with this code.
            // 1 = position has changed
            // 2 = rotation has changed
            // 3 = position and rotation have changed
            // 4 = Teleport Required (maybe something to do with spawning)
            // 5 = Teleport Required + position has changed
            // 6 = Teleport Required + rotation has changed
            // 7 = Teleport Required + position and rotation has changed
            //NOTE: Players should NOT be teleporting this often. This is probably causing some problems.
            if (oldpos[0] != pos[0] || oldpos[1] != pos[1] || oldpos[2] != pos[2])
                changed |= 1;

            if (oldrot[0] != rot[0] || oldrot[1] != rot[1])
            {
                changed |= 2;
            }
            if (Math.Abs(pos[0] - basepos[0]) > 32 || Math.Abs(pos[1] - basepos[1]) > 32 || Math.Abs(pos[2] - basepos[2]) > 32)
                changed |= 4;

            if ((oldpos[0] == pos[0] && oldpos[1] == pos[1] && oldpos[2] == pos[2]) && (basepos[0] != pos[0] || basepos[1] != pos[1] || basepos[2] != pos[2]))
                changed |= 4;

            byte[] buffer = new byte[0]; byte msg = 0;
            if ((changed & 4) != 0)
            {
                msg = 8; //Player teleport - used for spawning or moving too fast
                buffer = new byte[9]; buffer[0] = id;
                HTNO(pos[0]).CopyTo(buffer, 1);
                HTNO(pos[1]).CopyTo(buffer, 3);
                HTNO(pos[2]).CopyTo(buffer, 5);
                buffer[7] = rot[0];

                if (Server.flipHead)
                    if (rot[1] > 64 && rot[1] < 192)
                        buffer[8] = rot[1];
                    else
                        buffer[8] = (byte)(rot[1] - (rot[1] - 128));
                else
                    buffer[8] = rot[1];

                //Realcode
                //buffer[8] = rot[1];
            }
            else if (changed == 1)
            {
                try
                {
                    msg = 10; //Position update
                    buffer = new byte[4]; buffer[0] = id;
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[0] - oldpos[0])), 0, buffer, 1, 1);
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[1] - oldpos[1])), 0, buffer, 2, 1);
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[2] - oldpos[2])), 0, buffer, 3, 1);
                }
                catch { }
            }
            else if (changed == 2)
            {
                msg = 11; //Orientation update
                buffer = new byte[3]; buffer[0] = id;
                buffer[1] = rot[0];

                if (Server.flipHead)
                    if (rot[1] > 64 && rot[1] < 192)
                        buffer[2] = rot[1];
                    else
                        buffer[2] = (byte)(rot[1] - (rot[1] - 128));
                else
                    buffer[2] = rot[1];

                //Realcode
                //buffer[2] = rot[1];
            }
            else if (changed == 3)
            {
                try
                {
                    msg = 9; //Position and orientation update
                    buffer = new byte[6]; buffer[0] = id;
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[0] - oldpos[0])), 0, buffer, 1, 1);
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[1] - oldpos[1])), 0, buffer, 2, 1);
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[2] - oldpos[2])), 0, buffer, 3, 1);
                    buffer[4] = rot[0];

                    if (Server.flipHead)
                        if (rot[1] > 64 && rot[1] < 192)
                            buffer[5] = rot[1];
                        else
                            buffer[5] = (byte)(rot[1] - (rot[1] - 128));
                    else
                        buffer[5] = rot[1];

                    //Realcode
                    //buffer[5] = rot[1];
                }
                catch { }
            }
            
            BaseGame game = Server.allGames.Find(g => g.lvl == level);
            if (game != null) { if (game.cheat == false && game.gameOn) { cheatVerif(game); } }
            oldpos = pos; oldrot = rot;
            if (changed != 0)
            {
                try
                {
                    foreach (Player p in players)
                    {
                        if (p != this && p.level == level)
                        {
                            p.SendRaw(msg, buffer);
                        }
                    }
                }
                catch { }
            }
        }
        #endregion
        #region == GLOBAL MESSAGES ==
        public static void GlobalBlockchange(Level level, ushort x, ushort y, ushort z, byte type)
        {
            players.ForEach(delegate(Player p) { if (p.level == level) { p.SendBlockchange(x, y, z, type); } });
        }
        public static void GlobalChat(Player from, string message) { GlobalChat(from, message, true); }
        public static void GlobalChat(Player from, string message, bool showname)
        {
            if (showname) { message = from.color + from.voicestring + from.rang + from.color + from.prefix + from.Name() + ": &f" + message; } 
            players.ForEach(delegate(Player p) 
            { if (p.level.worldChat) Player.SendMessage(p, message); });
        }
        public static void GlobalChatLevel(Player from, string message, bool showname)
        {
            if (showname) { message = "<Level>" + from.color + from.voicestring + from.color + from.rang + from.prefix + from.Name() + ": &f" + message; }
            players.ForEach(delegate(Player p) { if (p.level == from.level) Player.SendMessage(p, Server.DefaultColor + message); });
        }
        public static void GlobalChatWorld(Player from, string message, bool showname)
        {
            if (showname) { message = "<World>" + from.color + from.voicestring + from.color + from.rang + from.prefix + from.Name() + ": &f" + message; }
            players.ForEach(delegate(Player p) { if (p.level.worldChat) Player.SendMessage(p, Server.DefaultColor + message); });
        }
        public static void GlobalMessage(string message)
        { players.ForEach(delegate(Player p) { if (p.level.worldChat) Player.SendMessage(p, message); }); }
        public static void GlobalMessageLevel(Level l, string message)
        {
            players.ForEach(delegate(Player p) { if (p.level == l) Player.SendMessage(p, message); });
        }
        public static void GlobalMessageOps(string message)
        {
            try
            {
                players.ForEach(delegate(Player p)
                {
                    if (p.group.Permission >= Server.opchatperm || Server.devs.Contains(p.name.ToLower()))
                    {
                        Player.SendMessage(p, message);
                    }
                });
            }
            catch { Server.s.Log("Error occured with Op Chat"); }
        }
        public static void GlobalSpawn(Player from, ushort x, ushort y, ushort z, byte rotx, byte roty, bool self, string possession = "")
        {
            players.ForEach(delegate(Player p)
            {

                if (p.Loading && p != from) { return; }
                if (p.level != from.level || (from.hidden && !self)) { return; }
                if (p != from) 
                { 
                    if (from.skin != "") { p.SendSpawn(from.id, from.color + from.skin + possession, x, y, z, rotx, roty); }
                    else { p.SendSpawn(from.id, from.color + from.name + possession, x, y, z, rotx, roty); }
                }
                else if (self)
                {
                    if (!p.ignorePermission)
                    {
                        p.pos = new ushort[3] { x, y, z }; p.rot = new byte[2] { rotx, roty };
                        p.oldpos = p.pos; p.basepos = p.pos; p.oldrot = p.rot;
                        unchecked { p.SendSpawn((byte)-1, from.color + from.name + possession, x, y, z, rotx, roty); }
                    }
                }
            });
        }
        public static void GlobalDie(Player from, bool self)
        {
            players.ForEach(delegate(Player p)
            {
                if (p.level != from.level || (from.hidden && !self)) { return; }
                if (p != from) { p.SendDie(from.id); }
                else if (self) { unchecked { p.SendDie((byte)-1); } }
            });
        }

        //custom
        public static void GlobalSalon(Server.salon sa, string message, bool viewname = true)
        {
            if (sa.pliste == null) { return; }

            foreach (Player who in sa.pliste)
            {
                if (who == null) { continue; }
                if (viewname) { Player.SendMessage(who, sa.name + " >> " + message); }
                else { Player.SendMessage(who, message); }
            }
        }

        public bool MarkPossessed(string marker = "")
        {
            if (marker != "")
            {
                Player controller = Player.Find(marker);
                if (controller == null)
                {
                    return false;
                }
                marker = " (" + controller.color + controller.name + color + ")";
            }
            GlobalDie(this, true);
            GlobalSpawn(this, pos[0], pos[1], pos[2], rot[0], rot[1], true, marker);
            return true;
        }

        public static void GlobalUpdate() { players.ForEach(delegate(Player p) { if (!p.hidden) { p.UpdatePosition(); } }); }
        #endregion
        #region == DISCONNECTING ==
        public void Disconnect() { leftGame(); }
        public void Kick(string kickString) { leftGame(kickString); }

        public void leftGame(string kickString = "", bool skip = false)
        {
            try
            {
                if (disconnected)
                {
                    if (connections.Contains(this)) connections.Remove(this);
                    return;
                }
                //   FlyBuffer.Clear();
                disconnected = true;
                pingTimer.Stop();
                afkTimer.Stop();
                afkCount = 0;
                afkStart = DateTime.Now;

                if (Server.afkset.Contains(name)) Server.afkset.Remove(name);

                if (kickString == "") kickString = "Deconnecte.";
                
                SendKick(kickString);

                if (loggedIn)
                {
                    isFlying = false;
                    aiming = false;

                    if (team != null)
                    {
                        team.RemoveMember(this);
                    }

                    GlobalDie(this, false);
                    if (kickString == "Deconnecte." || kickString.IndexOf("Arret du serveur") != -1 || kickString == Server.customShutdownMessage)
                    {
                        if (!hidden) { GlobalChat(this, "&c- " + color + prefix + Name() + Server.DefaultColor + " deconnecte.", false); }
                        else { GlobalMessageOps("&c- " + color + prefix + Name() + Server.DefaultColor + " deconnecte."); }
                        IRCBot.Say(name + " left the game.");
                        Server.s.Log(name + " disconnected.");
                    }
                    else
                    {
                        totalKicked++;
                        GlobalChat(this, "&c- " + color + prefix + Name() + Server.DefaultColor + " kick (" + kickString + ").", false);
                        IRCBot.Say(name + " kicked (" + kickString + ").");
                        Server.s.Log(name + " kicked (" + kickString + ").");
                    }

                    try { save(); }
                    catch (Exception e) { Server.ErrorLog(e); }

                    if (salon != "")
                    {
                        Server.salon sa = Server.findSalon(salon);
                        if (sa.name != "")
                        {sa.pliste.Remove(this);}
                    }

                    players.Remove(this);
                    Server.s.PlayerListUpdate();
                    left.Add(name.ToLower(), ip);

                    if (Server.AutoLoad && level.unload)
                    {
                        foreach (Player pl in Player.players)
                            if (pl.level == level) return;
                        if (!level.name.Contains("Museum " + Server.DefaultColor))
                        {
                            level.Unload();
                        }
                    }

                    try
                    {
                        if (!Directory.Exists("extra/undo")) Directory.CreateDirectory("extra/undo");
                        if (!Directory.Exists("extra/undoPrevious")) Directory.CreateDirectory("extra/undoPrevious");
                        DirectoryInfo di = new DirectoryInfo("extra/undo");
                        if (di.GetDirectories("*").Length >= Server.totalUndo)
                        {
                            Directory.Delete("extra/undoPrevious", true);
                            Directory.Move("extra/undo", "extra/undoPrevious");
                            Directory.CreateDirectory("extra/undo");
                        }

                        if (!Directory.Exists("extra/undo/" + name)) Directory.CreateDirectory("extra/undo/" + name);
                        di = new DirectoryInfo("extra/undo/" + name);
                        StreamWriter w = new StreamWriter(File.Create("extra/undo/" + name + "/" + di.GetFiles("*.undo").Length + ".undo"));

                        foreach (UndoPos uP in UndoBuffer)
                        {
                            w.Write(uP.mapName + " " + uP.worldName + " " +
                                    uP.x + " " + uP.y + " " + uP.z + " " +
                                    uP.timePlaced.ToString().Replace(' ', '&') + " " +
                                    uP.type + " " + uP.newtype + " ");
                        }
                        w.Flush();
                        w.Close();
                    }
                    catch (Exception e) { Server.ErrorLog(e); }

                    UndoBuffer.Clear();
                }
                else
                {
                    connections.Remove(this);
                    Server.s.Log(ip + " disconnected.");
                }
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }


        #endregion
        #region == CHECKING ==
        public static Player Find(string name)
        {
            List<Player> tempList = new List<Player>();
            tempList.AddRange(players);
            Player tempPlayer = null; bool returnNull = false;

            foreach (Player p in tempList)
            {
                if (p.name.ToLower() == name.ToLower()) return p;
                if (p.name.ToLower().IndexOf(name.ToLower()) != -1)
                {
                    if (tempPlayer == null) tempPlayer = p;
                    else returnNull = true;
                }
            }

            if (returnNull == true) return null;
            if (tempPlayer != null) return tempPlayer;
            return null;
        }
        #endregion
        #region == OTHER ==
        static byte FreeId()
        {
            for (byte i = 0; i < 255; i++)
            {
                bool used = false;
                foreach (Player p in players)
                    if (p.id == i) used = true;
                if (!used)
                    return i;
            }
            return (byte)1;
        }
        static byte[] StringFormat(string str, int size)
        {
            byte[] bytes = new byte[size];
            bytes = enc.GetBytes(str.PadRight(size).Substring(0, size));
            return bytes;
        }
        static List<string> Wordwrap(string message)
        {
            List<string> lines = new List<string>();
            message = Regex.Replace(message, @"(&[0-9a-f])+(&[0-9a-f])", "$2");
            message = Regex.Replace(message, @"(&[0-9a-f])+$", "");

            int limit = 64; string color = "";

            while (message.Length > 0)
            {
                //if (Regex.IsMatch(message, "&a")) break;

                if (lines.Count > 0)
                {
                    if (message[0].ToString() == "&")
                        message = "> " + message.Trim();
                    else
                        message = "> " + color + message.Trim();
                }

                if (message.IndexOf("&") == message.IndexOf("&", message.IndexOf("&") + 1) - 2)
                    message = message.Remove(message.IndexOf("&"), 2);

                if (message.Length <= limit) { lines.Add(message); break; }
                for (int i = limit - 1; i > limit - 20; --i)
                    if (message[i] == ' ')
                    {
                        lines.Add(message.Substring(0, i));
                        goto Next;
                    }

            retry:
                if (message.Length == 0 || limit == 0) { return lines; }

                try
                {
                    if (message.Substring(limit - 2, 1) == "&" || message.Substring(limit - 1, 1) == "&")
                    {
                        message = message.Remove(limit - 2, 1);
                        limit -= 2;
                        goto retry;
                    }
                    else if (message[limit - 1] < 32 || message[limit - 1] > 127)
                    {
                        message = message.Remove(limit - 1, 1);
                        limit -= 1;
                        //goto retry;
                    }
                }
                catch { return lines; }
                lines.Add(message.Substring(0, limit));

            Next: message = message.Substring(lines[lines.Count - 1].Length);
                if (lines.Count == 1) limit = 60;

                int index = lines[lines.Count - 1].LastIndexOf('&');
                if (index != -1)
                {
                    if (index < lines[lines.Count - 1].Length - 1)
                    {
                        char next = lines[lines.Count - 1][index + 1];
                        if ("0123456789abcdef".IndexOf(next) != -1) { color = "&" + next; }
                        if (index == lines[lines.Count - 1].Length - 1)
                        {
                            lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, lines[lines.Count - 1].Length - 2);
                        }
                    }
                    else if (message.Length != 0)
                    {
                        char next = message[0];
                        if ("0123456789abcdef".IndexOf(next) != -1)
                        {
                            color = "&" + next;
                        }
                        lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, lines[lines.Count - 1].Length - 1);
                        message = message.Substring(1);
                    }
                }
            } return lines;
        }
        public static bool ValidName(string name)
        {
            string allowedchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890._";
            foreach (char ch in name) { if (allowedchars.IndexOf(ch) == -1) { return false; } } return true;
        }
        public static byte[] GZip(byte[] bytes)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            GZipStream gs = new GZipStream(ms, CompressionMode.Compress, true);
            gs.Write(bytes, 0, bytes.Length);
            gs.Close();
            gs.Dispose();
            ms.Position = 0;
            bytes = new byte[ms.Length];
            ms.Read(bytes, 0, (int)ms.Length);
            ms.Close();
            ms.Dispose();
            return bytes;
        }
        #endregion
        #region == Host <> Network ==
        public static byte[] HTNO(ushort x)
        {
            byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y;
        }
        public static ushort NTHO(byte[] x, int offset)
        {
            byte[] y = new byte[2];
            Buffer.BlockCopy(x, offset, y, 0, 2); Array.Reverse(y);
            return BitConverter.ToUInt16(y, 0);
        }
        public static byte[] HTNO(short x)
        {
            byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y;
        }
        #endregion

        bool CheckBlockSpam()
        {
            if (spamBlockLog.Count >= spamBlockCount)
            {
                DateTime oldestTime = spamBlockLog.Dequeue();
                double spamTimer = DateTime.Now.Subtract(oldestTime).TotalSeconds;
                if (spamTimer < spamBlockTimer && !ignoreGrief)
                {
                    Kick("Vous avez ete expulse par le systeme antigrief. Ralentissez.");
                    GlobalMessageOps(c.red + name + " a été kick pour pour soupcon de grief.");
                    Server.s.Log(name + " was kicked for block spam (" + spamBlockCount + " blocks in " + spamTimer + " seconds)");
                    return true;
                }
            }
            spamBlockLog.Enqueue(DateTime.Now);
            return false;
        }
    }
}