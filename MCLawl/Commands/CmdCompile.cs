using System;

namespace MCWorlds
{
    public class CmdCompile : Command
    {
        public override string name { get { return "compile"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdCompile() { }

        public override void Use(Player p, string message)
        {
            if(message == "" || message.IndexOf(' ') != -1) { Help(p); return; }
            bool success = false;
            try
            {
                 success = Scripting.Compile(message);
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
                Player.SendMessage(p, "Une exception a ete trouve lors de la compilation.");
                return;
            }
            if (success)
            { Player.SendMessage(p, "Compilation reussi."); }
            else
            { Player.SendMessage(p, "Compilation echoue.  Lisez compile.log pour plus d'informations."); }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/compile [nom class] - compile le fichier class en DLL.");
            Player.SendMessage(p, "&cAttention, la commande doit etre en namespace MCWorlds !");
        }
    }
}
