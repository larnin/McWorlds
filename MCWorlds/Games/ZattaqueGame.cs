using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace MCWorlds
{
    public class ZattaqueGame : BaseGame
    {

        public ZattaqueGame()
        {
            typeGame = "zombieattaque";

            loadCmds();
        }

        public override void loadGame(Player p, string file)
        {

        }

        public override void saveGame(Player p, string file)
        {

        }

        public override void startGame(Player p)
        {

        }

        public override void stopGame(Player p)
        {

        }

        public override void deleteGame(Player p)
        {

        }

        public override bool changebloc(Player p, byte b, ushort x, ushort y, ushort z, byte action)
        {
            return true;
        }

        public override bool checkPos(Player p, ushort x, ushort y, ushort z)
        {
            return true;
        }

        public override void death(Player p)
        {

        }
    }
}
