

using System;

    namespace MCLawl
    {
        public class Cmdportail : Command
        {
            public override string name { get { return "portail"; } }
            public override string shortcut { get { return "p"; } }
            public override string type { get { return "other"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
            public override bool museumUsable { get { return false; } }

            public override void Use(Player p, string message)
            {
                 if ( message == "orange" )
                 {
                    p.BlockAction = 7 ;
                    Player.SendMessage(p, "portail orange active");
                 }
                 else if ( message == "blue" )
                 {
                    p.BlockAction = 8 ;
                    Player.SendMessage(p, "portail bleu active");
                 }
                 else if ( message == "off" )
                 {
                    p.BlockAction = 0 ;
                    Player.SendMessage(p, "portail desactive");
                 }
                 else
                 {
                     this.Help(p);
                 }
            }

            public override void Help(Player p)
            {
             Player.SendMessage(p, "/portail [orange/blue/off] - Active le mode portail, [off] pour aretter.");
            }
        }
    }
